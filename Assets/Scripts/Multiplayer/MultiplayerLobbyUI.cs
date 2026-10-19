using System.Collections;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerLobbyUI : MonoBehaviour
{
    private enum MatchmakingState { Idle, Connecting, Waiting, Reconciling, Cancelling, Error, Starting }

    public Button joinButton;
    public ServerFunctionsManager serverFunctionsManager;

    private string playerId;
    private string ownedRoomCode;
    private Coroutine waitForOpponentCoroutine;
    private Coroutine waitingHeartbeatCoroutine;
    private const float WaitingHeartbeatInterval = 5f;
    private float waitingDeadline;
    private int waitingPresenceVersion;
    private bool applicationPaused;
    private bool waitingResumeRequired;
    private TMP_Text joinButtonLabel;
    private MatchmakingState matchmakingState;
    private int matchmakingRequestVersion;
    private bool cancelRequested;
    private bool retryReconciliation;

    private void Start()
    {
        playerId = PlayFabManagerLogin.Instance?.LoggedInPlayerId;
        if (joinButton == null || serverFunctionsManager == null || string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("[Lobby] Missing button, server manager or authenticated player.");
            if (joinButton != null) joinButton.interactable = false;
            return;
        }
        joinButtonLabel = joinButton.GetComponentInChildren<TMP_Text>();
        joinButton.onClick.AddListener(OnJoinClicked);
        ownedRoomCode = PlayerPrefs.GetString("RoomCode", "");
        if (string.IsNullOrEmpty(ownedRoomCode)) ResetMatchmakingUi();
        else ReconcileExistingRoom();
        serverFunctionsManager.CleanupRooms();
    }

    private void OnDestroy()
    {
        matchmakingRequestVersion++;
        if (joinButton != null) joinButton.onClick.RemoveListener(OnJoinClicked);
        StopWaiting();
    }

    private void SetJoinButtonText(string text)
    {
        if (joinButtonLabel != null) joinButtonLabel.text = text;
    }

    private bool OwnsRequest(int version, string expectedRoom = null)
    {
        return this != null && isActiveAndEnabled && version == matchmakingRequestVersion
            && (expectedRoom == null || (ownedRoomCode == expectedRoom
                && PlayerPrefs.GetString("RoomCode", "") == expectedRoom));
    }

    private void OnJoinClicked()
    {
        switch (matchmakingState)
        {
            case MatchmakingState.Idle: StartMatchmaking(); break;
            case MatchmakingState.Connecting:
            case MatchmakingState.Waiting: RequestCancelMatchmaking(); break;
            case MatchmakingState.Error:
                if (retryReconciliation) ReconcileExistingRoom();
                else if (!string.IsNullOrEmpty(ownedRoomCode)) BeginCancellation();
                else StartMatchmaking();
                break;
        }
    }

    private void ReconcileExistingRoom()
    {
        int version = ++matchmakingRequestVersion;
        string expectedRoom = ownedRoomCode;
        retryReconciliation = true;
        SetBusy(MatchmakingState.Reconciling, "CONNECTING...", false);
        Debug.LogWarning($"[Lobby] Reconciling room={expectedRoom}, request={version}");
        serverFunctionsManager.GetMatchState(expectedRoom, playerId, result =>
        {
            if (!OwnsRequest(version, expectedRoom)) return;
            object response = result?.FunctionResult;
            if (IsMissingRoom(response, expectedRoom)) { ClearOwnedRoom(expectedRoom); return; }
            MatchStateDto state = ParseOwnedState(response, expectedRoom);
            if (state == null) { SetError("Room reconciliation failed"); return; }
            if (state.status == "completed") ClearOwnedRoom(expectedRoom);
            else if (state.status == "active" && !HasPendingStartupCancellation(expectedRoom)) StartConfirmedBattle(version);
            else { retryReconciliation = false; BeginCancellation(); }
        });
    }

    private void StartMatchmaking()
    {
        // Keep cancellation intent when retrying a CONNECT whose response was lost.
        int version = ++matchmakingRequestVersion;
        retryReconciliation = false;
        SetBusy(cancelRequested ? MatchmakingState.Cancelling : MatchmakingState.Connecting,
            "CONNECTING...", !cancelRequested);
        Debug.LogWarning($"[Lobby] CONNECT requested: player={playerId}, request={version}");
        serverFunctionsManager.JoinOrCreateRoom(playerId, PlayerPrefs.GetString("username", playerId), result =>
        {
            if (!OwnsRequest(version)) return;
            JObject response = ParseResponse(result?.FunctionResult);
            MatchStateDto state = ParseOwnedState(response, null);
            if (response?["success"]?.Value<bool>() != true || state == null)
            {
                SetError("CONNECT failed; retry will reconcile server membership");
                return;
            }
            ownedRoomCode = state.roomCode;
            PlayerPrefs.SetString("RoomCode", ownedRoomCode);
            PlayerPrefs.SetString("IsWaitingForOpponent", state.status == "waiting" ? "true" : "false");
            Debug.LogWarning($"[Lobby] CONNECT confirmed: room={ownedRoomCode}, status={state.status}");
            if (state.status == "completed") { ClearOwnedRoom(ownedRoomCode); return; }
            if (cancelRequested) { BeginCancellation(); return; }
            if (state.status == "active") { StartConfirmedBattle(version); return; }
            BeginWaiting(true);
        });
    }

    private void RequestCancelMatchmaking()
    {
        cancelRequested = true;
        StopWaiting();
        if (string.IsNullOrEmpty(ownedRoomCode))
        {
            // CONNECT is still in flight. Its callback must supply the exact room to cancel.
            SetBusy(MatchmakingState.Cancelling, "CANCELLING...", false);
            return;
        }
        BeginCancellation();
    }

    private void BeginCancellation()
    {
        StopWaiting();
        waitingResumeRequired = false;
        cancelRequested = true;
        retryReconciliation = false;
        int version = ++matchmakingRequestVersion;
        string expectedRoom = ownedRoomCode;
        SetBusy(MatchmakingState.Cancelling, "CANCELLING...", false);
        Debug.LogWarning($"[Lobby] CANCEL requested: room={expectedRoom}, request={version}");
        if (HasPendingStartupCancellation(expectedRoom))
            serverFunctionsManager.LeaveRoom(playerId, expectedRoom,
                result => ApplyCancellationResponse(result?.FunctionResult, expectedRoom, version));
        else serverFunctionsManager.CancelMatchmaking(playerId, expectedRoom,
            result => ApplyCancellationResponse(result?.FunctionResult, expectedRoom, version));
    }

    private void ApplyCancellationResponse(object response, string expectedRoom, int version)
    {
        if (!OwnsRequest(version, expectedRoom)) return;
        if (IsMissingRoom(response, expectedRoom)) { ClearOwnedRoom(expectedRoom); return; }
        MatchStateDto state = ParseOwnedState(response, expectedRoom);
        if (state == null) { SetError("CANCEL failed; room retained for retry"); return; }
        if (state.status == "completed") ClearOwnedRoom(expectedRoom);
        else if (state.status == "active" && !HasPendingStartupCancellation(expectedRoom)
            && !string.IsNullOrEmpty(state.startup?.startedAt))
        {
            Debug.LogWarning($"[Lobby] CANCEL lost to a match: room={expectedRoom}");
            StartConfirmedBattle(version);
        }
        else SetError("CANCEL not confirmed; room retained for retry");
    }

    private static bool HasPendingStartupCancellation(string room)
    {
        return PlayerPrefs.GetString(MultiplayerService.PendingStartupCancellationKey, "") == room;
    }

    private IEnumerator WaitForOpponent(int version, string expectedRoom)
    {
        int presenceVersion = waitingPresenceVersion;
        while (OwnsWaitingPresence(version, expectedRoom, presenceVersion) && Time.realtimeSinceStartup < waitingDeadline)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (!OwnsWaitingPresence(version, expectedRoom, presenceVersion)) yield break;
            if (Time.realtimeSinceStartup >= waitingDeadline) break;
            bool received = false;
            object response = null;
            serverFunctionsManager.GetMatchState(expectedRoom, playerId, result =>
            {
                response = result?.FunctionResult;
                received = true;
            });
            yield return new WaitUntil(() => received || !OwnsWaitingPresence(version, expectedRoom, presenceVersion)
                || Time.realtimeSinceStartup >= waitingDeadline);
            if (!OwnsWaitingPresence(version, expectedRoom, presenceVersion)) yield break;
            if (!received) break;
            if (IsMissingRoom(response, expectedRoom))
            {
                waitForOpponentCoroutine = null;
                ClearOwnedRoom(expectedRoom);
                yield break;
            }
            MatchStateDto state = ParseOwnedState(response, expectedRoom);
            if (state == null)
            {
                waitForOpponentCoroutine = null;
                SetError("Queue poll failed; room retained for cancellation");
                yield break;
            }
            if (state.status != "waiting")
            {
                waitForOpponentCoroutine = null;
                if (state.status == "completed") ClearOwnedRoom(expectedRoom);
                else StartConfirmedBattle(version);
                yield break;
            }
        }
        waitForOpponentCoroutine = null;
        if (OwnsWaitingPresence(version, expectedRoom, presenceVersion))
        {
            Debug.LogWarning($"[Lobby] Waiting timeout: cancelling room={expectedRoom}");
            RequestCancelMatchmaking();
        }
    }

    private void BeginWaiting(bool newDeadline)
    {
        StopWaiting();
        if (newDeadline) waitingDeadline = Time.realtimeSinceStartup + 60f;
        if (Time.realtimeSinceStartup >= waitingDeadline) { RequestCancelMatchmaking(); return; }
        SetBusy(MatchmakingState.Waiting, "SEARCHING FOR PREY...", true);
        waitingResumeRequired = applicationPaused;
        if (applicationPaused) return;
        Debug.LogWarning($"[Lobby] Waiting presence started: room={ownedRoomCode}, remaining={waitingDeadline - Time.realtimeSinceStartup:F1}s");
        waitForOpponentCoroutine = StartCoroutine(WaitForOpponent(matchmakingRequestVersion, ownedRoomCode));
        waitingHeartbeatCoroutine = StartCoroutine(WaitingHeartbeatLoop(matchmakingRequestVersion, ownedRoomCode, waitingPresenceVersion));
    }

    private bool OwnsWaitingPresence(int version, string expectedRoom, int presenceVersion)
    {
        return OwnsRequest(version, expectedRoom) && presenceVersion == waitingPresenceVersion
            && matchmakingState == MatchmakingState.Waiting && !cancelRequested && !applicationPaused;
    }

    private bool CanRenewWaitingPresence(int version, string expectedRoom, int presenceVersion)
    {
        return OwnsWaitingPresence(version, expectedRoom, presenceVersion) && Time.realtimeSinceStartup < waitingDeadline;
    }

    private IEnumerator WaitingHeartbeatLoop(int version, string expectedRoom, int presenceVersion)
    {
        // Let the coroutine handle be stored before a potentially synchronous failure stops waiting.
        yield return null;
        while (CanRenewWaitingPresence(version, expectedRoom, presenceVersion))
        {
            bool received = false;
            object response = null;
            serverFunctionsManager.Heartbeat(playerId, expectedRoom, result =>
            {
                response = result?.FunctionResult;
                received = true;
            });
            yield return new WaitUntil(() => received || !CanRenewWaitingPresence(version, expectedRoom, presenceVersion));
            if (!CanRenewWaitingPresence(version, expectedRoom, presenceVersion)) yield break;
            if (!ApplyWaitingHeartbeatResponse(response, version, expectedRoom, presenceVersion))
            {
                waitingHeartbeatCoroutine = null;
                yield break;
            }
            yield return new WaitForSecondsRealtime(WaitingHeartbeatInterval);
        }
        waitingHeartbeatCoroutine = null;
    }

    private bool ApplyWaitingHeartbeatResponse(object response, int version, string expectedRoom, int presenceVersion)
    {
        if (!CanRenewWaitingPresence(version, expectedRoom, presenceVersion)) return false;
        JObject data = ParseResponse(response);
        if (data?["success"]?.Value<bool>() != true || (string)data["roomCode"] != expectedRoom)
        {
            SetError("Waiting heartbeat failed; room retained for cancellation");
            return false;
        }
        if (data["roomCompleted"]?.Value<bool>() == true)
        {
            Debug.LogWarning($"[Lobby] Waiting presence ended; awaiting canonical state: room={expectedRoom}");
            return false;
        }
        return true;
    }

    private void OnApplicationPause(bool paused)
    {
        applicationPaused = paused;
        if (paused)
        {
            if (matchmakingState != MatchmakingState.Waiting && !waitingResumeRequired) return;
            waitingResumeRequired = true;
            matchmakingRequestVersion++;
            StopWaiting();
            Debug.LogWarning($"[Lobby] Waiting presence paused: room={ownedRoomCode}");
        }
        else if (waitingResumeRequired && !string.IsNullOrEmpty(ownedRoomCode))
        {
            int version = ++matchmakingRequestVersion;
            string expectedRoom = ownedRoomCode;
            retryReconciliation = true;
            SetBusy(MatchmakingState.Reconciling, "CONNECTING...", false);
            Debug.LogWarning($"[Lobby] Waiting resume verification: room={expectedRoom}, request={version}");
            serverFunctionsManager.GetMatchState(expectedRoom, playerId,
                result => ApplyWaitingResumeResponse(result?.FunctionResult, expectedRoom, version));
        }
    }

    private void ApplyWaitingResumeResponse(object response, string expectedRoom, int version)
    {
        if (!OwnsRequest(version, expectedRoom) || applicationPaused || matchmakingState != MatchmakingState.Reconciling) return;
        waitingResumeRequired = false;
        if (IsMissingRoom(response, expectedRoom)) { ClearOwnedRoom(expectedRoom); return; }
        MatchStateDto state = ParseOwnedState(response, expectedRoom);
        if (state == null) { SetError("Waiting resume verification failed; room retained for cancellation"); return; }
        retryReconciliation = false;
        if (state.status == "completed") ClearOwnedRoom(expectedRoom);
        else if (state.status == "active") StartConfirmedBattle(version);
        else BeginWaiting(false);
    }

    private static JObject ParseResponse(object response)
    {
        if (response == null) return null;
        try { return response as JObject ?? (response is string text ? JObject.Parse(text) : JObject.FromObject(response)); }
        catch (System.Exception) { return null; }
    }

    private static bool IsMissingRoom(object response, string expectedRoom)
    {
        JObject data = ParseResponse(response);
        return data?["success"]?.Value<bool>() == false && (string)data["error"] == "room_not_found"
            && (string)data["roomCode"] == expectedRoom;
    }

    private MatchStateDto ParseOwnedState(object response, string expectedRoom)
    {
        try
        {
            MatchStateDto state = BattleContractMapper.ParseMatchStateResult(response);
            if (state == null || string.IsNullOrEmpty(state.roomCode)
                || (expectedRoom != null && state.roomCode != expectedRoom)
                || state.viewerPlayerId != playerId || state.seats == null
                || !state.seats.Any(seat => seat?.playerId == playerId)
                || state.seats.Count != state.playersCount
                || state.seats.Any(seat => string.IsNullOrEmpty(seat?.playerId))
                || state.seats.Select(seat => seat.playerId).Distinct().Count() != state.playersCount)
                return null;
            if (state.status == "active" && state.playersCount == 2 && state.phase != "completed"
                && state.schemaVersion == 2 && state.startup != null
                && System.DateTimeOffset.TryParse(state.startup.matchedAt, out _)) return state;
            if (state.status == "waiting" && state.playersCount == 1 && state.phase == "waiting_for_players") return state;
            if (state.status == "completed" && state.phase == "completed") return state;
            return null;
        }
        catch (System.Exception) { return null; }
    }

    private void ClearOwnedRoom(string expectedRoom)
    {
        if (ownedRoomCode != expectedRoom || PlayerPrefs.GetString("RoomCode", "") != expectedRoom) return;
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        if (HasPendingStartupCancellation(expectedRoom))
            PlayerPrefs.DeleteKey(MultiplayerService.PendingStartupCancellationKey);
        PlayerPrefs.Save();
        Debug.LogWarning($"[Lobby] Terminal/missing room acknowledged: room={expectedRoom}");
        ownedRoomCode = null;
        ResetMatchmakingUi();
    }

    private void ResetMatchmakingUi()
    {
        StopWaiting();
        SceneLoadingOverlay.Hide();
        matchmakingState = MatchmakingState.Idle;
        cancelRequested = false;
        retryReconciliation = false;
        waitingResumeRequired = false;
        if (joinButton != null) joinButton.interactable = true;
        SetJoinButtonText("CONNECT");
    }

    private void SetError(string reason)
    {
        StopWaiting();
        waitingResumeRequired = false;
        SceneLoadingOverlay.Hide();
        matchmakingState = MatchmakingState.Error;
        if (joinButton != null) joinButton.interactable = true;
        SetJoinButtonText(retryReconciliation || string.IsNullOrEmpty(ownedRoomCode) ? "RETRY" : "RETRY CANCEL");
        serverFunctionsManager?.ShowNetworkErrorIndicator(reason);
        Debug.LogWarning($"[Lobby] {reason}; room={ownedRoomCode}");
    }

    private void StopWaiting()
    {
        waitingPresenceVersion++;
        if (waitForOpponentCoroutine != null)
        {
            StopCoroutine(waitForOpponentCoroutine);
            waitForOpponentCoroutine = null;
        }
        if (waitingHeartbeatCoroutine != null)
        {
            StopCoroutine(waitingHeartbeatCoroutine);
            waitingHeartbeatCoroutine = null;
            Debug.LogWarning($"[Lobby] Waiting presence stopped: room={ownedRoomCode}");
        }
    }

    private void SetBusy(MatchmakingState state, string message, bool canCancel)
    {
        matchmakingState = state;
        SetJoinButtonText("CANCEL");
        SceneLoadingOverlay.Show();
        SceneLoadingOverlay.SetMessage(message);
        if (joinButton == null) return;
        joinButton.interactable = canCancel;
        if (canCancel)
        {
            Canvas canvas = joinButton.GetComponentInParent<Canvas>();
            if (canvas == null || joinButton.transform.parent != canvas.transform)
                Debug.LogError("[Lobby] ConnectButton must be directly under Canvas to stay above loading.");
            else joinButton.transform.SetAsLastSibling();
        }
    }

    private void StartConfirmedBattle(int version)
    {
        StopWaiting();
        waitingResumeRequired = false;
        cancelRequested = false;
        retryReconciliation = false;
        PlayerPrefs.SetString("IsWaitingForOpponent", "false");
        SetBusy(MatchmakingState.Starting, "CONNECTING...", false);
        Debug.LogWarning($"[Lobby] Match confirmed: room={ownedRoomCode}");
        StartCoroutine(StartBattleWithDelay(version, ownedRoomCode));
    }

    private IEnumerator StartBattleWithDelay(int version, string expectedRoom)
    {
        yield return new WaitForSecondsRealtime(1f);
        if (OwnsRequest(version, expectedRoom)) SceneManager.LoadScene("Multiplayer");
    }
}
