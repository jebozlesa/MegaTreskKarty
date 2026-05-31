using UnityEngine;
using UnityEngine.EventSystems;

public class CampaignOnlineCardDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalLocalPosition;
    private CampaignOnlineShellController controller;
    private Kard kard;
    private Canvas canvas;

    public void Initialize(CampaignOnlineShellController controller, Kard card)
    {
        this.controller = controller;
        kard = card != null ? card : GetComponent<Kard>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (kard == null)
        {
            Debug.LogError("[CampaignOnlineCardDrag] Kard component not found on card prefab.");
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

        if (!CanDrag() || !IsOverBattleArea())
        {
            ResetToOriginalPosition();
            return;
        }

        if (controller != null && kard != null)
        {
            controller.OnCardDropped(kard, this);
            return;
        }

        Debug.LogWarning("[CampaignOnlineCardDrag] Missing Campaign controller or card reference when ending drag.");
        ResetToOriginalPosition();
    }

    public Vector3 GetOriginalLocalPosition()
    {
        return originalLocalPosition;
    }

    public void ResetToOriginalPosition()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.position = originalPosition;
        rectTransform.localPosition = originalLocalPosition;
    }

    private bool CanDrag()
    {
        return kard != null
            && controller != null
            && kard.isDragable
            && controller.CanDragCard(kard);
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
            Debug.LogWarning("[CampaignOnlineCardDrag] Missing collider references for drag detection.");
            return false;
        }

        return cardCollider.bounds.Intersects(battleAreaCollider.bounds);
    }
}
