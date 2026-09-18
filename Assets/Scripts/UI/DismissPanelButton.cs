using UnityEngine;
using UnityEngine.UI;

public sealed class DismissPanelButton : MonoBehaviour
{
    private GameObject panel;

    public void Configure(GameObject panelToDismiss)
    {
        panel = panelToDismiss;
        CustomButton custom = GetComponentInChildren<CustomButton>(true);
        Button standard = GetComponentInChildren<Button>(true);
        if (custom != null)
        {
            custom.onDelayedClick.RemoveListener(Dismiss);
            custom.onDelayedClick.AddListener(Dismiss);
        }
        else if (standard != null)
        {
            standard.onClick.RemoveListener(Dismiss);
            standard.onClick.AddListener(Dismiss);
        }
    }

    public void Dismiss()
    {
        if (panel != null) panel.SetActive(false);
    }
}
