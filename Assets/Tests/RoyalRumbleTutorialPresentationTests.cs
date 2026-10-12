using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class RoyalRumbleTutorialPresentationTests
{
    private GameObject canvasObject;
    private GameObject controllerObject;
    private Component controller;
    private Type controllerType;
    private Type flowType;
    private Type customButtonType;
    private GameObject[] panels;
    private Component helpButton;
    private GameObject exitButton;
    private RectTransform handTarget;
    private RectTransform[] attackTargets;
    private RectTransform confirmTarget;
    private RectTransform recordTarget;

    [SetUp]
    public void SetUp()
    {
        controllerType = Type.GetType("RoyalRumbleTutorialController, Assembly-CSharp");
        flowType = Type.GetType("RoyalRumbleTutorialFlow, Assembly-CSharp");
        customButtonType = Type.GetType("CustomButton, Assembly-CSharp");
        Assert.NotNull(controllerType, "Royal Rumble tutorial controller must exist.");
        Assert.NotNull(flowType, "Royal Rumble tutorial state machine must exist.");
        Assert.NotNull(customButtonType, "CustomButton must exist.");

        canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;

        controllerObject = new GameObject("RoyalRumbleTutorialController");
        controllerObject.transform.SetParent(canvasObject.transform, false);
        controller = controllerObject.AddComponent(controllerType);

        panels = new GameObject[10];
        Array acknowledgementButtons = Array.CreateInstance(customButtonType, panels.Length);
        for (int index = 0; index < panels.Length; index++)
        {
            panels[index] = new GameObject($"TutorialPanel{index + 1}", typeof(RectTransform));
            panels[index].transform.SetParent(canvasObject.transform, false);
            Component button = CreateCustomButton($"Acknowledge{index + 1}");
            button.transform.SetParent(panels[index].transform, false);
            acknowledgementButtons.SetValue(button, index);
        }

        helpButton = CreateCustomButton("HelpButton");
        helpButton.transform.SetParent(canvasObject.transform, false);
        exitButton = new GameObject("BackButton", typeof(RectTransform));
        exitButton.transform.SetParent(canvasObject.transform, false);
        exitButton.SetActive(true);

        handTarget = CreateImageTarget("Player");
        confirmTarget = CreateImageTarget("ConfirmButton");
        recordTarget = CreateImageTarget("Record");
        attackTargets = new[]
        {
            CreateImageTarget("Attack1"),
            CreateImageTarget("Attack2"),
            CreateImageTarget("Attack3"),
            CreateImageTarget("Attack4"),
        };

        SetField("tutorialCanvas", canvas);
        SetField("hintPanels", panels);
        SetField("acknowledgementButtons", acknowledgementButtons);
        SetField("helpButton", helpButton);
        SetField("exitButton", exitButton);
        SetField("playerHandTarget", handTarget);
        SetField("attackButtonTargets", attackTargets);
        SetField("confirmButtonTarget", confirmTarget);
        SetField("recordTarget", recordTarget);
        SetField("initialized", true);
        SetField("battleSceneReady", true);
        SetField("originalExitVisible", true);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(canvasObject);
    }

    [Test]
    public void HandInstruction_RaisesOnlyPlayerHandAboveDimmer()
    {
        object flow = GetFlow();
        Restore(flow);
        CallFlow(flow, "AcknowledgeInstruction");

        InvokeController("Render");

        Assert.IsTrue(panels[1].activeSelf);
        Assert.IsFalse(panels[0].activeSelf);
        Assert.IsFalse(exitButton.activeSelf);
        Assert.IsNotNull(handTarget.GetComponent<Outline>());
        Assert.IsTrue(handTarget.GetComponent<Canvas>().overrideSorting);
        Assert.IsNull(recordTarget.GetComponent<Outline>());
    }

    [Test]
    public void PendingPlacement_KeepsOnlyPlayerHandHighlightedAndShowsHelp()
    {
        object flow = GetFlow();
        Restore(flow);
        CallFlow(flow, "AcknowledgeInstruction");
        CallFlow(flow, "AcknowledgeInstruction");
        CallFlow(flow, "AcknowledgeInstruction");

        InvokeController("Render");

        Assert.IsFalse(panels[2].activeSelf);
        Assert.IsTrue(helpButton.gameObject.activeSelf);
        Assert.IsNotNull(handTarget.GetComponent<Outline>());
        Assert.IsNull(recordTarget.GetComponent<Outline>());
    }

    [Test]
    public void AttackInstruction_RaisesAllFourAttackButtonsOnly()
    {
        object flow = GetFlow();
        AdvanceToAttackInstruction(flow);

        InvokeController("Render");

        Assert.IsTrue(panels[3].activeSelf);
        Assert.IsNull(handTarget.GetComponent<Outline>());
        Assert.IsNull(confirmTarget.GetComponent<Outline>());
        Assert.IsNull(recordTarget.GetComponent<Outline>());
        foreach (RectTransform attackTarget in attackTargets)
        {
            Assert.IsNotNull(attackTarget.GetComponent<Outline>());
            Assert.IsTrue(attackTarget.GetComponent<Canvas>().overrideSorting);
        }
    }

    [Test]
    public void RecordInstruction_RaisesRecordAboveDimmer()
    {
        object flow = GetFlow();
        Restore(flow, "select_player_card", "select_attack", "confirm_attack");
        AdvanceInformationalHint(flow, 3);

        InvokeController("Render");

        Assert.IsTrue(panels[8].activeSelf);
        Assert.IsNotNull(recordTarget.GetComponent<Outline>());
        Assert.IsTrue(recordTarget.GetComponent<Canvas>().overrideSorting);
        Assert.IsNull(handTarget.GetComponent<Outline>());
        Assert.IsNull(confirmTarget.GetComponent<Outline>());
        foreach (RectTransform attackTarget in attackTargets)
        {
            Assert.IsNull(attackTarget.GetComponent<Outline>());
        }
    }

    [Test]
    public void CompletedTutorial_RestoresExitAndRemovesTemporaryHighlights()
    {
        object flow = GetFlow();
        Restore(
            flow,
            "select_player_card",
            "select_attack",
            "confirm_attack",
            "complete_controls"
        );

        InvokeController("Render");

        Assert.IsTrue(exitButton.activeSelf);
        Assert.IsFalse(helpButton.gameObject.activeSelf);
        Assert.IsNull(handTarget.GetComponent<Outline>());
        Assert.IsNull(handTarget.GetComponent<Canvas>());
        Assert.IsNull(recordTarget.GetComponent<Outline>());
        Assert.IsNull(recordTarget.GetComponent<Canvas>());
        foreach (GameObject panel in panels)
        {
            Assert.IsFalse(panel.activeSelf);
        }
    }

    private Component CreateCustomButton(string objectName)
    {
        GameObject button = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        return button.AddComponent(customButtonType);
    }

    private RectTransform CreateImageTarget(string objectName)
    {
        GameObject target = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        target.transform.SetParent(canvasObject.transform, false);
        return target.GetComponent<RectTransform>();
    }

    private object GetFlow()
    {
        return controllerType
            .GetField("flow", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(controller);
    }

    private void SetField(string name, object value)
    {
        controllerType
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(controller, value);
    }

    private void InvokeController(string methodName)
    {
        controllerType
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(controller, null);
    }

    private object CallFlow(object targetFlow, string methodName, params object[] arguments)
    {
        return flowType.GetMethod(methodName).Invoke(targetFlow, arguments);
    }

    private void Restore(object targetFlow, params string[] completedSteps)
    {
        CallFlow(targetFlow, "Restore", (object)completedSteps);
    }

    private void AdvanceToAttackInstruction(object targetFlow)
    {
        Restore(targetFlow);
        CallFlow(targetFlow, "AcknowledgeInstruction");
        CallFlow(targetFlow, "AcknowledgeInstruction");
        CallFlow(targetFlow, "AcknowledgeInstruction");
        CallFlow(targetFlow, "BeginCardSelectionSave");
        CallFlow(targetFlow, "CompleteCardSelectionSave");
    }

    private void AdvanceInformationalHint(object targetFlow, int count)
    {
        for (int index = 0; index < count; index++)
        {
            CallFlow(targetFlow, "AcknowledgeInstruction");
            CallFlow(targetFlow, "CompleteHintTransition");
        }
    }
}
