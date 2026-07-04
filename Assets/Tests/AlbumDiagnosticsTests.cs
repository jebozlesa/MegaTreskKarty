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

        StringAssert.Contains("Debug.LogWarning($\"[Album] PlayerCards load requested", source);
        StringAssert.Contains("Debug.LogWarning($\"[Album] PlayerCards raw loaded", source);
        StringAssert.Contains("Debug.LogWarning($\"[Album] PlayerCards parsed", source);
        StringAssert.Contains("Debug.LogWarning($\"[Album] Card loaded", source);
        StringAssert.DoesNotContain("Debug.Log($\"[Album]", source);
    }
}
