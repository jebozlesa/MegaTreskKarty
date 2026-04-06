using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class BattleRecentAttackCoverageTests
{
    [Test]
    public void AttackRegistry_RegistersRecentHandlersThroughAttack49()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "AttackRegistry.cs");

        StringAssert.Contains("[38] = new AttackDefinition(\"Marxism\"", source);
        StringAssert.Contains("Attack38Handler.Execute(", source);
        StringAssert.Contains("[39] = new AttackDefinition(\"Tesla Coil\"", source);
        StringAssert.Contains("Attack39Handler.Execute(", source);
        StringAssert.Contains("[40] = new AttackDefinition(\"Wireless Charger\"", source);
        StringAssert.Contains("Attack40Handler.Execute(", source);
        StringAssert.Contains("[41] = new AttackDefinition(\"Experiment\"", source);
        StringAssert.Contains("Attack41Handler.Execute(", source);
        StringAssert.Contains("[42] = new AttackDefinition(\"Tommy Gun\"", source);
        StringAssert.Contains("Attack42Handler.Execute(", source);
        StringAssert.Contains("[43] = new AttackDefinition(\"Tie Up\"", source);
        StringAssert.Contains("Attack43Handler.Execute(", source);
        StringAssert.Contains("[44] = new AttackDefinition(\"Corruption\"", source);
        StringAssert.Contains("Attack44Handler.Execute(", source);
        StringAssert.Contains("[45] = new AttackDefinition(\"Colt 1911\"", source);
        StringAssert.Contains("Attack45Handler.Execute(", source);
        StringAssert.Contains("[46] = new AttackDefinition(\"Mortar\"", source);
        StringAssert.Contains("Attack46Handler.Execute(", source);
        StringAssert.Contains("[47] = new AttackDefinition(\"Great Army\"", source);
        StringAssert.Contains("Attack47Handler.Execute(", source);
        StringAssert.Contains("[49] = new AttackDefinition(\"Double Envelopment\"", source);
        StringAssert.Contains("Attack49Handler.Execute(", source);
    }

    [Test]
    public void RecentHandlers_UseExpectedSharedPlaybackPatterns()
    {
        string marxism = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack38Handler.cs");
        StringAssert.Contains("PlayMarxismAnimation(attacker.transform)", marxism);
        Assert.IsFalse(marxism.Contains("HandleCharisma("), "Attack38 should rely on shared stat playback, not direct stat mutation.");

        string tesla = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack39Handler.cs");
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", tesla);
        StringAssert.Contains("PlayElectricityStartAnimation(defender.transform)", tesla);

        string charger = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack40Handler.cs");
        StringAssert.Contains("BattleValuePlayback.PlayHeal(", charger);
        Assert.IsFalse(charger.Contains("attacker.health +="), "Attack40 should use shared heal playback.");

        string experiment = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack41Handler.cs");
        StringAssert.Contains("AnimateDamage(defender, damage)", experiment);
        StringAssert.Contains("AnimateDamage(attacker, attackerSelfDamage)", experiment);

        string tommyGun = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack42Handler.cs");
        StringAssert.Contains("PlayTommyGunAnimation(attacker.transform, defender.transform, Mathf.Max(1, damage))", tommyGun);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", tommyGun);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", tommyGun);

        string tieUp = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack43Handler.cs");
        StringAssert.Contains("PlayTieUpAnimation(attacker.transform, defender.transform)", tieUp);
        StringAssert.Contains("PlayAnimationTiedUp(defender.transform)", tieUp);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", tieUp);

        string corruption = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack44Handler.cs");
        StringAssert.Contains("attackResult == \"corrupting_1\" ? 1 : 2", corruption);
        StringAssert.Contains("PlayCorruptionAnimation(attacker.transform, multiplier)", corruption);
        Assert.IsFalse(corruption.Contains("HandleSpeed("), "Attack44 should rely on shared stat playback, not direct stat mutation.");
        Assert.IsFalse(corruption.Contains("HandleDefense("), "Attack44 should rely on shared stat playback, not direct stat mutation.");
        Assert.IsFalse(corruption.Contains("HandleCharisma("), "Attack44 should rely on shared stat playback, not direct stat mutation.");

        string colt = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack45Handler.cs");
        StringAssert.Contains("PlayColt1911Animation(attacker.transform, defender.transform, hit)", colt);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", colt);
        StringAssert.Contains("!isMyAttack", colt);

        string mortar = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack46Handler.cs");
        StringAssert.Contains("attackResult == \"backfire\"", mortar);
        StringAssert.Contains("PlayMortarAnimation(attacker.transform, attacker.transform, true)", mortar);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", mortar);
        StringAssert.Contains("attackerSelfDamage", mortar);

        string greatArmy = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack47Handler.cs");
        StringAssert.Contains("PlayGreatArmyAnimation(attacker.transform)", greatArmy);
        StringAssert.Contains("attackResult == \"fortified\"", greatArmy);
        Assert.IsFalse(greatArmy.Contains("HandleAttack("), "Attack47 should rely on shared stat playback, not direct stat mutation.");
        Assert.IsFalse(greatArmy.Contains("HandleStrength("), "Attack47 should rely on shared stat playback, not direct stat mutation.");
        Assert.IsFalse(greatArmy.Contains("HandleDefense("), "Attack47 should rely on shared stat playback, not direct stat mutation.");

        string doubleEnvelopment = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack49Handler.cs");
        StringAssert.Contains("PlayDoubleEnvelopmentAnimation(attacker.transform)", doubleEnvelopment);
        Assert.IsFalse(doubleEnvelopment.Contains("HandleDefense("), "Attack49 should rely on shared stat playback, not direct stat mutation.");
    }

    [Test]
    public void ElectricityPlayback_IsCoveredBySharedEffectAndProcessorFlow()
    {
        string effectPlayback = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");
        StringAssert.Contains("effectType == 8", effectPlayback);
        StringAssert.Contains("PlayElectricityEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Electricity\";", effectPlayback);
        StringAssert.Contains("effectType == 9", effectPlayback);
        StringAssert.Contains("PlayTetherEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Tether\";", effectPlayback);

        string resultProcessor = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("case 8: // ELECTRICITY", resultProcessor);
        StringAssert.Contains("PlayElectricityAnimation(card.transform)", resultProcessor);
        StringAssert.Contains("ShowDialog($\"{card.cardName} cannot move\")", resultProcessor);
        StringAssert.Contains("case 9: // TETHER", resultProcessor);
        StringAssert.Contains("PlayTetherAnimation(card.transform)", resultProcessor);
        StringAssert.Contains("ShowDialog($\"{card.cardName} is locked\")", resultProcessor);
        StringAssert.Contains("doubleEnvelopment", resultProcessor);
        StringAssert.Contains("PlayDoubleEnvelopmentWaitAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("PlayDoubleEnvelopAttackAnimation(target.transform)", resultProcessor);
    }

    [Test]
    public void Readme_ListsRecentHandlersAndUpdatedProgress()
    {
        string readme = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "README.md");

        StringAssert.Contains("Attack38Handler.cs  - Marxism", readme);
        StringAssert.Contains("Attack39Handler.cs  - Tesla Coil", readme);
        StringAssert.Contains("Attack40Handler.cs  - Wireless Charger", readme);
        StringAssert.Contains("Attack41Handler.cs  - Experiment", readme);
        StringAssert.Contains("Attack42Handler.cs  - Tommy Gun", readme);
        StringAssert.Contains("Attack43Handler.cs  - Tie Up", readme);
        StringAssert.Contains("Attack44Handler.cs  - Corruption", readme);
        StringAssert.Contains("Attack45Handler.cs  - Colt 1911", readme);
        StringAssert.Contains("Attack46Handler.cs  - Mortar", readme);
        StringAssert.Contains("Attack47Handler.cs  - Great Army", readme);
        StringAssert.Contains("Attack49Handler.cs  - Double Envelopment", readme);
        StringAssert.Contains("Progress: 48/123 attacks (39.0%)", readme);
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



