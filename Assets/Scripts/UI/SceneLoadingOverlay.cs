using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoadingOverlay
{
    private static bool warnedMissingOverlay;

    public static void Show()
    {
        SetVisible(true);
    }

    public static void Hide()
    {
        SetVisible(false);
    }

    public static void SetMessage(string message)
    {
        LoadingOverlayView overlay = FindOverlayViewInActiveScene();
        if (overlay == null)
        {
            return;
        }

        overlay.SetMessage(message);
    }

    private static void SetVisible(bool isVisible)
    {
        LoadingOverlayView overlay = FindOverlayViewInActiveScene();
        if (overlay == null)
        {
            if (isVisible && !warnedMissingOverlay)
            {
                Debug.LogWarning("[SceneLoadingOverlay] Missing LoadingOverlayView in the active scene.");
                warnedMissingOverlay = true;
            }

            return;
        }

        warnedMissingOverlay = false;
        overlay.gameObject.SetActive(isVisible);
    }

    private static LoadingOverlayView FindOverlayViewInActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            return null;
        }

        LoadingOverlayView view = LoadingOverlayView.Current;
        if (view != null && view.gameObject != null && view.gameObject.scene == activeScene)
        {
            return view;
        }

        LoadingOverlayView[] views = Object.FindObjectsByType<LoadingOverlayView>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        for (int i = 0; i < views.Length; i++)
        {
            LoadingOverlayView candidate = views[i];
            if (candidate != null && candidate.gameObject != null && candidate.gameObject.scene == activeScene)
            {
                return candidate;
            }
        }

        return null;
    }
}
