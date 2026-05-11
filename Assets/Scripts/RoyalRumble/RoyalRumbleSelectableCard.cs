using UnityEngine;
using UnityEngine.EventSystems;

public class RoyalRumbleSelectableCard : MonoBehaviour, IPointerClickHandler
{
    private RoyalRumbleBattleCoordinator coordinator;
    private RoyalRumbleShellController shellController;

    public void Initialize(RoyalRumbleBattleCoordinator owner, Kard targetCard)
    {
        coordinator = owner;
        shellController = null;
    }

    public void Initialize(RoyalRumbleShellController owner, Kard targetCard)
    {
        shellController = owner;
        coordinator = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (coordinator == null && shellController == null)
        {
            Debug.LogWarning("[RoyalRumbleSelectableCard] Click ignored because RR owner is missing.");
            return;
        }

        if (coordinator != null)
        {
            coordinator.ShowDragSelectionHint();
        }
        else
        {
            shellController.ShowDragSelectionHint();
        }
    }
}
