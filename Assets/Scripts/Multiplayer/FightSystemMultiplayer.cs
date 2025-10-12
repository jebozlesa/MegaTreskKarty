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
        multiplayerUI?.ShowStatus("Choose fighter!");
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
        LoadAttackNames(card);
    }

    /// <summary>
    /// Načíta a zobrazí názvy útokov pre vybranú kartu
    /// </summary>
    /// <param name="card">Vybraná karta</param>
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

}
