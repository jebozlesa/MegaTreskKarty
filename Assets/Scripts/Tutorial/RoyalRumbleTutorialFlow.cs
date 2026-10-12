using System.Collections.Generic;

public enum RoyalRumbleTutorialStage
{
    Inactive,
    Instruction,
    AwaitingAction,
    Saving,
    Resolving,
    Transition,
    Complete,
}

public enum RoyalRumbleTutorialAcknowledgeResult
{
    Ignored,
    Advanced,
    AwaitingAction,
    ReplayClosed,
    TransitionRequested,
    CompletionRequested,
}

public sealed class RoyalRumbleTutorialFlow
{
    private const int WelcomeHintIndex = 0;
    private const int PlayerHandHintIndex = 1;
    private const int CardPlacementHintIndex = 2;
    private const int AttackSelectionHintIndex = 3;
    private const int AttackConfirmationHintIndex = 4;
    private const int FirstRuleHintIndex = 5;
    private const int FinalHintIndex = 9;

    private bool replayVisible;

    public int HintIndex { get; private set; } = -1;
    public RoyalRumbleTutorialStage Stage { get; private set; } = RoyalRumbleTutorialStage.Inactive;

    public bool IsActive => Stage != RoyalRumbleTutorialStage.Inactive
        && Stage != RoyalRumbleTutorialStage.Complete;
    public bool IsDone => Stage == RoyalRumbleTutorialStage.Complete;
    public bool IsInstructionVisible => Stage == RoyalRumbleTutorialStage.Instruction;
    public bool CanReplayInstruction => Stage == RoyalRumbleTutorialStage.AwaitingAction
        && IsActionHint(HintIndex);
    public bool AllowsCardPlacement => Stage == RoyalRumbleTutorialStage.AwaitingAction
        && HintIndex == CardPlacementHintIndex;
    public bool AllowsAttackSelection => Stage == RoyalRumbleTutorialStage.AwaitingAction
        && HintIndex == AttackSelectionHintIndex;
    public bool AllowsConfirmation => Stage == RoyalRumbleTutorialStage.AwaitingAction
        && HintIndex == AttackConfirmationHintIndex;
    public bool IsResolvingFirstRound => Stage == RoyalRumbleTutorialStage.Resolving
        && HintIndex == AttackConfirmationHintIndex;

    public void Restore(IEnumerable<string> completedStepIds)
    {
        var completed = completedStepIds == null
            ? new HashSet<string>()
            : new HashSet<string>(completedStepIds);

        replayVisible = false;
        if (completed.Contains(TutorialConstants.CompleteControls))
        {
            HintIndex = FinalHintIndex;
            Stage = RoyalRumbleTutorialStage.Complete;
            return;
        }

        HintIndex = completed.Contains(TutorialConstants.ConfirmAttack)
            ? FirstRuleHintIndex
            : WelcomeHintIndex;
        Stage = RoyalRumbleTutorialStage.Instruction;
    }

    public RoyalRumbleTutorialAcknowledgeResult AcknowledgeInstruction()
    {
        if (Stage != RoyalRumbleTutorialStage.Instruction)
        {
            return RoyalRumbleTutorialAcknowledgeResult.Ignored;
        }

        if (replayVisible)
        {
            replayVisible = false;
            Stage = RoyalRumbleTutorialStage.AwaitingAction;
            return RoyalRumbleTutorialAcknowledgeResult.ReplayClosed;
        }

        switch (HintIndex)
        {
            case WelcomeHintIndex:
            case PlayerHandHintIndex:
                HintIndex++;
                return RoyalRumbleTutorialAcknowledgeResult.Advanced;
            case CardPlacementHintIndex:
            case AttackSelectionHintIndex:
            case AttackConfirmationHintIndex:
                Stage = RoyalRumbleTutorialStage.AwaitingAction;
                return RoyalRumbleTutorialAcknowledgeResult.AwaitingAction;
            case FinalHintIndex:
                Stage = RoyalRumbleTutorialStage.Saving;
                return RoyalRumbleTutorialAcknowledgeResult.CompletionRequested;
            default:
                if (HintIndex >= FirstRuleHintIndex && HintIndex < FinalHintIndex)
                {
                    Stage = RoyalRumbleTutorialStage.Transition;
                    return RoyalRumbleTutorialAcknowledgeResult.TransitionRequested;
                }
                return RoyalRumbleTutorialAcknowledgeResult.Ignored;
        }
    }

    public bool ShowInstructionAgain()
    {
        if (!CanReplayInstruction)
        {
            return false;
        }

        replayVisible = true;
        Stage = RoyalRumbleTutorialStage.Instruction;
        return true;
    }

    public bool BeginCardSelectionSave()
    {
        return BeginSaveFromAction(CardPlacementHintIndex);
    }

    public void CompleteCardSelectionSave()
    {
        CompleteSave(CardPlacementHintIndex, AttackSelectionHintIndex);
    }

    public bool BeginAttackSelectionSave()
    {
        return BeginSaveFromAction(AttackSelectionHintIndex);
    }

    public void CompleteAttackSelectionSave()
    {
        CompleteSave(AttackSelectionHintIndex, AttackConfirmationHintIndex);
    }

    public bool BeginFirstRoundResolution()
    {
        if (!AllowsConfirmation)
        {
            return false;
        }

        Stage = RoyalRumbleTutorialStage.Resolving;
        return true;
    }

    public bool ReturnToConfirmationAfterFailure()
    {
        if (!IsResolvingFirstRound)
        {
            return false;
        }

        Stage = RoyalRumbleTutorialStage.AwaitingAction;
        return true;
    }

    public bool BeginFirstRoundSave()
    {
        if (!IsResolvingFirstRound)
        {
            return false;
        }

        Stage = RoyalRumbleTutorialStage.Saving;
        return true;
    }

    public void CompleteFirstRoundSave()
    {
        CompleteSave(AttackConfirmationHintIndex, FirstRuleHintIndex);
    }

    public bool CompleteHintTransition()
    {
        if (
            Stage != RoyalRumbleTutorialStage.Transition
            || HintIndex < FirstRuleHintIndex
            || HintIndex >= FinalHintIndex
        )
        {
            return false;
        }

        HintIndex++;
        Stage = RoyalRumbleTutorialStage.Instruction;
        return true;
    }

    public void CompleteTutorialSave()
    {
        if (Stage == RoyalRumbleTutorialStage.Saving && HintIndex == FinalHintIndex)
        {
            Stage = RoyalRumbleTutorialStage.Complete;
        }
    }

    public void ReturnToFinalInstructionAfterSaveFailure()
    {
        if (Stage == RoyalRumbleTutorialStage.Saving && HintIndex == FinalHintIndex)
        {
            Stage = RoyalRumbleTutorialStage.Instruction;
        }
    }

    private bool BeginSaveFromAction(int requiredHintIndex)
    {
        if (Stage != RoyalRumbleTutorialStage.AwaitingAction || HintIndex != requiredHintIndex)
        {
            return false;
        }

        Stage = RoyalRumbleTutorialStage.Saving;
        return true;
    }

    private void CompleteSave(int requiredHintIndex, int nextHintIndex)
    {
        if (Stage != RoyalRumbleTutorialStage.Saving || HintIndex != requiredHintIndex)
        {
            return;
        }

        HintIndex = nextHintIndex;
        Stage = RoyalRumbleTutorialStage.Instruction;
    }

    private static bool IsActionHint(int hintIndex)
    {
        return hintIndex >= CardPlacementHintIndex
            && hintIndex <= AttackConfirmationHintIndex;
    }
}
