public enum MarketplaceTutorialPhase
{
    Inactive,
    FirstHint,
    SavingFirstHint,
    BetweenHints,
    SecondHint,
    AwaitingPackSelection,
    Completed,
}

public sealed class MarketplaceTutorialFlow
{
    public MarketplaceTutorialPhase Phase { get; private set; } = MarketplaceTutorialPhase.Inactive;
    public bool IsActive => Phase != MarketplaceTutorialPhase.Inactive
        && Phase != MarketplaceTutorialPhase.Completed;
    public bool CanSelectPack => Phase == MarketplaceTutorialPhase.AwaitingPackSelection;

    public void Restore(bool stateIsValid, bool needsFirstPack, bool openMarketplaceCompleted)
    {
        if (!stateIsValid)
        {
            Phase = MarketplaceTutorialPhase.Inactive;
            return;
        }

        if (!needsFirstPack)
        {
            Phase = MarketplaceTutorialPhase.Completed;
            return;
        }

        Phase = openMarketplaceCompleted
            ? MarketplaceTutorialPhase.SecondHint
            : MarketplaceTutorialPhase.FirstHint;
    }

    public string AcknowledgeInstruction()
    {
        if (Phase == MarketplaceTutorialPhase.FirstHint)
        {
            Phase = MarketplaceTutorialPhase.SavingFirstHint;
            return TutorialConstants.OpenMarketplace;
        }

        if (Phase == MarketplaceTutorialPhase.SecondHint)
        {
            Phase = MarketplaceTutorialPhase.AwaitingPackSelection;
        }

        return null;
    }

    public bool BeginSecondHintTransition(
        bool stateIsValid,
        bool needsFirstPack,
        bool openMarketplaceCompleted
    )
    {
        if (
            Phase == MarketplaceTutorialPhase.SavingFirstHint
            && stateIsValid
            && needsFirstPack
            && openMarketplaceCompleted
        )
        {
            Phase = MarketplaceTutorialPhase.BetweenHints;
            return true;
        }

        Restore(stateIsValid, needsFirstPack, openMarketplaceCompleted);
        return false;
    }

    public void CompleteSecondHintTransition()
    {
        if (Phase == MarketplaceTutorialPhase.BetweenHints)
        {
            Phase = MarketplaceTutorialPhase.SecondHint;
        }
    }
}
