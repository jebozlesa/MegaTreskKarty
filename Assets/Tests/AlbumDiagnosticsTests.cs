using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class AlbumDiagnosticsTests
{
    [Test]
    public void AlbumPlayerCardsLifecycle_HasVisibleDiagnostics()
    {
        string albumSourcePath = Path.Combine(Application.dataPath, "Scripts", "Album.cs");
        Assert.IsTrue(File.Exists(albumSourcePath), "Album.cs was not found.");

        string source = File.ReadAllText(albumSourcePath);

        StringAssert.Contains("Debug.LogWarning($\"[Album] Library state load requested", source);
        StringAssert.Contains("Debug.LogWarning($\"[Album] Library cards render requested", source);
        StringAssert.Contains("Debug.LogWarning($\"[Album] Library cards rendered", source);
        StringAssert.DoesNotContain("Debug.Log($\"[Album]", source);
    }

    [Test]
    public void LibraryDeckRuntime_UsesServerBackedBoundary()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        string serverFunctionsPath = Path.Combine(scriptsPath, "Networking", "ServerFunctionsManager.cs");
        string libraryServicePath = Path.Combine(scriptsPath, "Album", "LibraryDeckService.cs");
        string libraryControllerPath = Path.Combine(scriptsPath, "Album", "LibraryDeckController.cs");
        string albumSourcePath = Path.Combine(scriptsPath, "Album.cs");
        string deckManagerPath = Path.Combine(scriptsPath, "Album", "DeckManager.cs");

        Assert.IsTrue(File.Exists(serverFunctionsPath), "ServerFunctionsManager.cs was not found.");
        Assert.IsTrue(File.Exists(libraryServicePath), "LibraryDeckService.cs was not found.");
        Assert.IsTrue(File.Exists(libraryControllerPath), "LibraryDeckController.cs was not found.");

        string serverFunctionsSource = File.ReadAllText(serverFunctionsPath);
        StringAssert.Contains("getLibraryDeckState", serverFunctionsSource);
        StringAssert.Contains("createLibraryDeck", serverFunctionsSource);
        StringAssert.Contains("setActiveLibraryDeck", serverFunctionsSource);
        StringAssert.Contains("swapLibraryDeckCard", serverFunctionsSource);

        string albumSource = File.ReadAllText(albumSourcePath);
        string deckManagerSource = File.ReadAllText(deckManagerPath);
        StringAssert.Contains("LibraryDeckController", albumSource);
        StringAssert.Contains("LibraryDeckController", deckManagerSource);
    }

    [Test]
    public void DeckSwap_KeepsSelectedCollectionCardDetailOpen()
    {
        string deckManagerPath = Path.Combine(
            Application.dataPath,
            "Scripts",
            "Album",
            "DeckManager.cs"
        );
        Assert.IsTrue(File.Exists(deckManagerPath), "DeckManager.cs was not found.");

        string source = File.ReadAllText(deckManagerPath);
        string swapMethod = ExtractMethodBody(
            source,
            "public async Task<bool> SwapWithSelectedCardAsync",
            "private void CreateCardInDeck"
        );

        StringAssert.DoesNotContain(
            "selectedCard.ZoomOut()",
            swapMethod,
            "A successful deck swap must leave the selected collection-card detail open."
        );
    }

    [Test]
    public void LegacyPlayerDecksRuntime_IsRemovedFromClientDeckFlow()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        Assert.IsFalse(
            File.Exists(Path.Combine(scriptsPath, "PlayerDeck.cs")),
            "Legacy PlayerDeck.cs must stay removed."
        );
        Assert.IsFalse(
            File.Exists(Path.Combine(scriptsPath, "Deck", "Deck.cs")),
            "Legacy Deck.cs must stay removed."
        );
        Assert.IsFalse(
            File.Exists(Path.Combine(scriptsPath, "Deck", "DeckListWrapper.cs")),
            "Legacy DeckListWrapper.cs must stay removed."
        );

        string[] runtimeFiles =
        {
            Path.Combine(scriptsPath, "Album", "DeckManager.cs"),
            Path.Combine(scriptsPath, "Card.cs"),
            Path.Combine(scriptsPath, "CardGenerator.cs"),
            Path.Combine(scriptsPath, "Marketplace", "MarketplaceManager.cs"),
        };

        foreach (string runtimeFile in runtimeFiles)
        {
            Assert.IsTrue(File.Exists(runtimeFile), $"{runtimeFile} was not found.");
            string source = File.ReadAllText(runtimeFile);
            StringAssert.DoesNotContain("PlayerDecks", source, runtimeFile);
            StringAssert.DoesNotContain("CreateFirstDeck", source, runtimeFile);
            StringAssert.DoesNotContain("SwapCardsInPlayFab", source, runtimeFile);
            StringAssert.DoesNotContain("UpdateDeckDataInPlayFab", source, runtimeFile);
        }
    }

    [Test]
    public void LibrarySortingRuntime_UsesApprovedLocalSortContract()
    {
        string controllerPath = Path.Combine(Application.dataPath, "Scripts", "Album", "LibraryDeckController.cs");
        Assert.IsTrue(File.Exists(controllerPath), "LibraryDeckController.cs was not found.");

        string source = File.ReadAllText(controllerPath);

        StringAssert.Contains("public void CycleSortCriterion()", source);
        StringAssert.Contains("public void ApplySort()", source);
        StringAssert.Contains("visibleCards = ApplyCurrentSort(visibleCards);", source);
        StringAssert.Contains("RenderCurrentContext(showTransitionFrame: true);", source);
        StringAssert.Contains("public static class LibraryCardSorter", source);
        StringAssert.Contains("public static List<GeneratedCard> SortCards(", source);
        StringAssert.Contains("CurrentState.cards", source);

        StringAssert.Contains("LibrarySortCriterion.Level", source);
        StringAssert.Contains("LibrarySortCriterion.Name", source);
        StringAssert.Contains("LibrarySortCriterion.Experience", source);
        StringAssert.Contains("LibrarySortCriterion.Health", source);
        StringAssert.Contains("LibrarySortCriterion.Strength", source);
        StringAssert.Contains("LibrarySortCriterion.Speed", source);
        StringAssert.Contains("LibrarySortCriterion.Attack", source);
        StringAssert.Contains("LibrarySortCriterion.Defense", source);
        StringAssert.Contains("LibrarySortCriterion.Knowledge", source);
        StringAssert.Contains("LibrarySortCriterion.Charisma", source);

        StringAssert.Contains("return \"LVL\";", source);
        StringAssert.Contains("return \"ABC\";", source);
        StringAssert.Contains("return \"XP\";", source);
        StringAssert.Contains("return \"HP\";", source);
        StringAssert.Contains("return \"STR\";", source);
        StringAssert.Contains("return \"SPD\";", source);
        StringAssert.Contains("return \"ATT\";", source);
        StringAssert.Contains("return \"DEF\";", source);
        StringAssert.Contains("return \"KNO\";", source);
        StringAssert.Contains("return \"CHA\";", source);
        StringAssert.Contains("return criterion == LibrarySortCriterion.Name;", source);

        string applySortBody = ExtractMethodBody(
            source,
            "public void ApplySort()",
            "public bool IsCardUsedInAnyKnownDeck"
        );

        StringAssert.DoesNotContain("libraryDeckService", applySortBody);
        StringAssert.DoesNotContain("GetLibraryDeckStateAsync", applySortBody);
        StringAssert.DoesNotContain("await", applySortBody);
    }

    [Test]
    public void MarketplacePackPurchaseRuntime_UsesServerOwnedBoundary()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        string marketplacePath = Path.Combine(
            scriptsPath,
            "Marketplace",
            "MarketplaceManager.cs"
        );
        string serverFunctionsPath = Path.Combine(
            scriptsPath,
            "Networking",
            "ServerFunctionsManager.cs"
        );
        string cardGeneratorPath = Path.Combine(scriptsPath, "CardGenerator.cs");
        string marketplaceScenePath = Path.Combine(Application.dataPath, "Scenes", "Marketplace.unity");
        string packButtonPrefabPath = Path.Combine(
            Application.dataPath,
            "Prefabs",
            "CreateCardPackButton.prefab"
        );

        Assert.IsTrue(File.Exists(marketplacePath), "MarketplaceManager.cs was not found.");
        Assert.IsTrue(File.Exists(serverFunctionsPath), "ServerFunctionsManager.cs was not found.");
        Assert.IsTrue(File.Exists(cardGeneratorPath), "CardGenerator.cs was not found.");
        Assert.IsTrue(File.Exists(marketplaceScenePath), "Marketplace.unity was not found.");
        Assert.IsTrue(File.Exists(packButtonPrefabPath), "CreateCardPackButton.prefab was not found.");

        string marketplaceSource = File.ReadAllText(marketplacePath);
        string serverFunctionsSource = File.ReadAllText(serverFunctionsPath);
        string cardGeneratorSource = File.ReadAllText(cardGeneratorPath);
        string marketplaceSceneSource = File.ReadAllText(marketplaceScenePath);
        string packButtonPrefabSource = File.ReadAllText(packButtonPrefabPath);

        StringAssert.Contains("serverFunctionsManager.PurchaseCardPack", marketplaceSource);
        StringAssert.Contains("Guid.NewGuid().ToString()", marketplaceSource);
        StringAssert.Contains("purchaseConfirmationPanel", marketplaceSource);
        StringAssert.Contains("public void ConfirmPendingPackPurchase()", marketplaceSource);
        StringAssert.Contains("public void CancelPendingPackPurchase()", marketplaceSource);
        StringAssert.Contains("ShouldOpenLibraryAfterPurchase", marketplaceSource);
        StringAssert.Contains("SceneLoadingOverlay.Show()", marketplaceSource);
        StringAssert.Contains("SceneLoadingOverlay.Hide()", marketplaceSource);
        StringAssert.Contains("currentCurrencyBalance", marketplaceSource);
        StringAssert.Contains("HasEnoughClientCurrencyForPack", marketplaceSource);
        StringAssert.Contains("GetClientPackPrice", marketplaceSource);
        StringAssert.Contains("Pack purchase blocked by client balance check", marketplaceSource);
        StringAssert.DoesNotContain("SubtractUserVirtualCurrency", marketplaceSource);
        StringAssert.DoesNotContain("AddUserVirtualCurrency", marketplaceSource);
        StringAssert.DoesNotContain("UpdateUserData", marketplaceSource);
        StringAssert.DoesNotContain("openCardPack", marketplaceSource);
        StringAssert.DoesNotContain("SceneManager.LoadScene(\"Cards\")", marketplaceSource);

        StringAssert.Contains("public void PurchaseCardPack", serverFunctionsSource);
        StringAssert.Contains("\"purchaseCardPack\"", serverFunctionsSource);
        StringAssert.DoesNotContain("public void OpenCardPack", serverFunctionsSource);
        StringAssert.DoesNotContain("\"openCardPack\"", serverFunctionsSource);

        StringAssert.Contains("ShowCardOnScreen", cardGeneratorSource);
        StringAssert.DoesNotContain("PlayFabClientAPI", cardGeneratorSource);
        StringAssert.DoesNotContain("UpdateUserData", cardGeneratorSource);
        StringAssert.DoesNotContain("GetUserData", cardGeneratorSource);
        StringAssert.DoesNotContain("AddRandomCard", cardGeneratorSource);
        StringAssert.DoesNotContain("AddCardById", cardGeneratorSource);
        StringAssert.DoesNotContain("AddAllCardsFromDatabase", cardGeneratorSource);
        StringAssert.DoesNotContain("Mono.Data.Sqlite", cardGeneratorSource);
        StringAssert.DoesNotContain("SqliteConnection", cardGeneratorSource);
        StringAssert.DoesNotContain("PlayerCards", cardGeneratorSource);
        StringAssert.DoesNotContain("displayBlock", cardGeneratorSource);
        StringAssert.DoesNotContain("themedPacks", cardGeneratorSource);
        StringAssert.DoesNotContain("GenerateCardPack", cardGeneratorSource);
        StringAssert.DoesNotContain(
            "m_TargetAssemblyTypeName: CardGenerator, Assembly-CSharp",
            marketplaceSceneSource
        );
        StringAssert.DoesNotContain(
            "m_TargetAssemblyTypeName: CardGenerator, Assembly-CSharp",
            packButtonPrefabSource
        );
    }

    [Test]
    public void CardRecycleRuntime_UsesServerOwnedBoundary()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        string cardPath = Path.Combine(scriptsPath, "Card.cs");
        string albumSourcePath = Path.Combine(scriptsPath, "Album.cs");
        string legacyLoveValuePath = Path.Combine(scriptsPath, "Album", "AlbumLoveValue.cs");
        string serverFunctionsPath = Path.Combine(
            scriptsPath,
            "Networking",
            "ServerFunctionsManager.cs"
        );
        string cardRecycleServicePath = Path.Combine(
            scriptsPath,
            "Album",
            "CardRecycleService.cs"
        );
        string confirmDialogPath = Path.Combine(
            scriptsPath,
            "UI",
            "ConfirmDialogController.cs"
        );
        string loadingOverlayPath = Path.Combine(
            scriptsPath,
            "UI",
            "LoadingOverlayView.cs"
        );
        string albumCardPrefabPath = Path.Combine(
            Application.dataPath,
            "Prefabs",
            "AlbumKard.prefab"
        );

        Assert.IsTrue(File.Exists(cardPath), "Card.cs was not found.");
        Assert.IsTrue(File.Exists(albumSourcePath), "Album.cs was not found.");
        Assert.IsFalse(File.Exists(legacyLoveValuePath), "Legacy AlbumLoveValue.cs must stay removed.");
        Assert.IsTrue(File.Exists(serverFunctionsPath), "ServerFunctionsManager.cs was not found.");
        Assert.IsTrue(File.Exists(cardRecycleServicePath), "CardRecycleService.cs was not found.");
        Assert.IsTrue(File.Exists(confirmDialogPath), "ConfirmDialogController.cs was not found.");
        Assert.IsTrue(File.Exists(loadingOverlayPath), "LoadingOverlayView.cs was not found.");
        Assert.IsTrue(File.Exists(albumCardPrefabPath), "AlbumKard.prefab was not found.");

        string cardSource = File.ReadAllText(cardPath);
        string albumSource = File.ReadAllText(albumSourcePath);
        string serverFunctionsSource = File.ReadAllText(serverFunctionsPath);
        string cardRecycleServiceSource = File.ReadAllText(cardRecycleServicePath);
        string confirmDialogSource = File.ReadAllText(confirmDialogPath);
        string loadingOverlaySource = File.ReadAllText(loadingOverlayPath);
        string albumCardPrefabSource = File.ReadAllText(albumCardPrefabPath);

        StringAssert.Contains("public void RequestRecycleCard()", cardSource);
        StringAssert.Contains("recycleConfirmationDialog.Show(\"ARE YOU SURE?\"", cardSource);
        StringAssert.Contains("Guid.NewGuid().ToString()", cardSource);
        StringAssert.Contains("cardRecycleService.RecycleCardAsync", cardSource);
        StringAssert.Contains("SceneLoadingOverlay.Show()", cardSource);
        StringAssert.Contains("SceneLoadingOverlay.Hide()", cardSource);
        StringAssert.Contains("LoadForCurrentPlayer(useCardRenderDelay: false)", cardSource);
        StringAssert.Contains("TryFindCardDeckUsage", cardSource);
        StringAssert.Contains("deckUsage", cardSource);
        StringAssert.Contains("albumController.RefreshPlayerCurrencyBalance()", cardSource);
        StringAssert.DoesNotContain("public void RemoveCard()", cardSource);
        StringAssert.DoesNotContain("UpdateUserDataRequest", cardSource);
        StringAssert.DoesNotContain("PlayFabClientAPI.UpdateUserData", cardSource);
        StringAssert.DoesNotContain("AddUserVirtualCurrencyRequest", cardSource);
        StringAssert.DoesNotContain("PlayFabClientAPI.AddUserVirtualCurrency", cardSource);
        StringAssert.DoesNotContain("PlayFabAlbumCardManager", cardSource);
        StringAssert.DoesNotContain("OnChangeAttackClick", cardSource);
        StringAssert.DoesNotContain("UpdateCardAttack", cardSource);
        StringAssert.DoesNotContain("ShowAttackList", cardSource);
        StringAssert.DoesNotContain("HideAttackList", cardSource);

        StringAssert.Contains("public CardRecycleService cardRecycleService;", albumSource);
        StringAssert.Contains("public ConfirmDialogController recycleConfirmationDialog;", albumSource);
        StringAssert.Contains("public IEnumerator RefreshPlayerCurrencyBalance()", albumSource);
        StringAssert.Contains("novaKarta.GetComponent<Card>().Initialize(deckPanel, this);", albumSource);
        StringAssert.Contains("novaKarta.GetComponent<Card>().cardRecycleService = cardRecycleService;", albumSource);
        StringAssert.Contains("novaKarta.GetComponent<Card>().recycleConfirmationDialog = recycleConfirmationDialog;", albumSource);

        StringAssert.Contains("public void RecyclePlayerCard", serverFunctionsSource);
        StringAssert.Contains("\"recyclePlayerCard\"", serverFunctionsSource);

        StringAssert.Contains("serverFunctionsManager.RecyclePlayerCard", cardRecycleServiceSource);
        StringAssert.Contains("public void Show(", confirmDialogSource);
        StringAssert.Contains("SetAsLastSibling()", confirmDialogSource);
        StringAssert.Contains("public void Confirm()", confirmDialogSource);
        StringAssert.Contains("public void Cancel()", confirmDialogSource);
        StringAssert.Contains("SetAsLastSibling()", loadingOverlaySource);
        StringAssert.Contains("blocksRaycasts = isVisible", loadingOverlaySource);

        StringAssert.Contains("m_MethodName: RequestRecycleCard", albumCardPrefabSource);
        StringAssert.DoesNotContain("m_MethodName: RemoveCard", albumCardPrefabSource);
        StringAssert.DoesNotContain("m_MethodName: OnChangeAttackClick", albumCardPrefabSource);
        StringAssert.DoesNotContain("m_MethodName: ShowAttackList", albumCardPrefabSource);
        StringAssert.DoesNotContain("m_MethodName: HideAttackList", albumCardPrefabSource);
        StringAssert.DoesNotContain("AttackScroll", albumCardPrefabSource);
    }

    [Test]
    public void CardRecycleBlockedPrompt_UsesOnlyItsVisibleAcknowledgementButton()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        string albumSourcePath = Path.Combine(scriptsPath, "Album.cs");
        string dismissHelperPath = Path.Combine(scriptsPath, "UI", "DismissPanelButton.cs");
        string cardsScenePath = Path.Combine(Application.dataPath, "Scenes", "Cards.unity");

        Assert.IsTrue(File.Exists(albumSourcePath), "Album.cs was not found.");
        Assert.IsTrue(File.Exists(cardsScenePath), "Cards.unity was not found.");

        string albumSource = File.ReadAllText(albumSourcePath);
        string cardsScene = File.ReadAllText(cardsScenePath).Replace("\r\n", "\n");

        Assert.IsFalse(
            File.Exists(dismissHelperPath),
            "The recycle blocker must not depend on a generic runtime button finder."
        );
        StringAssert.DoesNotContain("DismissPanelButton", albumSource);
        StringAssert.DoesNotContain(
            "m_Name: Hint1Btn",
            cardsScene,
            "The copied fullscreen tutorial click-catcher must stay removed from Cards.unity."
        );

        string acknowledgementButton = ExtractYamlObject(
            cardsScene,
            "--- !u!114 &4853700086726999121"
        );
        StringAssert.Contains("m_Enabled: 1", acknowledgementButton);
        StringAssert.Contains("m_Target: {fileID: 4853700087339883771}", acknowledgementButton);
        StringAssert.Contains("m_MethodName: SetActive", acknowledgementButton);

        StringAssert.DoesNotContain(
            "--- !u!114 &4853700087339883765",
            cardsScene,
            "BlockSellDeckCard root must not retain its obsolete disabled Button component."
        );
    }

    [Test]
    public void CustomButton_RestoresPressedColorWhenDisabled()
    {
        System.Type customButtonType = System.Type.GetType("CustomButton, Assembly-CSharp");
        Assert.NotNull(customButtonType, "CustomButton must exist in Assembly-CSharp.");

        GameObject buttonObject = new GameObject(
            "CustomButtonUnderTest",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        try
        {
            buttonObject.SetActive(false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = Color.black;

            Component customButton = buttonObject.AddComponent(customButtonType);
            FieldInfo innerImage = customButtonType.GetField("innerImage", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(innerImage, "CustomButton.innerImage must remain assignable.");
            innerImage.SetValue(customButton, image);

            MethodInfo awake = customButtonType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo onDisable = customButtonType.GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(awake, "CustomButton must capture its original visual state during Awake.");
            Assert.NotNull(onDisable, "CustomButton must restore its visual state when disabled.");

            awake.Invoke(customButton, null);
            image.color = Color.yellow;
            onDisable.Invoke(customButton, null);

            Assert.AreEqual(Color.black, image.color, "Disabling a pressed CustomButton must restore its original color.");
        }
        finally
        {
            Object.DestroyImmediate(buttonObject);
        }
    }

    [UnityTest]
    public IEnumerator CustomButton_NativeConfirmationCanDisableInteractionWithoutRetainingPressedColor()
    {
        Component custom = CreatePressedButton(Color.white, 0f, out var button, out var image, out var delayed);
        float previousScale = Time.timeScale;
        Time.timeScale = 1f;
        int nativeCalls = 0, delayedCalls = 0;
        button.onClick.AddListener(() => { nativeCalls++; button.interactable = false; });
        delayed.AddListener(() => delayedCalls++);
        try
        {
            for (int click = 1; click <= 2; click++)
            {
                button.interactable = true;
                image.color = Color.yellow;
                ReleaseButton(button);
                Assert.AreEqual(click, nativeCalls);
                Assert.IsFalse(button.interactable);
                yield return new WaitForSecondsRealtime(0.05f);
                Assert.AreEqual(Color.white, image.color);
                Assert.AreEqual(0, delayedCalls, "A disabled confirmation must not invoke the custom action.");
                Assert.IsFalse(button.interactable, "Visual cleanup must not re-enable gameplay input.");
            }
        }
        finally { Time.timeScale = previousScale; Object.DestroyImmediate(custom.gameObject); }
    }

    [Test]
    public void CustomButton_DisabledBeforeReleaseRestoresTheCapturedColorWithoutAnAction()
    {
        Component custom = CreatePressedButton(Color.cyan, 0f, out var button, out var image, out var delayed);
        int calls = 0;
        delayed.AddListener(() => calls++);
        button.onClick.AddListener(() => calls++);
        try
        {
            button.interactable = false;
            ReleaseButton(button);
            Assert.AreEqual(Color.cyan, image.color, "Restore the authored color, not hardcoded white.");
            Assert.AreEqual(0, calls);
            Assert.IsFalse(button.interactable);
        }
        finally { Object.DestroyImmediate(custom.gameObject); }
    }

    [TestCase(0f)]
    [TestCase(0.15f)]
    public void CustomButton_DisabledDuringDelayStillRestoresColorWithoutInvokingAction(float delay)
    {
        Component custom = CreatePressedButton(Color.cyan, delay, out var button, out var image, out var delayed);
        int calls = 0;
        delayed.AddListener(() => calls++);
        try
        {
            var routine = (IEnumerator)custom.GetType().GetMethod("ExecuteActionAfterDelay", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(custom, null);
            Assert.IsTrue(routine.MoveNext());
            Assert.IsInstanceOf<WaitForSeconds>(routine.Current);
            Assert.AreEqual(Color.yellow, image.color);
            button.interactable = false;
            Assert.IsFalse(routine.MoveNext());
            Assert.AreEqual(Color.cyan, image.color);
            Assert.AreEqual(0, calls);
            Assert.IsFalse(button.interactable);
        }
        finally { Object.DestroyImmediate(custom.gameObject); }
    }

    [UnityTest]
    public IEnumerator CustomButton_ValidActionKeepsDelayAndObservesRestoredOriginalColor()
    {
        const float delay = 0.15f;
        Component custom = CreatePressedButton(Color.cyan, delay, out var button, out var image, out var delayed);
        float previousScale = Time.timeScale;
        Time.timeScale = 1f;
        int calls = 0;
        Color callbackColor = Color.clear;
        delayed.AddListener(() => { calls++; callbackColor = image.color; });
        try
        {
            float deadline = Time.time + delay;
            ReleaseButton(button);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(Color.yellow, image.color, "Normal click feedback must keep its configured delay.");
            while (Time.time < deadline)
            {
                Assert.AreEqual(0, calls, "The delayed action must not run early.");
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.05f);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(Color.cyan, callbackColor);
            Assert.AreEqual(Color.cyan, image.color);
            Assert.IsTrue(button.interactable);
        }
        finally { Time.timeScale = previousScale; Object.DestroyImmediate(custom.gameObject); }
    }

    [UnityTest]
    public IEnumerator CustomButton_CallbackMayHideAndReopenPopupWithoutRetainingPressedColor()
    {
        Component custom = CreatePressedButton(Color.white, 0f, out var button, out var image, out var delayed);
        float previousScale = Time.timeScale;
        Time.timeScale = 1f;
        int calls = 0;
        delayed.AddListener(() =>
        {
            calls++;
            Assert.AreEqual(Color.white, image.color, "Restore before invoking a popup's hide action.");
            custom.gameObject.SetActive(false);
        });
        try
        {
            custom.gameObject.SetActive(false);
            for (int click = 1; click <= 2; click++)
            {
                custom.gameObject.SetActive(true);
                Assert.AreEqual(Color.white, image.color);
                image.color = Color.yellow;
                ReleaseButton(button);
                yield return new WaitForSecondsRealtime(0.05f);
                Assert.AreEqual(click, calls);
                Assert.IsFalse(custom.gameObject.activeSelf);
                Assert.AreEqual(Color.white, image.color);
            }
        }
        finally { Time.timeScale = previousScale; Object.DestroyImmediate(custom.gameObject); }
    }

    [Test]
    public void CustomButton_MpDialogKeepsItsExistingImageAndNativeConfirmationWiring()
    {
        string scene = File.ReadAllText(Path.Combine(Application.dataPath, "Scenes", "Multiplayer.unity")).Replace("\r\n", "\n");
        string custom = ExtractYamlObject(scene, "--- !u!114 &1327332877");
        StringAssert.Contains("guid: 6709565a3468a284ba9ce31c3a081877", custom);
        StringAssert.Contains("innerImage: {fileID: 1327332874}", custom);
        StringAssert.Contains("delay: 0", custom);
        StringAssert.Contains("m_Calls: []", custom);
        string confirm = ExtractYamlObject(scene, "--- !u!114 &909972134 stripped");
        StringAssert.Contains("m_CorrespondingSourceObject: {fileID: 4298181560895731028", confirm);
        StringAssert.Contains("confirmButton: {fileID: 909972134}", scene);
    }

    private static Component CreatePressedButton(Color original, float delay, out Button button, out Image image, out UnityEvent delayed)
    {
        var root = new GameObject("PressedColorUnderTest", typeof(RectTransform), typeof(Image), typeof(Button));
        root.SetActive(false);
        try
        {
            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(root.transform, false);
            image = inner.GetComponent<Image>();
            image.color = original;
            var type = System.Type.GetType("CustomButton, Assembly-CSharp", true);
            Component custom = root.AddComponent(type);
            type.GetField("innerImage").SetValue(custom, image);
            type.GetField("delay").SetValue(custom, delay);
            delayed = new UnityEvent();
            type.GetField("onDelayedClick").SetValue(custom, delayed);
            root.SetActive(true);
            button = root.GetComponent<Button>();
            // Reproduce the pressed visual without invoking the unrelated audio singleton.
            image.color = Color.yellow;
            return custom;
        }
        catch { Object.DestroyImmediate(root); throw; }
    }

    private static void ReleaseButton(Button button)
    {
        var pointer = new PointerEventData(null) { button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    [Test]
    public void LegacyAlbumAttackChangeRuntime_IsRemoved()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
        string prefabsPath = Path.Combine(Application.dataPath, "Prefabs");
        string albumCardPrefabPath = Path.Combine(prefabsPath, "AlbumKard.prefab");
        string gameScenePath = Path.Combine(Application.dataPath, "Scenes", "Game.unity");
        string marketplaceScenePath = Path.Combine(Application.dataPath, "Scenes", "Marketplace.unity");

        Assert.IsFalse(
            File.Exists(Path.Combine(scriptsPath, "Album", "PlayFabAlbumCardManager.cs")),
            "Legacy PlayFabAlbumCardManager runtime must stay removed."
        );
        Assert.IsFalse(
            File.Exists(Path.Combine(scriptsPath, "Album", "AttackListController.cs")),
            "Legacy AttackListController runtime must stay removed."
        );
        Assert.IsFalse(
            File.Exists(Path.Combine(scriptsPath, "Album", "AviableAttack.cs")),
            "Legacy AviableAttack runtime must stay removed."
        );
        Assert.IsFalse(
            File.Exists(Path.Combine(prefabsPath, "PlayFabAlbumCardManager.prefab")),
            "Legacy PlayFabAlbumCardManager prefab must stay removed."
        );
        Assert.IsFalse(
            File.Exists(Path.Combine(prefabsPath, "AviableAttackImg.prefab")),
            "Legacy attack option prefab must stay removed."
        );

        string albumCardPrefabSource = File.ReadAllText(albumCardPrefabPath);
        string gameSceneSource = File.ReadAllText(gameScenePath);
        string marketplaceSceneSource = File.ReadAllText(marketplaceScenePath);

        StringAssert.DoesNotContain("6e7d932567c580c4db7131c6563ebb5a", albumCardPrefabSource);
        StringAssert.DoesNotContain("d1ac93ae58a95b743ba7d3515a20fdcb", albumCardPrefabSource);
        StringAssert.DoesNotContain("cbaa86a481f2bbf4bb6b5dfdc7f13702", albumCardPrefabSource);
        StringAssert.DoesNotContain("755f21c7121aabf47a25cd81ff286459", albumCardPrefabSource);
        StringAssert.DoesNotContain("88eaf644aa09f74438d827a5f2207f88", albumCardPrefabSource);
        StringAssert.DoesNotContain("AttackScroll", albumCardPrefabSource);
        StringAssert.DoesNotContain("AddRandomCard", gameSceneSource);
        StringAssert.DoesNotContain("AddAllCardsFromDatabase", gameSceneSource);
        StringAssert.DoesNotContain("CardGenerator, Assembly-CSharp", gameSceneSource);
        StringAssert.DoesNotContain("displayBlock", marketplaceSceneSource);
    }

    private static string ExtractMethodBody(string source, string startMarker, string endMarker)
    {
        int start = source.IndexOf(startMarker, System.StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, $"Start marker was not found: {startMarker}");

        int end = source.IndexOf(endMarker, start, System.StringComparison.Ordinal);
        Assert.Greater(end, start, $"End marker was not found after: {startMarker}");

        return source.Substring(start, end - start);
    }

    private static string ExtractYamlObject(string source, string marker)
    {
        int start = source.IndexOf(marker, System.StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, marker + " was not found.");

        int end = source.IndexOf("\n--- !u!", start + marker.Length, System.StringComparison.Ordinal);
        if (end < 0)
        {
            end = source.Length;
        }

        return source.Substring(start, end - start);
    }
}
