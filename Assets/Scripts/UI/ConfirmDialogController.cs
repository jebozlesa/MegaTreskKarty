using System;
using TMPro;
using UnityEngine;

public sealed class ConfirmDialogController : MonoBehaviour
{
    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private string defaultMessage = "ARE YOU SURE?";

    private Action onConfirm;
    private Action onCancel;

    public void Show(string message, Action confirmAction, Action cancelAction = null)
    {
        if (confirmAction == null)
        {
            Debug.LogError("[ConfirmDialogController] Cannot show confirm dialog without confirm action.");
            return;
        }

        onConfirm = confirmAction;
        onCancel = cancelAction;

        if (messageText != null)
        {
            messageText.text = string.IsNullOrWhiteSpace(message) ? defaultMessage : message;
        }

        SetVisible(true);
    }

    public void Confirm()
    {
        Action callback = onConfirm;
        Hide();
        callback?.Invoke();
    }

    public void Cancel()
    {
        Action callback = onCancel;
        Hide();
        callback?.Invoke();
    }

    public void Hide()
    {
        onConfirm = null;
        onCancel = null;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        GameObject target = panelRoot != null ? panelRoot : gameObject;
        if (target != null)
        {
            if (visible)
            {
                NormalizeOverlayRoot(target);
            }

            target.SetActive(visible);
        }
    }

    private static void NormalizeOverlayRoot(GameObject target)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>(true);
        if (canvas != null && target.transform.parent != canvas.transform)
        {
            target.transform.SetParent(canvas.transform, false);
        }

        if (target.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        target.transform.SetAsLastSibling();
    }
}
