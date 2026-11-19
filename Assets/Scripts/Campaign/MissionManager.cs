using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    public Button[] levelButtons;
    public Image[] levelButtonImages;
    public GameObject campaignContentRoot;
    public string campaignId = "bushido";
    public CampaignOnlineService campaignService;

    private bool warnedMissingCampaignContentRoot;

    private async void Start()
    {
        SetCampaignContentVisible(false);
        SceneLoadingOverlay.SetMessage("LOADING...");
        SceneLoadingOverlay.Show();
        ApplyLevelAccess(0, 1);

        try
        {
            string playerId = PlayFabManagerLogin.Instance != null
                ? PlayFabManagerLogin.Instance.LoggedInPlayerId
                : string.Empty;
            if (string.IsNullOrWhiteSpace(playerId))
            {
                Debug.LogWarning("[MissionManager] Cannot load Campaign progress without player ID.");
                return;
            }

            if (campaignService == null)
            {
                Debug.LogError("[MissionManager] campaignService is not assigned.");
                return;
            }

            CampaignOnlineProgressEnvelopeDto envelope = await campaignService.GetProgressAsync(playerId, campaignId);
            if (envelope == null || !envelope.success || envelope.progress == null)
            {
                Debug.LogWarning(
                    $"[MissionManager] Failed to load Campaign progress. campaign={campaignId}, error={envelope?.error}"
                );
                return;
            }

            ApplyLevelAccess(
                envelope.progress.highestUnlockedCampaignLevel,
                envelope.progress.visibleCycleStart
            );
        }
        finally
        {
            SceneLoadingOverlay.Hide();
            SetCampaignContentVisible(true);
        }
    }

    private void ApplyLevelAccess(int highestUnlockedCampaignLevel, int visibleCycleStart)
    {
        int cycleStart = Mathf.Max(1, visibleCycleStart);
        GameParameters.CampaignCycleStart = cycleStart;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int campaignLevel = cycleStart + i;
            levelButtons[i].interactable = campaignLevel <= highestUnlockedCampaignLevel;

            if (i < levelButtonImages.Length)
            {
                Image buttonImage = levelButtonImages[i];
                if (buttonImage != null)
                {
                    buttonImage.color = levelButtons[i].interactable
                        ? new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 1f)
                        : new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 150f / 255f);
                }
            }
        }
    }

    private void SetCampaignContentVisible(bool visible)
    {
        if (campaignContentRoot == null)
        {
            if (!warnedMissingCampaignContentRoot)
            {
                Debug.LogWarning("[MissionManager] campaignContentRoot is not assigned.");
                warnedMissingCampaignContentRoot = true;
            }

            return;
        }

        warnedMissingCampaignContentRoot = false;
        campaignContentRoot.SetActive(visible);
    }
}
