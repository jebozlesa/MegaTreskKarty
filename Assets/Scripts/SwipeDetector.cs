using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeDown;

    private Vector2 startTouchPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - startTouchPosition;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal swipe
            if (direction.x > 0)
            {
                OnSwipeRight.Invoke();
            }
            else
            {
                OnSwipeLeft.Invoke();
            }
        }
        else
        {
            // Vertical swipe
            if (direction.y > 0)
            {
                OnSwipeUp.Invoke();
            }
            else
            {
                OnSwipeDown.Invoke();
            }
        }
    }
}
