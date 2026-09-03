using System.IO;
using NUnit.Framework;
using UnityEngine;

public class TutorialOnboardingRegressionTests
{
    [Test]
    public void ServerFunctionsManager_ExposesTutorialCloudFunctions()
    {
        string source = ReadScript("Networking", "ServerFunctionsManager.cs");

        StringAssert.Contains("GetTutorialState", source);
        StringAssert.Contains("CompleteTutorialStep", source);
        StringAssert.Contains("\"getTutorialState\"", source);
        StringAssert.Contains("\"completeTutorialStep\"", source);
    }

    [Test]
    public void Login_UsesServerTutorialRoute()
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
    public void RoyalRumble_CompletesFirstBattleTutorialThroughPlayerActions()
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
        string[] fullParts = new string[pathParts.Length + 1];
        fullParts[0] = Application.dataPath;
        pathParts.CopyTo(fullParts, 1);

        string sourcePath = Path.Combine(fullParts);
        Assert.IsTrue(File.Exists(sourcePath), sourcePath + " was not found.");
        return File.ReadAllText(sourcePath);
    }
}
