using System;
using NUnit.Framework;

public class RoyalRumbleTutorialFlowTests
{
    private const string SelectPlayerCard = "select_player_card";
    private const string SelectAttack = "select_attack";
    private const string ConfirmAttack = "confirm_attack";
    private const string CompleteControls = "complete_controls";

    private object flow;
    private Type flowType;

    [SetUp]
    public void SetUp()
    {
        flowType = Type.GetType("RoyalRumbleTutorialFlow, Assembly-CSharp");
        Assert.NotNull(flowType, "Royal Rumble tutorial state machine must exist.");
        flow = Activator.CreateInstance(flowType);
    }

    [Test]
    public void FreshProgress_StartsAtWelcomeInstruction()
    {
        Restore();

        Assert.IsTrue(Get<bool>("IsActive"));
        Assert.AreEqual(0, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());
        Assert.IsTrue(Get<bool>("IsInstructionVisible"));
        Assert.IsFalse(Get<bool>("AllowsCardPlacement"));
    }

    [Test]
    public void CompletedFirstRound_ResumesAtRuleExplanation()
    {
        Restore(SelectPlayerCard, SelectAttack, ConfirmAttack);

        Assert.IsTrue(Get<bool>("IsActive"));
        Assert.AreEqual(5, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());
    }

    [Test]
    public void PartialControlsWithoutCompletedRound_RestartFromWelcome()
    {
        Restore(SelectPlayerCard, SelectAttack);

        Assert.AreEqual(0, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());
    }

    [Test]
    public void CompletedFlow_IsInactiveAndDone()
    {
        Restore(SelectPlayerCard, SelectAttack, ConfirmAttack, CompleteControls);

        Assert.IsFalse(Get<bool>("IsActive"));
        Assert.IsTrue(Get<bool>("IsDone"));
        Assert.AreEqual("Complete", GetStage());
    }

    [Test]
    public void InteractionLesson_RequiresAcknowledgementThenRealActions()
    {
        Restore();

        Assert.AreEqual("Advanced", Acknowledge());
        Assert.AreEqual(1, Get<int>("HintIndex"));
        Assert.AreEqual("Advanced", Acknowledge());
        Assert.AreEqual(2, Get<int>("HintIndex"));
        Assert.AreEqual("AwaitingAction", Acknowledge());
        Assert.IsTrue(Get<bool>("AllowsCardPlacement"));
        Assert.IsTrue(Get<bool>("CanReplayInstruction"));

        Assert.IsTrue((bool)Call("BeginCardSelectionSave"));
        Assert.AreEqual("Saving", GetStage());
        Call("CompleteCardSelectionSave");
        Assert.AreEqual(3, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());

        Assert.AreEqual("AwaitingAction", Acknowledge());
        Assert.IsTrue(Get<bool>("AllowsAttackSelection"));
        Assert.IsTrue((bool)Call("BeginAttackSelectionSave"));
        Call("CompleteAttackSelectionSave");
        Assert.AreEqual(4, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());

        Assert.AreEqual("AwaitingAction", Acknowledge());
        Assert.IsTrue(Get<bool>("AllowsConfirmation"));
        Assert.IsTrue((bool)Call("BeginFirstRoundResolution"));
        Assert.AreEqual("Resolving", GetStage());
        Assert.IsTrue((bool)Call("BeginFirstRoundSave"));
        Call("CompleteFirstRoundSave");

        Assert.AreEqual(5, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());
    }

    [Test]
    public void HelpReplay_ReopensOnlyPendingActionAndDoesNotAdvance()
    {
        Restore();
        AdvanceToCardPlacement();

        Assert.IsTrue((bool)Call("ShowInstructionAgain"));
        Assert.IsTrue(Get<bool>("IsInstructionVisible"));
        Assert.IsFalse(Get<bool>("AllowsCardPlacement"));
        Assert.AreEqual(2, Get<int>("HintIndex"));

        Assert.AreEqual("ReplayClosed", Acknowledge());
        Assert.IsFalse(Get<bool>("IsInstructionVisible"));
        Assert.IsTrue(Get<bool>("AllowsCardPlacement"));
        Assert.AreEqual(2, Get<int>("HintIndex"));
    }

    [Test]
    public void FailedAttackRequest_ReturnsToConfirmAction()
    {
        Restore();
        AdvanceToConfirmAction();

        Assert.IsTrue((bool)Call("BeginFirstRoundResolution"));
        Assert.IsTrue((bool)Call("ReturnToConfirmationAfterFailure"));

        Assert.AreEqual(4, Get<int>("HintIndex"));
        Assert.AreEqual("AwaitingAction", GetStage());
        Assert.IsTrue(Get<bool>("AllowsConfirmation"));
        Assert.IsTrue(Get<bool>("CanReplayInstruction"));
    }

    [Test]
    public void RuleHints_UseDelayedTransitionsAndFinalSave()
    {
        Restore(SelectPlayerCard, SelectAttack, ConfirmAttack);

        for (int expectedHint = 6; expectedHint <= 9; expectedHint++)
        {
            Assert.AreEqual("TransitionRequested", Acknowledge());
            Assert.AreEqual("Transition", GetStage());
            Assert.IsTrue((bool)Call("CompleteHintTransition"));
            Assert.AreEqual(expectedHint, Get<int>("HintIndex"));
            Assert.AreEqual("Instruction", GetStage());
        }

        Assert.AreEqual("CompletionRequested", Acknowledge());
        Assert.AreEqual("Saving", GetStage());

        Call("CompleteTutorialSave");

        Assert.IsTrue(Get<bool>("IsDone"));
        Assert.IsFalse(Get<bool>("IsActive"));
    }

    [Test]
    public void ActionsCannotRunBeforeTheirApprovedPhase()
    {
        Restore();

        Assert.IsFalse((bool)Call("BeginCardSelectionSave"));
        Assert.IsFalse((bool)Call("BeginAttackSelectionSave"));
        Assert.IsFalse((bool)Call("BeginFirstRoundResolution"));
        Assert.IsFalse((bool)Call("BeginFirstRoundSave"));
        Assert.IsFalse((bool)Call("ShowInstructionAgain"));
    }

    [Test]
    public void FailedFinalSave_ReturnsToFinalInstructionForRetry()
    {
        Restore(SelectPlayerCard, SelectAttack, ConfirmAttack);

        for (int hint = 5; hint < 9; hint++)
        {
            Acknowledge();
            Call("CompleteHintTransition");
        }

        Assert.AreEqual("CompletionRequested", Acknowledge());

        Call("ReturnToFinalInstructionAfterSaveFailure");

        Assert.AreEqual(9, Get<int>("HintIndex"));
        Assert.AreEqual("Instruction", GetStage());
        Assert.IsTrue(Get<bool>("IsInstructionVisible"));
    }

    private object Call(string method, params object[] arguments)
    {
        return flowType.GetMethod(method).Invoke(flow, arguments);
    }

    private T Get<T>(string property)
    {
        return (T)flowType.GetProperty(property).GetValue(flow);
    }

    private string GetStage()
    {
        return Get<object>("Stage").ToString();
    }

    private string Acknowledge()
    {
        return Call("AcknowledgeInstruction").ToString();
    }

    private void Restore(params string[] completedSteps)
    {
        Call("Restore", (object)completedSteps);
    }

    private void AdvanceToCardPlacement()
    {
        Acknowledge();
        Acknowledge();
        Acknowledge();
    }

    private void AdvanceToConfirmAction()
    {
        AdvanceToCardPlacement();
        Call("BeginCardSelectionSave");
        Call("CompleteCardSelectionSave");
        Acknowledge();
        Call("BeginAttackSelectionSave");
        Call("CompleteAttackSelectionSave");
        Acknowledge();
    }
}
