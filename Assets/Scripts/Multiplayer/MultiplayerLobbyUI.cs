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
    public TMP_Text statusText;
    public ServerFunctionsManager serverFunctionsManager;

    private string PlayerId => PlayFabManagerLogin.Instance.LoggedInPlayerId;

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
        statusText.text = "";
        
        // Pri vstupe do lobby vycistime stare miestnosti
        CleanupOldRooms();
        
        // Ak sa vraciame z multiplayeru, opustime staru miestnost
        LeaveAnyExistingRoom();
    }

    void OnJoinClicked()
    {
        statusText.text = "CONNECTING...";
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
                        // [OK] Spusti polling pre druheho hraca
                        StartCoroutine(WaitForOpponentAndStartBattle());
                    }
                    else
                    {
                        statusText.text = "Both players ready!";
                        // [OK] Immediate transition - druhy hrac sa pripojil
                        StartCoroutine(StartBattleWithDelay());
                    }
                }
                else
                {
                    statusText.text = "Failed!";
                }
            }
            else
            {
                statusText.text = "Failed!";
            }
        });
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
            
            // Check room players count
            serverFunctionsManager.GetRoomPlayersInfo(roomCode, result =>
            {
                if (result?.FunctionResult != null)
                {
                    try
                    {
                        var resultData = JObject.Parse(result.FunctionResult.ToString());
                        if (resultData["room"]?["playersCount"] != null)
                        {
                            int playersCount = resultData["room"]["playersCount"].Value<int>();
                            bothPlayersReady = (playersCount >= 2);
                            
                            Debug.Log($"[Lobby] Polling: {playersCount}/2 players in room");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[Lobby] Error parsing GetRoomPlayersInfo: {e.Message}");
                    }
                }
                isCompleted = true;
            });
            
            yield return new WaitUntil(() => isCompleted);
            
            if (bothPlayersReady)
            {
                statusText.text = "Both players ready!";
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
        if (!string.IsNullOrEmpty(existingRoomCode) && serverFunctionsManager != null)
        {
            serverFunctionsManager.LeaveRoom(PlayerId, result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    Debug.Log("Left existing room: " + existingRoomCode);
                }
                
                // Vycisti lokalne udaje
                PlayerPrefs.DeleteKey("RoomCode");
                PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            });
        }
    }
}
