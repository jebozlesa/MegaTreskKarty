using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class LoadingOverlayRegressionTests
{
    private readonly System.Collections.Generic.List<GameObject> createdObjects =
        new System.Collections.Generic.List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void Show_MakesOverlayVisibleWithoutNeedingSceneObjectActivation()
    {
        GameObject overlay = CreateOverlay("WAIT!!!", isActive: true, out _);
        CanvasGroup canvasGroup = overlay.GetComponent<CanvasGroup>();
        Assert.IsNotNull(canvasGroup, "LoadingOverlayView should require a CanvasGroup.");
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        InvokeOverlayMethod("Show");

        Assert.IsTrue(overlay.activeSelf);
        Assert.AreEqual(1f, canvasGroup.alpha);
        Assert.IsTrue(canvasGroup.blocksRaycasts);
    }

    [Test]
    public void Hide_MakesOverlayTransparentWithoutDeactivatingGameObject()
    {
        GameObject overlay = CreateOverlay("WAIT!!!", isActive: true, out _);
        CanvasGroup canvasGroup = overlay.GetComponent<CanvasGroup>();
        Assert.IsNotNull(canvasGroup, "LoadingOverlayView should require a CanvasGroup.");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        InvokeOverlayMethod("Hide");

        Assert.IsTrue(overlay.activeSelf);
        Assert.AreEqual(0f, canvasGroup.alpha);
        Assert.IsFalse(canvasGroup.blocksRaycasts);
    }

    [Test]
    public void SetMessage_UpdatesInactiveOverlayText()
    {
        CreateOverlay("WAIT!!!", isActive: false, out Component label);

        InvokeOverlayMethod("SetMessage", "CONNECTING...");

        Assert.AreEqual("CONNECTING...", ReadTextProperty(label));
    }

    [Test]
    public void LoadingPrefabYaml_StartsActiveButHidden_UsesExplicitMessageReference_AndDisablesRaycastTargets()
    {
        string prefabPath = Path.Combine(Application.dataPath, "Prefabs", "Loading.prefab");
        Assert.IsTrue(File.Exists(prefabPath), "Loading prefab file was not found.");

        string yaml = File.ReadAllText(prefabPath);

        StringAssert.Contains("m_IsActive: 1", ReadRootGameObjectYaml(yaml));
        StringAssert.Contains("--- !u!225", yaml);
        StringAssert.Contains("m_Alpha: 0", yaml);
        StringAssert.Contains("m_BlocksRaycasts: 0", yaml);
        StringAssert.Contains("playOnEnable: 0", yaml);
        StringAssert.Contains("guid: 2c4f4e91d41b4f7d9b0d5b9c3a5df321", yaml);
        StringAssert.Contains("messageText: {fileID: 3679588370953756678}", yaml);
        StringAssert.DoesNotContain("m_RaycastTarget: 1", yaml);
    }

    [Test]
    public void SceneYaml_DoesNotOverrideLoadingPrefabRootInactive()
    {
        string scenesPath = Path.Combine(Application.dataPath, "Scenes");
        Assert.IsTrue(Directory.Exists(scenesPath), "Scenes directory was not found.");

        string inactiveOverridePattern =
            @"target: \{fileID: 561146073754149366, guid: 545bec8b424f92f4aba22e383ec6f895, type: 3\}\s+propertyPath: m_IsActive\s+value: 0";
        foreach (string scenePath in Directory.GetFiles(scenesPath, "*.unity"))
        {
            string yaml = File.ReadAllText(scenePath);
            Assert.IsFalse(
                Regex.IsMatch(yaml, inactiveOverridePattern),
                $"{Path.GetFileName(scenePath)} overrides the Loading prefab root inactive."
            );
        }
    }

    private GameObject CreateOverlay(string initialMessage, bool isActive, out Component label)
    {
        GameObject overlay = new GameObject("Loading");
        createdObjects.Add(overlay);

        Type overlayViewType = FindAssemblyType("LoadingOverlayView");
        Assert.IsNotNull(
            overlayViewType,
            "Type LoadingOverlayView was not found in Assembly-CSharp."
        );
        Component view = overlay.AddComponent(overlayViewType);

        GameObject labelObject = new GameObject("Text (TMP)");
        createdObjects.Add(labelObject);
        labelObject.transform.SetParent(overlay.transform, false);
        Type textMeshProType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        Assert.IsNotNull(textMeshProType, "Type TMPro.TextMeshProUGUI was not found.");
        label = labelObject.AddComponent(textMeshProType);
        WriteTextProperty(label, initialMessage);

        FieldInfo messageField = overlayViewType.GetField(
            "messageText",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        Assert.IsNotNull(messageField, "LoadingOverlayView.messageText field was not found.");
        messageField.SetValue(view, label);

        overlay.SetActive(isActive);
        return overlay;
    }

    private static void InvokeOverlayMethod(string methodName, params object[] args)
    {
        Type overlayType = FindAssemblyType("SceneLoadingOverlay");
        Assert.IsNotNull(overlayType, "Type SceneLoadingOverlay was not found in Assembly-CSharp.");

        MethodInfo method = overlayType.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Static
        );
        Assert.IsNotNull(method, $"Method {methodName} was not found on SceneLoadingOverlay.");
        method.Invoke(null, args);
    }

    private static Type FindAssemblyType(string typeName)
    {
        return System.Type.GetType($"{typeName}, Assembly-CSharp");
    }

    private static string ReadRootGameObjectYaml(string yaml)
    {
        Match match = Regex.Match(yaml, @"--- !u!1 &561146073754149366(?<body>.*?)(?=--- !u!)", RegexOptions.Singleline);
        Assert.IsTrue(match.Success, "Loading prefab root GameObject YAML block was not found.");
        return match.Groups["body"].Value;
    }

    private static string ReadTextProperty(Component component)
    {
        PropertyInfo property = component
            .GetType()
            .GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"Property text was not found on {component.GetType().Name}.");
        return property.GetValue(component) as string;
    }

    private static void WriteTextProperty(Component component, string value)
    {
        PropertyInfo property = component
            .GetType()
            .GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"Property text was not found on {component.GetType().Name}.");
        property.SetValue(component, value);
    }
}
