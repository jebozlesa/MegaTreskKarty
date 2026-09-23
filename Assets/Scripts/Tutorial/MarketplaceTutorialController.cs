using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class MarketplaceTutorialController : MonoBehaviour
{
    private const int HintTransitionDelayMilliseconds = 750;

    [SerializeField]
    private GameObject firstHintPanel;

    [SerializeField]
    private GameObject secondHintPanel;

    [SerializeField]
    private CustomButton firstHintAcknowledgeButton;

    [SerializeField]
    private CustomButton secondHintAcknowledgeButton;

    [SerializeField]
    private Button firstHintInteractableButton;

    private readonly MarketplaceTutorialFlow flow = new MarketplaceTutorialFlow();
    private TutorialService tutorialService;
    private TutorialStateResponse currentState;
    private bool buttonsBound;

    public bool CanSelectPack => flow.CanSelectPack;

    public bool Configure(TutorialService service)
    {
        if (
            service == null
            || firstHintPanel == null
            || secondHintPanel == null
            || firstHintAcknowledgeButton == null
            || secondHintAcknowledgeButton == null
            || firstHintInteractableButton == null
        )
        {
            Debug.LogError("[MarketplaceTutorial] Required tutorial references are not assigned.");
            return false;
        }

        tutorialService = service;
        BindButtons();
        Render();
        return true;
    }

    public void Begin(TutorialStateResponse state)
    {
        currentState = state;
        RestoreFlow(state);
        Debug.LogWarning($"[MarketplaceTutorial] Started: phase={flow.Phase}, active={flow.IsActive}");
        Render();
    }

    public void Hide()
    {
        SetPanelVisible(firstHintPanel, false);
        SetPanelVisible(secondHintPanel, false);
    }

    public async void AcknowledgeCurrentInstruction()
    {
        string stepId = flow.AcknowledgeInstruction();
        Render();

        if (string.IsNullOrWhiteSpace(stepId))
        {
            Debug.LogWarning($"[MarketplaceTutorial] Instruction acknowledged: phase={flow.Phase}");
            return;
        }

        Debug.LogWarning($"[MarketplaceTutorial] Saving instruction: step={stepId}");
        TutorialStateResponse result = await SaveStepAsync(stepId);
        if (this == null)
        {
            return;
        }

        if (result == null || !result.success)
        {
            Debug.LogError(
                $"[MarketplaceTutorial] Step save failed: step={stepId}, stage={result?.stage}, error={result?.error}"
            );
            RestoreFlow(currentState);
            Render();
            return;
        }

        currentState = result;
        bool valid = result.gates != null;
        bool transitionStarted = flow.BeginSecondHintTransition(
            valid,
            valid && result.gates.needsFirstPack,
            valid && result.HasCompletedStep(
                TutorialConstants.MarketplaceFirstPack,
                TutorialConstants.OpenMarketplace
            )
        );
        Render();

        if (transitionStarted)
        {
            await Task.Delay(HintTransitionDelayMilliseconds);
            if (this == null)
            {
                return;
            }

            flow.CompleteSecondHintTransition();
            Render();
        }

        Debug.LogWarning($"[MarketplaceTutorial] Step saved: step={stepId}, nextPhase={flow.Phase}");
    }

    private Task<TutorialStateResponse> SaveStepAsync(string stepId)
    {
        if (tutorialService == null)
        {
            Debug.LogError("[MarketplaceTutorial] TutorialService is not assigned.");
            return Task.FromResult<TutorialStateResponse>(null);
        }

        return tutorialService.CompleteCurrentPlayerStepAsync(
            TutorialConstants.MarketplaceFirstPack,
            stepId
        );
    }

    private void RestoreFlow(TutorialStateResponse state)
    {
        bool valid = state != null && state.success && state.gates != null;
        flow.Restore(
            valid,
            valid && state.gates.needsFirstPack,
            valid && state.HasCompletedStep(
                TutorialConstants.MarketplaceFirstPack,
                TutorialConstants.OpenMarketplace
            )
        );
    }

    private void BindButtons()
    {
        if (buttonsBound)
        {
            return;
        }

        firstHintAcknowledgeButton.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        secondHintAcknowledgeButton.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        buttonsBound = true;
    }

    private void OnDestroy()
    {
        if (!buttonsBound)
        {
            return;
        }

        firstHintAcknowledgeButton?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
        secondHintAcknowledgeButton?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
        buttonsBound = false;
    }

    private void Render()
    {
        bool showFirst = flow.Phase == MarketplaceTutorialPhase.FirstHint
            || flow.Phase == MarketplaceTutorialPhase.SavingFirstHint;
        SetPanelVisible(firstHintPanel, showFirst);
        SetPanelVisible(secondHintPanel, flow.Phase == MarketplaceTutorialPhase.SecondHint);

        firstHintInteractableButton.interactable = flow.Phase == MarketplaceTutorialPhase.FirstHint;

        if (showFirst && firstHintPanel != null)
        {
            firstHintPanel.transform.SetAsLastSibling();
        }
        else if (flow.Phase == MarketplaceTutorialPhase.SecondHint && secondHintPanel != null)
        {
            secondHintPanel.transform.SetAsLastSibling();
        }
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
        }
    }
}
