using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

    [Test]
    public void PendingActionCanReplayInstructionWithoutChangingProgress()
    {
        Call("Restore", (object)new[] { "see_owned_cards" });
        Assert.AreEqual("open_card_detail", Get<string>("StepId"));
        Assert.False(Get<bool>("CanReplayInstruction"));

        Assert.False((bool)Call("Acknowledge"));
        Assert.True(Get<bool>("IsAwaitingAction"));
        Assert.True(Get<bool>("CanReplayInstruction"));

        Assert.True((bool)Call("ShowInstructionAgain"));
        Assert.True(Get<bool>("IsInstructionVisible"));
        Assert.True(Get<bool>("IsInstructionReplayVisible"));
        Assert.AreEqual("open_card_detail", Get<string>("StepId"));
        Assert.False((bool)Call("TryAction", "open"));

        Assert.False((bool)Call("Acknowledge"));
        Assert.False(Get<bool>("IsInstructionVisible"));
        Assert.False(Get<bool>("IsInstructionReplayVisible"));
        Assert.True(Get<bool>("IsAwaitingAction"));
        Assert.True((bool)Call("TryAction", "open"));
    }

    [Test]
    public void InstructionReplayIsUnavailableOutsidePendingAction()
    {
        Call("Restore", (object)Array.Empty<string>());
        Assert.False(Get<bool>("CanReplayInstruction"));
        Assert.False((bool)Call("ShowInstructionAgain"));

        Assert.True((bool)Call("Acknowledge"));
        Assert.True(Get<bool>("IsSaving"));
        Assert.False(Get<bool>("CanReplayInstruction"));
        Assert.False((bool)Call("ShowInstructionAgain"));

        Call("Restore", (object)(string[])type.GetField("Steps").GetValue(null));
        Assert.True(Get<bool>("IsDone"));
        Assert.False(Get<bool>("CanReplayInstruction"));
        Assert.False((bool)Call("ShowInstructionAgain"));
    }

    [UnityTest]
    public IEnumerator TutorialSpotlightRaisesTargetWithoutChangingItsHierarchy()
    {
        Type controllerType = Type.GetType("LibraryTutorialController, Assembly-CSharp");
        Assert.NotNull(controllerType);

        GameObject canvasObject = new GameObject("RootCanvas", typeof(Canvas));
        GameObject controllerObject = new GameObject("TutorialController");
        GameObject targetObject = new GameObject("Target", typeof(RectTransform));
        try
        {
            Canvas rootCanvas = canvasObject.GetComponent<Canvas>();
            rootCanvas.overrideSorting = true;
            rootCanvas.sortingOrder = 7;
            targetObject.transform.SetParent(canvasObject.transform, false);
            Transform originalParent = targetObject.transform.parent;
            int originalSiblingIndex = targetObject.transform.GetSiblingIndex();

            object controller = controllerObject.AddComponent(controllerType);
            MethodInfo setSpotlight = controllerType.GetMethod(
                "SetSpotlight",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.NotNull(setSpotlight);

            setSpotlight.Invoke(controller, new object[] { targetObject.GetComponent<RectTransform>() });

            Canvas targetCanvas = targetObject.GetComponent<Canvas>();
            Assert.NotNull(targetCanvas);
            Assert.True(targetCanvas.overrideSorting);
            Assert.Greater(targetCanvas.sortingOrder, rootCanvas.sortingOrder);
            Assert.AreSame(originalParent, targetObject.transform.parent);
            Assert.AreEqual(originalSiblingIndex, targetObject.transform.GetSiblingIndex());

            setSpotlight.Invoke(controller, new object[] { null });
            Assert.False(targetCanvas.enabled, "A removed spotlight must stop rendering immediately.");
            yield return null;
            Assert.IsNull(targetObject.GetComponent<Canvas>());
            Assert.AreSame(originalParent, targetObject.transform.parent);
            Assert.AreEqual(originalSiblingIndex, targetObject.transform.GetSiblingIndex());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
            UnityEngine.Object.DestroyImmediate(canvasObject);
        }
    }

    [Test]
    public void TutorialSpotlightRestoresAnExistingCanvas()
    {
        Type controllerType = Type.GetType("LibraryTutorialController, Assembly-CSharp");
        Assert.NotNull(controllerType);

        GameObject rootObject = new GameObject("RootCanvas", typeof(Canvas));
        GameObject controllerObject = new GameObject("TutorialController");
        GameObject targetObject = new GameObject("Target", typeof(RectTransform), typeof(Canvas));
        try
        {
            targetObject.transform.SetParent(rootObject.transform, false);
            Canvas targetCanvas = targetObject.GetComponent<Canvas>();
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingOrder = 23;
            bool originalOverrideSorting = targetCanvas.overrideSorting;
            int originalSortingOrder = targetCanvas.sortingOrder;
            int originalSortingLayerId = targetCanvas.sortingLayerID;

            object controller = controllerObject.AddComponent(controllerType);
            MethodInfo setSpotlight = controllerType.GetMethod(
                "SetSpotlight",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            setSpotlight.Invoke(controller, new object[] { targetObject.GetComponent<RectTransform>() });
            setSpotlight.Invoke(controller, new object[] { null });

            Assert.AreSame(targetCanvas, targetObject.GetComponent<Canvas>());
            Assert.AreEqual(originalOverrideSorting, targetCanvas.overrideSorting);
            Assert.AreEqual(originalSortingOrder, targetCanvas.sortingOrder);
            Assert.AreEqual(originalSortingLayerId, targetCanvas.sortingLayerID);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
            UnityEngine.Object.DestroyImmediate(rootObject);
        }
    }

    [Test]
    public void TutorialInputBlockerAllowsActionAndHelpTargetsOnly()
    {
        Type blockerType = Type.GetType("LibraryTutorialInputBlocker, Assembly-CSharp");
        Assert.NotNull(blockerType);

        GameObject blockerObject = new GameObject("Blocker", typeof(RectTransform));
        GameObject actionObject = new GameObject("Action", typeof(RectTransform));
        GameObject helpObject = new GameObject("Help", typeof(RectTransform));
        try
        {
            RectTransform action = actionObject.GetComponent<RectTransform>();
            action.position = Vector3.zero;
            action.sizeDelta = new Vector2(100f, 100f);

            RectTransform help = helpObject.GetComponent<RectTransform>();
            help.position = new Vector3(200f, 0f, 0f);
            help.sizeDelta = new Vector2(100f, 100f);

            object blocker = blockerObject.AddComponent(blockerType);
            blockerType.GetProperty("AllowedTarget").SetValue(blocker, action);
            blockerType.GetProperty("SecondaryAllowedTarget").SetValue(blocker, help);
            MethodInfo isValid = blockerType.GetMethod("IsRaycastLocationValid");

            Assert.False((bool)isValid.Invoke(blocker, new object[] { Vector2.zero, null }));
            Assert.False((bool)isValid.Invoke(blocker, new object[] { new Vector2(200f, 0f), null }));
            Assert.True((bool)isValid.Invoke(blocker, new object[] { new Vector2(500f, 500f), null }));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(blockerObject);
            UnityEngine.Object.DestroyImmediate(actionObject);
            UnityEngine.Object.DestroyImmediate(helpObject);
        }
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

    [TestCase(true, false, false, true)]
    [TestCase(true, false, true, false)]
    [TestCase(false, false, false, false)]
    [TestCase(true, true, false, false)]
    public void TutorialOwnedCardPointerPathExcludesDeckCards(
        bool tutorialActive,
        bool isZoomed,
        bool isDeckCard,
        bool expected
    )
    {
        Type cardType = Type.GetType("Card, Assembly-CSharp");
        Assert.NotNull(cardType);

        MethodInfo policy = cardType.GetMethod(
            "ShouldHandleTutorialOwnedCardPointer",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        Assert.NotNull(policy, "Card pointer routing policy must remain independently testable.");

        bool actual = (bool)policy.Invoke(null, new object[] { tutorialActive, isZoomed, isDeckCard });
        Assert.AreEqual(expected, actual);
    }

    [TestCase("open_card_detail", true)]
    [TestCase("view_card_stats", false)]
    [TestCase("view_card_attacks", false)]
    [TestCase("browse_card_attacks", false)]
    [TestCase("return_to_card_front", false)]
    [TestCase("swap_deck_card", true)]
    public void ActionTargetHighlightOnlyMarksTheRequiredSelection(string stepId, bool expected)
    {
        Type controllerType = Type.GetType("LibraryTutorialController, Assembly-CSharp");
        Assert.NotNull(controllerType);

        MethodInfo policy = controllerType.GetMethod(
            "ShouldHighlightActionTarget",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        Assert.NotNull(policy, "Tutorial action highlight policy must remain independently testable.");

        bool actual = (bool)policy.Invoke(null, new object[] { stepId });
        Assert.AreEqual(expected, actual);
    }

    [TestCase("view_card_stats", "left")]
    [TestCase("view_card_stats", "right")]
    [TestCase("browse_card_attacks", "left")]
    [TestCase("browse_card_attacks", "right")]
    public void BrowseObservationWaitsThreeSecondsAfterLastHorizontalSwipe(string stepId, string action)
    {
        object gate = CreateTransitionGate(out Type gateType);

        Assert.True((bool)gateType.GetMethod("TryBegin").Invoke(gate, new object[] { stepId, 10f }));
        Assert.False((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 12.99f }));
        Assert.True((bool)gateType.GetMethod("TryObserve").Invoke(gate, new object[] { stepId, action, 12.5f }));
        Assert.False((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 15.49f }));
        Assert.True((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 15.5f }));
    }

    [TestCase("open_card_detail")]
    [TestCase("view_card_attacks")]
    [TestCase("return_to_card_front")]
    public void VisualActionKeepsItsResultVisibleForThreeQuartersOfASecond(string stepId)
    {
        object gate = CreateTransitionGate(out Type gateType);

        Assert.True((bool)gateType.GetMethod("TryBegin").Invoke(gate, new object[] { stepId, 20f }));
        Assert.False((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 20.74f }));
        Assert.True((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 20.75f }));
    }

    [Test]
    public void UnrelatedGestureDoesNotExtendBrowseObservation()
    {
        object gate = CreateTransitionGate(out Type gateType);
        const string stepId = "browse_card_attacks";

        Assert.True((bool)gateType.GetMethod("TryBegin").Invoke(gate, new object[] { stepId, 5f }));
        Assert.False((bool)gateType.GetMethod("TryObserve").Invoke(gate, new object[] { stepId, "up", 7.5f }));
        Assert.True((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 8f }));
    }

    [Test]
    public void ActiveTransitionCannotBeRestartedOrExtendedByDuplicateCompletion()
    {
        object gate = CreateTransitionGate(out Type gateType);
        const string stepId = "browse_card_attacks";

        Assert.True((bool)gateType.GetMethod("TryBegin").Invoke(gate, new object[] { stepId, 5f }));
        Assert.False((bool)gateType.GetMethod("TryBegin").Invoke(gate, new object[] { stepId, 7f }));
        Assert.True((bool)gateType.GetMethod("CanPresentNextStep").Invoke(gate, new object[] { stepId, 8f }));
    }

    private static object CreateTransitionGate(out Type gateType)
    {
        gateType = Type.GetType("LibraryTutorialTransitionGate, Assembly-CSharp");
        Assert.NotNull(gateType, "Library tutorial transition gate must exist.");
        return Activator.CreateInstance(gateType);
    }
}
