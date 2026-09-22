using System;
using System.Collections.Generic;
using System.Linq;

public sealed class LibraryTutorialFlow
{
    public static readonly string[] Steps =
    {
        TutorialConstants.SeeOwnedCards,
        TutorialConstants.OpenCardDetail,
        TutorialConstants.ViewCardStats,
        TutorialConstants.ViewCardAttacks,
        TutorialConstants.BrowseCardAttacks,
        TutorialConstants.ReturnToCardFront,
        TutorialConstants.SwapDeckCard,
        TutorialConstants.LibraryComplete,
    };

    private static readonly string[] Actions =
    {
        "acknowledge", "open", "left", "up", "horizontal", "down", "swap", "acknowledge"
    };

    private HashSet<string> completed = new HashSet<string>();

    public int StepIndex { get; private set; }
    public string StepId => IsDone ? string.Empty : Steps[StepIndex];
    public bool IsDone => StepIndex >= Steps.Length;
    public bool IsSaving { get; private set; }
    public bool IsAwaitingAction { get; private set; }
    public bool IsInstructionReplayVisible { get; private set; }
    public bool IsInstructionVisible => !IsDone
        && !IsSaving
        && (!IsAwaitingAction || IsInstructionReplayVisible);
    public bool CanReplayInstruction => !IsDone
        && !IsSaving
        && IsAwaitingAction
        && !IsInstructionReplayVisible;

    public void Restore(IEnumerable<string> completedStepIds)
    {
        completed = new HashSet<string>(completedStepIds ?? Array.Empty<string>());
        StepIndex = 0;
        while (StepIndex < Steps.Length && completed.Contains(Steps[StepIndex]))
        {
            StepIndex++;
        }

        IsSaving = false;
        IsAwaitingAction = false;
        IsInstructionReplayVisible = false;
    }

    public bool Acknowledge()
    {
        if (!IsInstructionVisible)
        {
            return false;
        }

        if (IsInstructionReplayVisible)
        {
            IsInstructionReplayVisible = false;
            return false;
        }

        if (Actions[StepIndex] == "acknowledge")
        {
            IsSaving = true;
            return true;
        }

        IsAwaitingAction = true;
        return false;
    }

    public bool ShowInstructionAgain()
    {
        if (!CanReplayInstruction)
        {
            return false;
        }

        IsInstructionReplayVisible = true;
        return true;
    }

    public bool TryAction(string action)
    {
        if (IsDone
            || IsSaving
            || !IsAwaitingAction
            || IsInstructionReplayVisible
            || string.IsNullOrWhiteSpace(action))
        {
            return false;
        }

        string expected = Actions[StepIndex];
        bool matches = expected == action
            || (expected == "horizontal" && (action == "left" || action == "right"));
        if (!matches)
        {
            return false;
        }

        IsSaving = true;
        IsAwaitingAction = false;
        IsInstructionReplayVisible = false;
        return true;
    }
}

public sealed class LibraryTutorialTransitionGate
{
    private const float BrowseIdleDelaySeconds = 3f;
    private const float VisualResultDelaySeconds = 0.75f;

    private string activeStepId;
    private float lastInteractionTime;

    public bool TryBegin(string stepId, float currentTime)
    {
        if (string.IsNullOrWhiteSpace(stepId) || activeStepId != null)
        {
            return false;
        }

        activeStepId = stepId;
        lastInteractionTime = currentTime;
        return true;
    }

    public bool TryObserve(string stepId, string action, float currentTime)
    {
        if (activeStepId != stepId || !AllowsRepeatedHorizontalGesture(stepId, action))
        {
            return false;
        }

        lastInteractionTime = currentTime;
        return true;
    }

    public bool CanPresentNextStep(string stepId, float currentTime)
    {
        if (activeStepId == null)
        {
            return true;
        }

        return activeStepId == stepId
            && currentTime - lastInteractionTime >= GetDelaySeconds(stepId);
    }

    public void Reset()
    {
        activeStepId = null;
        lastInteractionTime = 0f;
    }

    private static bool AllowsRepeatedHorizontalGesture(string stepId, string action)
    {
        bool isBrowseStep = stepId == TutorialConstants.ViewCardStats
            || stepId == TutorialConstants.BrowseCardAttacks;
        return isBrowseStep && (action == "left" || action == "right");
    }

    private static float GetDelaySeconds(string stepId)
    {
        if (stepId == TutorialConstants.ViewCardStats
            || stepId == TutorialConstants.BrowseCardAttacks)
        {
            return BrowseIdleDelaySeconds;
        }

        if (stepId == TutorialConstants.OpenCardDetail
            || stepId == TutorialConstants.ViewCardAttacks
            || stepId == TutorialConstants.ReturnToCardFront)
        {
            return VisualResultDelaySeconds;
        }

        return 0f;
    }
}
