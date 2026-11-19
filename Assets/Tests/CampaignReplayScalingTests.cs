using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class CampaignReplayScalingTests
{
    private GameObject root;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("CampaignReplayScalingTests");
    }

    [TearDown]
    public void TearDown()
    {
        if (root != null)
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void MissionManager_UnlocksOnlyCurrentCampaignLevelWithinVisibleCycle()
    {
        Type managerType = Runtime("MissionManager");
        Component manager = root.AddComponent(managerType);
        var buttons = new Button[10];
        var images = new Image[10];
        for (int index = 0; index < buttons.Length; index++)
        {
            var buttonObject = new GameObject($"Level {index + 1}");
            buttonObject.transform.SetParent(root.transform);
            images[index] = buttonObject.AddComponent<Image>();
            buttons[index] = buttonObject.AddComponent<Button>();
        }

        SetField(manager, "levelButtons", buttons);
        SetField(manager, "levelButtonImages", images);
        MethodInfo apply = managerType.GetMethod(
            "ApplyLevelAccess",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        Assert.IsNotNull(apply);
        apply.Invoke(manager, new object[] { 11, 11 });

        Assert.IsTrue(buttons[0].interactable);
        for (int index = 1; index < buttons.Length; index++)
        {
            Assert.IsFalse(buttons[index].interactable, $"Authored slot {index + 1} must stay locked.");
        }

        Assert.AreEqual(11, ReadStaticProperty<int>(Runtime("GameParameters"), "CampaignCycleStart"));
    }

    [TestCase(1, 11, 11)]
    [TestCase(10, 11, 20)]
    [TestCase(1, 21, 21)]
    public void PlayButton_MapsAuthoredMissionSlotToCampaignLevel(
        int missionId,
        int cycleStart,
        int expectedCampaignLevel
    )
    {
        MethodInfo resolve = Runtime("PlayButton").GetMethod(
            "ResolveCampaignLevel",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        Assert.IsNotNull(resolve);
        Assert.AreEqual(expectedCampaignLevel, resolve.Invoke(null, new object[] { missionId, cycleStart }));
    }

    private static Type Runtime(string typeName)
    {
        return Type.GetType(typeName + ", Assembly-CSharp", true);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );
        Assert.IsNotNull(field, fieldName);
        field.SetValue(target, value);
    }

    private static T ReadStaticProperty<T>(Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
        Assert.IsNotNull(property, propertyName);
        return (T)property.GetValue(null);
    }
}
