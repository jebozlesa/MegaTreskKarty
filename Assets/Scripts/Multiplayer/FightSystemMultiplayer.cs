using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;
using System.Threading.Tasks;

public enum FightStateMultiplayer { START, TURN, ENDTURN, PLAYERDEATH, ENEMYDEATH, WON, LOST }

public class FightSystemMultiplayer : MonoBehaviour
{
    private const string MatchPhaseCompleted = "completed";
    private const int MatchReadyPollDelayMs = 1000;
    private const int MatchReadyMaxAttempts = 30;

    [System.Serializable]
    public class PendingOngoingActionTurn
    {
        public string actionType;
        public int sourceAttackId;
        public string targetCardId;
        public int turnsRemaining;
    }

    // Multiplayer player references
    public Player player;
    public Player enemy;

    // Boards
    public GameObject playerBoard;
    public GameObject enemyBoard;

    // UI elements
    public TMP_Text dialogText;
    public Image dialogButtonBorder;

    public TMP_Text button1Text;
    public TMP_Text button2Text;
    public TMP_Text button3Text;
    public TMP_Text button4Text;

    public TMP_Text button1CountText;
    public TMP_Text button2CountText;
    public TMP_Text button3CountText;
    public TMP_Text button4CountText;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;

    public FightStateMultiplayer state;

    // Card prefabs and objects
    public GameObject kartaPrefab;
    public GameObject hrac;
    public GameObject nepriatel;

    public Attack attack;
    public AttackDescriptions attackDescriptions;
    public AttackNamesLoader attackNamesLoader;
    public AttackCountLoader attackCountLoader;
    public AttackSelectionManager attackSelectionManager;

    public Effects effects;

    int playerAttack;
    int enemyAttack;

    public RecordHandler recordHandler;

    private string connectionString;

    public static bool IsLoggedIn = false;

    // Multiplayer-specific variables
    public string myPlayerId;
    public string roomCode;
    public bool isWaitingForOpponent;
    public ServerFunctionsManager serverFunctionsManager;
    public List<Player> roomPlayers;
    public List<string> playerDecks;

    // Nove referencie na battle komponenty
    public MultiplayerService multiplayerService;
    public MultiplayerUI multiplayerUI;
    public MultiplayerHandManager multiplayerHandManager;
    public MultiplayerBoardManager multiplayerBoardManager;
    
    // Battle system komponenty
    public BattleSubmitter battleSubmitter;
    public BattleResultProcessor battleResultProcessor;

    private PendingOngoingActionTurn pendingOngoingActionTurn;


    void Start()
    {
        Debug.LogWarning("[FightSystemMultiplayer] Start() called");
        
        // [OK] Auto-find MultiplayerUI ak nie je assigned v Inspector
        if (multiplayerUI == null)
        {
            multiplayerUI = FindFirstObjectByType<MultiplayerUI>();
            if (multiplayerUI == null)
            {
                Debug.LogError("[FightSystemMultiplayer] [ERR] MultiplayerUI not found in scene!");
            }
            else
            {
                Debug.LogWarning("[FightSystemMultiplayer] [WARN] Auto-found MultiplayerUI (prefer Inspector setup)");
            }
        }
        
        // [OK] Set initial status message (replaces Unity Inspector default "Fight!")
        if (multiplayerUI != null)
        {
            Debug.LogWarning("[FightSystemMultiplayer] Setting status to 'Loading...'");
            multiplayerUI.ShowStatus(MultiplayerUI.MSG_LOADING);
        }
        else if (dialogText != null)
        {
            // Fallback ak MultiplayerUI chyba
            Debug.LogWarning("[FightSystemMultiplayer] Using dialogText fallback");
            dialogText.text = MultiplayerUI.MSG_LOADING;
        }
        
        if (multiplayerBoardManager == null)
        {
            multiplayerBoardManager = GetComponent<MultiplayerBoardManager>() ?? GetComponentInChildren<MultiplayerBoardManager>();
        }
        if (multiplayerBoardManager != null)
        {
            multiplayerBoardManager.Configure(this);
        }
        else
        {
            Debug.LogWarning("[FightSystemMultiplayer] MultiplayerBoardManager not assigned; turn flow will not function.");
        }

        state = FightStateMultiplayer.START;
        _ = StartMultiplayerAsync();
    }

    private async Task StartMultiplayerAsync()
    {
        Debug.LogWarning("[FightSystemMultiplayer] StartMultiplayerAsync() called");
        
        myPlayerId = PlayFabManagerLogin.Instance.LoggedInPlayerId;
        roomCode = PlayerPrefs.GetString("RoomCode", "");
        
        Debug.LogWarning($"[FightSystemMultiplayer] Initializing game for player {myPlayerId} in room {roomCode}");
        await multiplayerService.InitGame();
        
        bool decksReady = await WaitForDecksReadyAsync();
        if (!decksReady)
        {
            Debug.LogError("[FightSystemMultiplayer] Timed out waiting for both decks to load");
            multiplayerUI?.ShowStatus("Failed to sync decks!");
            return;
        }

        Debug.LogWarning("[FightSystemMultiplayer] InitGame completed and decks are ready, loading cards...");
        
        // Use retry logic for card loading
        bool cardsLoaded = await LoadPlayerCardsWithRetry(myPlayerId, roomCode);
        if (!cardsLoaded)
        {
            Debug.LogError("[FightSystemMultiplayer] Failed to load cards after all retries");
            multiplayerUI?.ShowStatus("Failed to load cards!");
            return;
        }
        
        Debug.LogWarning("[FightSystemMultiplayer] Cards loaded successfully! Setting status to 'Choose fighter!'");
        multiplayerUI?.ShowStatus(MultiplayerUI.MSG_CHOOSE_FIGHTER);
    }

    private async Task<bool> WaitForDecksReadyAsync()
    {
        for (int attempt = 1; attempt <= MatchReadyMaxAttempts; attempt++)
        {
            multiplayerUI?.ShowStatus("Syncing decks...");

            MatchStateDto matchState = await multiplayerService.GetMatchStateAsync(roomCode, myPlayerId);
            if (matchState != null)
            {
                bool allDecksLoaded = matchState.decks != null && matchState.decks.allLoaded;
                Debug.Log($"[FightSystemMultiplayer] MatchState poll {attempt}/{MatchReadyMaxAttempts}: phase={matchState.phase}, loaded={matchState.decks?.loadedCount ?? 0}, allLoaded={allDecksLoaded}");

                if (allDecksLoaded)
                {
                    return true;
                }

                if (string.Equals(matchState.phase, MatchPhaseCompleted, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning("[FightSystemMultiplayer] Room completed while waiting for decks");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"[FightSystemMultiplayer] MatchState poll {attempt}/{MatchReadyMaxAttempts} returned null");
            }

            if (attempt < MatchReadyMaxAttempts)
            {
                await Task.Delay(MatchReadyPollDelayMs);
            }
        }

        return false;
    }

    private async System.Threading.Tasks.Task<bool> LoadPlayerCardsWithRetry(string myPlayerId, string roomCode)
    {
        const int MAX_RETRIES = 3;
        const int RETRY_DELAY_MS = 3000;
        
        for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
        {
            Debug.Log($"[FightSystemMultiplayer] Loading cards attempt {attempt}/{MAX_RETRIES}");
            
            // Reset cards state before each attempt to prevent conflicts
            multiplayerHandManager.ResetCardsState();
            
            // Start card loading
            multiplayerHandManager.CreateCardsFromDecks(myPlayerId, roomCode);
            
            // Wait up to 10 seconds for cards to load
            for (int wait = 0; wait < 100; wait++) // 100 * 100ms = 10 seconds
            {
                await System.Threading.Tasks.Task.Delay(100);
                
                if (player != null && player.hand != null && player.hand.Count > 0)
                {
                    Debug.Log($"[FightSystemMultiplayer] Cards loaded successfully! Player has {player.hand.Count} cards");
                    return true;
                }
            }
            
            Debug.LogWarning($"[FightSystemMultiplayer] Cards loading attempt {attempt} failed, retrying...");
            
            if (attempt < MAX_RETRIES)
            {
                await System.Threading.Tasks.Task.Delay(RETRY_DELAY_MS);
            }
        }
        
        return false;
    }

    public void OnCardDropped(Kard card, MultiplayerCardDrag dragHandler)
    {
        if (card == null)
        {
            Debug.LogWarning("[FightSystemMultiplayer] OnCardDropped called with null card");
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        if (multiplayerBoardManager != null && multiplayerBoardManager.IsProcessingSelection)
        {
            Debug.LogWarning("[FightSystemMultiplayer] Already submitting a card selection");
            dragHandler?.ResetToOriginalPosition();
            return;
        }

        if (multiplayerBoardManager != null)
        {
            _ = multiplayerBoardManager.HandleCardSelectedAsync(card, dragHandler);
        }
        else
        {
            Debug.LogError("[FightSystemMultiplayer] MultiplayerBoardManager missing when card dropped.");
            dragHandler?.ResetToOriginalPosition();
        }
        
        // Nacitaj nazvy utokov hned, count-y az po serverom potvrdenom setSelectedCard.
        LoadAttackNames(card);
    }

    /// <summary>
    /// Nacita a zobrazi nazvy utokov pre vybranu kartu
    /// </summary>
    /// <param name="card">Vybrana karta</param>
    public void LoadAttackNames(Kard card)
    {
        if (attackNamesLoader != null)
        {
            attackNamesLoader.LoadAttackNames(card);
        }
        else
        {
            Debug.LogWarning("[FightSystemMultiplayer] AttackNamesLoader not assigned");
        }
    }

    /// <summary>
    /// Nacita a zobrazi pocty utokov pre vybranu kartu zo servera
    /// </summary>
    /// <param name="card">Vybrana karta</param>
    public void LoadAttackCounts(Kard card)
    {
        if (attackCountLoader != null)
        {
            attackCountLoader.LoadAttackCounts(card, result =>
            {
                if (result != null)
                {
                    Debug.Log($"[FightSystemMultiplayer] Attack counts loaded successfully");
                    
                    // Po nacitani attack counts priprav UI pre vyber utoku
                    if (attackSelectionManager != null)
                    {
                        attackSelectionManager.PrepareAttackSelection(card, result);
                    }
                }
                else
                {
                    Debug.LogWarning($"[FightSystemMultiplayer] Failed to load attack counts");
                }
            });
        }
        else
        {
            Debug.LogWarning("[FightSystemMultiplayer] AttackCountLoader not assigned");
        }
    }

    /// <summary>
    /// Callback volany po potvrdeni vyberu utoku
    /// </summary>
    /// <param name="attackData">Data o vybratom utoku</param>
    public void OnAttackConfirmed(SelectedAttackData attackData)
    {
        Debug.Log($"[FightSystemMultiplayer] Attack confirmed: Type={attackData.attackType}, ID={attackData.attackId}");
        
        if (battleSubmitter == null)
        {
            Debug.LogError("[FightSystemMultiplayer] BattleSubmitter not assigned!");
            return;
        }
        
        // Ziskaj cardId aktualnej karty
        Kard myCard = player?.cardInGame;
        if (myCard == null)
        {
            Debug.LogError("[FightSystemMultiplayer] No card in game!");
            return;
        }
        
        // Deleguj na BattleSubmitter - posli IDs + slot pre attack count decrement
        battleSubmitter.SubmitAttack(roomCode, myPlayerId, myCard.cardId, attackData.attackId, attackData.attackType);
        
        // Zobraz status
        multiplayerUI?.ShowStatus(MultiplayerUI.MSG_WAITING_OPPONENT);
    }

    public void UpdatePendingOngoingAction(SelectedCardData selectedCardData)
    {
        if (selectedCardData?.ongoingActions == null || selectedCardData.ongoingActions.Length == 0)
        {
            pendingOngoingActionTurn = null;
            return;
        }

        var action = selectedCardData.ongoingActions[0];
        pendingOngoingActionTurn = new PendingOngoingActionTurn
        {
            actionType = action.type,
            sourceAttackId = action.sourceAttackId,
            targetCardId = action.targetCardId,
            turnsRemaining = action.turnsRemaining
        };
    }

    public bool TryGetPendingOngoingActionTurn(out PendingOngoingActionTurn pendingTurn)
    {
        pendingTurn = pendingOngoingActionTurn;
        return pendingTurn != null;
    }

    public bool TryResolveAttackTypeForAttackId(Kard card, int attackId, out int attackType)
    {
        attackType = 0;
        if (card == null || attackId <= 0)
        {
            return false;
        }

        if (card.attack1 == attackId)
        {
            attackType = 1;
            return true;
        }

        if (card.attack2 == attackId)
        {
            attackType = 2;
            return true;
        }

        if (card.attack3 == attackId)
        {
            attackType = 3;
            return true;
        }

        if (card.attack4 == attackId)
        {
            attackType = 4;
            return true;
        }

        return false;
    }


}

