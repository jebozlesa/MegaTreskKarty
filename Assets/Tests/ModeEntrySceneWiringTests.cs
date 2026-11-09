using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModeEntrySceneWiringTests
{
    [TestCase("MpDeckUnavailableDialog", "MP DECK")]
    [TestCase("BushidoDeckUnavailableDialog", "BUSHIDO DECK")]
    [TestCase("EntryCheckUnavailableDialog", "CHECK YOUR DECK")]
    public void AuthoredNoticeHasOneVisibleOkAndNoFullScreenDismissal(string name, string expectedCopy)
    {
        Type database = Type.GetType("UnityEditor.AssetDatabase, UnityEditor.CoreModule", true);
        MethodInfo load = database.GetMethod("LoadAssetAtPath", new[] { typeof(string), typeof(Type) });
        var prefab = (GameObject)load.Invoke(null, new object[] { "Assets/Prefabs/ModeEntry/" + name + ".prefab", typeof(GameObject) });
        Assert.IsNotNull(prefab, "Expected authored mode-entry prefab.");
        Assert.IsFalse(prefab.activeSelf);
        var rect = prefab.GetComponent<RectTransform>();
        Assert.AreEqual(Vector2.zero, rect.anchorMin);
        Assert.AreEqual(Vector2.one, rect.anchorMax);
        Assert.AreEqual(Vector3.one, rect.localScale);
        Component dialog = prefab.GetComponent(Type.GetType("ConfirmDialogController, Assembly-CSharp", true));
        Assert.IsNotNull(dialog);
        var text = (Component)Field(dialog, "messageText");
        StringAssert.Contains(expectedCopy, (string)text.GetType().GetProperty("text").GetValue(text));
        Button[] buttons = prefab.GetComponentsInChildren<Button>(true);
        Assert.AreEqual(1, buttons.Length);
        Component custom = buttons[0].GetComponent(Type.GetType("CustomButton, Assembly-CSharp", true));
        Assert.IsNotNull(custom);
        var click = (UnityEvent)custom.GetType().GetField("onDelayedClick").GetValue(custom);
        Assert.AreEqual(1, click.GetPersistentEventCount());
        Assert.AreEqual("Confirm", click.GetPersistentMethodName(0));
        Assert.AreSame(dialog, click.GetPersistentTarget(0));
        Assert.AreEqual(0, buttons[0].onClick.GetPersistentEventCount());
    }

    [Test]
    public void MainReplacesOnlyMpAndCampaignDirectSceneListeners()
    {
        string scene = File.ReadAllText(Path.Combine(Application.dataPath, "Scenes", "Main.unity"));
        Assert.AreEqual(2, Regex.Matches(scene, "m_MethodName: LoadSceneWithDeckCheck").Count);
        StringAssert.Contains("deckContextId: mp", scene);
        StringAssert.Contains("deckContextId: campaign:bushido", scene);
        foreach (Match match in Regex.Matches(scene, @"m_MethodName: (\w+)(?:(?!m_MethodName:)[\s\S])*?m_StringArgument: (MultiplayerLobby|Campaign)\r?\n"))
            Assert.AreEqual("LoadSceneWithDeckCheck", match.Groups[1].Value);
        StringAssert.Contains("m_StringArgument: Cards", scene);
        StringAssert.Contains("m_StringArgument: Settings", scene);
    }

    private static object Field(Component component, string name) => component.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component);
}
