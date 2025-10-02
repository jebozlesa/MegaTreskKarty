using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using System.Collections;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

public class MultiplayerGameUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text enemyNameText;
    public TMP_Text statusText;
    public Button exitButton; // Nové tlačidlo Exit
    
    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    private string myPlayerId;
    private string roomCode;
    private bool isWaitingForOpponent;
    private Coroutine pollingCoroutine;
    private Coroutine heartbeatCoroutine;

    [Header("Heartbeat Settings")]
    [SerializeField] private float heartbeatInterval = 30f; // 30 sekúnd

    void Start()
    {
        myPlayerId = PlayFabManagerLogin.Instance.LoggedInPlayerId;
        roomCode = PlayerPrefs.GetString("RoomCode", "");
        isWaitingForOpponent = PlayerPrefs.GetString("IsWaitingForOpponent", "false") == "true";
        
        // Pripojenie exit tlačidla
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitToLobby);
        }
        
        if (string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "No room found!";
            return;
        }

        // Načítanie informácií o hráčoch
        LoadPlayersInfo();
        
        // Spustenie heartbeat systému
        StartHeartbeat();
        
        // Ak čakáme na súpera, spustíme polling
        if (isWaitingForOpponent)
        {
            statusText.text = "Waiting for opponent...";
            pollingCoroutine = StartCoroutine(PollForOpponent());
        }
    }

    void OnDestroy()
    {
        // Zastavíme všetky systémy
        StopHeartbeat();
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
        }
        
        // Opustime miestnosť pri zatvorení (emergency cleanup)
        if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
        {
            // Pri emergency cleanup len opustime miestnosť
            // MarkRoomAsCompleted sa zavolá len pri manuálnom exite
            serverFunctionsManager.LeaveRoom(myPlayerId, null);
        }
    }

    void LoadPlayersInfo()
    {
        // Použijeme joinOrCreateRoom ktoré funguje a vráti aktuálne informácie o miestnosti
        string username = PlayerPrefs.GetString("username", myPlayerId);
        serverFunctionsManager.JoinOrCreateRoom(myPlayerId, username, result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                try
                {
                    JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                    if (functionResult["room"] != null)
                    {
                        var room = functionResult["room"];
                        
                        // Spracovanie playersInfo ak existuje (nové API)
                        if (room["playersInfo"] != null)
                        {
                            JArray playersInfo = room["playersInfo"] as JArray;
                            ProcessPlayersInfo(playersInfo);
                        }
                        // Fallback na staré players pole
                        else if (room["players"] != null)
                        {
                            ProcessOldPlayersFormat(room);
                        }
                        else
                        {
                            SetFallbackPlayerNames();
                        }
                    }
                    else
                    {
                        Debug.LogError("No room data in response");
                        SetFallbackPlayerNames();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing room info: " + e.Message);
                    SetFallbackPlayerNames();
                }
            }
            else
            {
                Debug.LogError("No result from JoinOrCreateRoom");
                SetFallbackPlayerNames();
            }
        });
    }

    void ProcessOldPlayersFormat(JToken room)
    {
        // Spracovanie starého formátu pre spätná kompatibilitu
        var playersArray = room["players"] as JArray;
        if (playersArray != null && playersArray.Count >= 2)
        {
            string myUsername = PlayerPrefs.GetString("username", myPlayerId);
            string opponentId = "";
            
            // Nájdeme ID súpera
            foreach (var playerId in playersArray)
            {
                if (playerId.ToString() != myPlayerId)
                {
                    opponentId = playerId.ToString();
                    break;
                }
            }
            
            // Určíme kto je prvý hráč (domáci)
            bool amIFirstPlayer = playersArray[0].ToString() == myPlayerId;
            
            if (amIFirstPlayer)
            {
                // Som prvý hráč (domáci) - ľavá strana
                playerNameText.text = myUsername;
                enemyNameText.text = opponentId; // Dočasne ID, ideálne by malo byť meno
            }
            else
            {
                // Som druhý hráč (hosť) - pravá strana  
                playerNameText.text = playersArray[0].ToString(); // Domáci hráč (dočasne ID)
                enemyNameText.text = myUsername; // Ja som hosť
            }
            
            statusText.text = "Connected!";
            isWaitingForOpponent = false;
            
            // Zastavíme polling ak bežal
            if (pollingCoroutine != null)
            {
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }
        }
        else if (playersArray != null && playersArray.Count == 1)
        {
            string myUsername = PlayerPrefs.GetString("username", myPlayerId);
            playerNameText.text = myUsername;
            enemyNameText.text = "";
            statusText.text = "Waiting for opponent...";
            isWaitingForOpponent = true;
        }
    }

    void ProcessPlayersInfo(JArray playersInfo)
    {
        string myUsername = PlayerPrefs.GetString("username", myPlayerId);
        string opponentUsername = "";
        
        // Nájdeme informácie o hráčoch
        foreach (JObject playerInfo in playersInfo)
        {
            string playerId = playerInfo["playerId"].ToString();
            string username = playerInfo["username"].ToString();
            
            if (playerId == myPlayerId)
            {
                myUsername = username;
            }
            else
            {
                opponentUsername = username;
            }
        }
        
        // Nastavíme mená podľa poradia hráčov
        if (playersInfo.Count >= 2)
        {
            // Obaja hráči sú pripojení
            JObject firstPlayer = playersInfo[0] as JObject;
            bool amIFirstPlayer = firstPlayer["playerId"].ToString() == myPlayerId;
            
            if (amIFirstPlayer)
            {
                // Som prvý hráč (domáci)
                playerNameText.text = myUsername;
                enemyNameText.text = opponentUsername;
            }
            else
            {
                // Som druhý hráč (hosť)
                enemyNameText.text = myUsername;
                playerNameText.text = opponentUsername;
            }
            
            statusText.text = "Connected!";
            isWaitingForOpponent = false;
            
            // Zastavíme polling ak bežal
            if (pollingCoroutine != null)
            {
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }
        }
        else if (playersInfo.Count == 1)
        {
            // Len jeden hráč je pripojený (som to ja)
            playerNameText.text = myUsername;
            enemyNameText.text = "";
            statusText.text = "Waiting for opponent...";
            isWaitingForOpponent = true;
        }
    }

    void SetFallbackPlayerNames()
    {
        // Fallback na staré správanie - ak čakáme na súpera, zobrazíme len svoje meno
        string myUsername = PlayerPrefs.GetString("username", myPlayerId);
        playerNameText.text = myUsername;
        enemyNameText.text = "";
        statusText.text = "Loading...";
    }

    IEnumerator PollForOpponent()
    {
        while (isWaitingForOpponent)
        {
            yield return new WaitForSeconds(2f); // Kontrola každé 2 sekundy
            LoadPlayersInfo();
        }
    }

    // Verejné metódy pre manuálne nastavenie mien (pre spätnu kompatibilitu)
    public void ShowPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void ShowEnemyName(string enemyName)
    {
        enemyNameText.text = enemyName;
    }

    // === HEARTBEAT SYSTÉM ===
    
    void StartHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
        }
        heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
        Debug.Log("Heartbeat system started");
    }

    void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
            Debug.Log("Heartbeat system stopped");
        }
    }

    IEnumerator HeartbeatLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(heartbeatInterval);
            
            // Pošli heartbeat na server
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    // === CLEANUP PRI OPUSTENÍ ===
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Aplikácia sa pozastavuje - pošli heartbeat
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Aplikácia stráca focus - pošli heartbeat
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    public void LeaveMultiplayerRoom()
    {
        // Manuálne opustenie miestnosti
        if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
        {
            // Najprv označíme miestnosť ako completed aby sa do nej nikto nový nepripojil
            if (!string.IsNullOrEmpty(roomCode))
            {
                serverFunctionsManager.MarkRoomAsCompleted(roomCode, myPlayerId, result =>
                {
                    Debug.Log("Room marked as completed");
                });
            }
            
            // Potom opustime miestnosť
            serverFunctionsManager.LeaveRoom(myPlayerId, result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    Debug.Log("Successfully left room");
                }
                else
                {
                    Debug.LogWarning("Failed to leave room, but continuing anyway");
                }
            });
        }
        
        // Vyčisti lokálne údaje
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        
        // Zastav všetky systémy
        StopHeartbeat();
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }
    }

    // === EXIT FUNKCIONALITA ===
    
    public void OnExitToLobby()
    {
        // Volaj túto metódu z UI tlačidla "Exit" 
        Debug.Log("Player requested exit to lobby");
        
        // Opusti miestnosť a vráť sa do lobby
        LeaveMultiplayerRoom();
        
        // Počkaj chvíľu na dokončenie cleanup a potom nahraj lobby
        StartCoroutine(ExitToLobbyCoroutine());
    }

    public void OnExitToMainMenu()
    {
        // Alternatívna metóda pre exit do hlavného menu
        Debug.Log("Player requested exit to main menu");
        
        // Opusti miestnosť
        LeaveMultiplayerRoom();
        
        // Počkaj chvíľu na dokončenie cleanup a potom nahraj hlavné menu
        StartCoroutine(ExitToMainMenuCoroutine());
    }

    IEnumerator ExitToLobbyCoroutine()
    {
        statusText.text = "Leaving room...";
        
        // Počkaj chvíľu na dokončenie server komunikácie
        yield return new WaitForSeconds(0.5f);
        
        // Nahraj lobby scénu
        SceneManager.LoadScene("MultiplayerLobby"); // Zmeň na správny názov scény
    }

    IEnumerator ExitToMainMenuCoroutine()
    {
        statusText.text = "Leaving room...";
        
        // Počkaj chvíľu na dokončenie server komunikácie
        yield return new WaitForSeconds(0.5f);
        
        // Nahraj hlavné menu
        SceneManager.LoadScene("MainMenu"); // Zmeň na správny názov scény
    }
}
