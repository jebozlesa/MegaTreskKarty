using System;
using System.Reflection;
using NUnit.Framework;

public class LibraryTutorialFlowTests
{
    private object flow;
    private Type type;

    [SetUp]
    public void SetUp()
    {
        type = Type.GetType("LibraryTutorialFlow, Assembly-CSharp");
        Assert.NotNull(type, "Library tutorial state machine must exist.");
        flow = Activator.CreateInstance(type);
    }

    private object Call(string method, params object[] arguments) =>
        type.GetMethod(method).Invoke(flow, arguments);
    private T Get<T>(string property) => (T)type.GetProperty(property).GetValue(flow);

    [Test]
    public void WelcomeRequiresAcknowledgementAndServerConfirmation()
    {
        Call("Restore", (object)Array.Empty<string>());
        Assert.AreEqual("see_owned_cards", Get<string>("StepId"));
        Assert.False((bool)Call("TryAction", "open"));
        Assert.True((bool)Call("Acknowledge"));
        Assert.True(Get<bool>("IsSaving"));
        Assert.False((bool)Call("Acknowledge"));
        Assert.AreEqual("see_owned_cards", Get<string>("StepId"));
        Call("Restore", (object)new[] { "see_owned_cards" });
        Assert.AreEqual("open_card_detail", Get<string>("StepId"));
    }

    [TestCase(1, "open")]
    [TestCase(2, "left")]
    [TestCase(3, "up")]
    [TestCase(4, "left")]
    [TestCase(4, "right")]
    [TestCase(5, "down")]
    [TestCase(6, "swap")]
    public void ActionCannotSkipInstructionOrBeSubmittedTwice(int step, string action)
    {
        var steps = (string[])type.GetField("Steps").GetValue(null);
        Call("Restore", (object)steps[..step]);
        Assert.False((bool)Call("TryAction", action));
        Assert.False((bool)Call("Acknowledge"));
        Assert.False((bool)Call("TryAction", "unrelated"));
        Assert.True((bool)Call("TryAction", action));
        Assert.False((bool)Call("TryAction", action));
        Assert.True(Get<bool>("IsSaving"));
    }

    [Test]
    public void ResumeUsesFirstMissingStepAndDoesNotInferCompletion()
    {
        Call("Restore", (object)new[] { "see_owned_cards", "view_card_attacks", "complete" });
        Assert.AreEqual("open_card_detail", Get<string>("StepId"));
        Assert.False(Get<bool>("IsDone"));
    }

    [Test]
    public void CompletedAccountIsNotBlocked()
    {
        Call("Restore", (object)(string[])type.GetField("Steps").GetValue(null));
        Assert.True(Get<bool>("IsDone"));
        Assert.False((bool)Call("TryAction", "swap"));
    }

    [TestCase(true, 2, true, true)]
    [TestCase(true, 2, false, false)]
    [TestCase(true, 1, true, false)]
    [TestCase(false, 2, true, false)]
    public void ActivationUsesExplicitVersionedServerDecision(
        bool success,
        int contractVersion,
        bool serverShouldRun,
        bool expected
    )
    {
        Type policyType = Type.GetType("LibraryTutorialActivationPolicy, Assembly-CSharp");
        Type stateType = Type.GetType("TutorialStateResponse, Assembly-CSharp");
        Type libraryStateType = Type.GetType("LibraryTutorialStateDto, Assembly-CSharp");
        Assert.NotNull(policyType);
        Assert.NotNull(stateType);
        Assert.NotNull(libraryStateType);

        object state = Activator.CreateInstance(stateType);
        object libraryState = Activator.CreateInstance(libraryStateType);
        stateType.GetField("success").SetValue(state, success);
        stateType.GetField("tutorialContractVersion").SetValue(state, contractVersion);
        libraryStateType.GetField("shouldRun").SetValue(libraryState, serverShouldRun);
        stateType.GetField("libraryTutorial").SetValue(state, libraryState);

        bool actual = (bool)policyType.GetMethod("ShouldRun").Invoke(null, new[] { state });
        Assert.AreEqual(expected, actual);
    }
}
