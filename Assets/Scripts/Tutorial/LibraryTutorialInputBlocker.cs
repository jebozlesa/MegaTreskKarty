using UnityEngine;
using UnityEngine.UI;

public sealed class LibraryTutorialInputBlocker : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform AllowedTarget { get; set; }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return AllowedTarget == null
            || !RectTransformUtility.RectangleContainsScreenPoint(AllowedTarget, screenPoint, eventCamera);
    }
}
