using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Helper script pre testovanie multiplayer funkcionalite
/// Pridajte ho do niektorej sceny a pouzite na debugovanie
/// </summary>
public class MultiplayerTestHelper : MonoBehaviour
{
    public ServerFunctionsManager serverFunctionsManager;
    
    [Header("Test Controls")]
    public bool testGetRoomInfo = false;
    public bool testUpdatePlayerInfo = false;
    public string testRoomCode = "";
    public string testUsername = "";

    void Update()
    {
        if (testGetRoomInfo && !string.IsNullOrEmpty(testRoomCode))
        {
            testGetRoomInfo = false;
            TestGetRoomPlayersInfo();
        }
        
        if (testUpdatePlayerInfo && !string.IsNullOrEmpty(testUsername))
        {
            testUpdatePlayerInfo = false;
            TestUpdatePlayerInfo();
        }
    }

    void TestGetRoomPlayersInfo()
    {
        Debug.Log($"Testing GetRoomPlayersInfo with roomCode: {testRoomCode}");
        serverFunctionsManager.GetRoomPlayersInfo(testRoomCode, result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                Debug.Log($"GetRoomPlayersInfo result: {functionResult}");
                
                if (functionResult["success"].Value<bool>())
                {
                    JArray playersInfo = functionResult["playersInfo"] as JArray;
                    Debug.Log($"Found {playersInfo.Count} players in room:");
                    foreach (JObject player in playersInfo)
                    {
                        Debug.Log($"  - Player ID: {player["playerId"]}, Username: {player["username"]}");
                    }
                }
                else
                {
                    Debug.LogError($"GetRoomPlayersInfo failed: {functionResult["message"]}");
                }
            }
            else
            {
                Debug.LogError("GetRoomPlayersInfo: No result received");
            }
        });
    }

    void TestUpdatePlayerInfo()
    {
        string playerId = PlayFabManagerLogin.Instance.LoggedInPlayerId;
        Debug.Log($"Testing UpdatePlayerInfo with playerId: {playerId}, username: {testUsername}");
        
        serverFunctionsManager.UpdatePlayerInfo(playerId, testUsername, result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                Debug.Log($"UpdatePlayerInfo result: {functionResult}");
                
                if (functionResult["success"].Value<bool>())
                {
                    Debug.Log("Player info updated successfully!");
                    // Update local username
                    PlayerPrefs.SetString("username", testUsername);
                }
                else
                {
                    Debug.LogError($"UpdatePlayerInfo failed: {functionResult["message"]}");
                }
            }
            else
            {
                Debug.LogError("UpdatePlayerInfo: No result received");
            }
        });
    }

    [ContextMenu("Log Current Player Info")]
    void LogCurrentPlayerInfo()
    {
        string playerId = PlayFabManagerLogin.Instance.LoggedInPlayerId;
        string username = PlayerPrefs.GetString("username", "Not set");
        string roomCode = PlayerPrefs.GetString("RoomCode", "Not set");
        string isWaiting = PlayerPrefs.GetString("IsWaitingForOpponent", "false");
        
        Debug.Log("=== CURRENT PLAYER INFO ===");
        Debug.Log($"Player ID: {playerId}");
        Debug.Log($"Username: {username}");
        Debug.Log($"Room Code: {roomCode}");
        Debug.Log($"Is Waiting for Opponent: {isWaiting}");
        Debug.Log("===========================");
    }
}