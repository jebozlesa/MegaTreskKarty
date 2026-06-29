using TMPro;
using UnityEngine;

public sealed class LoadingOverlayView : MonoBehaviour
{
    public static LoadingOverlayView Current { get; private set; }

    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}
