using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuTutorialController : MonoBehaviour
{
    private const int HintTransitionDelayMilliseconds = 750;

    [Header("Dependencies")]
    [SerializeField] private TutorialService tutorialService;

    [Header("Presentation")]
    [SerializeField] private GameObject introHintPanel;
    [SerializeField] private GameObject marketplaceHintPanel;
    [SerializeField] private GameObject libraryHintPanel;
    [SerializeField] private GameObject royalRumbleHintPanel;
    [SerializeField] private CustomButton introAcknowledgeButton;
    [SerializeField] private CustomButton marketplaceAcknowledgeButton;
    [SerializeField] private CustomButton libraryAcknowledgeButton;
    [SerializeField] private CustomButton royalRumbleAcknowledgeButton;

    [Header("Menu Controls")]
    [SerializeField] private Button marketplaceButton;
    [SerializeField] private Button libraryButton;
    [SerializeField] private Button royalRumbleButton;
    [SerializeField] private Selectable[] navigationControls;

    private readonly MainMenuTutorialFlow flow = new MainMenuTutorialFlow();
    private readonly TutorialTargetSpotlight spotlight = new TutorialTargetSpotlight();
    private readonly Dictionary<Selectable, bool> originalInteractable = new Dictionary<Selectable, bool>();
    private bool buttonsBound;
    private bool initialized;

    private async void Start()
    {
        if (!ValidateReferences())
        {
            FailClosed("Required Main tutorial references are not assigned.");
            return;
        }

        CaptureOriginalStates();
        BindButtons();
        HideHintPanels();
        SetAllNavigationInteractable(false);
        SceneLoadingOverlay.Hide();

        string playerId = ResolvePlayerId();
        bool hasCachedState = TutorialSessionState.TryGet(playerId, out TutorialStateResponse state);
        if (!hasCachedState)
        {
            SceneLoadingOverlay.Show();
            state = await tutorialService.GetCurrentPlayerStateAsync();
            if (this == null)
            {
                return;
            }
            SceneLoadingOverlay.Hide();
        }

        if (state == null || !state.success || state.gates == null)
        {
            FailClosed($"Tutorial state unavailable: stage={state?.stage}, error={state?.error}");
            return;
        }

        bool royalRumbleCompleted = state.IsFlowCompleted(
            TutorialConstants.RoyalRumbleFirstBattle
        );
        flow.Restore(
            true,
            state.gates.needsFirstPack,
            state.gates.needsLibraryDeckSwap,
            royalRumbleCompleted
        );
        initialized = true;
        Debug.LogWarning(
            $"[MainMenuTutorial] Started: target={flow.Target}, phase={flow.Phase}, cached={hasCachedState}"
        );
        Render();
    }

    public async void AcknowledgeCurrentInstruction()
    {
        if (!initialized || !flow.IsActive)
        {
            return;
        }

        MainMenuTutorialPhase previousPhase = flow.Phase;
        flow.AcknowledgeInstruction();
        Debug.LogWarning(
            $"[MainMenuTutorial] Instruction acknowledged: target={flow.Target}, {previousPhase}->{flow.Phase}"
        );
        Render();

        if (
            previousPhase == MainMenuTutorialPhase.IntroInstruction
            && flow.Phase == MainMenuTutorialPhase.IntroTransition
        )
        {
            await CompleteIntroTransitionAfterDelayAsync();
        }
    }

    private bool ValidateReferences()
    {
        if (
            tutorialService == null
            || introHintPanel == null
            || marketplaceHintPanel == null
            || libraryHintPanel == null
            || royalRumbleHintPanel == null
            || introAcknowledgeButton == null
            || marketplaceAcknowledgeButton == null
            || libraryAcknowledgeButton == null
            || royalRumbleAcknowledgeButton == null
            || marketplaceButton == null
            || libraryButton == null
            || royalRumbleButton == null
            || navigationControls == null
            || navigationControls.Length == 0
        )
        {
            return false;
        }

        return ContainsNavigationControl(marketplaceButton)
            && ContainsNavigationControl(libraryButton)
            && ContainsNavigationControl(royalRumbleButton);
    }

    private bool ContainsNavigationControl(Selectable control)
    {
        foreach (Selectable candidate in navigationControls)
        {
            if (candidate == control)
            {
                return true;
            }
        }
        return false;
    }

    private void CaptureOriginalStates()
    {
        originalInteractable.Clear();
        foreach (Selectable control in navigationControls)
        {
            if (control != null && !originalInteractable.ContainsKey(control))
            {
                originalInteractable.Add(control, control.interactable);
            }
        }
    }

    private void BindButtons()
    {
        if (buttonsBound)
        {
            return;
        }

        introAcknowledgeButton.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        marketplaceAcknowledgeButton.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        libraryAcknowledgeButton.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        royalRumbleAcknowledgeButton.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
        buttonsBound = true;
    }

    private async Task CompleteIntroTransitionAfterDelayAsync()
    {
        await Task.Delay(HintTransitionDelayMilliseconds);
        if (this == null || !flow.CompleteIntroTransition())
        {
            return;
        }

        Debug.LogWarning("[MainMenuTutorial] Intro transition complete; showing Marketplace target.");
        Render();
    }

    private void Render()
    {
        HideHintPanels();

        if (!flow.IsActive)
        {
            spotlight.Clear();
            RestoreNavigationStates();
            return;
        }

        SetAllNavigationInteractable(false);

        if (flow.Phase == MainMenuTutorialPhase.IntroInstruction)
        {
            spotlight.Clear();
            ShowInstructionPanel(introHintPanel);
            return;
        }

        if (flow.Phase == MainMenuTutorialPhase.IntroTransition)
        {
            spotlight.Clear();
            return;
        }

        Button targetButton = GetTargetButton();
        spotlight.Show(targetButton?.transform as RectTransform, null);

        if (flow.Phase == MainMenuTutorialPhase.TargetInstruction)
        {
            ShowInstructionPanel(GetTargetPanel());
        }
        else if (flow.Phase == MainMenuTutorialPhase.AwaitingTarget && targetButton != null)
        {
            targetButton.interactable = true;
        }
    }

    private Button GetTargetButton()
    {
        switch (flow.Target)
        {
            case MainMenuTutorialTarget.Marketplace:
                return marketplaceButton;
            case MainMenuTutorialTarget.Library:
                return libraryButton;
            case MainMenuTutorialTarget.RoyalRumble:
                return royalRumbleButton;
            default:
                return null;
        }
    }

    private GameObject GetTargetPanel()
    {
        switch (flow.Target)
        {
            case MainMenuTutorialTarget.Marketplace:
                return marketplaceHintPanel;
            case MainMenuTutorialTarget.Library:
                return libraryHintPanel;
            case MainMenuTutorialTarget.RoyalRumble:
                return royalRumbleHintPanel;
            default:
                return null;
        }
    }

    private void SetAllNavigationInteractable(bool interactable)
    {
        if (navigationControls == null)
        {
            return;
        }

        foreach (Selectable control in navigationControls)
        {
            if (control != null)
            {
                control.interactable = interactable;
            }
        }
    }

    private void RestoreNavigationStates()
    {
        foreach (KeyValuePair<Selectable, bool> entry in originalInteractable)
        {
            if (entry.Key != null)
            {
                entry.Key.interactable = entry.Value;
            }
        }
    }

    private void FailClosed(string reason)
    {
        initialized = false;
        spotlight.Clear();
        HideHintPanels();
        SetAllNavigationInteractable(false);
        SceneLoadingOverlay.SetMessage("TUTORIAL LOAD FAILED");
        SceneLoadingOverlay.Show();
        Debug.LogError($"[MainMenuTutorial] {reason}");
    }

    private void HideHintPanels()
    {
        SetPanelVisible(introHintPanel, false);
        SetPanelVisible(marketplaceHintPanel, false);
        SetPanelVisible(libraryHintPanel, false);
        SetPanelVisible(royalRumbleHintPanel, false);
    }

    private void ShowInstructionPanel(GameObject panel)
    {
        SetPanelVisible(panel, true);
        panel?.transform.SetAsLastSibling();
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
        }
    }

    private static string ResolvePlayerId()
    {
        return PlayFabManagerLogin.Instance != null
            ? PlayFabManagerLogin.Instance.LoggedInPlayerId
            : PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);
    }

    private void OnDestroy()
    {
        if (buttonsBound)
        {
            introAcknowledgeButton?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
            marketplaceAcknowledgeButton?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
            libraryAcknowledgeButton?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
            royalRumbleAcknowledgeButton?.onDelayedClick.RemoveListener(AcknowledgeCurrentInstruction);
            buttonsBound = false;
        }

        spotlight.Clear();
        RestoreNavigationStates();
    }
}
