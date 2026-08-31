using System.IO;
using NUnit.Framework;
using UnityEngine;

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
    public void LegacyPlayerDecksRuntime_IsRemovedFromClientDeckFlow()
    {
        string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
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
        StringAssert.Contains("AlbumLoveValue.Instance.GetPlayerCurrencyBalance()", cardSource);
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
}
