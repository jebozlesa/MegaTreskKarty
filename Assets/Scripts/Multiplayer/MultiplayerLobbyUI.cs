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
                        StartCoroutine(ShowWaitingAndSwitchScene());
                    }
                    else
                    {
                        statusText.text = "CONNECTED!";
                        SceneManager.LoadScene("Multiplayer");
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

    private IEnumerator ShowWaitingAndSwitchScene()
    {
        statusText.text = "Waiting for opponent...";
        yield return new WaitForSeconds(1f);
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
