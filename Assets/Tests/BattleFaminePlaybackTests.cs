using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
public class BattleFaminePlaybackTests
{
    [Test]
    public void Attack37Handler_RendersCastAndNoEffectBranches()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack37Handler.cs");
        StringAssert.Contains("animations.PlayFamineAnimation(defender.transform)", source);
        StringAssert.Contains("AttackPlaybackShared.PlayStandardTargetDamage(", source);
        StringAssert.Contains("PlayAnimationNotImpressed(defender.transform)", source);
        StringAssert.Contains("Be a human, {attacker.cardName}!", source);
    }
    [Test]
    public void BattleResultProcessor_HandlesFamineHealStepsBeforeAttacks()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("case BattleStepType.Heal:", source);
        StringAssert.Contains("string.Equals(step.Source, \"famine\", StringComparison.OrdinalIgnoreCase)", source);
        StringAssert.Contains("animations.PlayFamineContinueAnimation(target.transform)", source);
        StringAssert.Contains("cardAnimator.AnimateHeal(target, step.Amount)", source);
        StringAssert.Contains("ShowDialog(step.Note)", source);
    }
    [Test]
    public void BattleEffectVisuals_PlaysFamineEndAnimationOnEffectRemoval()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectVisuals.cs");
        StringAssert.Contains("effectType == 7", source);
        StringAssert.Contains("PlayFamineEndAnimation(card.transform)", source);
    }
    [Test]
    public void AttackRegistry_RegistersAttack37Handler()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "AttackRegistry.cs");
        StringAssert.Contains("[37] = new AttackDefinition(\"Famine\"", source);
        StringAssert.Contains("Attack37Handler.Execute(", source);
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
