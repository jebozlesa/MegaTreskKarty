using UnityEngine;
using UnityEngine.EventSystems;

public class RoyalRumbleCardDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalLocalPosition;
    private RoyalRumbleBattleCoordinator coordinator;
    private RoyalRumbleShellController shellController;
    private Kard kard;
    private Canvas canvas;

    public void Initialize(RoyalRumbleBattleCoordinator owner)
    {
        coordinator = owner;
        shellController = null;
        InitializeShared();
    }

    public void Initialize(RoyalRumbleShellController owner)
    {
        shellController = owner;
        coordinator = null;
        InitializeShared();
    }

    private void InitializeShared()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        kard = GetComponent<Kard>();
        canvas = GetComponentInParent<Canvas>();

        if (kard == null)
        {
            Debug.LogError("[RoyalRumbleCardDrag] Kard component not found on card prefab.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        originalPosition = rectTransform.position;
        originalLocalPosition = rectTransform.localPosition;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }

        Vector3 delta = eventData.delta;
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            delta /= canvas.scaleFactor;
        }

        rectTransform.position += delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (!CanDrag())
        {
            ResetToOriginalPosition();
            return;
        }

        if (!IsOverBattleArea())
        {
            ResetToOriginalPosition();
            return;
        }

        if (coordinator != null && kard != null)
        {
            coordinator.OnCardDropped(kard, this);
        }
        else if (shellController != null && kard != null)
        {
            shellController.OnCardDropped(kard, this);
        }
        else
        {
            Debug.LogWarning("[RoyalRumbleCardDrag] Missing RR owner or card reference when ending drag.");
            ResetToOriginalPosition();
        }
    }

    public void ResetToOriginalPosition()
    {
        rectTransform.position = originalPosition;
        rectTransform.localPosition = originalLocalPosition;
    }

    public Vector3 GetOriginalLocalPosition()
    {
        return originalLocalPosition;
    }

    private bool CanDrag()
    {
        if (kard == null)
        {
            return false;
        }

        if (coordinator != null)
        {
            return coordinator.CanDragCard(kard) && kard.isDragable;
        }

        if (shellController != null)
        {
            return shellController.CanDragCard(kard) && kard.isDragable;
        }

        return false;
    }

    private bool IsOverBattleArea()
    {
        if (kard == null || kard.battleArea == null)
        {
            return false;
        }

        Collider2D cardCollider = GetComponent<Collider2D>();
        Collider2D battleAreaCollider = kard.battleArea.GetComponent<Collider2D>();

        if (cardCollider == null || battleAreaCollider == null)
        {
            Debug.LogWarning("[RoyalRumbleCardDrag] Missing collider references for drag detection.");
            return false;
        }

        return cardCollider.bounds.Intersects(battleAreaCollider.bounds);
    }
}
