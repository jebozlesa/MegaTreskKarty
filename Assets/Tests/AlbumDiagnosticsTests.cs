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
}
