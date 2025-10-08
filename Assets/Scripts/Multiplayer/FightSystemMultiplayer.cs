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
    // Multiplayer player references
    public Player player;
    public Player enemy;

    // Boards
    public GameObject playerBoard;
    public GameObject enemyBoard;

    // ...existing code...

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

    // ...existing code...

    public Attack attack;
    public AttackDescriptions attackDescriptions;

    public Effects effects;

    int playerAttack;
    int enemyAttack;

    public RecordHandler recordHandler;
    int enemyLevel = 0;

    private string connectionString;

    public static bool IsLoggedIn = false;

    private bool campaign = false;
    private int plyerCardsUsage = 0;
    int missionID = 0;

    // Multiplayer-specific variables
    public string myPlayerId;
    public string roomCode;
    public bool isWaitingForOpponent;
    public ServerFunctionsManager serverFunctionsManager;
    public List<Player> roomPlayers;
    public List<string> playerDecks;

    // Nové referencie na služby
    public MultiplayerService multiplayerService;
    public MultiplayerUI multiplayerUI;
    public MultiplayerHandManager multiplayerHandManager;
    public MultiplayerBoardManager multiplayerBoardManager;


    void Start()
    {
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
        myPlayerId = PlayFabManagerLogin.Instance.LoggedInPlayerId;
        roomCode = PlayerPrefs.GetString("RoomCode", "");
        await multiplayerService.InitGame();
        multiplayerHandManager.CreateCardsFromDecks(myPlayerId, roomCode);
        multiplayerUI?.ShowStatus("Vyber si bojovníka");
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
    }

    // private async Task HandleCardSelectedAsync(Kard card, MultiplayerCardDrag dragHandler)
    // {
    //     isSubmittingSelection = true;
    //     localSelectedCard = card;
    //     localSelectedCardDrag = dragHandler;
    //     opponentCardRevealed = false;

    //     try
    //     {
    //         if (player == null)
    //         {
    //             Debug.LogError("[FightSystemMultiplayer] Player reference is missing when selecting a card");
    //             dragHandler?.ResetToOriginalPosition();
    //             return;
    //         }

    //         player.PlayCard(card, playerBoard);
    //         player.cardInGame.isDragable = false;
    //         playerLifeBar?.SetBar(player.cardInGame);
    //         state = FightStateMultiplayer.START;

    //         var definition = multiplayerHandManager.GetCardDefinition(card.cardId);
    //         localSelectedCardData = SelectedCardData.FromCard(card, definition, myPlayerId);

    //         if (localSelectedCardData == null)
    //         {
    //             Debug.LogError("[FightSystemMultiplayer] Failed to create payload for selected card");
    //             dragHandler?.ResetToOriginalPosition();
    //             ResetLocalSelectionState(false);
    //             return;
    //         }

    //         await multiplayerService.SubmitSelectedCardAsync(roomCode, myPlayerId, localSelectedCardData);
    //         multiplayerUI?.ShowStatus("Čaká sa na súpera...");

    //         await WaitForOpponentSelectionAsync();

    //         if (opponentSelectedCardData != null)
    //         {
    //             await RevealCardsAsync();
    //         }
    //         else
    //         {
    //             Debug.LogWarning("[FightSystemMultiplayer] Opponent selection timed out");
    //             multiplayerUI?.ShowStatus("Súper nevybral kartu včas");
    //             ResetLocalSelectionState(false);
    //         }
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"[FightSystemMultiplayer] Error while handling card selection: {ex.Message}");
    //         dragHandler?.ResetToOriginalPosition();
    //     }
    //     finally
    //     {
    //         isSubmittingSelection = false;
    //     }
    // }

    // private async Task WaitForOpponentSelectionAsync()
    // {
    //     opponentSelectedCardData = null;

    //     opponentSelectionCancellation?.Cancel();
    //     opponentSelectionCancellation = new CancellationTokenSource();
    //     var token = opponentSelectionCancellation.Token;

    //     for (int attempt = 0; attempt < selectedCardPollAttempts; attempt++)
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             opponentSelectionCancellation?.Dispose();
    //             opponentSelectionCancellation = null;
    //             return;
    //         }

    //         var selectedCards = await multiplayerService.GetSelectedCardsAsync(roomCode);
    //         if (selectedCards != null)
    //         {
    //             if (selectedCards.TryGetValue(myPlayerId, out var mine) && mine != null)
    //             {
    //                 localSelectedCardData = mine;
    //             }

    //             var opponentId = GetOpponentPlayerId(selectedCards);
    //             if (!string.IsNullOrEmpty(opponentId) && selectedCards.TryGetValue(opponentId, out var opponentCard) && opponentCard != null)
    //             {
    //                 opponentSelectedCardData = opponentCard;
    //                 Debug.Log($"[FightSystemMultiplayer] Opponent selected card {opponentCard.cardId}");
    //                 opponentSelectionCancellation?.Cancel();
    //                 opponentSelectionCancellation?.Dispose();
    //                 opponentSelectionCancellation = null;
    //                 return;
    //             }
    //         }

    //         await Task.Delay(System.TimeSpan.FromSeconds(selectedCardPollIntervalSeconds));
    //     }

    //     opponentSelectionCancellation?.Dispose();
    //     opponentSelectionCancellation = null;
    // }

    // private string GetOpponentPlayerId(Dictionary<string, SelectedCardData> selectedCards)
    // {
    //     foreach (var entry in selectedCards)
    //     {
    //         if (entry.Key != myPlayerId)
    //         {
    //             return entry.Key;
    //         }
    //     }

    //     return string.Empty;
    // }

    // private async Task RevealCardsAsync()
    // {
    //     if (opponentCardRevealed)
    //     {
    //         return;
    //     }

    //     if (localSelectedCardData == null || opponentSelectedCardData == null)
    //     {
    //         Debug.LogWarning("[FightSystemMultiplayer] RevealCardsAsync called without both cards present");
    //         return;
    //     }

    //     Debug.Log($"[FightSystemMultiplayer] Revealing opponent card: mine={localSelectedCardData.cardId}, opponent={opponentSelectedCardData.cardId}");

    //     if (enemy == null || enemyBoard == null)
    //     {
    //         Debug.LogError("[FightSystemMultiplayer] Enemy references missing, cannot reveal opponent card");
    //         return;
    //     }

    //     if (enemy.cardInGame != null)
    //     {
    //         if (enemy.cardInGame.gameObject != null)
    //         {
    //             Destroy(enemy.cardInGame.gameObject);
    //         }
    //         enemy.cardInGame = null;
    //     }

    //     enemy.hand.Clear();

    //     var opponentGeneratedCard = opponentSelectedCardData.ToGeneratedCard();
    //     var enemyCard = multiplayerHandManager.CreateCardInGame(opponentGeneratedCard, enemy.gameObject, enemy, true, false, enemyBoard);
    //     if (enemyCard == null)
    //     {
    //         Debug.LogError("[FightSystemMultiplayer] Failed to instantiate opponent card");
    //         return;
    //     }

    //     enemyCard.isDragable = false;
    //     enemy.PlayCard(enemyCard, enemyBoard);
    //     enemyLifeBar?.SetBar(enemy.cardInGame);

    //     multiplayerUI?.ShowStatus("Obaja hráči sú pripravení – zvoľ útok");
    //     opponentCardRevealed = true;

    //     await multiplayerService.ClearSelectedCardsAsync(roomCode);

    //     state = FightStateMultiplayer.TURN;
    // }

    // private void ResetLocalSelectionState(bool keepCardOnBoard)
    // {
    //     if (!keepCardOnBoard && localSelectedCard != null)
    //     {
    //         if (player != null)
    //         {
    //             Vector3 returnPosition = localSelectedCardDrag != null ? localSelectedCardDrag.GetOriginalLocalPosition() : Vector3.zero;
    //             player.ReturnCardToHand(localSelectedCard, returnPosition);
    //             localSelectedCard.isDragable = true;
    //         }
    //         else
    //         {
    //             var dragHandler = localSelectedCardDrag;
    //             dragHandler?.ResetToOriginalPosition();
    //         }
    //     }

    //     localSelectedCardDrag = null;
    //     if (!keepCardOnBoard)
    //     {
    //         localSelectedCard = null;
    //     }
    //     localSelectedCardData = null;
    //     opponentSelectedCardData = null;
    //     opponentCardRevealed = false;
    // }
}
