using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialTargetSpotlight
{
    private RectTransform target;
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private Outline outline;
    private bool canvasAdded;
    private bool raycasterAdded;
    private bool outlineAdded;
    private bool originalOutlineEnabled;
    private bool originalOverrideSorting;
    private int originalSortingLayerId;
    private int originalSortingOrder;

    public void Show(RectTransform nextTarget, Canvas tutorialCanvas)
    {
        if (nextTarget == target && canvas != null)
        {
            return;
        }

        Clear();
        if (nextTarget == null)
        {
            return;
        }

        target = nextTarget;
        canvas = target.GetComponent<Canvas>();
        canvasAdded = canvas == null;
        if (canvasAdded)
        {
            canvas = target.gameObject.AddComponent<Canvas>();
        }

        raycaster = target.GetComponent<GraphicRaycaster>();
        raycasterAdded = raycaster == null;
        if (raycasterAdded)
        {
            raycaster = target.gameObject.AddComponent<GraphicRaycaster>();
        }

        originalOverrideSorting = canvas.overrideSorting;
        originalSortingLayerId = canvas.sortingLayerID;
        originalSortingOrder = canvas.sortingOrder;
        canvas.overrideSorting = true;
        if (tutorialCanvas != null)
        {
            canvas.sortingLayerID = tutorialCanvas.sortingLayerID;
            canvas.sortingOrder = tutorialCanvas.sortingOrder + 1;
        }
        else
        {
            Canvas parentCanvas = target.parent == null
                ? null
                : target.parent.GetComponentInParent<Canvas>();
            canvas.sortingOrder = (parentCanvas?.sortingOrder ?? 0) + 1;
        }

        outline = target.GetComponent<Outline>();
        outlineAdded = outline == null;
        if (outlineAdded)
        {
            outline = target.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.yellow;
            outline.effectDistance = new Vector2(4f, -4f);
        }
        originalOutlineEnabled = outline.enabled;
        outline.enabled = true;
    }

    public void Clear()
    {
        if (outline != null)
        {
            if (outlineAdded)
            {
                outline.enabled = false;
                DestroyComponent(outline);
            }
            else
            {
                outline.enabled = originalOutlineEnabled;
            }
        }

        if (raycaster != null && raycasterAdded)
        {
            raycaster.enabled = false;
            DestroyComponent(raycaster);
        }

        if (canvas != null)
        {
            if (canvasAdded)
            {
                canvas.enabled = false;
                DestroyComponent(canvas);
            }
            else
            {
                canvas.overrideSorting = originalOverrideSorting;
                canvas.sortingLayerID = originalSortingLayerId;
                canvas.sortingOrder = originalSortingOrder;
            }
        }

        target = null;
        canvas = null;
        raycaster = null;
        outline = null;
        canvasAdded = false;
        raycasterAdded = false;
        outlineAdded = false;
    }

    private static void DestroyComponent(Object component)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(component);
        }
        else
        {
            Object.DestroyImmediate(component);
        }
    }
}
