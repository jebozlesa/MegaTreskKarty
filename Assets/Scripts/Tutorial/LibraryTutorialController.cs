using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class LibraryTutorialController : MonoBehaviour
{
    private readonly LibraryTutorialFlow flow = new LibraryTutorialFlow();
    private readonly LibraryTutorialTransitionGate transitionGate = new LibraryTutorialTransitionGate();
    private readonly List<GameObject> panels = new List<GameObject>();
    private readonly List<Card> renderedCards = new List<Card>();
    private readonly List<string> persistedSteps = new List<string>();

    private TutorialService tutorialService;
    private LibraryDeckController deckController;
    private DeckManager deckManager;
    private GameObject albumTutorialRoot;
    private GameObject cardTutorialRoot;
    private GameObject backButton;
    private GameObject helpButton;
    private LibraryTutorialInputBlocker inputBlocker;
    private Card selectedCard;
    private Outline targetOutline;
    private RectTransform spotlightTarget;
    private Canvas spotlightCanvas;
    private bool spotlightCanvasAdded;
    private bool spotlightOriginalOverrideSorting;
    private int spotlightOriginalSortingLayerId;
    private int spotlightOriginalSortingOrder;
    private bool helpButtonConfigured;
    private string swapRequestId;
    private string saveInFlightStepId;
    private bool initialized;

    public bool IsActive => initialized && !flow.IsDone;
    public bool IsSaving => IsActive && flow.IsSaving;
    public string CurrentStepId => flow.StepId;
    public string TutorialSwapRequestId => swapRequestId;

    public void Configure(
        TutorialService service,
        LibraryDeckController libraryDeckController,
        DeckManager manager,
        GameObject libraryRoot,
        GameObject detailRoot,
        GameObject navigationBackButton,
        GameObject instructionHelpButton)
    {
        tutorialService = service;
        deckController = libraryDeckController;
        deckManager = manager;
        albumTutorialRoot = libraryRoot;
        cardTutorialRoot = detailRoot;
        backButton = navigationBackButton;
        helpButton = instructionHelpButton;
        BuildPanelList();
        BindAcknowledgementButtons();
        BindHelpButton();
        EnsureInputBlocker();
    }

    public void SetRenderedCards(IEnumerable<Card> cards)
    {
        renderedCards.Clear();
        if (cards != null) renderedCards.AddRange(cards.Where(card => card != null && !card.deckCard));
        if (selectedCard != null && !renderedCards.Contains(selectedCard)) selectedCard = null;
        SelectTutorialCardIfNeeded();
        PrepareCurrentStepView();
        Render();
    }

    public void Begin(TutorialStateResponse state)
    {
        TutorialFlowProgressDto progress = null;
        state?.progress?.flows?.TryGetValue(TutorialConstants.LibraryIntro, out progress);
        transitionGate.Reset();
        RestoreProgress(progress?.completedStepIds);
        initialized = LibraryTutorialActivationPolicy.ShouldRun(state);
        swapRequestId = flow.StepId == TutorialConstants.SwapDeckCard
            ? Guid.NewGuid().ToString()
            : null;
        SelectTutorialCardIfNeeded();
        PrepareCurrentStepView();
        Debug.LogWarning($"[LibraryTutorial] Started: active={IsActive}, step={flow.StepId}, completed={progress?.completedStepIds?.Count ?? 0}");
        Render();
    }

    public void AcknowledgeCurrentInstruction()
    {
        if (!IsActive || !flow.IsInstructionVisible) return;

        string stepId = flow.StepId;
        bool wasReplay = flow.IsInstructionReplayVisible;
        bool savesStep = flow.Acknowledge();
        Debug.LogWarning($"[LibraryTutorial] Instruction acknowledged: step={stepId}, replay={wasReplay}, awaitsAction={!savesStep}");
        Render();
        if (savesStep) _ = SaveCurrentStepAsync(stepId);
    }

    public void ShowCurrentInstructionAgain()
    {
        if (!IsActive || !flow.ShowInstructionAgain()) return;

        Debug.LogWarning($"[LibraryTutorial] Instruction replay requested: step={flow.StepId}");
        Render();
    }

    public bool TryOpenCard(Card card)
    {
        if (!IsActive
            || card == null
            || card != selectedCard
            || flow.StepId != TutorialConstants.OpenCardDetail
            || !flow.TryAction("open"))
        {
            return false;
        }

        transitionGate.TryBegin(flow.StepId, Time.unscaledTime);
        BeginActionTransition(keepTargetHighlight: true);
        return true;
    }

    public void ConfirmCardOpened(Card card)
    {
        if (card == selectedCard && flow.StepId == TutorialConstants.OpenCardDetail)
        {
            RemoveTargetHighlight();
            _ = SaveCurrentStepAsync(TutorialConstants.OpenCardDetail);
        }
    }

    public bool TryCardGesture(Card card, string action)
    {
        if (!IsActive || card == null || card != selectedCard) return false;

        string stepId = flow.StepId;
        if (flow.TryAction(action))
        {
            transitionGate.TryBegin(stepId, Time.unscaledTime);
            BeginActionTransition(keepTargetHighlight: false);
            return true;
        }

        return transitionGate.TryObserve(stepId, action, Time.unscaledTime);
    }

    public void ConfirmCardGesture(string completedStepId)
    {
        if (flow.StepId == completedStepId) _ = SaveCurrentStepAsync(completedStepId);
    }

    public bool CanSwap(Card outgoingDeckCard, Card incomingCard)
    {
        if (!IsActive
            || flow.StepId != TutorialConstants.SwapDeckCard
            || outgoingDeckCard == null
            || incomingCard != selectedCard
            || !flow.TryAction("swap"))
        {
            return false;
        }

        BeginActionTransition(keepTargetHighlight: false);
        return true;
    }

    public async Task<bool> ReconcileSwapAsync()
    {
        if (!IsActive || flow.StepId != TutorialConstants.SwapDeckCard) return false;

        await RefreshAsync();
        return flow.StepId != TutorialConstants.SwapDeckCard;
    }

    public bool AllowsOrdinaryLibraryAction()
    {
        return !IsActive;
    }

    private async Task SaveCurrentStepAsync(string stepId)
    {
        if (saveInFlightStepId == stepId) return;
        saveInFlightStepId = stepId;

        try
        {
            TutorialStateResponse result = await tutorialService.CompleteCurrentPlayerStepAsync(
                TutorialConstants.LibraryIntro,
                stepId
            );
            if (this == null) return;
            if (result == null || !result.success)
            {
                Debug.LogError($"[LibraryTutorial] Step save failed: step={stepId}, stage={result?.stage}, error={result?.error}");
                transitionGate.Reset();
                await RefreshAsync();
                return;
            }

            while (this != null
                && flow.StepId == stepId
                && !transitionGate.CanPresentNextStep(stepId, Time.unscaledTime))
            {
                await Task.Yield();
            }

            if (this == null || flow.StepId != stepId) return;
            transitionGate.Reset();
            ApplyServerState(result);
        }
        finally
        {
            if (saveInFlightStepId == stepId) saveInFlightStepId = null;
        }
    }

    private async Task RefreshAsync()
    {
        TutorialStateResponse state = await tutorialService.RefreshCurrentPlayerStateAsync();
        if (state == null || !state.success)
        {
            Debug.LogError($"[LibraryTutorial] Progress refresh failed: stage={state?.stage}, error={state?.error}");
            flow.Restore(persistedSteps);
            Render();
            return;
        }
        ApplyServerState(state);
    }

    private void ApplyServerState(TutorialStateResponse state)
    {
        TutorialFlowProgressDto progress = null;
        state.progress?.flows?.TryGetValue(TutorialConstants.LibraryIntro, out progress);
        string previousStep = flow.StepId;
        RestoreProgress(progress?.completedStepIds);
        transitionGate.Reset();
        initialized = LibraryTutorialActivationPolicy.ShouldRun(state);
        if (flow.StepId == TutorialConstants.SwapDeckCard && string.IsNullOrWhiteSpace(swapRequestId))
        {
            swapRequestId = Guid.NewGuid().ToString();
        }
        PrepareCurrentStepView();
        Debug.LogWarning($"[LibraryTutorial] Progress applied: {previousStep}->{flow.StepId}, done={flow.IsDone}");
        Render();
    }

    private void RestoreProgress(IEnumerable<string> completedStepIds)
    {
        persistedSteps.Clear();
        if (completedStepIds != null) persistedSteps.AddRange(completedStepIds);
        flow.Restore(persistedSteps);
    }

    private void BuildPanelList()
    {
        panels.Clear();
        panels.Add(FindDescendant(albumTutorialRoot, "Hint1"));
        panels.Add(FindDescendant(albumTutorialRoot, "Hint2"));
        for (int index = 1; index <= 6; index++)
        {
            panels.Add(FindDescendant(cardTutorialRoot, $"CardHint{index}"));
        }

        for (int index = 0; index < panels.Count; index++)
        {
            if (panels[index] == null)
            {
                Debug.LogError($"[LibraryTutorial] Missing visual panel for step {LibraryTutorialFlow.Steps[index]}");
            }
        }
    }

    private void BindAcknowledgementButtons()
    {
        foreach (GameObject panel in panels)
        {
            if (panel == null) continue;
            CustomButton custom = panel.GetComponentInChildren<CustomButton>(true);
            Button standard = panel.GetComponentInChildren<Button>(true);
            if (custom != null)
            {
                custom.onDelayedClick.RemoveAllListeners();
                custom.onDelayedClick.AddListener(AcknowledgeCurrentInstruction);
            }
            if (standard != null)
            {
                standard.onClick.RemoveAllListeners();
                if (custom == null) standard.onClick.AddListener(AcknowledgeCurrentInstruction);
            }
        }
    }

    private void BindHelpButton()
    {
        helpButtonConfigured = false;
        if (helpButton == null) return;

        CustomButton custom = helpButton.GetComponentInChildren<CustomButton>(true);
        Button standard = helpButton.GetComponentInChildren<Button>(true);
        int persistentCallbacks = (custom?.onDelayedClick?.GetPersistentEventCount() ?? 0)
            + (standard?.onClick?.GetPersistentEventCount() ?? 0);
        if (persistentCallbacks > 0)
        {
            helpButton.SetActive(false);
            Debug.LogError(
                "[LibraryTutorial] Tutorial help button contains persistent callbacks. "
                + "Remove copied navigation callbacks in the Cards scene before using it."
            );
            return;
        }

        if (custom != null)
        {
            custom.onDelayedClick.RemoveAllListeners();
            custom.onDelayedClick.AddListener(ShowCurrentInstructionAgain);
        }
        if (standard != null)
        {
            standard.onClick.RemoveAllListeners();
            if (custom == null) standard.onClick.AddListener(ShowCurrentInstructionAgain);
        }

        helpButtonConfigured = custom != null || standard != null;
        if (!helpButtonConfigured)
        {
            helpButton.SetActive(false);
            Debug.LogError("[LibraryTutorial] Tutorial help object has no Button or CustomButton component.");
        }
    }

    private void SelectTutorialCardIfNeeded()
    {
        if (selectedCard != null || deckController?.CurrentDeck?.cardIds == null) return;
        HashSet<string> deckCardIds = new HashSet<string>(deckController.CurrentDeck.cardIds);
        HashSet<string> deckNames = new HashSet<string>(
            deckManager == null
                ? Array.Empty<string>()
                : deckManager.GetRenderedDeckCardNames()
        );
        selectedCard = renderedCards.FirstOrDefault(card =>
            !deckCardIds.Contains(card.cardId) && !deckNames.Contains(card.cardName));
        if (selectedCard == null && IsActive)
        {
            Debug.LogError("[LibraryTutorial] No legal replacement card is available for the Library lesson.");
        }
    }

    private void PrepareCurrentStepView()
    {
        if (!IsActive || selectedCard == null || flow.StepIndex < 2 || flow.StepIndex > 6) return;
        if (!selectedCard.IsZoomed) selectedCard.ZoomIn();
        selectedCard.RestoreTutorialView(flow.StepId);
    }

    private void Render()
    {
        foreach (GameObject panel in panels) if (panel != null) panel.SetActive(false);
        RemoveTargetOutline();

        if (albumTutorialRoot != null) albumTutorialRoot.SetActive(IsActive);
        if (cardTutorialRoot != null) cardTutorialRoot.SetActive(IsActive);
        if (backButton != null) backButton.SetActive(!IsActive);
        if (helpButton != null)
        {
            helpButton.SetActive(helpButtonConfigured && IsActive && flow.CanReplayInstruction);
        }

        if (!IsActive)
        {
            SetSpotlight(null);
            SetBlocker(false, null);
            return;
        }

        if (flow.IsInstructionVisible)
        {
            SetBlocker(false, null);
            if (flow.StepIndex < panels.Count && panels[flow.StepIndex] != null)
            {
                panels[flow.StepIndex].SetActive(true);
                panels[flow.StepIndex].transform.SetAsLastSibling();
            }

            RectTransform spotlight = flow.StepId == TutorialConstants.OpenCardDetail
                ? selectedCard?.transform as RectTransform
                : null;
            SetSpotlight(spotlight);
            AddTargetOutline(spotlight);
            return;
        }

        SetSpotlight(null);
        RectTransform allowed = null;
        if (flow.IsAwaitingAction)
        {
            allowed = flow.StepId == TutorialConstants.SwapDeckCard
                ? deckManager?.deckPanel?.transform as RectTransform
                : selectedCard?.transform as RectTransform;
            if (ShouldHighlightActionTarget(flow.StepId)) AddTargetOutline(allowed);
        }
        SetBlocker(true, allowed);
    }

    private static bool ShouldHighlightActionTarget(string stepId)
    {
        return stepId == TutorialConstants.OpenCardDetail
            || stepId == TutorialConstants.SwapDeckCard;
    }

    private void SetSpotlight(RectTransform target)
    {
        if (target == spotlightTarget && spotlightCanvas != null) return;

        ClearSpotlight();
        if (target == null) return;

        Canvas parentCanvas = target.parent == null
            ? null
            : target.parent.GetComponentInParent<Canvas>();
        Canvas canvas = target.GetComponent<Canvas>();
        spotlightCanvasAdded = canvas == null;
        if (spotlightCanvasAdded)
        {
            canvas = target.gameObject.AddComponent<Canvas>();
        }

        spotlightTarget = target;
        spotlightCanvas = canvas;
        spotlightOriginalOverrideSorting = canvas.overrideSorting;
        spotlightOriginalSortingLayerId = canvas.sortingLayerID;
        spotlightOriginalSortingOrder = canvas.sortingOrder;

        canvas.overrideSorting = true;
        if (parentCanvas != null)
        {
            canvas.sortingLayerID = parentCanvas.sortingLayerID;
            canvas.sortingOrder = parentCanvas.sortingOrder + 100;
        }
        else
        {
            canvas.sortingOrder = 100;
        }
    }

    private void ClearSpotlight()
    {
        if (spotlightCanvas != null)
        {
            if (spotlightCanvasAdded)
            {
                spotlightCanvas.enabled = false;
                if (Application.isPlaying) Destroy(spotlightCanvas);
                else DestroyImmediate(spotlightCanvas);
            }
            else
            {
                spotlightCanvas.overrideSorting = spotlightOriginalOverrideSorting;
                spotlightCanvas.sortingLayerID = spotlightOriginalSortingLayerId;
                spotlightCanvas.sortingOrder = spotlightOriginalSortingOrder;
            }
        }

        spotlightTarget = null;
        spotlightCanvas = null;
        spotlightCanvasAdded = false;
    }

    private void OnDestroy()
    {
        ClearSpotlight();
    }

    private void EnsureInputBlocker()
    {
        if (inputBlocker != null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject blockerObject = new GameObject("LibraryTutorialInputBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LibraryTutorialInputBlocker));
        blockerObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = (RectTransform)blockerObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        blockerObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.001f);
        inputBlocker = blockerObject.GetComponent<LibraryTutorialInputBlocker>();
        blockerObject.SetActive(false);
    }

    private void SetBlocker(bool active, RectTransform allowed)
    {
        if (inputBlocker == null) return;
        inputBlocker.AllowedTarget = allowed;
        inputBlocker.SecondaryAllowedTarget = active
            && helpButtonConfigured
            && helpButton != null
            && helpButton.activeInHierarchy
                ? helpButton.transform as RectTransform
                : null;
        inputBlocker.gameObject.SetActive(active);
        if (active) inputBlocker.transform.SetAsLastSibling();
    }

    private void BeginActionTransition(bool keepTargetHighlight)
    {
        if (helpButton != null) helpButton.SetActive(false);
        if (!keepTargetHighlight) RemoveTargetHighlight();
    }

    private void RemoveTargetHighlight()
    {
        RemoveTargetOutline();
        SetSpotlight(null);
    }

    private void AddTargetOutline(RectTransform target)
    {
        if (target == null) return;
        Graphic graphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>();
        if (graphic == null) return;
        targetOutline = graphic.gameObject.AddComponent<Outline>();
        targetOutline.effectColor = Color.yellow;
        targetOutline.effectDistance = new Vector2(4f, -4f);
    }

    private void RemoveTargetOutline()
    {
        if (targetOutline != null)
        {
            targetOutline.enabled = false;
            Destroy(targetOutline);
        }
        targetOutline = null;
    }

    private static GameObject FindDescendant(GameObject root, string objectName)
    {
        if (root == null) return null;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName) return child.gameObject;
        }
        return null;
    }
}
