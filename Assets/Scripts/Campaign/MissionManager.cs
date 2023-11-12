using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    public Button[] levelButtons; // Pole pre všetky tlačidlá úrovní
    public Image[] levelButtonImages; // Pole pre obrázky dcérskych komponent tlačidiel

    private void OnEnable()
    {
        Debug.Log("OnEnable volám");
        UpdateLevelAccess("bushido");
    }

    private void UpdateLevelAccess(string campaignName)
    {
        var campaignData = CampaignManager.Instance.LoadCampaignData(campaignName);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = i < campaignData.mission;

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
}
