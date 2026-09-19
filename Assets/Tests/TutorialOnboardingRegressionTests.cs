using System.IO;
using NUnit.Framework;
using UnityEngine;

public class TutorialOnboardingRegressionTests
{
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
        StringAssert.Contains("ResolveSceneForTutorialRoute", source);
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
    public void RoyalRumble_SourceReferencesPlayerActionCheckpoints()
    {
        string shell = ReadScript("RoyalRumble", "RoyalRumbleShellController.cs");
        string coordinator = ReadScript("RoyalRumble", "RoyalRumbleBattleCoordinator.cs");

        StringAssert.Contains("TutorialConstants.SelectPlayerCard", shell);
        StringAssert.Contains("TutorialConstants.SelectAttack", shell);
        StringAssert.Contains("TutorialConstants.ConfirmAttack", shell);
        StringAssert.Contains("TutorialConstants.CompleteControls", shell);
        StringAssert.Contains("TutorialConstants.SelectPlayerCard", coordinator);
        StringAssert.Contains("TutorialConstants.SelectAttack", coordinator);
        StringAssert.Contains("TutorialConstants.ConfirmAttack", coordinator);
        StringAssert.Contains("TutorialConstants.CompleteControls", coordinator);
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
}
