using UnityEngine;
using UnityEngine.EventSystems;

public class MultiplayerCardDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalLocalPosition;
    private FightSystemMultiplayer fightSystem;
    private Kard kard;
    private Canvas canvas;

    public void Initialize(FightSystemMultiplayer fightSystem)
    {
        this.fightSystem = fightSystem;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        kard = GetComponent<Kard>();
        canvas = GetComponentInParent<Canvas>();

        if (kard == null)
        {
            Debug.LogError("[MultiplayerCardDrag] Kard component not found on card prefab.");
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

        if (fightSystem != null && kard != null)
        {
            fightSystem.OnCardDropped(kard, this);
        }
        else
        {
            Debug.LogError("[MultiplayerCardDrag] Missing fight system or card reference when ending drag.");
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
        if (fightSystem == null || kard == null)
        {
            return false;
        }

        return (fightSystem.state == FightStateMultiplayer.PLAYERDEATH || fightSystem.state == FightStateMultiplayer.START) && kard.isDragable;
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
            Debug.LogWarning("[MultiplayerCardDrag] Missing collider references for drag detection.");
            return false;
        }

        return cardCollider.bounds.Intersects(battleAreaCollider.bounds);
    }
}
