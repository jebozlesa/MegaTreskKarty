public enum MainMenuTutorialTarget
{
    None,
    Marketplace,
    Library,
    RoyalRumble,
}

public enum MainMenuTutorialPhase
{
    Inactive,
    IntroInstruction,
    IntroTransition,
    TargetInstruction,
    AwaitingTarget,
}

public sealed class MainMenuTutorialFlow
{
    public MainMenuTutorialTarget Target { get; private set; }
    public MainMenuTutorialPhase Phase { get; private set; }
    public bool IsActive => Phase != MainMenuTutorialPhase.Inactive;

    public void Restore(
        bool stateIsValid,
        bool needsFirstPack,
        bool needsLibraryDeckSwap,
        bool royalRumbleCompleted
    )
    {
        Target = ResolveTarget(
            stateIsValid,
            needsFirstPack,
            needsLibraryDeckSwap,
            royalRumbleCompleted
        );
        if (Target == MainMenuTutorialTarget.None)
        {
            Phase = MainMenuTutorialPhase.Inactive;
            return;
        }

        Phase = Target == MainMenuTutorialTarget.Marketplace
            ? MainMenuTutorialPhase.IntroInstruction
            : MainMenuTutorialPhase.TargetInstruction;
    }

    public void AcknowledgeInstruction()
    {
        if (Phase == MainMenuTutorialPhase.IntroInstruction)
        {
            Phase = MainMenuTutorialPhase.IntroTransition;
        }
        else if (Phase == MainMenuTutorialPhase.TargetInstruction)
        {
            Phase = MainMenuTutorialPhase.AwaitingTarget;
        }
    }

    public bool CompleteIntroTransition()
    {
        if (
            Phase != MainMenuTutorialPhase.IntroTransition
            || Target != MainMenuTutorialTarget.Marketplace
        )
        {
            return false;
        }

        Phase = MainMenuTutorialPhase.TargetInstruction;
        return true;
    }

    public bool CanNavigate(string sceneName)
    {
        if (!IsActive)
        {
            return true;
        }

        return Phase == MainMenuTutorialPhase.AwaitingTarget
            && IsTargetScene(sceneName);
    }

    private bool IsTargetScene(string sceneName)
    {
        switch (Target)
        {
            case MainMenuTutorialTarget.Marketplace:
                return sceneName == "Marketplace";
            case MainMenuTutorialTarget.Library:
                return sceneName == "Cards";
            case MainMenuTutorialTarget.RoyalRumble:
                return sceneName == "Game";
            default:
                return false;
        }
    }

    private static MainMenuTutorialTarget ResolveTarget(
        bool stateIsValid,
        bool needsFirstPack,
        bool needsLibraryDeckSwap,
        bool royalRumbleCompleted
    )
    {
        if (!stateIsValid)
        {
            return MainMenuTutorialTarget.None;
        }
        if (needsFirstPack)
        {
            return MainMenuTutorialTarget.Marketplace;
        }
        if (needsLibraryDeckSwap)
        {
            return MainMenuTutorialTarget.Library;
        }
        if (!royalRumbleCompleted)
        {
            return MainMenuTutorialTarget.RoyalRumble;
        }
        return MainMenuTutorialTarget.None;
    }
}
