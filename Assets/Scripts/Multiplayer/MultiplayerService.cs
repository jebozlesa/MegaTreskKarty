using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private Coroutine heartbeatCoroutine;
    private Coroutine matchStateMonitorCoroutine;
    private bool leaveRequested;
    private bool matchCompletionHandled;
    private bool leaveConfirmed;
    private int lifecycleVersion;
    public const string PendingStartupCancellationKey = "PendingStartupCancellationRoomCode";
    private bool localPrepared;
    private bool serverStarted;
    private bool startupAborted;
    private bool applicationPaused;
    private bool applicationFocused = true;
    private double startupDeadline;
    public bool CanInteractWithBattle => CanContinueStartup && localPrepared && serverStarted;
    public bool CanAcknowledgeStartup => CanContinueStartup && localPrepared && !applicationPaused && applicationFocused
        && StartupRemainingMilliseconds > 0;
    public int StartupRemainingMilliseconds => Mathf.Max(0,
        (int)((startupDeadline - Time.realtimeSinceStartupAsDouble) * 1000));
    public bool CanContinueStartup => this != null && isActiveAndEnabled && !leaveRequested
        && !leaveConfirmed && !matchCompletionHandled && !startupAborted;

    [Header("Heartbeat Settings")]
    [SerializeField]
    private float heartbeatInterval = 30f; // 30 sekund

    public MultiplayerHandManager handManager;
    public FightSystemMultiplayer fightSystem;

    public async Task InitGame(System.Action onGameInitialized = null)
    {
        myPlayerId = PlayFabManagerLogin.Instance?.LoggedInPlayerId;
        roomCode = PlayerPrefs.GetString("RoomCode", "");
        startupDeadline = Time.realtimeSinceStartupAsDouble + 30;
        int version = ++lifecycleVersion;
        if (serverFunctionsManager == null || string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(myPlayerId))
            throw new System.InvalidOperationException("Missing authenticated player, room or server manager");
        Debug.LogWarning($"[MultiplayerService] Startup requested: player={myPlayerId}, room={roomCode}");
        await AwaitStartupAsync(LoadPlayersInfoAsync());
        EnsureStartupCurrent(version);
        StartHeartbeat();
        StartMatchStateMonitor();
        Debug.LogWarning($"[MultiplayerService] Startup confirmed: room={roomCode}");
        onGameInitialized?.Invoke();
    }

    public async Task LoadPlayersInfoAsync()
    {
        int version = lifecycleVersion;
        double requestedAt = Time.realtimeSinceStartupAsDouble;
        MatchStateDto state = await AwaitStartupAsync(GetMatchStateAsync(roomCode, myPlayerId));
        EnsureStartupCurrent(version);
        ValidateStartupState(state, roomCode, myPlayerId);
        ApplyStartupSnapshot(state, requestedAt);
        ProcessPlayersInfo(state);
        requestedAt = Time.realtimeSinceStartupAsDouble;
        var result = await AwaitStartupAsync(serverFunctionsManager.LoadPlayerDecksIntoRoomAsync(myPlayerId, roomCode));
        EnsureStartupCurrent(version);
        JObject response = result?.FunctionResult == null ? null
            : result.FunctionResult is string text ? JObject.Parse(text) : JObject.FromObject(result.FunctionResult);
        if (response?["success"]?.Value<bool>() != true)
            throw new System.InvalidOperationException("Room deck load failed: " + response?["error"]);
        state = BattleContractMapper.ParseMatchStateResult(response);
        ValidateStartupState(state, roomCode, myPlayerId);
        ApplyStartupSnapshot(state, requestedAt);
        Debug.LogWarning($"[MultiplayerService] Deck load confirmed: room={roomCode}, alreadyLoaded={response["alreadyLoaded"]}");
    }

    private void EnsureStartupCurrent(int version)
    {
        if (!CanContinueStartup || version != lifecycleVersion)
            throw new System.OperationCanceledException("Multiplayer startup no longer owns this scene");
    }

    private static void ValidateStartupState(MatchStateDto state, string expectedRoom, string expectedPlayer)
    {
        if (state == null || state.roomCode != expectedRoom || state.viewerPlayerId != expectedPlayer
            || state.status != "active" || state.phase == MatchPhaseCompleted
            || state.schemaVersion != 2 || state.startup == null
            || !System.DateTimeOffset.TryParse(state.startup.matchedAt, out _)
            || state.playersCount != 2 || state.seats == null || state.seats.Count != 2
            || state.seats.Any(seat => string.IsNullOrEmpty(seat?.playerId))
            || state.seats.Select(seat => seat.playerId).Distinct().Count() != 2
            || !state.seats.Any(seat => seat.playerId == expectedPlayer))
            throw new System.InvalidOperationException("Room is not the requested active two-player match");
    }

    private void ApplyStartupSnapshot(MatchStateDto state, double requestedAt)
    {
        ValidateStartupState(state, roomCode, myPlayerId);
        serverStarted = !string.IsNullOrEmpty(state.startup.startedAt) && state.startup.bothReady;
        if (!serverStarted)
        {
            double remaining = state.startup.remainingMs / 1000.0;
            startupDeadline = System.Math.Min(startupDeadline, requestedAt + remaining);
        }
    }

    public async Task AwaitStartupAsync(Task task)
    {
        int version = lifecycleVersion;
        int remaining = StartupRemainingMilliseconds;
        if (remaining <= 0) throw new System.TimeoutException("Startup deadline reached");
        if (await Task.WhenAny(task, Task.Delay(remaining)) != task)
        {
            _ = task.ContinueWith(failed => { _ = failed.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            throw new System.TimeoutException("Startup deadline reached");
        }
        EnsureStartupCurrent(version);
        await task;
    }

    public async Task<T> AwaitStartupAsync<T>(Task<T> task)
    {
        await AwaitStartupAsync((Task)task);
        return await task;
    }

    public async Task<MatchStateDto> PollStartupAsync()
    {
        double requestedAt = Time.realtimeSinceStartupAsDouble;
        var state = await AwaitStartupAsync(GetMatchStateAsync(roomCode, myPlayerId));
        if (state?.status == "completed")
        {
            HandleRemoteMatchCompletion(state);
            throw new System.OperationCanceledException("Startup ended on server");
        }
        ApplyStartupSnapshot(state, requestedAt);
        return state;
    }

    public async Task ConfirmLocalStartupAsync()
    {
        localPrepared = true;
        while (CanContinueStartup && (applicationPaused || !applicationFocused))
            await AwaitStartupAsync(Task.Delay(100));
        if (!CanAcknowledgeStartup) throw new System.OperationCanceledException("Startup is not current/foreground");
        var ready = new TaskCompletionSource<object>();
        double requestedAt = Time.realtimeSinceStartupAsDouble;
        serverFunctionsManager.MarkMatchStartupReady(roomCode, myPlayerId, result => ready.TrySetResult(result?.FunctionResult));
        var response = await AwaitStartupAsync(ready.Task);
        var state = BattleContractMapper.ParseMatchStateResult(response);
        if (state?.status == "completed")
        {
            HandleRemoteMatchCompletion(state);
            throw new System.OperationCanceledException("Startup cancelled before readiness");
        }
        JObject envelope = response is string text ? JObject.Parse(text) : response == null ? null : JObject.FromObject(response);
        if (envelope?["success"]?.Value<bool>() != true) throw new System.InvalidOperationException("Readiness was not confirmed");
        ApplyStartupSnapshot(state, requestedAt);
        while (!serverStarted)
        {
            await AwaitStartupAsync(Task.Delay(250));
            await PollStartupAsync();
        }
        Debug.LogWarning($"[MultiplayerService] Both clients prepared; game started: room={roomCode}");
    }

    public async Task AbortStartupAsync(string reason)
    {
        if (this == null || leaveRequested || leaveConfirmed || matchCompletionHandled || startupAborted) return;
        startupAborted = true;
        lifecycleVersion++;
        handManager?.CancelPreparation();
        StopHeartbeat(); StopMatchStateMonitor();
        if (PlayerPrefs.GetString("RoomCode", "") == roomCode)
        {
            PlayerPrefs.SetString(PendingStartupCancellationKey, roomCode);
            PlayerPrefs.Save();
        }
        Debug.LogWarning($"[MultiplayerService] Startup aborted: room={roomCode}, reason={reason}");
        var left = new TaskCompletionSource<bool>();
        LeaveMultiplayerRoom(success => left.TrySetResult(success));
        await Task.WhenAny(left.Task, Task.Delay(10000));
        if (this != null && gameObject.scene == SceneManager.GetActiveScene())
            SceneManager.LoadScene("MultiplayerLobby");
    }

    private void OnDestroy()
    {
        lifecycleVersion++;
        StopHeartbeat();
        StopMatchStateMonitor();
        if (serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId)
            && !string.IsNullOrEmpty(roomCode) && !leaveRequested && !leaveConfirmed && !matchCompletionHandled)
        {
            Debug.LogWarning($"[MultiplayerService] Emergency leave: room={roomCode}");
            serverFunctionsManager.LeaveRoom(myPlayerId, roomCode);
        }
    }

    private void ProcessPlayersInfo(MatchStateDto state)
    {
        bool amIFirstPlayer = state.seats[0].playerId == myPlayerId;
        string mine = state.seats.First(seat => seat.playerId == myPlayerId).username;
        string opponent = state.seats.First(seat => seat.playerId != myPlayerId).username;
        playerNameText.text = amIFirstPlayer ? mine : opponent;
        enemyNameText.text = amIFirstPlayer ? opponent : mine;
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
        Debug.Log(
            $"[MultiplayerService] MatchState monitor started: playerId={myPlayerId}, roomCode={roomCode}, interval={MatchStateMonitorIntervalSeconds}s"
        );
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
        while (CanContinueStartup)
        {
            yield return new WaitForSeconds(MatchStateMonitorIntervalSeconds);

            if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(myPlayerId))
            {
                Debug.LogWarning(
                    $"[MultiplayerService] MatchState monitor skipped poll: roomCode={roomCode}, playerId={myPlayerId}"
                );
                continue;
            }

            Debug.Log(
                $"[MultiplayerService] MatchState monitor poll request: roomCode={roomCode}, playerId={myPlayerId}"
            );
            var matchStateTask = GetMatchStateAsync(roomCode, myPlayerId);
            yield return new WaitUntil(() => matchStateTask.IsCompleted);

            if (
                matchStateTask.Status != TaskStatus.RanToCompletion
                || matchStateTask.Result == null
            )
            {
                Debug.LogWarning(
                    $"[MultiplayerService] MatchState monitor poll returned no state: taskStatus={matchStateTask.Status}"
                );
                continue;
            }

            if (!CanContinueStartup) yield break;
            MatchStateDto matchState = matchStateTask.Result;
            Debug.Log(
                $"[MultiplayerService] MatchState monitor poll result: roomCode={matchState.roomCode}, status={matchState.status}, phase={matchState.phase}, playersCount={matchState.playersCount}, reason={matchState.matchResult?.reason}, winner={matchState.matchResult?.winnerPlayerId}, loser={matchState.matchResult?.loserPlayerId}"
            );

            if (
                string.Equals(
                    matchState.phase,
                    MatchPhaseCompleted,
                    System.StringComparison.OrdinalIgnoreCase
                )
            )
            {
                HandleRemoteMatchCompletion(matchState);
                yield break;
            }
        }
    }

    private void HandleRemoteMatchCompletion(MatchStateDto matchState)
    {
        if (matchCompletionHandled || matchState == null || matchState.roomCode != roomCode
            || matchState.viewerPlayerId != myPlayerId)
        {
            Debug.Log("[MultiplayerService] HandleRemoteMatchCompletion skipped: already handled");
            return;
        }

        Debug.Log(
            $"[MultiplayerService] HandleRemoteMatchCompletion start: phase={matchState.phase}, reason={matchState.matchResult?.reason}, winner={matchState.matchResult?.winnerPlayerId}, loser={matchState.matchResult?.loserPlayerId}"
        );
        matchCompletionHandled = true;
        lifecycleVersion++;
        if (string.IsNullOrEmpty(matchState.startup?.startedAt)) handManager?.CancelPreparation();
        StopHeartbeat();

        ClearOwnedRoomKeys();
        Debug.Log(
            "[MultiplayerService] Cleared local room PlayerPrefs after confirmed match completion"
        );

        string winnerPlayerId = matchState.matchResult?.winnerPlayerId;
        string loserPlayerId = matchState.matchResult?.loserPlayerId;
        string reason = matchState.matchResult?.reason ?? matchState.completionReason;
        bool iWon = !string.IsNullOrEmpty(winnerPlayerId) && winnerPlayerId == myPlayerId;
        bool iLost = !string.IsNullOrEmpty(loserPlayerId) && loserPlayerId == myPlayerId;

        if (reason == "cancelled" && !iWon && !iLost)
        {
            Debug.LogWarning($"[MultiplayerService] Startup cancelled without result: room={roomCode}");
            SceneManager.LoadScene("MultiplayerLobby");
            return;
        }

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
        if (statusText != null) statusText.text = message;
        fightSystem?.multiplayerUI?.ShowStatus(message);
        Debug.Log(
            $"[MultiplayerService] Match completed. reason={reason}, winner={winnerPlayerId}, loser={loserPlayerId}"
        );
    }

    private void ClearOwnedRoomKeys()
    {
        if (PlayerPrefs.GetString("RoomCode", "") != roomCode) return;
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        if (PlayerPrefs.GetString(PendingStartupCancellationKey, "") == roomCode)
            PlayerPrefs.DeleteKey(PendingStartupCancellationKey);
        PlayerPrefs.Save();
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
        while (CanContinueStartup)
        {
            yield return new WaitForSecondsRealtime(heartbeatInterval);

            // Posli heartbeat na server
            if (CanContinueStartup && serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId) && !string.IsNullOrEmpty(roomCode))
            {
                Debug.Log(
                    $"[MultiplayerService] Heartbeat request: playerId={myPlayerId}, roomCode={roomCode}"
                );
                serverFunctionsManager.Heartbeat(myPlayerId, roomCode);
            }
        }
    }

    // === CLEANUP PRI OPUSTENI ===

    void OnApplicationPause(bool pauseStatus)
    {
        applicationPaused = pauseStatus;
        Debug.Log(
            $"[MultiplayerService] OnApplicationPause: pauseStatus={pauseStatus}, playerId={myPlayerId}, roomCode={roomCode}"
        );
        if (pauseStatus)
        {
            // Aplikacia sa pozastavuje - posli heartbeat
            if (CanContinueStartup && serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId) && !string.IsNullOrEmpty(roomCode))
            {
                serverFunctionsManager.Heartbeat(myPlayerId, roomCode);
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        applicationFocused = hasFocus;
        Debug.Log(
            $"[MultiplayerService] OnApplicationFocus: hasFocus={hasFocus}, playerId={myPlayerId}, roomCode={roomCode}"
        );
        if (!hasFocus)
        {
            // Aplikacia straca focus - posli heartbeat
            if (CanContinueStartup && serverFunctionsManager != null && !string.IsNullOrEmpty(myPlayerId) && !string.IsNullOrEmpty(roomCode))
            {
                serverFunctionsManager.Heartbeat(myPlayerId, roomCode);
            }
        }
    }

    public void LeaveMultiplayerRoom(System.Action<bool> onCompleted = null)
    {
        if (leaveConfirmed || matchCompletionHandled)
        {
            // The server-confirmed terminal snapshot already ended our membership.
            onCompleted?.Invoke(true);
            return;
        }
        if (leaveRequested || serverFunctionsManager == null
            || string.IsNullOrEmpty(myPlayerId) || string.IsNullOrEmpty(roomCode))
        {
            onCompleted?.Invoke(false);
            return;
        }
        leaveRequested = true;
        int version = ++lifecycleVersion;
        string expectedRoom = roomCode;
        StopHeartbeat();
        StopMatchStateMonitor();
        if (statusText != null) statusText.text = "Leaving room...";
        Debug.LogWarning($"[MultiplayerService] Leave requested: room={expectedRoom}");
        serverFunctionsManager.LeaveRoom(myPlayerId, expectedRoom, result =>
        {
            if (this == null || version != lifecycleVersion || roomCode != expectedRoom) return;
            bool succeeded = false;
            try
            {
                JObject response = result?.FunctionResult == null ? null
                    : result.FunctionResult is string text ? JObject.Parse(text) : JObject.FromObject(result.FunctionResult);
                MatchStateDto state = BattleContractMapper.ParseMatchStateResult(response);
                succeeded = response?["success"]?.Value<bool>() == true
                    && (string)response["roomCode"] == expectedRoom
                    && state?.roomCode == expectedRoom && state.viewerPlayerId == myPlayerId
                    && state.status == "completed" && state.phase == MatchPhaseCompleted;
            }
            catch (System.Exception ex) { Debug.LogWarning("[MultiplayerService] Leave response invalid: " + ex.Message); }
            leaveRequested = false;
            if (!succeeded)
            {
                if (statusText != null) statusText.text = "Leave failed";
                serverFunctionsManager.ShowNetworkErrorIndicator("Leave failed");
                Debug.LogWarning($"[MultiplayerService] Leave failed; room retained: {expectedRoom}");
                onCompleted?.Invoke(false);
                return;
            }
            leaveConfirmed = true;
            ClearOwnedRoomKeys();
            Debug.LogWarning($"[MultiplayerService] Leave confirmed: room={expectedRoom}");
            onCompleted?.Invoke(true);
        });
    }

    // === EXIT FUNKCIONALITA ===

    public void OnExitToLobby()
    {
        // Volaj tuto metodu z UI tlacidla "Exit"
        Debug.Log(
            $"[MultiplayerService] OnExitToLobby pressed: playerId={myPlayerId}, roomCode={roomCode}"
        );

        LeaveMultiplayerRoom(success =>
        {
            Debug.Log($"[MultiplayerService] OnExitToLobby leave completed: success={success}");
            if (success)
            {
                Debug.Log(
                    "[MultiplayerService] Loading MultiplayerLobby scene after successful leave"
                );
                SceneManager.LoadScene("MultiplayerLobby");
            }
        });
    }

    public void OnExitToMainMenu()
    {
        // Alternativna metoda pre exit do hlavneho menu
        Debug.Log(
            $"[MultiplayerService] OnExitToMainMenu pressed: playerId={myPlayerId}, roomCode={roomCode}"
        );

        LeaveMultiplayerRoom(success =>
        {
            Debug.Log($"[MultiplayerService] OnExitToMainMenu leave completed: success={success}");
            if (success)
            {
                Debug.Log("[MultiplayerService] Loading Main scene after successful leave");
                SceneManager.LoadScene("Main");
            }
        });
    }

    public async Task SubmitSelectedCardAsync(
        string roomCode,
        string playerId,
        SelectedCardData cardData
    )
    {
        if (serverFunctionsManager == null)
        {
            Debug.LogError("SubmitSelectedCardAsync: serverFunctionsManager is null");
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        bool isCompleted = false;

        serverFunctionsManager.SetSelectedCard(
            roomCode,
            playerId,
            cardData,
            result =>
            {
                if (isCompleted)
                    return; // Ignore late responses
                isCompleted = true;

                if (result == null)
                {
                    Debug.LogError(
                        "SubmitSelectedCardAsync: Failed to set selected card on server"
                    );
                }
                else
                {
                    Debug.Log(
                        $"SubmitSelectedCardAsync: Card {cardData?.cardId} stored for player {playerId}"
                    );
                }

                tcs.TrySetResult(true);
            }
        );

        // Add 15-second timeout
        _ = Task.Delay(15000)
            .ContinueWith(_ =>
            {
                if (!isCompleted)
                {
                    isCompleted = true;
                    Debug.LogWarning(
                        $"SubmitSelectedCardAsync: Timeout for player {playerId}, card {cardData?.cardId}"
                    );
                    tcs.TrySetResult(false);
                }
            });

        await tcs.Task;
    }

    public async Task<Dictionary<string, SelectedCardData>> GetSelectedCardsFromMatchStateAsync(
        string roomCode,
        string playerId
    )
    {
        var tcs = new TaskCompletionSource<Dictionary<string, SelectedCardData>>();

        if (serverFunctionsManager == null)
        {
            Debug.LogError("GetSelectedCardsFromMatchStateAsync: serverFunctionsManager is null");
            tcs.TrySetResult(new Dictionary<string, SelectedCardData>());
            return await tcs.Task;
        }

        serverFunctionsManager.GetMatchState(
            roomCode,
            playerId,
            result =>
            {
                if (result?.FunctionResult != null)
                {
                    try
                    {
                        var matchState = BattleContractMapper.ParseMatchStateResult(
                            result.FunctionResult
                        );
                        tcs.TrySetResult(
                            matchState?.selectedCards ?? new Dictionary<string, SelectedCardData>()
                        );
                        return;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(
                            $"GetSelectedCardsFromMatchStateAsync: Error parsing response - {ex.Message}"
                        );
                    }
                }
                else
                {
                    Debug.LogWarning(
                        "GetSelectedCardsFromMatchStateAsync: No result returned from server"
                    );
                }

                tcs.TrySetResult(new Dictionary<string, SelectedCardData>());
            }
        );

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

        serverFunctionsManager.GetMatchState(
            roomCode,
            playerId,
            result =>
            {
                if (result?.FunctionResult == null)
                {
                    Debug.LogWarning("GetMatchStateAsync: No result returned from server");
                    tcs.TrySetResult(null);
                    return;
                }

                try
                {
                    var matchState = BattleContractMapper.ParseMatchStateResult(
                        result.FunctionResult
                    );
                    if (matchState?.roomCode != roomCode || matchState?.viewerPlayerId != playerId)
                        matchState = null;
                    if (matchState == null)
                    {
                        Debug.LogWarning(
                            $"GetMatchStateAsync: Failed to parse match state for room {roomCode}"
                        );
                    }

                    tcs.TrySetResult(matchState);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"GetMatchStateAsync: Error parsing response - {ex.Message}");
                    tcs.TrySetResult(null);
                }
            }
        );

        return await tcs.Task;
    }

}
