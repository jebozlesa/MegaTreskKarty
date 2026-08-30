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
        StringAssert.DoesNotContain("SubtractUserVirtualCurrency", marketplaceSource);
        StringAssert.DoesNotContain("AddUserVirtualCurrency", marketplaceSource);
        StringAssert.DoesNotContain("UpdateUserData", marketplaceSource);
        StringAssert.DoesNotContain("openCardPack", marketplaceSource);

        StringAssert.Contains("public void PurchaseCardPack", serverFunctionsSource);
        StringAssert.Contains("\"purchaseCardPack\"", serverFunctionsSource);
        StringAssert.DoesNotContain("public void OpenCardPack", serverFunctionsSource);
        StringAssert.DoesNotContain("\"openCardPack\"", serverFunctionsSource);

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

    private static string ExtractMethodBody(string source, string startMarker, string endMarker)
    {
        int start = source.IndexOf(startMarker, System.StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, $"Start marker was not found: {startMarker}");

        int end = source.IndexOf(endMarker, start, System.StringComparison.Ordinal);
        Assert.Greater(end, start, $"End marker was not found after: {startMarker}");

        return source.Substring(start, end - start);
    }
}
