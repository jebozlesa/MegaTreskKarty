using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogControllerTests
{
    private GameObject panel;
    private Component dialog;
    private Component bubble;
    private Image artwork;

    [SetUp]
    public void SetUp()
    {
        panel = new GameObject("AuthoredDialog", typeof(RectTransform));
        panel.SetActive(false);
        dialog = panel.AddComponent(Type.GetType("ConfirmDialogController, Assembly-CSharp", true));
        var bubbleObject = new GameObject("Bubble", typeof(RectTransform));
        bubbleObject.transform.SetParent(panel.transform);
        bubble = bubbleObject.AddComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true));
        var artObject = new GameObject("Artwork", typeof(RectTransform), typeof(Image));
        artObject.transform.SetParent(panel.transform);
        artwork = artObject.GetComponent<Image>();
        artwork.color = Color.green;
        SetField("panelRoot", panel);
        SetField("messageText", bubble);
        SetField("defaultMessage", "Default confirmation");
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(panel);
    }

    [TestCase("Authored bubble copy")]
    [TestCase("")]
    public void ShowAuthoredPreservesTextAndArtworkOnEveryOpening(string copy)
    {
        SetText(copy);
        for (int opening = 0; opening < 2; opening++)
        {
            Invoke("ShowAuthored", (Action)(() => { }), null);
            Assert.IsTrue(panel.activeSelf);
            Assert.AreEqual(copy, Text());
            Assert.AreEqual(Color.green, artwork.color);
            Assert.AreSame(panel.transform, artwork.transform.parent);
            Invoke("Hide");
            Assert.IsFalse(panel.activeSelf);
            Assert.AreEqual(copy, Text());
        }
    }

    [Test]
    public void AuthoredConfirmHidesBeforeCallbackAndDoesNotRepeatOrLeakCancel()
    {
        int confirmed = 0, cancelled = 0;
        SetText("Fixed copy");
        Action confirm = () => { Assert.IsFalse(panel.activeSelf); confirmed++; };
        Action cancel = () => { Assert.IsFalse(panel.activeSelf); cancelled++; };
        Invoke("ShowAuthored", confirm, cancel);
        Invoke("Confirm");
        Invoke("Confirm");
        Invoke("Cancel");
        Assert.AreEqual(1, confirmed);
        Assert.AreEqual(0, cancelled);

        Invoke("ShowAuthored", confirm, cancel);
        Invoke("Cancel");
        Invoke("Cancel");
        Assert.AreEqual(1, confirmed);
        Assert.AreEqual(1, cancelled);

        Invoke("ShowAuthored", confirm, null);
        Invoke("Hide");
        Invoke("Confirm");
        Assert.AreEqual(1, confirmed);
        Assert.AreEqual("Fixed copy", Text());
    }

    [TestCase("Buy this pack?", "Buy this pack?")]
    [TestCase(null, "Default confirmation")]
    [TestCase("   ", "Default confirmation")]
    public void ExistingDynamicShowKeepsItsMessageAndConfirmationBehavior(string message, string expected)
    {
        int confirmed = 0;
        SetText("Original copy");
        Invoke("Show", message, (Action)(() => confirmed++), null);
        Assert.IsTrue(panel.activeSelf);
        Assert.AreEqual(expected, Text());
        Invoke("Confirm");
        Assert.IsFalse(panel.activeSelf);
        Assert.AreEqual(1, confirmed);
    }

    private void SetField(string name, object value)
    {
        FieldInfo field = dialog.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, name);
        field.SetValue(dialog, value);
    }

    private void SetText(string value) => bubble.GetType().GetProperty("text").SetValue(bubble, value);
    private string Text() => (string)bubble.GetType().GetProperty("text").GetValue(bubble);
    private void Invoke(string name, params object[] args)
    {
        MethodInfo method = dialog.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, name);
        method.Invoke(dialog, args);
    }
}
