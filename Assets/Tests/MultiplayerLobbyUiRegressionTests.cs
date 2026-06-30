using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerLobbyUiRegressionTests
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
    public void SetJoinButtonText_UpdatesTmpLabel()
    {
        Component lobby = CreateLobbyComponent();
        GameObject labelObject = new GameObject("TmpLabel");
        createdObjects.Add(labelObject);
        Type textMeshProType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        Assert.IsNotNull(textMeshProType, "Type TMPro.TextMeshProUGUI was not found.");
        Component label = labelObject.AddComponent(textMeshProType);

        SetPrivateField(lobby, "joinButtonLabel", label);
        InvokeSetJoinButtonText(lobby, "CONNECT");

        Assert.AreEqual("CONNECT", ReadTextProperty(label));
    }

    [Test]
    public void SetJoinButtonText_UpdatesLegacyLabel_WhenTmpLabelIsMissing()
    {
        Component lobby = CreateLobbyComponent();
        GameObject labelObject = new GameObject("LegacyLabel");
        createdObjects.Add(labelObject);
        Text label = labelObject.AddComponent<Text>();

        SetPrivateField(lobby, "joinButtonLabel", null);
        SetPrivateField(lobby, "joinButtonLegacyLabel", label);
        InvokeSetJoinButtonText(lobby, "CANCEL");

        Assert.AreEqual("CANCEL", label.text);
    }

    private Component CreateLobbyComponent()
    {
        GameObject root = new GameObject("LobbyUiTest");
        createdObjects.Add(root);

        Type lobbyType = System.Type.GetType("MultiplayerLobbyUI, Assembly-CSharp");
        Assert.IsNotNull(lobbyType, "Type MultiplayerLobbyUI was not found in Assembly-CSharp.");
        return root.AddComponent(lobbyType);
    }

    private static void InvokeSetJoinButtonText(object lobby, string text)
    {
        MethodInfo method = lobby
            .GetType()
            .GetMethod("SetJoinButtonText", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, "Method SetJoinButtonText was not found.");
        method.Invoke(lobby, new object[] { text });
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        FieldInfo field = instance
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Field {fieldName} was not found.");
        field.SetValue(instance, value);
    }

    private static string ReadTextProperty(Component component)
    {
        PropertyInfo property = component
            .GetType()
            .GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"Property text was not found on {component.GetType().Name}.");
        return property.GetValue(component) as string;
    }
}

public class LeaderboardSceneRegressionTests
{
    [Test]
    public void PlayFabManagerLeaderboard_IsSceneLocal()
    {
        string source = ReadSource("Leaderboard", "PlayFabManagerLeaderboard.cs");

        StringAssert.DoesNotContain(
            "DontDestroyOnLoad",
            source,
            "Leaderboard scene manager must stay scene-local; DontDestroyOnLoad only works on root objects."
        );
        StringAssert.DoesNotContain(
            "static PlayFabManagerLeaderboard Instance",
            source,
            "Leaderboard scene manager should not own a persistent singleton instance."
        );
    }

    [Test]
    public void RecordsLoaderOwnsLeaderboardRequestAfterSubscribing()
    {
        string source = ReadSource("Leaderboard", "RecordsLoader.cs");

        StringAssert.Contains(
            "OnLeaderboardLoaded +=",
            source,
            "RecordsLoader should subscribe before requesting leaderboard data."
        );
        StringAssert.Contains(
            "GetLeaderboard()",
            source,
            "RecordsLoader should request leaderboard data after it is ready to receive the event."
        );
        StringAssert.Contains(
            "OnLeaderboardLoaded -=",
            source,
            "RecordsLoader should unsubscribe when destroyed."
        );
    }

    private static string ReadSource(params string[] relativeParts)
    {
        string path = Path.Combine(Application.dataPath, "Scripts", Path.Combine(relativeParts));
        Assert.IsTrue(File.Exists(path), $"{Path.GetFileName(path)} source was not found.");
        return File.ReadAllText(path);
    }
}
