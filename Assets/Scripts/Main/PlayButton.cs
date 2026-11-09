using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.Events.UnityEvent<int> onButtonClick;

    [Header("Guarded Mode Entry")]
    [SerializeField] private string deckContextId;
    [SerializeField] private LibraryDeckService libraryDeckService;
    [SerializeField] private ConfirmDialogController missingDeckDialog;
    [SerializeField] private ConfirmDialogController entryCheckUnavailableDialog;

    private static PlayButton entryOwner;
    private int entryVersion;

    public async void LoadSceneWithDeckCheck(string sceneName)
    {
        if (!isActiveAndEnabled || entryOwner != null) return;
        if (string.IsNullOrWhiteSpace(sceneName) || string.IsNullOrWhiteSpace(deckContextId)
            || libraryDeckService == null || missingDeckDialog == null || entryCheckUnavailableDialog == null)
        {
            Debug.LogError("[ModeEntry] Guarded entry references are incomplete.");
            return;
        }
        if (ShouldReconcileOwnedRoom())
        {
            SceneManager.LoadScene(sceneName);
            return;
        }
        string playerId = PlayFabManagerLogin.Instance?.LoggedInPlayerId;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            entryCheckUnavailableDialog.ShowAuthored(() => { });
            return;
        }
        if (!TryBeginEntryCheck()) return;
        int version = ++entryVersion;
        string contextId = deckContextId;
        Debug.LogWarning($"[ModeEntry] Check requested: player={playerId}, context={contextId}, destination={sceneName}");
        SceneLoadingOverlay.SetMessage("LOADING...");
        SceneLoadingOverlay.Show();
        try
        {
            ModeEntryStateResponse response = await libraryDeckService.GetModeEntryStateAsync(playerId, contextId);
            if (!OwnsEntryCheck(version)) return;
            EndEntryCheck();
            if (PlayFabManagerLogin.Instance?.LoggedInPlayerId != playerId) return;
            if (ApplyEntryResponse(response, playerId, contextId)) SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception error)
        {
            if (!OwnsEntryCheck(version)) return;
            EndEntryCheck();
            Debug.LogError($"[ModeEntry] Check failed: context={contextId}, error={error.Message}");
            if (PlayFabManagerLogin.Instance?.LoggedInPlayerId == playerId)
                entryCheckUnavailableDialog.ShowAuthored(() => { });
        }
        finally
        {
            if (OwnsEntryCheck(version)) EndEntryCheck();
        }
    }

    private bool ApplyEntryResponse(ModeEntryStateResponse response, string playerId, string contextId)
    {
        if (response == null || !response.success || !response.canEnter.HasValue || !string.IsNullOrEmpty(response.error)
            || response.playerId != playerId || response.contextId != contextId
            || (response.canEnter == true && !string.IsNullOrEmpty(response.reason))
            || (response.canEnter == false && response.reason != "missing_active_deck"))
        {
            Debug.LogWarning($"[ModeEntry] Check unavailable: player={playerId}, context={contextId}, error={response?.error}, stage={response?.stage}");
            entryCheckUnavailableDialog.ShowAuthored(() => { });
            return false;
        }
        Debug.LogWarning($"[ModeEntry] Check confirmed: player={playerId}, context={contextId}, canEnter={response.canEnter}");
        if (response.canEnter == true) return true;
        missingDeckDialog.ShowAuthored(() => { });
        return false;
    }

    private bool ShouldReconcileOwnedRoom() => deckContextId == "mp" && !string.IsNullOrEmpty(PlayerPrefs.GetString("RoomCode", ""));

    private bool TryBeginEntryCheck()
    {
        if (!isActiveAndEnabled || entryOwner != null) return false;
        entryOwner = this;
        return true;
    }

    private bool OwnsEntryCheck(int version) => this != null && isActiveAndEnabled && entryOwner == this && version == entryVersion;

    private void EndEntryCheck()
    {
        if (entryOwner != this) return;
        SceneLoadingOverlay.Hide();
        entryOwner = null;
    }

    private void OnDisable()
    {
        entryVersion++;
        if (entryOwner == this)
        {
            // Never hide an overlay belonging to the scene that replaced this one.
            if (gameObject.scene == SceneManager.GetActiveScene()) SceneLoadingOverlay.Hide();
            entryOwner = null;
        }
    }

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

    public void LogoutAndLoadSceneLogin()
    {
        PlayerSessionLogout.LogoutAndLoadLogin();
    }
}
