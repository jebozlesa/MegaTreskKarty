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
    private const string MatchPhaseCompleted = "completed";
    private const float MatchStateMonitorIntervalSeconds = 2f;

    // Vytvorenie kariet v ruke hraca v multiplayeri podla udajov z miestnosti

    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text enemyNameText;
    public TMP_Text statusText;

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
    private Coroutine matchStateMonitorCoroutine;
    private bool leaveRequested;
    private bool matchCompletionHandled;

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

        Debug.Log($"[MultiplayerService] InitGame start: playerId={myPlayerId}, roomCode={roomCode}, isWaitingForOpponent={isWaitingForOpponent}, platform={Application.platform}");

        Debug.Log("[MultiplayerService] Exit flow expects the scene Button.OnClick to call OnExitToLobby or OnExitToMainMenu directly");

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("[MultiplayerService] InitGame aborted: RoomCode PlayerPrefs value is empty");
            statusText.text = "No room found!";
            return;
        }

        // Nacitanie informacii o hracoch a balickov
        await LoadPlayersInfoAsync();
        Debug.Log("[MultiplayerService] LoadPlayersInfoAsync completed");

        // Spustenie heartbeat systemu
        StartHeartbeat();
        StartMatchStateMonitor();

        if (isWaitingForOpponent)
        {
            statusText.text = "Waiting for opponent...";
            pollingCoroutine = StartCoroutine(PollForOpponent());
        }

        // Zavolaj callback az ked je vsetko hotove
        Debug.Log("[MultiplayerService] InitGame completed");
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
                    MarkPlayersInfoLoadFailed("Failed to parse room info");
                }
            }
            else
            {
                Debug.LogError("No result from JoinOrCreateRoom");
                MarkPlayersInfoLoadFailed("No room info returned");
            }
            tcs.SetResult(true);
        });

        await tcs.Task;
    }

    void OnDestroy()
    {
        Debug.Log($"[MultiplayerService] OnDestroy: playerId={myPlayerId}, roomCode={roomCode}, leaveRequested={leaveRequested}, matchCompletionHandled={matchCompletionHandled}");

        // Zastavime vsetky systemy
        StopHeartbeat();
        StopMatchStateMonitor();
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
        }

        // Opustime miestnost pri zatvoreni (emergency cleanup)
        if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId) && !leaveRequested && !matchCompletionHandled)
        {
            // Pri emergency cleanup len opustime miestnost
            Debug.Log("[MultiplayerService] OnDestroy emergency LeaveRoom requested");
            serverFunctionsManager.LeaveRoom(myPlayerId);
        }
        else
        {
            Debug.Log("[MultiplayerService] OnDestroy emergency LeaveRoom skipped");
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

    void MarkPlayersInfoLoadFailed(string reason)
    {
        Debug.LogError($"[MultiplayerService] Player info load failed: {reason}");
        enemyNameText.text = "";
        statusText.text = "Failed to load room!";
    }

    IEnumerator PollForOpponent()
    {
        while (isWaitingForOpponent)
        {
            yield return new WaitForSeconds(2f); // Kontrola kazde 2 sekundy

            serverFunctionsManager.GetMatchState(roomCode, myPlayerId, result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    try
                    {
                        MatchStateDto matchState = BattleContractMapper.ParseMatchStateResult(result.FunctionResult);
                        if (matchState != null && matchState.seats != null)
                        {
                            ProcessPlayersInfo(matchState);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing match state (polling): " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError("No result from GetMatchState (polling)");
                }
            });
        }
    }

    void ProcessPlayersInfo(MatchStateDto matchState)
    {
        if (matchState == null || matchState.seats == null)
        {
            return;
        }

        string myUsername = PlayerPrefs.GetString("username", myPlayerId);
        string opponentUsername = "";

        foreach (var seat in matchState.seats)
        {
            if (seat == null || string.IsNullOrEmpty(seat.playerId))
            {
                continue;
            }

            if (seat.playerId == myPlayerId)
            {
                myUsername = string.IsNullOrEmpty(seat.username) ? myUsername : seat.username;
            }
            else
            {
                opponentUsername = seat.username;
            }
        }

        if (matchState.playersCount >= 2)
        {
            bool amIFirstPlayer = matchState.seats.Count > 0 && matchState.seats[0]?.playerId == myPlayerId;
            if (amIFirstPlayer)
            {
                playerNameText.text = myUsername;
                enemyNameText.text = opponentUsername;
            }
            else
            {
                enemyNameText.text = myUsername;
                playerNameText.text = opponentUsername;
            }

            isWaitingForOpponent = false;
            if (pollingCoroutine != null)
            {
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }
        }
        else
        {
            playerNameText.text = myUsername;
            enemyNameText.text = "";
            statusText.text = "Waiting for opponent...";
            isWaitingForOpponent = true;
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

    private void StartMatchStateMonitor()
    {
        if (matchStateMonitorCoroutine != null)
        {
            Debug.Log("[MultiplayerService] Restarting existing MatchState monitor");
            StopCoroutine(matchStateMonitorCoroutine);
        }

        matchStateMonitorCoroutine = StartCoroutine(MatchStateMonitorLoop());
        Debug.Log($"[MultiplayerService] MatchState monitor started: playerId={myPlayerId}, roomCode={roomCode}, interval={MatchStateMonitorIntervalSeconds}s");
    }

    private void StopMatchStateMonitor()
    {
        if (matchStateMonitorCoroutine != null)
        {
            StopCoroutine(matchStateMonitorCoroutine);
            matchStateMonitorCoroutine = null;
            Debug.Log("[MultiplayerService] MatchState monitor stopped");
        }
    }

    private IEnumerator MatchStateMonitorLoop()
    {
        while (!matchCompletionHandled)
        {
            yield return new WaitForSeconds(MatchStateMonitorIntervalSeconds);

            if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(myPlayerId))
            {
                Debug.LogWarning($"[MultiplayerService] MatchState monitor skipped poll: roomCode={roomCode}, playerId={myPlayerId}");
                continue;
            }

            Debug.Log($"[MultiplayerService] MatchState monitor poll request: roomCode={roomCode}, playerId={myPlayerId}");
            var matchStateTask = GetMatchStateAsync(roomCode, myPlayerId);
            yield return new WaitUntil(() => matchStateTask.IsCompleted);

            if (matchStateTask.Status != TaskStatus.RanToCompletion || matchStateTask.Result == null)
            {
                Debug.LogWarning($"[MultiplayerService] MatchState monitor poll returned no state: taskStatus={matchStateTask.Status}");
                continue;
            }

            MatchStateDto matchState = matchStateTask.Result;
            Debug.Log($"[MultiplayerService] MatchState monitor poll result: roomCode={matchState.roomCode}, status={matchState.status}, phase={matchState.phase}, playersCount={matchState.playersCount}, reason={matchState.matchResult?.reason}, winner={matchState.matchResult?.winnerPlayerId}, loser={matchState.matchResult?.loserPlayerId}");

            if (string.Equals(matchState.phase, MatchPhaseCompleted, System.StringComparison.OrdinalIgnoreCase))
            {
                HandleRemoteMatchCompletion(matchState);
                yield break;
            }
        }
    }

    private void HandleRemoteMatchCompletion(MatchStateDto matchState)
    {
        if (matchCompletionHandled)
        {
            Debug.Log("[MultiplayerService] HandleRemoteMatchCompletion skipped: already handled");
            return;
        }

        Debug.Log($"[MultiplayerService] HandleRemoteMatchCompletion start: phase={matchState.phase}, reason={matchState.matchResult?.reason}, winner={matchState.matchResult?.winnerPlayerId}, loser={matchState.matchResult?.loserPlayerId}");
        matchCompletionHandled = true;
        StopHeartbeat();
        if (pollingCoroutine != null)
        {
            StopCoroutine(pollingCoroutine);
            pollingCoroutine = null;
        }

        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        Debug.Log("[MultiplayerService] Cleared local room PlayerPrefs after confirmed match completion");

        string winnerPlayerId = matchState.matchResult?.winnerPlayerId;
        string loserPlayerId = matchState.matchResult?.loserPlayerId;
        string reason = matchState.matchResult?.reason ?? matchState.completionReason;
        bool iWon = !string.IsNullOrEmpty(winnerPlayerId) && winnerPlayerId == myPlayerId;
        bool iLost = !string.IsNullOrEmpty(loserPlayerId) && loserPlayerId == myPlayerId;

        if (fightSystem != null)
        {
            if (iWon)
            {
                fightSystem.state = FightStateMultiplayer.WON;
            }
            else if (iLost)
            {
                fightSystem.state = FightStateMultiplayer.LOST;
            }
        }

        string message = BuildMatchCompletedMessage(reason, iWon, iLost);
        statusText.text = message;
        fightSystem?.multiplayerUI?.ShowStatus(message);
        Debug.Log($"[MultiplayerService] Match completed. reason={reason}, winner={winnerPlayerId}, loser={loserPlayerId}");
    }

    private static string BuildMatchCompletedMessage(string reason, bool iWon, bool iLost)
    {
        if (reason == "opponent_left")
        {
            return iWon ? "Opponent left. You win!" : "You left the match.";
        }

        if (reason == "timeout")
        {
            return iWon ? "Opponent timed out. You win!" : "Connection timed out. You lose.";
        }

        if (reason == "surrender")
        {
            return iWon ? "Opponent surrendered. You win!" : "You surrendered.";
        }

        if (reason == "cancelled")
        {
            return "Matchmaking cancelled.";
        }

        if (iWon)
        {
            return "You win!";
        }

        if (iLost)
        {
            return "You lose.";
        }

        return "Match completed.";
    }

    IEnumerator HeartbeatLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(heartbeatInterval);

            // Posli heartbeat na server
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                Debug.Log($"[MultiplayerService] Heartbeat request: playerId={myPlayerId}, roomCode={roomCode}");
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    // === CLEANUP PRI OPUSTENI ===

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[MultiplayerService] OnApplicationPause: pauseStatus={pauseStatus}, playerId={myPlayerId}, roomCode={roomCode}");
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
        Debug.Log($"[MultiplayerService] OnApplicationFocus: hasFocus={hasFocus}, playerId={myPlayerId}, roomCode={roomCode}");
        if (!hasFocus)
        {
            // Aplikacia straca focus - posli heartbeat
            if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId))
            {
                serverFunctionsManager.Heartbeat(myPlayerId);
            }
        }
    }

    public void LeaveMultiplayerRoom(System.Action<bool> onCompleted = null)
    {
        Debug.Log($"[MultiplayerService] LeaveMultiplayerRoom requested: playerId={myPlayerId}, roomCode={roomCode}, leaveRequested={leaveRequested}, matchCompletionHandled={matchCompletionHandled}");

        // Manualne opustenie miestnosti
        if (serverFunctionsManager == null || string.IsNullOrEmpty(myPlayerId))
        {
            Debug.LogError("[MultiplayerService] Cannot leave room: serverFunctionsManager or playerId missing");
            onCompleted?.Invoke(false);
            return;
        }

        leaveRequested = true;
        statusText.text = "Leaving room...";
        Debug.Log("[MultiplayerService] LeaveRoom server call starting");

        serverFunctionsManager.LeaveRoom(myPlayerId, result =>
        {
            Debug.Log($"[MultiplayerService] LeaveRoom callback received: resultNull={result == null}, functionResultNull={result?.FunctionResult == null}");
            if (result == null || result.FunctionResult == null)
            {
                leaveRequested = false;
                Debug.LogError("[MultiplayerService] LeaveRoom failed; keeping local room state");
                statusText.text = "Leave failed";
                onCompleted?.Invoke(false);
                return;
            }

            bool leaveSucceeded = false;
            try
            {
                Debug.Log($"[MultiplayerService] LeaveRoom raw FunctionResult: {result.FunctionResult}");
                var functionResult = JObject.Parse(result.FunctionResult.ToString());
                leaveSucceeded = functionResult["success"]?.Value<bool>() == true;
                if (!leaveSucceeded)
                {
                    Debug.LogError($"[MultiplayerService] LeaveRoom rejected: {functionResult["message"] ?? functionResult["error"]}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MultiplayerService] Failed to parse LeaveRoom result: {e.Message}");
            }

            if (!leaveSucceeded)
            {
                leaveRequested = false;
                statusText.text = "Leave failed";
                onCompleted?.Invoke(false);
                return;
            }

            Debug.Log($"[MultiplayerService] LeaveRoom succeeded: {result.FunctionResult}");

            PlayerPrefs.DeleteKey("RoomCode");
            PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            Debug.Log("[MultiplayerService] Cleared local room PlayerPrefs after successful LeaveRoom");

            StopHeartbeat();
            StopMatchStateMonitor();
            if (pollingCoroutine != null)
            {
                StopCoroutine(pollingCoroutine);
                pollingCoroutine = null;
            }

            onCompleted?.Invoke(true);
        });
    }

    // === EXIT FUNKCIONALITA ===

    public void OnExitToLobby()
    {
        // Volaj tuto metodu z UI tlacidla "Exit" 
        Debug.Log($"[MultiplayerService] OnExitToLobby pressed: playerId={myPlayerId}, roomCode={roomCode}");

        LeaveMultiplayerRoom(success =>
        {
            Debug.Log($"[MultiplayerService] OnExitToLobby leave completed: success={success}");
            if (success)
            {
                Debug.Log("[MultiplayerService] Loading MultiplayerLobby scene after successful leave");
                SceneManager.LoadScene("MultiplayerLobby");
            }
        });
    }

    public void OnExitToMainMenu()
    {
        // Alternativna metoda pre exit do hlavneho menu
        Debug.Log($"[MultiplayerService] OnExitToMainMenu pressed: playerId={myPlayerId}, roomCode={roomCode}");

        LeaveMultiplayerRoom(success =>
        {
            Debug.Log($"[MultiplayerService] OnExitToMainMenu leave completed: success={success}");
            if (success)
            {
                Debug.Log("[MultiplayerService] Loading MainMenu scene after successful leave");
                SceneManager.LoadScene("MainMenu");
            }
        });
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

    public async Task<Dictionary<string, SelectedCardData>> GetSelectedCardsFromMatchStateAsync(string roomCode, string playerId)
    {
        var tcs = new TaskCompletionSource<Dictionary<string, SelectedCardData>>();

        if (serverFunctionsManager == null)
        {
            Debug.LogError("GetSelectedCardsFromMatchStateAsync: serverFunctionsManager is null");
            tcs.TrySetResult(new Dictionary<string, SelectedCardData>());
            return await tcs.Task;
        }

        serverFunctionsManager.GetMatchState(roomCode, playerId, result =>
        {
            if (result?.FunctionResult != null)
            {
                try
                {
                    var matchState = BattleContractMapper.ParseMatchStateResult(result.FunctionResult);
                    tcs.TrySetResult(matchState?.selectedCards ?? new Dictionary<string, SelectedCardData>());
                    return;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"GetSelectedCardsFromMatchStateAsync: Error parsing response - {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("GetSelectedCardsFromMatchStateAsync: No result returned from server");
            }

            tcs.TrySetResult(new Dictionary<string, SelectedCardData>());
        });

        return await tcs.Task;
    }

    public async Task<MatchStateDto> GetMatchStateAsync(string roomCode, string playerId)
    {
        var tcs = new TaskCompletionSource<MatchStateDto>();

        if (serverFunctionsManager == null)
        {
            Debug.LogError("GetMatchStateAsync: serverFunctionsManager is null");
            tcs.TrySetResult(null);
            return await tcs.Task;
        }

        serverFunctionsManager.GetMatchState(roomCode, playerId, result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning("GetMatchStateAsync: No result returned from server");
                tcs.TrySetResult(null);
                return;
            }

            try
            {
                var matchState = BattleContractMapper.ParseMatchStateResult(result.FunctionResult);
                if (matchState == null)
                {
                    Debug.LogWarning($"GetMatchStateAsync: Failed to parse match state for room {roomCode}");
                }

                tcs.TrySetResult(matchState);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GetMatchStateAsync: Error parsing response - {ex.Message}");
                tcs.TrySetResult(null);
            }
        });

        return await tcs.Task;
    }

    // Legacy note: selected-card clearing is no longer a client flow-control primitive.
}
