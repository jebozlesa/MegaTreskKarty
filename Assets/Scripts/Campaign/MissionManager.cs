using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    public Button[] levelButtons;
    public Image[] levelButtonImages;
    public string campaignId = "bushido";
    public CampaignOnlineService campaignService;

    private async void OnEnable()
    {
        ApplyLevelAccess(0);

        string playerId = PlayFabManagerLogin.Instance != null
            ? PlayFabManagerLogin.Instance.LoggedInPlayerId
            : string.Empty;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogWarning("[MissionManager] Cannot load Campaign progress without player ID.");
            return;
        }

        EnsureCampaignService();
        CampaignOnlineProgressEnvelopeDto envelope = await campaignService.GetProgressAsync(playerId, campaignId);
        if (envelope == null || !envelope.success || envelope.progress == null)
        {
            Debug.LogWarning(
                $"[MissionManager] Failed to load Campaign progress. campaign={campaignId}, error={envelope?.error}"
            );
            return;
        }

        ApplyLevelAccess(envelope.progress.highestUnlockedMissionId);
    }

    private void ApplyLevelAccess(int highestUnlockedMissionId)
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = i < highestUnlockedMissionId;

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

    private void EnsureCampaignService()
    {
        if (campaignService != null)
        {
            return;
        }

        campaignService = GetComponent<CampaignOnlineService>();
        if (campaignService == null)
        {
            campaignService = gameObject.AddComponent<CampaignOnlineService>();
        }
    }
}
