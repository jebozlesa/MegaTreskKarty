using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerLobbyUI : MonoBehaviour
{
    public Button joinButton;
    public TMP_Text statusText;
    public ServerFunctionsManager serverFunctionsManager;

    public string roomCode = "123456"; // môžeš generovať náhodne
    public string playerId; // automaticky nastavené podľa PlayFabId

    void Start()
    {
        joinButton.onClick.AddListener(OnJoinClicked);
        statusText.text = "";
        playerId = PlayFabManager.CurrentPlayFabId;
    }

    void OnJoinClicked()
    {
        statusText.text = "CONNECTING...";
        serverFunctionsManager.JoinOrCreateRoom(roomCode, playerId, result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                statusText.text = "CONNECTED!";
                // Tu môžeš spracovať ďalšie dáta z result.FunctionResult
            }
            else
            {
                statusText.text = "FCKING SHIT!";
            }
        });
    }
}
