using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class BattleSharedDamagePlaybackTests
{
    [Test]
    public void BattleResultProcessor_DoesNotContainSharedDamageFallbackAfterHandlerExecution()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");

        StringAssert.Contains("yield return AttackRegistry.ExecuteOrFallback(attackId, context);", source);
        Assert.IsFalse(source.Contains("defender.health == defenderHealthBeforeAttack"));
        Assert.IsFalse(source.Contains("PlayTimelineDamageAnimation(defender, damage, isMyTarget)"));
        Assert.IsFalse(source.Contains("[AttackPlayback] Applying shared damage fallback"));
    }

    [Test]
    public void StandardDamageHandlers_UseSharedDamageHelper()
    {
        string[] helperHandlers =
        {
            "Attack1Handler.cs",
            "Attack2Handler.cs",
            "Attack5Handler.cs",
            "Attack8Handler.cs",
            "Attack10Handler.cs",
            "Attack12Handler.cs",
            "Attack13Handler.cs",
            "Attack16Handler.cs",
            "Attack17Handler.cs",
            "Attack18Handler.cs",
            "Attack19Handler.cs",
            "Attack20Handler.cs",
            "Attack23Handler.cs",
            "Attack24Handler.cs",
            "Attack25Handler.cs",
            "Attack28Handler.cs",
            "Attack32Handler.cs",
            "Attack34Handler.cs",
            "Attack36Handler.cs",
        };

        foreach (string fileName in helperHandlers)
        {
            string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", fileName);
            StringAssert.Contains("AttackPlaybackShared.PlayStandardTargetDamage(", source, $"Expected shared damage helper in {fileName}");
            Assert.IsFalse(source.Contains("defender.health -= damage;"), $"Manual defender damage should be removed from {fileName}");
        }
    }

    [Test]
    public void OnlyExplicitSpecialCaseHandler_UsesManualDefenderDamage()
    {
        string handlersDir = Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "AttackHandlers");
        string[] allHandlerFiles = Directory.GetFiles(handlersDir, "Attack*Handler.cs");

        var manualDamageHandlers = new List<string>();
        foreach (string path in allHandlerFiles)
        {
            string source = File.ReadAllText(path);
            if (source.Contains("defender.health -= damage;"))
            {
                manualDamageHandlers.Add(Path.GetFileName(path));
            }
        }

        CollectionAssert.AreEquivalent(new[] { "Attack7Handler.cs" }, manualDamageHandlers);
    }

    private static string ReadProjectFile(params string[] relativeParts)
    {
        var allParts = new List<string> { Application.dataPath };
        allParts.AddRange(relativeParts[1..]);
        string fullPath = Path.Combine(allParts.ToArray());
        Assert.IsTrue(File.Exists(fullPath), $"Missing project file: {fullPath}");
        return File.ReadAllText(fullPath);
    }
}
