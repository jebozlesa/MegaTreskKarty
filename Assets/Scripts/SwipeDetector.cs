using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float minimumSwipeDistance = 80f;
    public UnityEvent OnSwipeLeft = new UnityEvent();
    public UnityEvent OnSwipeRight = new UnityEvent();
    public UnityEvent OnSwipeUp = new UnityEvent();
    public UnityEvent OnSwipeDown = new UnityEvent();

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
        float horizontalDistance = Mathf.Abs(direction.x);
        float verticalDistance = Mathf.Abs(direction.y);

        if (Mathf.Max(horizontalDistance, verticalDistance) < minimumSwipeDistance)
        {
            return;
        }

        if (horizontalDistance > verticalDistance)
        {
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
