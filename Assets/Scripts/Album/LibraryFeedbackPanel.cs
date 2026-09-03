using TMPro;
using UnityEngine;

public class LibraryFeedbackPanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;
    public string blockedRecycleMessage = "Card is used in a deck.";

    private void Awake()
    {
        Hide();
    }

    public void ShowBlockedRecycle()
    {
        Show(blockedRecycleMessage);
    }

    public void Show(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
