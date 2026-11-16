using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    [Serializable]
    public class ModeBackgroundBinding
    {
        public string mode;
        public Sprite sprite;
    }

    private const string MultiplayerMode = "mp";
    private const string RoyalRumbleMode = "rr";
    private static string nextInitialMode;

    [Header("Service")]
    public LeaderboardService leaderboardService;

    [Header("Rows")]
    public GameObject rowPrefab;
    public Transform rowsParent;
    public ScrollRect leaderboardScrollRect;

    [Header("View State")]
    public GameObject contentRoot;
    public GameObject errorPanel;
    public Image backgroundImage;
    public List<ModeBackgroundBinding> modeBackgrounds = new List<ModeBackgroundBinding>();

    public string SelectedMode { get; private set; } = MultiplayerMode;

    private readonly List<string> availableModes = new List<string>();
    private readonly Dictionary<string, LeaderboardStateResponse> modeCache =
        new Dictionary<string, LeaderboardStateResponse>();
    private int loadVersion;

    private async void Start()
    {
        await LoadModeAsync(ConsumeInitialMode());
    }

    public static bool QueueInitialMode(string mode)
    {
        string normalizedMode = NormalizeMode(mode);
        if (normalizedMode == null)
        {
            Debug.LogError($"[LeaderboardController] Unsupported initial leaderboard mode: {mode}");
            return false;
        }

        nextInitialMode = normalizedMode;
        return true;
    }

    private static string ConsumeInitialMode()
    {
        string initialMode = nextInitialMode;
        nextInitialMode = null;
        return initialMode ?? MultiplayerMode;
    }

    public async void SelectNextMode()
    {
        await LoadRelativeModeAsync(1);
    }

    public async void SelectPreviousMode()
    {
        await LoadRelativeModeAsync(-1);
    }

    public async Task<bool> LoadModeAsync(string mode)
    {
        string normalizedMode = NormalizeMode(mode);
        if (normalizedMode == null)
        {
            Debug.LogError($"[LeaderboardController] Unsupported leaderboard mode: {mode}");
            return false;
        }
        if (availableModes.Count > 0 && !availableModes.Contains(normalizedMode))
        {
            Debug.LogWarning(
                $"[LeaderboardController] Mode ignored: mode={normalizedMode}, available={string.Join(",", availableModes)}"
            );
            return false;
        }

        int requestVersion = ++loadVersion;
        SelectedMode = normalizedMode;
        ApplyBackground(normalizedMode);

        if (modeCache.TryGetValue(normalizedMode, out LeaderboardStateResponse cachedState))
        {
            ApplyViewState(isLoading: false, hasError: false);
            ApplyLoadedState(cachedState);
            Debug.LogWarning($"[LeaderboardController] Cache rendered: mode={normalizedMode}");
            return true;
        }

        ApplyViewState(isLoading: true, hasError: false);
        ClearRows(rowsParent);

        if (!ValidateReferences())
        {
            ApplyViewState(isLoading: false, hasError: true);
            return false;
        }

        string playerId = ResolvePlayerId();
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError("[LeaderboardController] Cannot load leaderboard: playerId is empty");
            ApplyViewState(isLoading: false, hasError: true);
            return false;
        }

        Debug.LogWarning($"[LeaderboardController] Loading: player={playerId}, mode={normalizedMode}");
        LeaderboardStateResponse state = await leaderboardService.GetLeaderboardStateAsync(
            playerId,
            normalizedMode
        );
        if (requestVersion != loadVersion)
        {
            Debug.LogWarning(
                $"[LeaderboardController] Stale response ignored: mode={normalizedMode}, request={requestVersion}, current={loadVersion}"
            );
            return false;
        }
        if (!IsValidState(state))
        {
            Debug.LogError(
                $"[LeaderboardController] Load failed: mode={normalizedMode}, stage={state?.stage}, error={state?.error}"
            );
            ApplyViewState(isLoading: false, hasError: true);
            return false;
        }

        ApplyViewState(isLoading: false, hasError: false);
        ApplyLoadedState(state);
        return true;
    }

    private async Task<bool> LoadRelativeModeAsync(int direction)
    {
        string targetMode = ResolveRelativeMode(direction);
        if (targetMode == SelectedMode)
        {
            Debug.LogWarning(
                $"[LeaderboardController] Swipe ignored: availableModes={availableModes.Count}"
            );
            return false;
        }
        return await LoadModeAsync(targetMode);
    }

    private string ResolveRelativeMode(int direction)
    {
        if (availableModes.Count <= 1)
        {
            return availableModes.Count == 1 ? availableModes[0] : SelectedMode;
        }

        int currentIndex = availableModes.IndexOf(SelectedMode);
        if (currentIndex < 0) currentIndex = 0;
        int offset = direction >= 0 ? 1 : -1;
        int targetIndex = (currentIndex + offset + availableModes.Count) % availableModes.Count;
        return availableModes[targetIndex];
    }

    private void ApplyLoadedState(LeaderboardStateResponse state)
    {
        availableModes.Clear();
        foreach (string mode in state.availableModes)
        {
            string normalized = NormalizeMode(mode);
            availableModes.Add(normalized);
        }

        SelectedMode = NormalizeMode(state.mode);
        modeCache[SelectedMode] = state;
        ApplyBackground(SelectedMode);
        RenderState(state);
    }

    private void RenderState(LeaderboardStateResponse state)
    {
        ClearRows(rowsParent);
        List<LeaderboardEntryDto> entries = state.entries ?? new List<LeaderboardEntryDto>();
        RectTransform currentPlayerRow = null;

        foreach (LeaderboardEntryDto entry in entries)
        {
            bool highlight = entry != null && entry.playerId == state.currentPlayerId;
            RectTransform row = RenderRow(entry, highlight);
            if (highlight) currentPlayerRow = row;
        }

        PositionScroll(currentPlayerRow);
        Debug.LogWarning(
            $"[LeaderboardController] Rendered: mode={SelectedMode}, entries={entries.Count}, currentVisible={currentPlayerRow != null}"
        );
    }

    private RectTransform RenderRow(LeaderboardEntryDto entry, bool highlight)
    {
        if (rowsParent == null || rowPrefab == null || entry == null) return null;
        GameObject row = Instantiate(rowPrefab, rowsParent);
        TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>(true);
        if (texts.Length < 3)
        {
            Debug.LogError("[LeaderboardController] Leaderboard row prefab must contain 3 TMP text fields");
            row.SetActive(false);
            Destroy(row);
            return null;
        }

        texts[0].text = entry.rank > 0 ? $"{entry.rank}." : "-";
        texts[1].text = string.IsNullOrWhiteSpace(entry.displayName)
            ? entry.playerId
            : entry.displayName;
        texts[2].text = entry.score.ToString();

        Image rowImage = row.transform.childCount > 0
            ? row.transform.GetChild(0).GetComponent<Image>()
            : row.GetComponent<Image>();
        if (rowImage != null)
        {
            rowImage.color = highlight ? Color.yellow : Color.white;
        }
        return row.transform as RectTransform;
    }

    private void PositionScroll(RectTransform currentPlayerRow)
    {
        if (leaderboardScrollRect == null) return;
        leaderboardScrollRect.verticalNormalizedPosition = 1f;
        if (currentPlayerRow == null) return;

        RectTransform content = leaderboardScrollRect.content ?? rowsParent as RectTransform;
        RectTransform viewport = leaderboardScrollRect.viewport
            ?? leaderboardScrollRect.transform as RectTransform;
        if (content == null || viewport == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        float hiddenHeight = content.rect.height - viewport.rect.height;
        if (hiddenHeight <= 0f) return;

        Vector3 rowCenterLocal = content.InverseTransformPoint(
            currentPlayerRow.TransformPoint(currentPlayerRow.rect.center)
        );
        float centerFromTop = content.rect.height * content.pivot.y - rowCenterLocal.y;
        float topOffset = Mathf.Clamp(centerFromTop - viewport.rect.height * 0.5f, 0f, hiddenHeight);
        leaderboardScrollRect.verticalNormalizedPosition = 1f - topOffset / hiddenHeight;
    }

    private void ApplyBackground(string mode)
    {
        if (backgroundImage == null || modeBackgrounds == null) return;
        ModeBackgroundBinding binding = modeBackgrounds.Find(item => item != null && item.mode == mode);
        if (binding != null) backgroundImage.sprite = binding.sprite;
    }

    private void ApplyViewState(bool isLoading, bool hasError)
    {
        if (isLoading)
        {
            SceneLoadingOverlay.Show();
        }
        else
        {
            SceneLoadingOverlay.Hide();
        }
        if (errorPanel != null) errorPanel.SetActive(hasError);
        if (contentRoot != null) contentRoot.SetActive(!isLoading && !hasError);
    }

    private bool ValidateReferences()
    {
        bool valid = leaderboardService != null
            && rowPrefab != null
            && rowsParent != null
            && leaderboardScrollRect != null
            && leaderboardScrollRect.content != null
            && contentRoot != null
            && errorPanel != null
            && backgroundImage != null
            && HasBackground(MultiplayerMode)
            && HasBackground(RoyalRumbleMode);
        if (!valid)
        {
            Debug.LogError("[LeaderboardController] Required scene references are not assigned");
        }
        return valid;
    }

    private bool HasBackground(string mode)
    {
        return modeBackgrounds != null
            && modeBackgrounds.Exists(item => item != null && item.mode == mode && item.sprite != null);
    }

    private static bool IsValidState(LeaderboardStateResponse state)
    {
        if (state == null || !state.success || state.entries == null
            || state.availableModes == null || state.availableModes.Count == 0
            || string.IsNullOrWhiteSpace(state.currentPlayerId)) return false;
        string mode = NormalizeMode(state.mode);
        if (mode == null || !IsValidAvailableModes(state.availableModes)) return false;
        return state.availableModes.Contains(mode);
    }

    private static bool IsValidAvailableModes(List<string> modes)
    {
        if (modes.Count == 1) return modes[0] == RoyalRumbleMode;
        return modes.Count == 2
            && modes[0] == MultiplayerMode
            && modes[1] == RoyalRumbleMode;
    }

    private static string NormalizeMode(string mode)
    {
        if (mode == MultiplayerMode) return MultiplayerMode;
        if (mode == RoyalRumbleMode) return RoyalRumbleMode;
        return null;
    }

    private static void ClearRows(Transform parent)
    {
        if (parent == null) return;
        for (int index = parent.childCount - 1; index >= 0; index--)
        {
            GameObject child = parent.GetChild(index).gameObject;
            if (Application.isPlaying)
            {
                child.SetActive(false);
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private static string ResolvePlayerId()
    {
        return PlayFabManagerLogin.Instance != null
            ? PlayFabManagerLogin.Instance.LoggedInPlayerId
            : PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);
    }
}
