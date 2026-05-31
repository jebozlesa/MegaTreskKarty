using UnityEngine;
using UnityEngine.EventSystems;

public class CampaignOnlineSelectableCard : MonoBehaviour, IPointerClickHandler
{
    private CampaignOnlineShellController controller;
    private Kard card;

    public void Initialize(CampaignOnlineShellController controller, Kard card)
    {
        this.controller = controller;
        this.card = card;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller == null)
        {
            Debug.LogWarning("[CampaignOnlineSelectableCard] Click ignored because Campaign controller is missing.");
            return;
        }

        controller.ShowDragSelectionHint(card);
    }
}
