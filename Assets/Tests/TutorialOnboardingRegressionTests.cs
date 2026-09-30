using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class TutorialOnboardingRegressionTests
{
    [SetUp]
    public void SetUp()
    {
        ClearTutorialSessionState();
    }

    [TearDown]
    public void TearDown()
    {
        ClearTutorialSessionState();
    }

    [Test]
    public void TutorialSessionState_IsScopedToTheCurrentPlayer()
    {
        Type cacheType = RuntimeType("TutorialSessionState");
        Type stateType = RuntimeType("TutorialStateResponse");
        object state = Activator.CreateInstance(stateType);
        stateType.GetField("success").SetValue(state, true);
        stateType.GetField("playerId").SetValue(state, "player-a");

        cacheType.GetMethod("Store").Invoke(null, new[] { "player-a", state });

        object[] matchingArguments = { "player-a", null };
        Assert.IsTrue((bool)cacheType.GetMethod("TryGet").Invoke(null, matchingArguments));
        Assert.AreSame(state, matchingArguments[1]);

        object[] otherArguments = { "player-b", null };
        Assert.IsFalse((bool)cacheType.GetMethod("TryGet").Invoke(null, otherArguments));

        cacheType.GetMethod("Clear").Invoke(null, null);
        matchingArguments[1] = null;
        Assert.IsFalse((bool)cacheType.GetMethod("TryGet").Invoke(null, matchingArguments));
    }

    [Test]
    public void MarketplaceFlow_RestoresAndRequiresTheRealPurchaseAction()
    {
        Type flowType = RuntimeType("MarketplaceTutorialFlow");
        object flow = Activator.CreateInstance(flowType);

        flowType.GetMethod("Restore").Invoke(flow, new object[] { true, true, false });
        Assert.AreEqual("FirstHint", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)ReadProperty(flowType, flow, "CanSelectPack"));

        Assert.AreEqual("open_marketplace", flowType.GetMethod("AcknowledgeInstruction").Invoke(flow, null));
        Assert.AreEqual("SavingFirstHint", ReadProperty(flowType, flow, "Phase").ToString());

        MethodInfo beginTransition = flowType.GetMethod("BeginSecondHintTransition");
        Assert.NotNull(beginTransition);
        Assert.IsTrue((bool)beginTransition.Invoke(flow, new object[] { true, true, true }));
        Assert.AreEqual("BetweenHints", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)ReadProperty(flowType, flow, "CanSelectPack"));

        MethodInfo completeTransition = flowType.GetMethod("CompleteSecondHintTransition");
        Assert.NotNull(completeTransition);
        completeTransition.Invoke(flow, null);
        Assert.AreEqual("SecondHint", ReadProperty(flowType, flow, "Phase").ToString());

        Assert.IsNull(flowType.GetMethod("AcknowledgeInstruction").Invoke(flow, null));
        Assert.AreEqual("AwaitingPackSelection", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsTrue((bool)ReadProperty(flowType, flow, "CanSelectPack"));

        flowType.GetMethod("Restore").Invoke(flow, new object[] { true, false, true });
        Assert.AreEqual("Completed", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)ReadProperty(flowType, flow, "CanSelectPack"));
    }

    [Test]
    public void MainMenuFlow_RequiresIntroBeforeTheMarketplaceDestination()
    {
        Type flowType = RuntimeType("MainMenuTutorialFlow");
        object flow = Activator.CreateInstance(flowType);
        MethodInfo restore = flowType.GetMethod("Restore");
        MethodInfo acknowledge = flowType.GetMethod("AcknowledgeInstruction");
        MethodInfo completeIntroTransition = flowType.GetMethod("CompleteIntroTransition");
        MethodInfo canNavigate = flowType.GetMethod("CanNavigate");

        restore.Invoke(flow, new object[] { true, true, false, false });
        Assert.AreEqual("Marketplace", ReadProperty(flowType, flow, "Target").ToString());
        Assert.AreEqual("IntroInstruction", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)canNavigate.Invoke(flow, new object[] { "Marketplace" }));

        acknowledge.Invoke(flow, null);
        Assert.AreEqual("IntroTransition", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)canNavigate.Invoke(flow, new object[] { "Marketplace" }));

        Assert.IsTrue((bool)completeIntroTransition.Invoke(flow, null));
        Assert.AreEqual("TargetInstruction", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)canNavigate.Invoke(flow, new object[] { "Marketplace" }));

        acknowledge.Invoke(flow, null);
        Assert.AreEqual("AwaitingTarget", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsTrue((bool)canNavigate.Invoke(flow, new object[] { "Marketplace" }));
        Assert.IsFalse((bool)canNavigate.Invoke(flow, new object[] { "Cards" }));
    }

    [Test]
    public void MainMenuFlow_UsesOnlyTheCurrentCanonicalDestination()
    {
        Type flowType = RuntimeType("MainMenuTutorialFlow");
        object flow = Activator.CreateInstance(flowType);
        MethodInfo restore = flowType.GetMethod("Restore");
        MethodInfo acknowledge = flowType.GetMethod("AcknowledgeInstruction");
        MethodInfo completeIntroTransition = flowType.GetMethod("CompleteIntroTransition");
        MethodInfo canNavigate = flowType.GetMethod("CanNavigate");

        restore.Invoke(flow, new object[] { true, false, true, false });
        Assert.AreEqual("Library", ReadProperty(flowType, flow, "Target").ToString());
        Assert.AreEqual("TargetInstruction", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsFalse((bool)completeIntroTransition.Invoke(flow, null));
        acknowledge.Invoke(flow, null);
        Assert.IsTrue((bool)canNavigate.Invoke(flow, new object[] { "Cards" }));
        Assert.IsFalse((bool)canNavigate.Invoke(flow, new object[] { "Game" }));

        restore.Invoke(flow, new object[] { true, false, false, false });
        Assert.AreEqual("RoyalRumble", ReadProperty(flowType, flow, "Target").ToString());
        Assert.AreEqual("TargetInstruction", ReadProperty(flowType, flow, "Phase").ToString());
        acknowledge.Invoke(flow, null);
        Assert.IsTrue((bool)canNavigate.Invoke(flow, new object[] { "Game" }));

        restore.Invoke(flow, new object[] { true, false, false, true });
        Assert.AreEqual("None", ReadProperty(flowType, flow, "Target").ToString());
        Assert.AreEqual("Inactive", ReadProperty(flowType, flow, "Phase").ToString());
        Assert.IsTrue((bool)canNavigate.Invoke(flow, new object[] { "Settings" }));
    }

    [Test]
    public void MainMenuFlow_IgnoresDuplicateOrOutOfOrderAcknowledgements()
    {
        Type flowType = RuntimeType("MainMenuTutorialFlow");
        object flow = Activator.CreateInstance(flowType);
        MethodInfo restore = flowType.GetMethod("Restore");
        MethodInfo acknowledge = flowType.GetMethod("AcknowledgeInstruction");
        MethodInfo completeIntroTransition = flowType.GetMethod("CompleteIntroTransition");

        restore.Invoke(flow, new object[] { true, true, false, false });
        Assert.IsFalse((bool)completeIntroTransition.Invoke(flow, null));

        acknowledge.Invoke(flow, null);
        acknowledge.Invoke(flow, null);
        Assert.AreEqual("IntroTransition", ReadProperty(flowType, flow, "Phase").ToString());

        Assert.IsTrue((bool)completeIntroTransition.Invoke(flow, null));
        Assert.IsFalse((bool)completeIntroTransition.Invoke(flow, null));
        acknowledge.Invoke(flow, null);
        acknowledge.Invoke(flow, null);
        Assert.AreEqual("AwaitingTarget", ReadProperty(flowType, flow, "Phase").ToString());
    }

    [Test]
    public void MarketplaceBootstrap_ShowsResolvedShopBeforeCurrencyLoadingCompletes()
    {
        Type managerType = RuntimeType("MarketplaceManager");
        Type stateType = RuntimeType("TutorialStateResponse");
        Type gatesType = RuntimeType("TutorialGatesDto");
        GameObject managerObject = new GameObject("MarketplaceManagerTest");
        GameObject intro = new GameObject("Intro");
        GameObject shop = new GameObject("Shop");
        GameObject back = new GameObject("Back");

        try
        {
            Component manager = managerObject.AddComponent(managerType);
            managerType.GetField("introScreen").SetValue(manager, intro);
            managerType.GetField("shopScreen").SetValue(manager, shop);
            managerType.GetField("backButton").SetValue(manager, back);

            object state = Activator.CreateInstance(stateType);
            object gates = Activator.CreateInstance(gatesType);
            gatesType.GetField("needsFirstPack").SetValue(gates, true);
            stateType.GetField("gates").SetValue(state, gates);

            MethodInfo applyPresentation = managerType.GetMethod(
                "ApplyResolvedTutorialPresentation",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.NotNull(applyPresentation);
            applyPresentation.Invoke(manager, new[] { state });

            Assert.IsFalse(intro.activeSelf);
            Assert.IsTrue(shop.activeSelf);
            Assert.IsFalse(back.activeSelf);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(managerObject);
            UnityEngine.Object.DestroyImmediate(intro);
            UnityEngine.Object.DestroyImmediate(shop);
            UnityEngine.Object.DestroyImmediate(back);
        }
    }

    [Test]
    public void MarketplaceTutorial_UsesApprovedDelayBetweenHints()
    {
        Type controllerType = RuntimeType("MarketplaceTutorialController");
        FieldInfo delayField = controllerType.GetField(
            "HintTransitionDelayMilliseconds",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        Assert.NotNull(delayField);
        Assert.AreEqual(750, delayField.GetRawConstantValue());
    }

    [Test]
    public void ServerFunctionsManager_SourceReferencesTutorialCloudFunctions()
    {
        string source = ReadScript("Networking", "ServerFunctionsManager.cs");

        StringAssert.Contains("GetTutorialState", source);
        StringAssert.Contains("CompleteTutorialStep", source);
        StringAssert.Contains("\"getTutorialState\"", source);
        StringAssert.Contains("\"completeTutorialStep\"", source);
    }

    [Test]
    public void Login_SourceReferencesServerTutorialRoute()
    {
        string source = ReadScript("Login", "PlayFabManagerLogin.cs");

        StringAssert.Contains("LoadTutorialRouteAfterDelay", source);
        StringAssert.Contains("GetTutorialStateAsync(LoggedInPlayerId)", source);
        StringAssert.Contains("SceneManager.LoadScene(mainSceneName)", source);
        StringAssert.DoesNotContain("ResolveSceneForTutorialRoute", source);
        StringAssert.DoesNotContain("marketplaceSceneName", source);
        StringAssert.DoesNotContain("librarySceneName", source);
    }

    [Test]
    public void MarketplaceAndLibrary_ReturnToMainAfterTheirTutorialMilestones()
    {
        string marketplace = ReadScript("Marketplace", "MarketplaceManager.cs");
        string library = ReadScript("Tutorial", "LibraryTutorialController.cs");

        StringAssert.Contains("postTutorialSceneName = \"Main\"", marketplace);
        StringAssert.Contains("SceneManager.LoadScene(postTutorialSceneName)", marketplace);
        StringAssert.Contains("postTutorialSceneName = \"Main\"", library);
        StringAssert.Contains("SceneManager.LoadScene(postTutorialSceneName)", library);
        StringAssert.DoesNotContain("librarySceneName", marketplace);
    }

    [Test]
    public void RuntimeScripts_DoNotUseOldTutorialPlayerPrefsFlags()
    {
        string scriptsRoot = Path.Combine(Application.dataPath, "Scripts");
        string[] scripts = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories);

        foreach (string script in scripts)
        {
            string source = File.ReadAllText(script);
            Assert.False(
                source.Contains("HasCompletedTutorial"),
                $"{script} still references an old local tutorial flag."
            );
        }
    }

    [Test]
    public void MarketplaceTutorial_UsesSessionStateAndServerVerifiedPurchase()
    {
        string marketplace = ReadScript("Marketplace", "MarketplaceManager.cs");
        string controller = ReadScript("Tutorial", "MarketplaceTutorialController.cs");
        string service = ReadScript("Tutorial", "TutorialService.cs");
        string album = ReadScript("Album.cs");
        string logout = ReadScript("PlayerSessionLogout.cs");
        string legacyPresenter = Path.Combine(
            Application.dataPath,
            "Scripts",
            "Tutorial",
            "MarketplaceTutorial.cs"
        );

        StringAssert.Contains("GetCurrentPlayerStateAsync()", marketplace);
        StringAssert.Contains("RefreshCurrentPlayerStateAsync()", marketplace);
        StringAssert.Contains("safeCardDeckState == true", marketplace);
        StringAssert.DoesNotContain("TutorialConstants.BuyFirstPack", marketplace);
        StringAssert.Contains("TutorialConstants.OpenMarketplace", controller);
        StringAssert.DoesNotContain("GetComponentInChildren", controller);
        StringAssert.DoesNotContain("RemoveAllListeners", controller);
        StringAssert.Contains("firstHintAcknowledgeButton", controller);
        StringAssert.Contains("secondHintAcknowledgeButton", controller);
        StringAssert.Contains("TutorialSessionState.TryGet", service);
        StringAssert.DoesNotContain("ShouldShowFlow(", service);
        StringAssert.DoesNotContain("public bool HasCompletedStep(", service);
        StringAssert.Contains("GetCurrentPlayerStateAsync()", album);
        StringAssert.Contains("TutorialSessionState.Clear()", logout);
        Assert.IsFalse(File.Exists(legacyPresenter), "Legacy MarketplaceTutorial presenter must be removed.");
    }

    [Test]
    public void MarketplaceScene_UsesTheActionGatedControllerWithoutLegacyCallbacks()
    {
        string scene = ReadAsset("Scenes", "Marketplace.unity").Replace("\r\n", "\n");
        string tutorialPanelPrefab = ReadAsset("Prefabs", "TutorialPanel.prefab");

        StringAssert.Contains("guid: 8ef8dcd6cf14446695667ece16f13d60", scene);
        string firstHintButton = AssertTutorialButtonReference(
            scene,
            "firstHintAcknowledgeButton"
        );
        AssertTutorialButtonReference(scene, "secondHintAcknowledgeButton");
        AssertComponentReferencesGameObject(
            scene,
            "firstHintInteractableButton",
            firstHintButton,
            "4e29b1a8efbd4b44bb3f3716e73f07ff"
        );
        StringAssert.DoesNotContain("m_Name: TutorialPanel1Btn", scene);
        StringAssert.DoesNotContain("m_Name: TutorialPanel2Btn", scene);
        StringAssert.DoesNotContain("MarketplaceTutorial, Assembly-CSharp", scene);
        StringAssert.DoesNotContain("guid: e9ac9957ad63b7b41acdc36df4e1179b", scene);
        StringAssert.DoesNotContain("MarketplaceTutorial, Assembly-CSharp", tutorialPanelPrefab);
    }

    private static Type RuntimeType(string name)
    {
        Type type = Type.GetType(name + ", Assembly-CSharp");
        Assert.NotNull(type, name + " must exist in Assembly-CSharp.");
        return type;
    }

    private static object ReadProperty(Type type, object instance, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName);
        Assert.NotNull(property, propertyName + " property was not found.");
        return property.GetValue(instance);
    }

    private static void ClearTutorialSessionState()
    {
        Type cacheType = Type.GetType("TutorialSessionState, Assembly-CSharp");
        cacheType?.GetMethod("Clear")?.Invoke(null, null);
    }

    [Test]
    public void CardsScene_AlbumTutorialOwnsBothLibraryHintPanels()
    {
        string scene = ReadAsset("Scenes", "Cards.unity").Replace("\r\n", "\n");
        string tutorialRoot = ReadYamlObject(scene, "--- !u!224 &1074998132");
        string hint1 = ReadYamlObject(scene, "--- !u!224 &7741742324158438542");
        string hint2 = ReadYamlObject(scene, "--- !u!224 &1272711093");

        StringAssert.Contains("- {fileID: 7741742324158438542}", tutorialRoot);
        StringAssert.Contains("- {fileID: 1272711093}", tutorialRoot);
        StringAssert.Contains("m_Father: {fileID: 755544324}", tutorialRoot);
        StringAssert.Contains("m_Father: {fileID: 1074998132}", hint1);
        StringAssert.Contains("m_Father: {fileID: 1074998132}", hint2);
    }

    [Test]
    public void MainMenuTutorial_UsesSessionCacheAndExplicitSceneReferences()
    {
        string controller = ReadScript("Tutorial", "MainMenuTutorialController.cs");

        StringAssert.Contains("TutorialSessionState.TryGet", controller);
        StringAssert.Contains("introHintPanel", controller);
        StringAssert.Contains("introAcknowledgeButton", controller);
        StringAssert.Contains("marketplaceButton", controller);
        StringAssert.Contains("libraryButton", controller);
        StringAssert.Contains("royalRumbleButton", controller);
        StringAssert.Contains("HintTransitionDelayMilliseconds = 750", controller);
        StringAssert.Contains("SceneLoadingOverlay.Hide()", controller);
        StringAssert.Contains("spotlight.Show(targetButton?.transform as RectTransform, null)", controller);
        StringAssert.DoesNotContain("tutorialCanvas", controller);
        StringAssert.DoesNotContain("originalPanelChildStates", controller);
        StringAssert.DoesNotContain("ShowPanelBackdropOnly", controller);
        StringAssert.DoesNotContain("FindFirstObjectByType", controller);
        StringAssert.DoesNotContain("GetComponentInChildren", controller);
    }

    [Test]
    public void MainMenuScene_TutorialAcknowledgementUsesVisibleButtonsOnly()
    {
        string scene = ReadAsset("Scenes", "Main.unity").Replace("\r\n", "\n");

        AssertTutorialButtonReference(scene, "introAcknowledgeButton");
        AssertTutorialButtonReference(scene, "marketplaceAcknowledgeButton");
        AssertTutorialButtonReference(scene, "libraryAcknowledgeButton");
        AssertTutorialButtonReference(scene, "royalRumbleAcknowledgeButton");
        StringAssert.DoesNotContain("m_Name: TutorialPanel1Btn", scene);
        StringAssert.DoesNotContain("m_Name: TutorialPanel2Btn", scene);
        StringAssert.DoesNotContain("m_Name: TutorialPanel3Btn", scene);
        StringAssert.DoesNotContain("m_Name: TutorialPanel4Btn", scene);
    }

    private static string ReadScript(params string[] pathParts)
    {
        string[] fullParts = new string[pathParts.Length + 2];
        fullParts[0] = Application.dataPath;
        fullParts[1] = "Scripts";
        pathParts.CopyTo(fullParts, 2);

        string sourcePath = Path.Combine(fullParts);
        Assert.IsTrue(File.Exists(sourcePath), sourcePath + " was not found.");
        return File.ReadAllText(sourcePath);
    }

    private static string ReadAsset(params string[] pathParts)
    {
        string sourcePath = Path.Combine(Application.dataPath, Path.Combine(pathParts));
        Assert.IsTrue(File.Exists(sourcePath), sourcePath + " was not found.");
        return File.ReadAllText(sourcePath);
    }

    private static string ReadYamlObject(string source, string header)
    {
        int start = source.IndexOf(header, System.StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, header + " was not found.");
        int end = source.IndexOf("\n--- !u!", start + header.Length, System.StringComparison.Ordinal);
        return end < 0 ? source.Substring(start) : source.Substring(start, end - start);
    }

    private static string AssertTutorialButtonReference(string scene, string fieldName)
    {
        const string customButtonScriptGuid = "6709565a3468a284ba9ce31c3a081877";
        string componentId = ReadReferencedFileId(scene, fieldName);
        string component = ReadYamlObject(scene, "--- !u!114 &" + componentId);

        StringAssert.Contains("guid: " + customButtonScriptGuid, component);
        StringAssert.Contains("m_Calls: []", component);

        string gameObjectId = ReadReferencedFileId(component, "m_GameObject");
        string gameObject = ReadYamlObject(scene, "--- !u!1 &" + gameObjectId);
        StringAssert.Contains("m_Name: Button", gameObject);
        AssertEnabledButtonOnGameObject(scene, gameObjectId);
        return gameObjectId;
    }

    private static void AssertEnabledButtonOnGameObject(string scene, string gameObjectId)
    {
        const string buttonScriptGuid = "4e29b1a8efbd4b44bb3f3716e73f07ff";
        string gameObjectReference = "m_GameObject: {fileID: " + gameObjectId + "}";
        string[] yamlObjects = scene.Split(
            new[] { "\n--- !u!" },
            StringSplitOptions.None
        );

        foreach (string yamlObject in yamlObjects)
        {
            if (
                yamlObject.Contains("guid: " + buttonScriptGuid)
                && yamlObject.Contains(gameObjectReference)
            )
            {
                StringAssert.Contains("m_Enabled: 1", yamlObject);
                StringAssert.Contains("m_Calls: []", yamlObject);
                return;
            }
        }

        Assert.Fail("Visible tutorial object has no Button component: " + gameObjectId);
    }

    private static void AssertComponentReferencesGameObject(
        string scene,
        string fieldName,
        string expectedGameObjectId,
        string expectedScriptGuid
    )
    {
        string componentId = ReadReferencedFileId(scene, fieldName);
        string component = ReadYamlObject(scene, "--- !u!114 &" + componentId);

        StringAssert.Contains("guid: " + expectedScriptGuid, component);
        StringAssert.Contains("m_Enabled: 1", component);
        Assert.AreEqual(
            expectedGameObjectId,
            ReadReferencedFileId(component, "m_GameObject"),
            fieldName + " must reference the same visible tutorial button."
        );
    }

    private static string ReadReferencedFileId(string source, string fieldName)
    {
        string prefix = fieldName + ": {fileID: ";
        int start = source.IndexOf(prefix, StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, fieldName + " reference was not found.");
        start += prefix.Length;
        int end = source.IndexOf('}', start);
        Assert.Greater(end, start, fieldName + " reference is malformed.");
        string fileId = source.Substring(start, end - start).Trim();
        Assert.AreNotEqual("0", fileId, fieldName + " must be assigned.");
        return fileId;
    }
}
