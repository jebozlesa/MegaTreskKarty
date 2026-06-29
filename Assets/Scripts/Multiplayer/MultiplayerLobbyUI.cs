using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerLobbyUI : MonoBehaviour
{
    private static bool VerboseLobbyLogs => false;

    private enum MatchmakingState
    {
        Idle,
        Connecting,
        Waiting,
        Starting,
    }

    public Button joinButton;
    public ServerFunctionsManager serverFunctionsManager;

    private string PlayerId => PlayFabManagerLogin.Instance.LoggedInPlayerId;
    private Coroutine waitForOpponentCoroutine;
    private TMP_Text joinButtonLabel;
    private Text joinButtonLegacyLabel;
    private MatchmakingState matchmakingState = MatchmakingState.Idle;
    private int matchmakingRequestVersion;
    private bool cancelRequested;

    private static void LogVerbose(string message)
    {
        if (VerboseLobbyLogs)
        {
            Debug.Log(message);
        }
    }

    void Start()
    {
        if (joinButton == null)
        {
            Debug.LogError("[Lobby] joinButton is not assigned on MultiplayerLobbyUI.");
            return;
        }

        joinButton.onClick.AddListener(OnJoinClicked);
        joinButtonLabel = joinButton != null ? joinButton.GetComponentInChildren<TMP_Text>() : null;
        joinButtonLegacyLabel =
            joinButtonLabel == null ? joinButton.GetComponentInChildren<Text>() : null;

        SetJoinButtonText("CONNECT");

        // Pri vstupe do lobby vycistime stare miestnosti
        CleanupOldRooms();

        // Ak sa vraciame z multiplayeru, opustime staru miestnost
        LeaveAnyExistingRoom();
    }

    private void SetJoinButtonText(string text)
    {
        if (joinButtonLabel != null)
        {
            joinButtonLabel.text = text;
            return;
        }

        if (joinButtonLegacyLabel != null)
        {
            joinButtonLegacyLabel.text = text;
        }
    }

    void OnJoinClicked()
    {
        if (matchmakingState == MatchmakingState.Idle)
        {
            StartMatchmaking();
            return;
        }

        RequestCancelMatchmaking();
    }

    private void StartMatchmaking()
    {
        cancelRequested = false;
        matchmakingState = MatchmakingState.Connecting;
        matchmakingRequestVersion++;
        int requestVersion = matchmakingRequestVersion;

        SetJoinButtonText("CANCEL");
        SceneLoadingOverlay.Show();
        SceneLoadingOverlay.SetMessage("CONNECTING...");

        string username = PlayerPrefs.GetString("username", PlayerId);
        serverFunctionsManager.JoinOrCreateRoom(
            PlayerId,
            username,
            result =>
            {
                if (requestVersion != matchmakingRequestVersion)
                {
                    return;
                }

                if (VerboseLobbyLogs)
                {
                    LogVerbose(
                        $"FunctionResult raw: {Newtonsoft.Json.JsonConvert.SerializeObject(result?.FunctionResult)}"
                    );
                }
                if (result != null && result.FunctionResult != null)
                {
                    JObject functionResult = null;
                    try
                    {
                        functionResult = JObject.Parse(result.FunctionResult.ToString());
                    }
                    catch
                    {
                        Debug.LogError("Failed to parse FunctionResult to JObject");
                    }
                    LogVerbose($"FunctionResult JObject: {functionResult}");
                    if (functionResult != null && functionResult["room"] != null)
                    {
                        var room = functionResult["room"];
                        LogVerbose($"Room object: {room}");

                        // Ulozenie informacii o miestnosti
                        string roomCode = room["roomCode"].ToString();
                        PlayerPrefs.SetString("RoomCode", roomCode);

                        if (cancelRequested)
                        {
                            CancelAfterJoinResponse(roomCode, requestVersion);
                            return;
                        }

                        // Kontrola poctu hracov na urcenie spravania
                        var playersArray = room["players"] as JArray;
                        bool isWaiting = playersArray != null && playersArray.Count == 1;

                        // Ulozenie stavu cakania
                        PlayerPrefs.SetString("IsWaitingForOpponent", isWaiting ? "true" : "false");

                        if (isWaiting)
                        {
                            matchmakingState = MatchmakingState.Waiting;
                            SceneLoadingOverlay.SetMessage("SEARCHING FOR PREY...");
                            waitForOpponentCoroutine = StartCoroutine(
                                WaitForOpponentAndStartBattle(requestVersion)
                            );
                        }
                        else
                        {
                            matchmakingState = MatchmakingState.Starting;
                            // [OK] Immediate transition - druhy hrac sa pripojil
                            StartCoroutine(StartBattleWithDelay(requestVersion));
                        }
                    }
                    else
                    {
                        ResetMatchmakingUi();
                    }
                }
                else
                {
                    ResetMatchmakingUi();
                }
            }
        );
    }

    private void RequestCancelMatchmaking()
    {
        string roomCode = PlayerPrefs.GetString("RoomCode", "");
        if (waitForOpponentCoroutine != null)
        {
            StopCoroutine(waitForOpponentCoroutine);
            waitForOpponentCoroutine = null;
        }

        cancelRequested = true;
        ResetMatchmakingUi(true);

        if (string.IsNullOrEmpty(roomCode) || serverFunctionsManager == null)
        {
            return;
        }

        serverFunctionsManager.CancelMatchmaking(
            PlayerId,
            roomCode,
            result =>
            {
                if (result == null)
                {
                    Debug.LogError("[Lobby] Cancel matchmaking failed; keeping local queue state");
                    return;
                }

                if (result.FunctionResult == null)
                {
                    Debug.LogError(
                        "[Lobby] Cancel matchmaking returned empty FunctionResult; keeping local queue state"
                    );
                    return;
                }

                bool cancelled = false;
                try
                {
                    var functionResult = JObject.Parse(result.FunctionResult.ToString());
                    cancelled = functionResult["success"]?.Value<bool>() == true;
                    if (!cancelled)
                    {
                        Debug.LogError(
                            $"[Lobby] Cancel matchmaking rejected: {functionResult["error"]}"
                        );
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(
                        $"[Lobby] Failed to parse cancel matchmaking result: {e.Message}"
                    );
                }

                if (!cancelled)
                {
                    return;
                }

                PlayerPrefs.DeleteKey("RoomCode");
                PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            }
        );
    }

    private void CancelAfterJoinResponse(string roomCode, int requestVersion)
    {
        if (string.IsNullOrEmpty(roomCode) || serverFunctionsManager == null)
        {
            return;
        }

        serverFunctionsManager.CancelMatchmaking(
            PlayerId,
            roomCode,
            result =>
            {
                if (requestVersion != matchmakingRequestVersion)
                {
                    return;
                }

                if (result == null || result.FunctionResult == null)
                {
                    Debug.LogError(
                        "[Lobby] Cancel matchmaking after join response failed; keeping local queue state"
                    );
                    ResetMatchmakingUi();
                    return;
                }

                PlayerPrefs.DeleteKey("RoomCode");
                PlayerPrefs.DeleteKey("IsWaitingForOpponent");
                ResetMatchmakingUi();
            }
        );
    }

    private void ResetMatchmakingUi(bool keepCancelRequested = false)
    {
        SceneLoadingOverlay.Hide();
        SceneLoadingOverlay.SetMessage("WAIT!!!");
        matchmakingState = MatchmakingState.Idle;
        cancelRequested = keepCancelRequested;
        SetJoinButtonText("CONNECT");
    }

    /// <summary>
    /// Caka kym sa nepripoji druhy hrac, potom spusti battle
    /// </summary>
    private IEnumerator WaitForOpponentAndStartBattle(int requestVersion)
    {
        string roomCode = PlayerPrefs.GetString("RoomCode", "");
        int pollAttempts = 0;
        const int MAX_POLL_ATTEMPTS = 60; // 60 sekund timeout

        while (pollAttempts < MAX_POLL_ATTEMPTS)
        {
            yield return new WaitForSeconds(1f);
            pollAttempts++;

            bool isCompleted = false;
            bool bothPlayersReady = false;

            serverFunctionsManager.GetMatchState(
                roomCode,
                PlayerId,
                result =>
                {
                    if (result?.FunctionResult != null)
                    {
                        try
                        {
                            var resultData = JObject.Parse(result.FunctionResult.ToString());
                            if (resultData["matchState"]?["playersCount"] != null)
                            {
                                int playersCount = resultData["matchState"]
                                    ["playersCount"]
                                    .Value<int>();
                                bothPlayersReady = (playersCount >= 2);

                                Debug.Log($"[Lobby] Polling: {playersCount}/2 players in room");
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"[Lobby] Error parsing GetMatchState: {e.Message}");
                        }
                    }
                    isCompleted = true;
                }
            );

            yield return new WaitUntil(() => isCompleted);

            if (requestVersion != matchmakingRequestVersion || cancelRequested)
            {
                yield break;
            }

            if (bothPlayersReady)
            {
                Debug.Log("[Lobby] Both players connected - starting battle!");
                yield return StartCoroutine(StartBattleWithDelay(requestVersion));
                break;
            }

            SceneLoadingOverlay.SetMessage("SEARCHING FOR PREY...");
        }

        if (requestVersion != matchmakingRequestVersion || cancelRequested)
        {
            yield break;
        }

        if (pollAttempts >= MAX_POLL_ATTEMPTS)
        {
            ResetMatchmakingUi();
            Debug.LogError("[Lobby] Timeout waiting for opponent");
            // Mozno by sme mohli vratit hraca spat alebo restart lobby
        }
    }

    /// <summary>
    /// Spusti battle scene s kratkym delay pre UI feedback
    /// </summary>
    private IEnumerator StartBattleWithDelay(int requestVersion)
    {
        if (requestVersion != matchmakingRequestVersion || cancelRequested)
        {
            yield break;
        }

        Debug.Log("[Lobby] StartBattleWithDelay() called");
        SceneLoadingOverlay.SetMessage("CONNECTING...");
        yield return new WaitForSeconds(1f); // UI feedback delay

        if (requestVersion != matchmakingRequestVersion || cancelRequested)
        {
            yield break;
        }

        Debug.Log("[Lobby] Loading Multiplayer battle scene...");
        SceneManager.LoadScene("Multiplayer");
    }

    // === CLEANUP METODY ===

    void CleanupOldRooms()
    {
        if (serverFunctionsManager != null)
        {
            serverFunctionsManager.CleanupRooms(result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    Debug.Log("Cleanup completed: " + result.FunctionResult);
                }
            });
        }
    }

    void LeaveAnyExistingRoom()
    {
        // Ak ma hrac ulozeny roomCode, opusti tuto miestnost
        string existingRoomCode = PlayerPrefs.GetString("RoomCode", "");
        string isWaiting = PlayerPrefs.GetString("IsWaitingForOpponent", "false");
        if (string.IsNullOrEmpty(existingRoomCode))
        {
            Debug.Log("[Lobby] LeaveAnyExistingRoom skipped: no local RoomCode");
            return;
        }

        if (serverFunctionsManager == null)
        {
            Debug.LogError(
                "[Lobby] LeaveAnyExistingRoom cannot run: serverFunctionsManager is null"
            );
            return;
        }

        Debug.Log(
            $"[Lobby] LeaveAnyExistingRoom found local room state: roomCode={existingRoomCode}, isWaiting={isWaiting}"
        );
        if (isWaiting != "true")
        {
            Debug.LogWarning(
                "[Lobby] Clearing stale local room state without leaveRoom because player is not marked as waiting"
            );
            PlayerPrefs.DeleteKey("RoomCode");
            PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            return;
        }

        serverFunctionsManager.LeaveRoom(
            PlayerId,
            result =>
            {
                if (result == null || result.FunctionResult == null)
                {
                    Debug.LogError(
                        $"[Lobby] LeaveAnyExistingRoom failed for room {existingRoomCode}; keeping local state"
                    );
                    return;
                }

                Debug.Log(
                    $"[Lobby] LeaveAnyExistingRoom succeeded for room {existingRoomCode}: {result.FunctionResult}"
                );
                PlayerPrefs.DeleteKey("RoomCode");
                PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            }
        );
    }
}
