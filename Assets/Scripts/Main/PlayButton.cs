using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.Events.UnityEvent<int> onButtonClick;
    public void OnButtonClick(int missionID)
    {
        onButtonClick?.Invoke(missionID);
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneGame(int missionID = 0)
    {
        GameParameters.MissionID = missionID;
        SceneManager.LoadScene("Game");
    }

    public void LoadSceneCampaign(int campaignID)
    {
        GameParameters.CampaignID = campaignID;
        SceneManager.LoadScene("Campaign");
    }
}