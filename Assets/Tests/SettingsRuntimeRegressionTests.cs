using System.IO;
using NUnit.Framework;
using UnityEngine;

public class SettingsRuntimeRegressionTests
{
    [Test]
    public void GameAudioSettingsSource_DefinesReleaseAudioOwnership()
    {
        string source = ReadScript("GameAudioSettings.cs");

        StringAssert.Contains("SoundMutedKey = \"isSoundMuted\"", source);
        StringAssert.Contains("MusicMutedKey = \"isMusicMuted\"", source);
        StringAssert.Contains("AudioListener.volume = IsSoundEnabled ? 1f : 0f", source);
        StringAssert.Contains("ShouldPlaySoundEffect()", source);
        StringAssert.Contains("sceneName == \"Settings\"", source);
    }

    [Test]
    public void SettingsControllerSource_HidesAvatarAndUsesTemporaryGooglePlayLinks()
    {
        string source = ReadScript("SettingsController.cs");

        StringAssert.Contains("https://play.google.com/store", source);
        StringAssert.Contains("avatarEntryObject.SetActive(false)", source);
        StringAssert.Contains("PlayerSessionLogout.LogoutAndLoadLogin", source);
    }

    [Test]
    public void LogoutSource_ClearsLocalSessionKeys()
    {
        string source = ReadScript("PlayerSessionLogout.cs");
        StringAssert.Contains("DeleteKey(\"username\")", source);
        StringAssert.Contains("DeleteKey(\"email\")", source);
        StringAssert.Contains("DeleteKey(\"password\")", source);
        StringAssert.Contains("DeleteKey(\"LoggedInPlayerId\")", source);
        StringAssert.Contains("DeleteKey(\"RoomCode\")", source);
        StringAssert.Contains("DeleteKey(\"IsWaitingForOpponent\")", source);
        StringAssert.Contains("ClearRuntimeLoginState", source);
    }

    private static string ReadScript(string fileName)
    {
        string sourcePath = Path.Combine(Application.dataPath, "Scripts", fileName);
        Assert.IsTrue(File.Exists(sourcePath), fileName + " was not found.");
        return File.ReadAllText(sourcePath);
    }
}
