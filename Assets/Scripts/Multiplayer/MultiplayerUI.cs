using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text enemyNameText;
    public TMP_Text statusText;
    public Button exitButton;

    // Verejné metódy pre volanie z FightSystemMultiplayer
    public void UpdatePlayerInfo(string playerName, string enemyName)
    {
        playerNameText.text = playerName;
        enemyNameText.text = enemyName;
    }

    public void ShowStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    public void SetExitButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(action);
        }
    }

    // Pridať ďalšie metódy podľa potreby (ShowCards, ShowVictory, ShowDefeat, ...)
}
