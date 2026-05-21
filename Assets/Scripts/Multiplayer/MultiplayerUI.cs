using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerUI : MonoBehaviour
{
    // [OK] Konstanty pre dialog messages - centralne miesto pre vsetky UI texty
    public const string MSG_CHOOSE_ATTACK = "Choose your attack!";
    public const string MSG_WAITING_OPPONENT = "Waiting for opponent...";
    public const string MSG_LOADING = "Loading...";
    public const string MSG_CHOOSE_FIGHTER = "Choose fighter!";
    
    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text enemyNameText;
    public TMP_Text statusText;
    public Button exitButton;

    // Verejne metody pre volanie z FightSystemMultiplayer
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
            Debug.Log("[MultiplayerUI] Exit button listener registered");
        }
        else
        {
            Debug.LogError("[MultiplayerUI] Exit button is not assigned");
        }
    }

    // Pridat dalsie metody podla potreby (ShowCards, ShowVictory, ShowDefeat, ...)
}
