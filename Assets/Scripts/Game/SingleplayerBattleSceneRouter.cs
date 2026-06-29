using UnityEngine;

public class SingleplayerBattleSceneRouter : MonoBehaviour
{
    private const string DefaultCampaignId = "bushido";

    [Header("Online Modes")]
    [SerializeField] private RoyalRumbleShellController royalRumbleShell;
    [SerializeField] private GameObject campaignRoot;
    [SerializeField] private CampaignOnlineShellController campaignShell;

    private bool campaignModeActive;

    private void Awake()
    {
        RouteForCurrentParameters();
    }

    public void RouteForCurrentParameters()
    {
        campaignModeActive = GameParameters.MissionID > 0;
        if (!campaignModeActive)
        {
            if (campaignRoot != null)
            {
                campaignRoot.SetActive(false);
            }

            if (royalRumbleShell != null)
            {
                royalRumbleShell.enabled = true;
                royalRumbleShell.autoStart = true;
            }

            Debug.LogWarning("[SingleplayerBattleSceneRouter] Routed Game scene to Royal Rumble.");
            return;
        }

        if (royalRumbleShell != null)
        {
            royalRumbleShell.autoStart = false;
            royalRumbleShell.enabled = false;
        }

        if (campaignRoot == null || campaignShell == null)
        {
            Debug.LogError("[SingleplayerBattleSceneRouter] Campaign scene references are missing.");
            return;
        }

        campaignRoot.SetActive(false);
        string campaignId = ResolveCampaignId();
        campaignShell.ConfigureForMission(campaignId, GameParameters.MissionID);
        if (!campaignShell.HasRequiredSceneReferences(out string error))
        {
            Debug.LogError($"[SingleplayerBattleSceneRouter] Campaign scene configuration is invalid: {error}");
            return;
        }

        campaignRoot.SetActive(true);
        Debug.LogWarning(
            $"[SingleplayerBattleSceneRouter] Routed Game scene to Campaign: campaign={campaignId}, mission={GameParameters.MissionID}."
        );
    }

    public void AttackButton(int attackSlot)
    {
        if (campaignModeActive)
        {
            campaignShell?.AttackButton(attackSlot);
            return;
        }

        royalRumbleShell?.AttackButton(attackSlot);
    }

    public void ConfirmAttackButton()
    {
        if (campaignModeActive)
        {
            campaignShell?.ConfirmAttackButton();
            return;
        }

        royalRumbleShell?.ConfirmAttackButton();
    }

    private static string ResolveCampaignId()
    {
        if (GameParameters.CampaignID <= 0 || GameParameters.CampaignID == 1)
        {
            return DefaultCampaignId;
        }

        Debug.LogWarning(
            $"[SingleplayerBattleSceneRouter] Unknown CampaignID={GameParameters.CampaignID}; using {DefaultCampaignId}."
        );
        return DefaultCampaignId;
    }
}
