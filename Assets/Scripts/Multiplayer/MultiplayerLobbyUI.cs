using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class MultiplayerLobbyUI : MonoBehaviour
{
    private static bool VerboseLobbyLogs => false;

    public Button joinButton;
    public Button cancelButton;
    public TMP_Text statusText;
    public ServerFunctionsManager serverFunctionsManager;

    private string PlayerId => PlayFabManagerLogin.Instance.LoggedInPlayerId;
    private Coroutine waitForOpponentCoroutine;

    private static void LogVerbose(string message)
    {
        if (VerboseLobbyLogs)
        {
            Debug.Log(message);
        }
    }

    void Start()
    {
        joinButton.onClick.AddListener(OnJoinClicked);
        EnsureCancelButton();
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
            cancelButton.gameObject.SetActive(false);
        }
        statusText.text = "";
        
        // Pri vstupe do lobby vycistime stare miestnosti
        CleanupOldRooms();
        
        // Ak sa vraciame z multiplayeru, opustime staru miestnost
        LeaveAnyExistingRoom();
    }

    private void EnsureCancelButton()
    {
        if (cancelButton != null || joinButton == null)
        {
            return;
        }

        cancelButton = Instantiate(joinButton, joinButton.transform.parent);
        cancelButton.name = "CancelMatchmakingButton";
        cancelButton.onClick.RemoveAllListeners();

        var rectTransform = cancelButton.GetComponent<RectTransform>();
        var joinRectTransform = joinButton.GetComponent<RectTransform>();
        if (rectTransform != null && joinRectTransform != null)
        {
            rectTransform.anchoredPosition = joinRectTransform.anchoredPosition + new Vector2(0f, -90f);
        }

        var label = cancelButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = "CANCEL";
        }
    }

    void OnJoinClicked()
    {
        statusText.text = "CONNECTING...";
        if (joinButton != null) joinButton.interactable = false;
        string username = PlayerPrefs.GetString("username", PlayerId);
        serverFunctionsManager.JoinOrCreateRoom(PlayerId, username, result =>
        {
            LogVerbose($"FunctionResult raw: {Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            if (result != null && result.FunctionResult != null)
            {
                JObject functionResult = null;
                try {
                    functionResult = JObject.Parse(result.FunctionResult.ToString());
                } catch {
                    Debug.LogError("Failed to parse FunctionResult to JObject");
                }
                LogVerbose($"FunctionResult JObject: {functionResult}");
                if (functionResult != null && functionResult["room"] != null)
                {
                    var room = functionResult["room"];
                    LogVerbose($"Room object: {room}");
                    string myId = PlayerId;
                    
                    // Ulozenie informacii o miestnosti
                    PlayerPrefs.SetString("RoomCode", room["roomCode"].ToString());
                    
                    // Kontrola poctu hracov na urcenie spravania
                    var playersArray = room["players"] as JArray;
                    bool isWaiting = playersArray != null && playersArray.Count == 1;
                    
                    // Ulozenie stavu cakania
                    PlayerPrefs.SetString("IsWaitingForOpponent", isWaiting ? "true" : "false");
                    
                    if (isWaiting)
                    {
                        statusText.text = "Waiting for opponent...";
                        if (cancelButton != null) cancelButton.gameObject.SetActive(true);
                        waitForOpponentCoroutine = StartCoroutine(WaitForOpponentAndStartBattle());
                    }
                    else
                    {
                        statusText.text = "Both players ready!";
                        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
                        // [OK] Immediate transition - druhy hrac sa pripojil
                        StartCoroutine(StartBattleWithDelay());
                    }
                }
                else
                {
                    statusText.text = "Failed!";
                    if (joinButton != null) joinButton.interactable = true;
                }
            }
            else
            {
                statusText.text = "Failed!";
                if (joinButton != null) joinButton.interactable = true;
            }
        });
    }

    void OnCancelClicked()
    {
        string roomCode = PlayerPrefs.GetString("RoomCode", "");
        if (string.IsNullOrEmpty(roomCode) || serverFunctionsManager == null)
        {
            ResetMatchmakingUi("Cancelled");
            return;
        }

        statusText.text = "Cancelling...";
        if (cancelButton != null) cancelButton.interactable = false;

        serverFunctionsManager.CancelMatchmaking(PlayerId, roomCode, result =>
        {
            if (result == null)
            {
                Debug.LogError("[Lobby] Cancel matchmaking failed; keeping local queue state");
                statusText.text = "Cancel failed";
                if (cancelButton != null) cancelButton.interactable = true;
                return;
            }

            if (result.FunctionResult == null)
            {
                Debug.LogError("[Lobby] Cancel matchmaking returned empty FunctionResult; keeping local queue state");
                statusText.text = "Cancel failed";
                if (cancelButton != null) cancelButton.interactable = true;
                return;
            }

            bool cancelled = false;
            try
            {
                var functionResult = JObject.Parse(result.FunctionResult.ToString());
                cancelled = functionResult["success"]?.Value<bool>() == true;
                if (!cancelled)
                {
                    Debug.LogError($"[Lobby] Cancel matchmaking rejected: {functionResult["error"]}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Lobby] Failed to parse cancel matchmaking result: {e.Message}");
            }

            if (!cancelled)
            {
                statusText.text = "Cancel failed";
                if (cancelButton != null) cancelButton.interactable = true;
                return;
            }

            if (waitForOpponentCoroutine != null)
            {
                StopCoroutine(waitForOpponentCoroutine);
                waitForOpponentCoroutine = null;
            }

            PlayerPrefs.DeleteKey("RoomCode");
            PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            ResetMatchmakingUi("Cancelled");
        });
    }

    private void ResetMatchmakingUi(string message)
    {
        statusText.text = message;
        if (joinButton != null) joinButton.interactable = true;
        if (cancelButton != null)
        {
            cancelButton.interactable = true;
            cancelButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Caka kym sa nepripoji druhy hrac, potom spusti battle
    /// </summary>
    private IEnumerator WaitForOpponentAndStartBattle()
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
            
            serverFunctionsManager.GetMatchState(roomCode, PlayerId, result =>
            {
                if (result?.FunctionResult != null)
                {
                    try
                    {
                        var resultData = JObject.Parse(result.FunctionResult.ToString());
                        if (resultData["matchState"]?["playersCount"] != null)
                        {
                            int playersCount = resultData["matchState"]["playersCount"].Value<int>();
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
            });
            
            yield return new WaitUntil(() => isCompleted);
            
            if (bothPlayersReady)
            {
                statusText.text = "Both players ready!";
                if (cancelButton != null) cancelButton.gameObject.SetActive(false);
                Debug.Log("[Lobby] Both players connected - starting battle!");
                yield return StartCoroutine(StartBattleWithDelay());
                break;
            }
            
            // Update waiting message
            statusText.text = $"Waiting for opponent... ({pollAttempts}/60)";
        }
        
        if (pollAttempts >= MAX_POLL_ATTEMPTS)
        {
            statusText.text = "Timeout - opponent didn't join";
            if (joinButton != null) joinButton.interactable = true;
            if (cancelButton != null) cancelButton.gameObject.SetActive(false);
            Debug.LogError("[Lobby] Timeout waiting for opponent");
            // Mozno by sme mohli vratit hraca spat alebo restart lobby
        }
    }
    
    /// <summary>
    /// Spusti battle scene s kratkym delay pre UI feedback
    /// </summary>
    private IEnumerator StartBattleWithDelay()
    {
        Debug.Log("[Lobby] StartBattleWithDelay() called");
        statusText.text = "Starting battle...";
        yield return new WaitForSeconds(1f); // UI feedback delay
        
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
            Debug.LogError("[Lobby] LeaveAnyExistingRoom cannot run: serverFunctionsManager is null");
            return;
        }

        Debug.Log($"[Lobby] LeaveAnyExistingRoom found local room state: roomCode={existingRoomCode}, isWaiting={isWaiting}");
        if (isWaiting != "true")
        {
            Debug.LogWarning("[Lobby] Clearing stale local room state without leaveRoom because player is not marked as waiting");
            PlayerPrefs.DeleteKey("RoomCode");
            PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            return;
        }

        serverFunctionsManager.LeaveRoom(PlayerId, result =>
        {
            if (result == null || result.FunctionResult == null)
            {
                Debug.LogError($"[Lobby] LeaveAnyExistingRoom failed for room {existingRoomCode}; keeping local state");
                return;
            }

            Debug.Log($"[Lobby] LeaveAnyExistingRoom succeeded for room {existingRoomCode}: {result.FunctionResult}");
            PlayerPrefs.DeleteKey("RoomCode");
            PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        });
    }
}
