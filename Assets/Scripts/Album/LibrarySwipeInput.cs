using UnityEngine;
using UnityEngine.EventSystems;

public class LibrarySwipeInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum SwipeTarget
    {
        Context,
        Deck,
    }

    public LibraryDeckController libraryDeckController;
    public SwipeTarget target = SwipeTarget.Context;
    public float swipeThreshold = 80f;

    private Vector2 pointerDownPosition;
    private bool hasPointerDownPosition;
    private Camera pointerDownCamera;

    public void OnPointerDown(PointerEventData eventData)
    {
        CapturePointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TryHandlePointerUp(eventData);
    }

    public void CapturePointerDown(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        pointerDownCamera = eventData.pressEventCamera;
        CapturePointerDown(eventData.position);
    }

    public void CapturePointerDown(Vector2 position)
    {
        pointerDownPosition = position;
        hasPointerDownPosition = true;
    }

    public bool TryHandlePointerUp(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return false;
        }

        return TryHandlePointerUp(eventData.position);
    }

    public bool TryHandlePointerUp(Vector2 pointerUpPosition)
    {
        if (libraryDeckController == null)
        {
            Debug.LogError("[LibrarySwipeInput] libraryDeckController is not assigned");
            return false;
        }

        if (!hasPointerDownPosition)
        {
            return false;
        }

        hasPointerDownPosition = false;
        Vector2 delta = pointerUpPosition - pointerDownPosition;
        if (Mathf.Abs(delta.x) < swipeThreshold || Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
        {
            return false;
        }

        if (target == SwipeTarget.Context && Card.IsAnyCardDetailOpen)
        {
            Debug.LogWarning(
                $"[LibrarySwipeInput] Swipe ignored: object={name}, target={target}, reason=card_detail_open, down={pointerDownPosition}, up={pointerUpPosition}, delta={delta}"
            );
            return false;
        }

        if (target == SwipeTarget.Context && IsPointerDownInsideActiveDeckPanel())
        {
            Debug.LogWarning(
                $"[LibrarySwipeInput] Swipe ignored: object={name}, target={target}, reason=started_inside_deck_panel, down={pointerDownPosition}, up={pointerUpPosition}, delta={delta}"
            );
            return false;
        }

        bool next = delta.x < 0f;
        Debug.LogWarning(
            $"[LibrarySwipeInput] Swipe accepted: object={name}, target={target}, direction={(next ? "next" : "previous")}, down={pointerDownPosition}, up={pointerUpPosition}, delta={delta}, threshold={swipeThreshold}"
        );

        if (target == SwipeTarget.Context)
        {
            if (next) libraryDeckController.SelectNextContext();
            else libraryDeckController.SelectPreviousContext();
        }
        else
        {
            if (next) libraryDeckController.SelectNextDeck();
            else libraryDeckController.SelectPreviousDeck();
        }

        return true;
    }

    private bool IsPointerDownInsideActiveDeckPanel()
    {
        GameObject deckPanel = libraryDeckController?.deckManager?.deckPanel;
        if (deckPanel == null || !deckPanel.activeInHierarchy)
        {
            return false;
        }

        RectTransform deckRect = deckPanel.GetComponent<RectTransform>();
        if (deckRect == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(deckRect, pointerDownPosition, pointerDownCamera);
    }
}
