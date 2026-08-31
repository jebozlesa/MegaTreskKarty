using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class LoadingOverlayView : MonoBehaviour
{
    public static LoadingOverlayView Current { get; private set; }

    [SerializeField]
    private TMP_Text messageText;

    private CanvasGroup canvasGroup;
    private LoadingCharacterRandomDance[] characterDances;

    private void Awake()
    {
        Current = this;
        EnsureCachedReferences();
        ApplyVisibility(false);
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

    public void SetVisible(bool isVisible)
    {
        if (isVisible && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        EnsureCachedReferences();
        if (isVisible)
        {
            NormalizeOverlayRoot();
        }

        ApplyVisibility(isVisible);

        if (isVisible)
        {
            StartCharacterDances();
        }
        else
        {
            StopCharacterDances();
        }
    }

    private void EnsureCachedReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (characterDances == null)
        {
            characterDances = GetComponentsInChildren<LoadingCharacterRandomDance>(true);
        }
    }

    private void ApplyVisibility(bool isVisible)
    {
        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.blocksRaycasts = isVisible;
        canvasGroup.interactable = false;
    }

    private void NormalizeOverlayRoot()
    {
        Canvas canvas = GetComponentInParent<Canvas>(true);
        if (canvas != null && transform.parent != canvas.transform)
        {
            transform.SetParent(canvas.transform, false);
        }

        if (transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        transform.SetAsLastSibling();
    }

    private void StartCharacterDances()
    {
        for (int i = 0; i < characterDances.Length; i++)
        {
            if (characterDances[i] != null)
            {
                characterDances[i].StartDance();
            }
        }
    }

    private void StopCharacterDances()
    {
        for (int i = 0; i < characterDances.Length; i++)
        {
            if (characterDances[i] != null)
            {
                characterDances[i].StopDance();
            }
        }
    }
}
