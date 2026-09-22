using UnityEngine;
using UnityEngine.UI;

public sealed class LibraryTutorialInputBlocker : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform AllowedTarget { get; set; }
    public RectTransform SecondaryAllowedTarget { get; set; }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return !ContainsScreenPoint(AllowedTarget, screenPoint, eventCamera)
            && !ContainsScreenPoint(SecondaryAllowedTarget, screenPoint, eventCamera);
    }

    private static bool ContainsScreenPoint(
        RectTransform target,
        Vector2 screenPoint,
        Camera eventCamera)
    {
        return target != null
            && RectTransformUtility.RectangleContainsScreenPoint(target, screenPoint, eventCamera);
    }
}
