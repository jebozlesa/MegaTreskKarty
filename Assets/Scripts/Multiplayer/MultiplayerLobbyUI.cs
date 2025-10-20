using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class MultiplayerLobbyUI : MonoBehaviour
{
    public Button joinButton;
    public TMP_Text statusText;
    public ServerFunctionsManager serverFunctionsManager;

    private string PlayerId => PlayFabManagerLogin.Instance.LoggedInPlayerId;

    void Start()
    {
        joinButton.onClick.AddListener(OnJoinClicked);
        statusText.text = "";
        
        // Pri vstupe do lobby vyčistime staré miestnosti
        CleanupOldRooms();
        
        // Ak sa vracíame z multiplayeru, opustime starú miestnosť
        LeaveAnyExistingRoom();
    }

    void OnJoinClicked()
    {
        statusText.text = "CONNECTING...";
        string username = PlayerPrefs.GetString("username", PlayerId);
        serverFunctionsManager.JoinOrCreateRoom(PlayerId, username, result =>
        {
            Debug.LogWarning($"FunctionResult raw: {Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            if (result != null && result.FunctionResult != null)
            {
                JObject functionResult = null;
                try {
                    functionResult = JObject.Parse(result.FunctionResult.ToString());
                } catch {
                    Debug.LogError("Failed to parse FunctionResult to JObject");
                }
                Debug.LogWarning($"FunctionResult JObject: {functionResult}");
                if (functionResult != null && functionResult["room"] != null)
                {
                    var room = functionResult["room"];
                    Debug.LogWarning($"Room object: {room}");
                    string myId = PlayerId;
                    
                    // Uloženie informácií o miestnosti
                    PlayerPrefs.SetString("RoomCode", room["roomCode"].ToString());
                    
                    // Kontrola počtu hráčov na určenie správania
                    var playersArray = room["players"] as JArray;
                    bool isWaiting = playersArray != null && playersArray.Count == 1;
                    
                    // Uloženie stavu čakania
                    PlayerPrefs.SetString("IsWaitingForOpponent", isWaiting ? "true" : "false");
                    
                    if (isWaiting)
                    {
                        statusText.text = "Waiting for opponent...";
                        // ✅ Spusti polling pre druhého hráča
                        StartCoroutine(WaitForOpponentAndStartBattle());
                    }
                    else
                    {
                        statusText.text = "Both players ready!";
                        // ✅ Immediate transition - druhý hráč sa pripojil
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
    /// Čaká kým sa nepripojí druhý hráč, potom spustí battle
    /// </summary>
    private IEnumerator WaitForOpponentAndStartBattle()
    {
        string roomCode = PlayerPrefs.GetString("RoomCode", "");
        int pollAttempts = 0;
        const int MAX_POLL_ATTEMPTS = 60; // 60 sekúnd timeout
        
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
            // Možno by sme mohli vrátiť hráča späť alebo restart lobby
        }
    }
    
    /// <summary>
    /// Spustí battle scene s krátkym delay pre UI feedback
    /// </summary>
    private IEnumerator StartBattleWithDelay()
    {
        Debug.Log("[Lobby] StartBattleWithDelay() called");
        statusText.text = "Starting battle...";
        yield return new WaitForSeconds(1f); // UI feedback delay
        
        Debug.Log("[Lobby] Loading Multiplayer battle scene...");
        SceneManager.LoadScene("Multiplayer");
    }

    // === CLEANUP METÓDY ===
    
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
        // Ak má hráč uložený roomCode, opusti túto miestnosť
        string existingRoomCode = PlayerPrefs.GetString("RoomCode", "");
        if (!string.IsNullOrEmpty(existingRoomCode) && serverFunctionsManager != null)
        {
            serverFunctionsManager.LeaveRoom(PlayerId, result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    Debug.Log("Left existing room: " + existingRoomCode);
                }
                
                // Vyčisti lokálne údaje
                PlayerPrefs.DeleteKey("RoomCode");
                PlayerPrefs.DeleteKey("IsWaitingForOpponent");
            });
        }
    }
}
