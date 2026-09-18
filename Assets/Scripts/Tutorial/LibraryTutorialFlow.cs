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
    public bool IsInstructionVisible => !IsDone && !IsSaving && !IsAwaitingAction;

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
    }

    public bool Acknowledge()
    {
        if (!IsInstructionVisible)
        {
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

    public bool TryAction(string action)
    {
        if (IsDone || IsSaving || !IsAwaitingAction || string.IsNullOrWhiteSpace(action))
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
        return true;
    }
}
