using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using System.Collections;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MultiplayerService : MonoBehaviour
{
    // Vytvorenie kariet v ruke hraca v multiplayeri podla udajov z miestnosti

    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text enemyNameText;
    public TMP_Text statusText;
    public Button exitButton; // Nove tlacidlo Exit

    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    private string myPlayerId;
    private string roomCode;
    
    // [OK] Public getters pre BattleResultProcessor a ostatne komponenty
    public string RoomCode => roomCode;
    public string MyPlayerId => myPlayerId;
    
    private bool isWaitingForOpponent;
    private Coroutine pollingCoroutine;
    private Coroutine heartbeatCoroutine;

    [Header("Heartbeat Settings")]
    [SerializeField] private float heartbeatInterval = 30f; // 30 sekund

    public MultiplayerHandManager handManager;
    public FightSystemMultiplayer fightSystem;
    // private bool cardsCreated = false;

    public async Task InitGame(System.Action onGameInitialized = null)
    {
        myPlayerId = PlayFabManagerLogin.Instance.LoggedInPlayerId;
        roomCode = PlayerPrefs.GetString("RoomCode", "");
        isWaitingForOpponent = PlayerPrefs.GetString("IsWaitingForOpponent", "false") == "true";

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitToLobby);

        if (string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "No room found!";
            return;
        }

        // Nacitanie informacii o hracoch a balickov
        await LoadPlayersInfoAsync();

        // Spustenie heartbeat systemu
        StartHeartbeat();

        if (isWaitingForOpponent)
        {
            statusText.text = "Waiting for opponent...";
            pollingCoroutine = StartCoroutine(PollForOpponent());
        }

        // Zavolaj callback az ked je vsetko hotove
        onGameInitialized?.Invoke();
    }

    // Pridaj async verziu LoadPlayersInfo
    public async Task LoadPlayersInfoAsync()
    {
        string username = PlayerPrefs.GetString("username", myPlayerId);
        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

        serverFunctionsManager.JoinOrCreateRoom(myPlayerId, username, async result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                try
                {
                    JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                    if (functionResult["room"] != null)
                    {
                        var room = functionResult["room"];
                        if (room["playersInfo"] != null)
                        {
                            JArray playersInfo = room["playersInfo"] as JArray;
                            ProcessPlayersInfo(playersInfo);

                            await LoadPlayerDecks(myPlayerId, roomCode);
                        }
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
            tcs.SetResult(true);
        });

        await tcs.Task;
    }

    void OnDestroy()
    {
        // Zastavime vsetky systemy
        StopHeartbeat();
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
        }

        // Opustime miestnost pri zatvoreni (emergency cleanup)
        if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
        {
            // Pri emergency cleanup len opustime miestnost
            // MarkRoomAsCompleted sa zavola len pri manualnom exite
            serverFunctionsManager.LeaveRoom(myPlayerId, null);
        }
    }

    async Task LoadPlayerDecks(string playerId, string roomCode)
    {
        try
        {
            var result = await serverFunctionsManager.LoadPlayerDecksIntoRoomAsync(playerId, roomCode);
            if (result != null && result.FunctionResult != null)
            {
                Debug.Log("Player decks loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load player decks: " + result?.Error);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error loading player decks: " + ex.Message);
        }
    }

    void LoadPlayersInfo()
    {
        // Pouzijeme joinOrCreateRoom ktore funguje a vrati aktualne informacie o miestnosti
        string username = PlayerPrefs.GetString("username", myPlayerId);
        serverFunctionsManager.JoinOrCreateRoom(myPlayerId, username, async result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                try
                {
                    JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                    if (functionResult["room"] != null)
                    {
                        var room = functionResult["room"];

                        // Spracovanie playersInfo ak existuje (nove API)
                        if (room["playersInfo"] != null)
                        {
                            JArray playersInfo = room["playersInfo"] as JArray;
                            ProcessPlayersInfo(playersInfo);

                            // Nacitanie balickov hracov
                            await LoadPlayerDecks(myPlayerId, roomCode);

                            // // Vytvorenie kariet v ruke hraca podla udajov z miestnosti
                            // if (room["playerDecks"] != null && room["playerDecks"][myPlayerId] != null && room["playerDecks"][myPlayerId]["cards"] != null && !cardsCreated)
                            // {
                            //     JArray cardsArray = room["playerDecks"][myPlayerId]["cards"] as JArray;
                            //     CreateMultiplayerHandFromRoom(cardsArray);
                            //     cardsCreated = true;
                            // }
                        }
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
    // void CreateCards()
    // {
    //     //if (cardsCreated) return; // Uz sme vytvorili karty

    //     serverFunctionsManager.GetRoomPlayersInfo(roomCode, result =>
    //     {
    //         if (result != null && result.FunctionResult != null)
    //         {
    //             try
    //             {
    //                 JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
    //                 if (functionResult["room"] != null)
    //                 {
    //                     var room = functionResult["room"];

    //                     // Vytvorenie kariet v ruke hraca podla udajov z miestnosti
    //                     if (room["playerDecks"] != null && room["playerDecks"][myPlayerId] != null && room["playerDecks"][myPlayerId]["cards"] != null && !cardsCreated)
    //                     {
    //                         JArray cardsArray = room["playerDecks"][myPlayerId]["cards"] as JArray;
    //                         CreateMultiplayerHandFromRoom(cardsArray);
    //                         cardsCreated = true;
    //                     }
    //                 }
    //             }
    //             catch (System.Exception e)
    //             {
    //                 Debug.LogError("Error parsing room info for cards: " + e.Message);
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogError("No result from GetRoomPlayersInfo for cards");
    //         }
    //     });
    // }
    // void CreateMultiplayerHandFromRoom(JArray cardsArray)
    // {
    //     if (handManager == null || fightSystem == null || fightSystem.player == null)
    //     {
    //         Debug.LogError("HandManager alebo FightSystem/player nie je dostupny!");
    //         return;
    //     }
    //     Player player = fightSystem.player;
    //     GameObject playerGO = fightSystem.hrac;
    //     foreach (JObject cardData in cardsArray)
    //     {
    //         GeneratedCard card = new GeneratedCard
    //         {
    //             CardID = cardData["CardID"]?.ToString(),
    //             StyleID = cardData["StyleID"]?.ToObject<int>() ?? 0,
    //             PersonName = cardData["PersonName"]?.ToString(),
    //             Health = cardData["MaxHealth"]?.ToObject<int>() ?? cardData["Health"]?.ToObject<int>() ?? 0,
    //             Color = cardData["Color"]?.ToObject<List<int>>()?.ToArray() ?? new int[] { 255, 255, 255 },
    //             Level = cardData["Level"]?.ToObject<int>() ?? 1,
    //             CardPicture = cardData["CardPicture"]?.ToString(),
    //             Attack1 = cardData["Attack1"]?.ToObject<int>() ?? 0,
    //             Attack2 = cardData["Attack2"]?.ToObject<int>() ?? 0,
    //             Attack3 = cardData["Attack3"]?.ToObject<int>() ?? 0,
    //             Attack4 = cardData["Attack4"]?.ToObject<int>() ?? 0
    //             // ...dopln dalsie polia podla potreby
    //         };
    //         handManager.CreateCardInGame(card, playerGO, player);
    //     }
    // }

    void ProcessPlayersInfo(JArray playersInfo)
    {
        string myUsername = PlayerPrefs.GetString("username", myPlayerId);
        string opponentUsername = "";

        // Najdeme informacie o hracoch
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

        // Nastavime mena podla poradia hracov
        if (playersInfo.Count >= 2)
        {
            // Obaja hraci su pripojeni
            JObject firstPlayer = playersInfo[0] as JObject;
            bool amIFirstPlayer = firstPlayer["playerId"].ToString() == myPlayerId;

            if (amIFirstPlayer)
            {
                // Som prvy hrac (domaci)
                playerNameText.text = myUsername;
                enemyNameText.text = opponentUsername;
            }
            else
            {
                // Som druhy hrac (host)
                enemyNameText.text = myUsername;
                playerNameText.text = opponentUsername;
            }

            // [ERR] Removed "Connected!" message - hraci idu priamo na "Choose fighter!"
            // statusText.text = "Connected!";
            isWaitingForOpponent = false;

            // Zastavime polling ak bezal
            if (pollingCoroutine != null)
            {
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }
        }
        else if (playersInfo.Count == 1)
        {
            // Len jeden hrac je pripojeny (som to ja)
            playerNameText.text = myUsername;
            enemyNameText.text = "";
            statusText.text = "Waiting for opponent...";
            isWaitingForOpponent = true;
        }
    }

    void SetFallbackPlayerNames()
    {
        // Fallback na stare spravanie - ak cakame na supera, zobrazime len svoje meno
        string myUsername = PlayerPrefs.GetString("username", myPlayerId);
        playerNameText.text = myUsername;
        enemyNameText.text = "";
        statusText.text = "Loading...";
    }

    IEnumerator PollForOpponent()
    {
        while (isWaitingForOpponent)
        {
            yield return new WaitForSeconds(2f); // Kontrola kazde 2 sekundy

            // POUZI GetRoomPlayersInfo namiesto JoinOrCreateRoom!
            serverFunctionsManager.GetRoomPlayersInfo(roomCode, result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    try
                    {
                        JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                        if (functionResult["room"] != null)
                        {
                            var room = functionResult["room"];
                            if (room["playersInfo"] != null)
                            {
                                JArray playersInfo = room["playersInfo"] as JArray;
                                ProcessPlayersInfo(playersInfo);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing room info (polling): " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError("No result from GetRoomPlayersInfo (polling)");
                }
            });
        }
    }

    // Verejne metody pre manualne nastavenie mien (pre spatnu kompatibilitu)
    public void ShowPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void ShowEnemyName(string enemyName)
    {
        enemyNameText.text = enemyName;
    }

    // === HEARTBEAT SYSTEM ===

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

            // Posli heartbeat na server
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    // === CLEANUP PRI OPUSTENI ===

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Aplikacia sa pozastavuje - posli heartbeat
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
            // Aplikacia straca focus - posli heartbeat
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    public void LeaveMultiplayerRoom()
    {
        // Manualne opustenie miestnosti
        if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
        {
            // Najprv oznacime miestnost ako completed aby sa do nej nikto novy nepripojil
            if (!string.IsNullOrEmpty(roomCode))
            {
                serverFunctionsManager.MarkRoomAsCompleted(roomCode, myPlayerId, result =>
                {
                    Debug.Log("Room marked as completed");
                });
            }

            // Potom opustime miestnost
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

        // Vycisti lokalne udaje
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");

        // Zastav vsetky systemy
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
        // Volaj tuto metodu z UI tlacidla "Exit" 
        Debug.Log("Player requested exit to lobby");

        // Opusti miestnost a vrat sa do lobby
        LeaveMultiplayerRoom();

        // Pockaj chvilu na dokoncenie cleanup a potom nahraj lobby
        StartCoroutine(ExitToLobbyCoroutine());
    }

    public void OnExitToMainMenu()
    {
        // Alternativna metoda pre exit do hlavneho menu
        Debug.Log("Player requested exit to main menu");

        // Opusti miestnost
        LeaveMultiplayerRoom();

        // Pockaj chvilu na dokoncenie cleanup a potom nahraj hlavne menu
        StartCoroutine(ExitToMainMenuCoroutine());
    }

    IEnumerator ExitToLobbyCoroutine()
    {
        statusText.text = "Leaving room...";

        // Pockaj chvilu na dokoncenie server komunikacie
        yield return new WaitForSeconds(0.5f);

        // Nahraj lobby scenu
        SceneManager.LoadScene("MultiplayerLobby"); // Zmen na spravny nazov sceny
    }

    IEnumerator ExitToMainMenuCoroutine()
    {
        statusText.text = "Leaving room...";

        // Pockaj chvilu na dokoncenie server komunikacie
        yield return new WaitForSeconds(0.5f);

        // Nahraj hlavne menu
        SceneManager.LoadScene("MainMenu"); // Zmen na spravny nazov sceny
    }

    public async Task SubmitSelectedCardAsync(string roomCode, string playerId, SelectedCardData cardData)
    {
        if (serverFunctionsManager == null)
        {
            Debug.LogError("SubmitSelectedCardAsync: serverFunctionsManager is null");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        bool isCompleted = false;

        serverFunctionsManager.SetSelectedCard(roomCode, playerId, cardData, result =>
        {
            if (isCompleted) return; // Ignore late responses
            isCompleted = true;
            
            if (result == null)
            {
                Debug.LogError("SubmitSelectedCardAsync: Failed to set selected card on server");
            }
            else
            {
                Debug.Log($"SubmitSelectedCardAsync: Card {cardData?.cardId} stored for player {playerId}");
            }

            tcs.TrySetResult(true);
        });

        // Add 15-second timeout
        _ = Task.Delay(15000).ContinueWith(_ => {
            if (!isCompleted)
            {
                isCompleted = true;
                Debug.LogWarning($"SubmitSelectedCardAsync: Timeout for player {playerId}, card {cardData?.cardId}");
                tcs.TrySetResult(false);
            }
        });

        await tcs.Task;
    }

    public async Task<Dictionary<string, SelectedCardData>> GetSelectedCardsAsync(string roomCode)
    {
        var tcs = new TaskCompletionSource<Dictionary<string, SelectedCardData>>();

        if (serverFunctionsManager == null)
        {
            Debug.LogError("GetSelectedCardsAsync: serverFunctionsManager is null");
            tcs.TrySetResult(new Dictionary<string, SelectedCardData>());
            return await tcs.Task;
        }

        Debug.Log($"[GetSelectedCardsAsync] Calling server for roomCode: {roomCode}");
        
        serverFunctionsManager.GetSelectedCards(roomCode, result =>
        {
            var map = new Dictionary<string, SelectedCardData>();

            if (result != null && result.FunctionResult != null)
            {
                Debug.Log($"[GetSelectedCardsAsync] Raw server response: {result.FunctionResult}");
                
                try
                {
                    JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                    if (functionResult["selectedCards"] is JObject selectedCards)
                    {
                        Debug.Log($"[GetSelectedCardsAsync] Found selectedCards with {selectedCards.Count} players");
                        
                        foreach (var property in selectedCards)
                        {
                            Debug.Log($"[GetSelectedCardsAsync] Processing player: {property.Key}");
                            if (property.Value is JObject cardObject)
                            {
                                var cardData = SelectedCardData.FromJson(property.Key, cardObject);
                                if (cardData != null)
                                {
                                    map[property.Key] = cardData;
                                    Debug.Log($"[GetSelectedCardsAsync] Added card {cardData.cardId} for player {property.Key}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[GetSelectedCardsAsync] No selectedCards field in response");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"GetSelectedCardsAsync: Error parsing response - {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("GetSelectedCardsAsync: No result returned from server");
            }

            Debug.Log($"[GetSelectedCardsAsync] Final result: {map.Count} cards loaded");
            tcs.TrySetResult(map);
        });

        return await tcs.Task;
    }

    public async Task ClearSelectedCardsAsync(string roomCode)
    {
        if (serverFunctionsManager == null)
        {
            Debug.LogError("ClearSelectedCardsAsync: serverFunctionsManager is null");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();

        serverFunctionsManager.ClearSelectedCards(roomCode, _ =>
        {
            tcs.TrySetResult(true);
        });

        await tcs.Task;
    }
}
