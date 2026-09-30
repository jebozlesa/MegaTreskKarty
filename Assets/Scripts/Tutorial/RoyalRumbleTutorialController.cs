using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class RoyalRumbleTutorialController : MonoBehaviour
{
    private const int HintCount = 10;
    private const int HintTransitionDelayMilliseconds = 750;

    [Header("Dependencies")]
    [SerializeField] private TutorialService tutorialService;
    [SerializeField] private RoyalRumbleShellController shellController;
    [SerializeField] private Canvas tutorialCanvas;

    [Header("Tutorial Presentation")]
    [SerializeField] private GameObject[] hintPanels = new GameObject[HintCount];
    [SerializeField] private CustomButton[] acknowledgementButtons = new CustomButton[HintCount];
    [SerializeField] private CustomButton helpButton;
    [SerializeField] private GameObject exitButton;

    [Header("Gameplay Targets")]
    [SerializeField] private RectTransform playerHandTarget;
    [SerializeField] private RectTransform[] attackButtonTargets = new RectTransform[4];
    [SerializeField] private RectTransform confirmButtonTarget;
    [SerializeField] private RectTransform recordTarget;

    private readonly RoyalRumbleTutorialFlow flow = new RoyalRumbleTutorialFlow();
    private readonly Dictionary<RectTransform, TutorialTargetSpotlight> spotlights =
        new Dictionary<RectTransform, TutorialTargetSpotlight>();
    private readonly HashSet<RectTransform> desiredHighlights = new HashSet<RectTransform>();

    private bool initialized;
    private bool battleSceneReady;
    private bool listenersBound;
    private bool failedClosed;
    private bool originalExitVisible;
    private string saveInFlightStep;

    public bool IsActive => initialized && (flow.IsActive || failedClosed);
    public bool AllowsCardPlacement => initialized && !failedClosed
        && (!flow.IsActive || flow.AllowsCardPlacement);
    public bool AllowsAttackSelection => initialized && !failedClosed
        && (!flow.IsActive || flow.AllowsAttackSelection);
    public bool AllowsConfirmation => initialized && !failedClosed
        && (!flow.IsActive || flow.AllowsConfirmation);
    public bool IsResolvingFirstRound => initialized && !failedClosed
        && flow.IsResolvingFirstRound;

    public async Task<bool> InitializeAsync()
    {
        if (initialized)
        {
            return !failedClosed;
        }

        originalExitVisible = exitButton != null && exitButton.activeSelf;
        HideAllPanels();
        SetHelpVisible(false);

        if (!ValidateReferences(out string validationError))
        {
            FailClosed(validationError);
            return false;
        }

        BindButtons();
        TutorialStateResponse state = await tutorialService.GetCurrentPlayerStateAsync();
        if (this == null)
        {
            return false;
        }

        if (
            state == null
            || !state.success
            || state.tutorialContractVersion != TutorialConstants.ContractVersion
        )
        {
            FailClosed(
                $"Tutorial state unavailable: stage={state?.stage}, error={state?.error}"
            );
            return false;
        }

        TutorialFlowProgressDto progress = null;
        state.progress?.flows?.TryGetValue(TutorialConstants.RoyalRumbleFirstBattle, out progress);
        flow.Restore(progress?.completedStepIds);
        initialized = true;
        failedClosed = false;
        Debug.LogWarning(
            $"[RoyalRumbleTutorial] Initialized: active={flow.IsActive}, hint={flow.HintIndex + 1}, stage={flow.Stage}"
        );
        Render();
        return true;
    }

    public void OnBattleSceneReady()
    {
        if (!initialized || failedClosed)
        {
            return;
        }

        battleSceneReady = true;
        Debug.LogWarning(
            $"[RoyalRumbleTutorial] Battle scene ready: active={flow.IsActive}, hint={flow.HintIndex + 1}, stage={flow.Stage}"
        );
        Render();
    }

    public async Task<bool> OnPlayerCardSelectedAsync()
    {
        if (!flow.IsActive)
        {
            return true;
        }

        if (!flow.BeginCardSelectionSave())
        {
            return false;
        }

        Render();
        if (!await SaveAndReconcileStepAsync(TutorialConstants.SelectPlayerCard))
        {
            FailClosed("The selected fighter was accepted, but tutorial progress could not be saved.");
            return false;
        }

        flow.CompleteCardSelectionSave();
        Debug.LogWarning("[RoyalRumbleTutorial] Fighter selection completed; showing attack instruction.");
        Render();
        return true;
    }

    public async Task<bool> OnAttackSelectedAsync()
    {
        if (!flow.IsActive)
        {
            return true;
        }

        if (!flow.BeginAttackSelectionSave())
        {
            return false;
        }

        Render();
        if (!await SaveAndReconcileStepAsync(TutorialConstants.SelectAttack))
        {
            FailClosed("The selected attack could not be recorded in tutorial progress.");
            return false;
        }

        flow.CompleteAttackSelectionSave();
        Debug.LogWarning("[RoyalRumbleTutorial] Attack selection completed; showing confirmation instruction.");
        Render();
        return true;
    }

    public bool OnAttackSubmissionStarted()
    {
        if (!flow.IsActive)
        {
            return true;
        }

        bool started = flow.BeginFirstRoundResolution();
        if (started)
        {
            Debug.LogWarning("[RoyalRumbleTutorial] First attack submitted; waiting for stable battle resolution.");
            Render();
        }
        return started;
    }

    public void OnAttackSubmissionFailed()
    {
        if (flow.ReturnToConfirmationAfterFailure())
        {
            Debug.LogWarning("[RoyalRumbleTutorial] First attack failed; confirmation action restored.");
            Render();
        }
    }

    public async Task<bool> OnBattleExchangeCompletedAsync(bool automaticContinuationPending)
    {
        if (!flow.IsActive || !flow.IsResolvingFirstRound)
        {
            return true;
        }

        if (automaticContinuationPending)
        {
            Debug.LogWarning(
                "[RoyalRumbleTutorial] Automatic ongoing action remains; delaying rule hints until the chain is stable."
            );
            return true;
        }

        if (!flow.BeginFirstRoundSave())
        {
            return false;
        }

        Render();
        if (!await SaveAndReconcileStepAsync(TutorialConstants.ConfirmAttack))
        {
            FailClosed("The first battle completed, but tutorial progress could not be saved.");
            return false;
        }

        flow.CompleteFirstRoundSave();
        Debug.LogWarning("[RoyalRumbleTutorial] First battle completed; showing Royal Rumble rule hints.");
        Render();
        return true;
    }

    public async void AcknowledgeCurrentInstruction()
    {
        if (!initialized || failedClosed || !battleSceneReady)
        {
            return;
        }

        int acknowledgedHint = flow.HintIndex;
        RoyalRumbleTutorialAcknowledgeResult result = flow.AcknowledgeInstruction();
        if (result == RoyalRumbleTutorialAcknowledgeResult.Ignored)
        {
            return;
        }

        Debug.LogWarning(
            $"[RoyalRumbleTutorial] Hint acknowledged: hint={acknowledgedHint + 1}, result={result}, nextHint={flow.HintIndex + 1}, stage={flow.Stage}"
        );
        Render();

        if (result == RoyalRumbleTutorialAcknowledgeResult.TransitionRequested)
        {
            await Task.Delay(HintTransitionDelayMilliseconds);
            if (this == null || failedClosed || !flow.CompleteHintTransition())
            {
                return;
            }
            Render();
            return;
        }

        if (result != RoyalRumbleTutorialAcknowledgeResult.CompletionRequested)
        {
            return;
        }

        if (!await SaveAndReconcileStepAsync(TutorialConstants.CompleteControls))
        {
            if (this != null)
            {
                flow.ReturnToFinalInstructionAfterSaveFailure();
                Render();
            }
            return;
        }

        flow.CompleteTutorialSave();
        Debug.LogWarning("[RoyalRumbleTutorial] Tutorial completed; restoring normal Royal Rumble controls.");
        Render();
    }

    public void ShowCurrentInstructionAgain()
    {
        if (!initialized || failedClosed || !battleSceneReady || !flow.ShowInstructionAgain())
        {
            return;
        }

        Debug.LogWarning($"[RoyalRumbleTutorial] Replaying hint {flow.HintIndex + 1}.");
        Render();
    }

    private void RefreshControlledInputs()
    {
        shellController?.RefreshTutorialControlledInputs();
    }

    private async Task<bool> SaveAndReconcileStepAsync(string stepId)
    {
        if (!string.IsNullOrEmpty(saveInFlightStep))
        {
            return false;
        }

        saveInFlightStep = stepId;
        try
        {
            TutorialStateResponse result = await tutorialService.CompleteCurrentPlayerStepAsync(
                TutorialConstants.RoyalRumbleFirstBattle,
                stepId
            );
            if (this == null)
            {
                return false;
            }

            if (HasCanonicalStep(result, stepId))
            {
                return true;
            }

            TutorialStateResponse refreshed = await tutorialService.RefreshCurrentPlayerStateAsync();
            return this != null && HasCanonicalStep(refreshed, stepId);
        }
        finally
        {
            if (saveInFlightStep == stepId)
            {
                saveInFlightStep = null;
            }
        }
    }

    private static bool HasCanonicalStep(TutorialStateResponse state, string stepId)
    {
        return state != null
            && state.success
            && state.HasCompletedStep(TutorialConstants.RoyalRumbleFirstBattle, stepId);
    }

    private bool ValidateReferences(out string error)
    {
        var missing = new List<string>();
        if (tutorialService == null) missing.Add(nameof(tutorialService));
        if (shellController == null) missing.Add(nameof(shellController));
        if (tutorialCanvas == null) missing.Add(nameof(tutorialCanvas));
        if (helpButton == null) missing.Add(nameof(helpButton));
        if (exitButton == null) missing.Add(nameof(exitButton));
        if (playerHandTarget == null) missing.Add(nameof(playerHandTarget));
        if (confirmButtonTarget == null) missing.Add(nameof(confirmButtonTarget));
        if (recordTarget == null) missing.Add(nameof(recordTarget));

        ValidateArray(hintPanels, HintCount, nameof(hintPanels), missing);
        ValidateArray(acknowledgementButtons, HintCount, nameof(acknowledgementButtons), missing);
        ValidateArray(attackButtonTargets, 4, nameof(attackButtonTargets), missing);

        if (missing.Count > 0)
        {
            error = "Missing RR tutorial references: " + string.Join(", ", missing);
            return false;
        }

        for (int index = 0; index < HintCount; index++)
        {
            Transform buttonTransform = acknowledgementButtons[index].transform;
            Transform panelTransform = hintPanels[index].transform;
            if (buttonTransform != panelTransform && !buttonTransform.IsChildOf(panelTransform))
            {
                error = $"Acknowledgement button {index + 1} is not inside TutorialPanel{index + 1}.";
                return false;
            }

            if (acknowledgementButtons[index].onDelayedClick.GetPersistentEventCount() > 0)
            {
                error = $"TutorialPanel{index + 1} acknowledgement button contains persistent callbacks.";
                return false;
            }

            Button unityButton = acknowledgementButtons[index].GetComponent<Button>();
            if (unityButton != null && unityButton.onClick.GetPersistentEventCount() > 0)
            {
                error = $"TutorialPanel{index + 1} acknowledgement Button contains persistent callbacks.";
                return false;
            }
        }

        if (helpButton.onDelayedClick.GetPersistentEventCount() > 0)
        {
            error = "HelpButton contains persistent callbacks.";
            return false;
        }

        Button helpUnityButton = helpButton.GetComponent<Button>();
        if (helpUnityButton != null && helpUnityButton.onClick.GetPersistentEventCount() > 0)
        {
            error = "HelpButton Button component contains persistent callbacks.";
            return false;
        }

        error = null;
        return true;
    }

    private static void ValidateArray<T>(T[] values, int expectedCount, string label, List<string> missing)
        where T : UnityEngine.Object
    {
        if (values == null || values.Length != expectedCount)
        {
            missing.Add($"{label}[{expectedCount}]");
            return;
        }

        for (int index = 0; index < values.Length; index++)
        {
            if (values[index] == null)
            {
                missing.Add($"{label}[{index}]");
            }
        }
    }

    private void BindButtons()
    {
        if (listenersBound)
        {
            return;
        }

        foreach (CustomButton button in acknowledgementButtons)
        {
            button.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        }
        helpButton.onDelayedClick.AddListener(ShowCurrentInstructionAgain);
        listenersBound = true;
    }

    private void UnbindButtons()
    {
        if (!listenersBound)
        {
            return;
        }

        foreach (CustomButton button in acknowledgementButtons)
        {
            button?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
        }
        helpButton?.onDelayedClick.RemoveListener(ShowCurrentInstructionAgain);
        listenersBound = false;
    }

    private void Render()
    {
        HideAllPanels();
        SetHelpVisible(false);
        desiredHighlights.Clear();

        bool activeAndReady = initialized && !failedClosed && flow.IsActive && battleSceneReady;
        if (exitButton != null)
        {
            exitButton.SetActive(flow.IsActive || failedClosed ? false : originalExitVisible);
        }

        if (!activeAndReady)
        {
            ApplyHighlights();
            RefreshControlledInputs();
            return;
        }

        AddHighlightsForCurrentHint();
        if (flow.IsInstructionVisible)
        {
            GameObject panel = hintPanels[flow.HintIndex];
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
        }
        else if (flow.CanReplayInstruction)
        {
            SetHelpVisible(true);
        }

        ApplyHighlights();
        RefreshControlledInputs();
    }

    private void AddHighlightsForCurrentHint()
    {
        switch (flow.HintIndex)
        {
            case 1:
                desiredHighlights.Add(playerHandTarget);
                break;
            case 2:
                desiredHighlights.Add(playerHandTarget);
                break;
            case 3:
                foreach (RectTransform target in attackButtonTargets)
                {
                    desiredHighlights.Add(target);
                }
                break;
            case 4:
                desiredHighlights.Add(confirmButtonTarget);
                break;
            case 8:
                desiredHighlights.Add(recordTarget);
                break;
        }
    }

    private void ApplyHighlights()
    {
        var removed = new List<RectTransform>();
        foreach (KeyValuePair<RectTransform, TutorialTargetSpotlight> entry in spotlights)
        {
            if (!desiredHighlights.Contains(entry.Key))
            {
                entry.Value.Clear();
                removed.Add(entry.Key);
            }
        }

        foreach (RectTransform target in removed)
        {
            spotlights.Remove(target);
        }

        foreach (RectTransform target in desiredHighlights)
        {
            if (target == null || spotlights.ContainsKey(target))
            {
                continue;
            }

            var spotlight = new TutorialTargetSpotlight();
            spotlight.Show(target, tutorialCanvas);
            spotlights.Add(target, spotlight);
        }
    }

    private void ClearHighlights()
    {
        foreach (TutorialTargetSpotlight spotlight in spotlights.Values)
        {
            spotlight.Clear();
        }
        spotlights.Clear();
        desiredHighlights.Clear();
    }

    private void HideAllPanels()
    {
        if (hintPanels == null)
        {
            return;
        }

        foreach (GameObject panel in hintPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    private void SetHelpVisible(bool visible)
    {
        if (helpButton != null)
        {
            helpButton.gameObject.SetActive(visible);
        }
    }

    private void FailClosed(string reason)
    {
        initialized = true;
        failedClosed = true;
        HideAllPanels();
        SetHelpVisible(false);
        ClearHighlights();
        if (exitButton != null)
        {
            exitButton.SetActive(false);
        }
        SceneLoadingOverlay.SetMessage("TUTORIAL LOAD FAILED");
        SceneLoadingOverlay.Show();
        Debug.LogError($"[RoyalRumbleTutorial] {reason}");
        RefreshControlledInputs();
    }

    private void OnDestroy()
    {
        UnbindButtons();
        ClearHighlights();
        if (exitButton != null)
        {
            exitButton.SetActive(originalExitVisible);
        }
    }
}
