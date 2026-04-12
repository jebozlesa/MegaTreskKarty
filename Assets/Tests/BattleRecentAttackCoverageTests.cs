using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class BattleRecentAttackCoverageTests
{
    [Test]
    public void AttackRegistry_RegistersRecentHandlersThroughAttack60()
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
        StringAssert.Contains("[50] = new AttackDefinition(\"Continental Blockade\"", source);
        StringAssert.Contains("Attack50Handler.Execute(", source);
        StringAssert.Contains("[51] = new AttackDefinition(\"Depression\"", source);
        StringAssert.Contains("Attack51Handler.Execute(", source);
        StringAssert.Contains("[52] = new AttackDefinition(\"Self Isolation\"", source);
        StringAssert.Contains("Attack52Handler.Execute(", source);
        StringAssert.Contains("[53] = new AttackDefinition(\"Knife\"", source);
        StringAssert.Contains("Attack53Handler.Execute(", source);
        StringAssert.Contains("[54] = new AttackDefinition(\"Autoportrait\"", source);
        StringAssert.Contains("Attack54Handler.Execute(", source);
        StringAssert.Contains("[55] = new AttackDefinition(\"Gravity Pull\"", source);
        StringAssert.Contains("Attack55Handler.Execute(", source);
        StringAssert.Contains("[56] = new AttackDefinition(\"Kamikaze\"", source);
        StringAssert.Contains("Attack56Handler.Execute(", source);
        StringAssert.Contains("[57] = new AttackDefinition(\"Take Off\"", source);
        StringAssert.Contains("Attack57Handler.Execute(", source);
        StringAssert.Contains("[58] = new AttackDefinition(\"Air Strike\"", source);
        StringAssert.Contains("Attack58Handler.Execute(", source);
        StringAssert.Contains("[59] = new AttackDefinition(\"Justice Crusade\"", source);
        StringAssert.Contains("Attack59Handler.Execute(", source);
        StringAssert.Contains("[60] = new AttackDefinition(\"Rapier\"", source);
        StringAssert.Contains("Attack60Handler.Execute(", source);
        StringAssert.Contains("[61] = new AttackDefinition(\"Expeditionary Assault\"", source);
        StringAssert.Contains("Attack61Handler.Execute(", source);
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

        string blockade = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack50Handler.cs");
        StringAssert.Contains("PlayContinentalBlockadeAnimation(attacker.transform, defender.transform)", blockade);
        StringAssert.Contains("effectsApplied != null", blockade);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", blockade);

        string depression = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack51Handler.cs");
        StringAssert.Contains("PlayDepressionAnimation(attacker.transform)", depression);
        StringAssert.Contains("PlayDepressionStartAnimation(defender.transform)", depression);
        StringAssert.Contains("PlayArtInspirationStartAnimation(attacker.transform)", depression);
        Assert.IsFalse(depression.Contains("HandleStrength("), "Attack51 should rely on shared stat/effect playback, not direct stat mutation.");

        string selfIsolation = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack52Handler.cs");
        StringAssert.Contains("PlaySelfIsolationAnimation(attacker.transform)", selfIsolation);
        StringAssert.Contains("attackResult == \"isolated_inspired\"", selfIsolation);
        StringAssert.Contains("PlayArtInspirationStartAnimation(attacker.transform)", selfIsolation);
        Assert.IsFalse(selfIsolation.Contains("HandleDefense("), "Attack52 should rely on shared stat playback, not direct stat mutation.");

        string knife = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack53Handler.cs");
        StringAssert.Contains("PlayKnifeAnimation(attacker.transform, defender.transform)", knife);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", knife);
        StringAssert.Contains("!isMyAttack", knife);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", knife);

        string autoportrait = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack54Handler.cs");
        StringAssert.Contains("PlayAutoportraitAnimation(attacker.transform)", autoportrait);
        Assert.IsFalse(autoportrait.Contains("HandleKnowledge("), "Attack54 should rely on shared stat playback, not direct stat mutation.");
        Assert.IsFalse(autoportrait.Contains("HandleStrength("), "Attack54 should rely on shared stat playback, not direct stat mutation.");
        Assert.IsFalse(autoportrait.Contains("HandleDefense("), "Attack54 should rely on shared stat playback, not direct stat mutation.");

        string gravityPull = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack55Handler.cs");
        StringAssert.Contains("PlayGravityPullAnimation(attacker.transform, defender.transform, variantIndex)", gravityPull);
        StringAssert.Contains("effectsApplied != null", gravityPull);
        StringAssert.Contains("PlayKnockoutAnimation(defender.transform)", gravityPull);

        string kamikaze = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack56Handler.cs");
        StringAssert.Contains("PlayKamikazeAnimation(attacker.transform, defender.transform, hit)", kamikaze);
        StringAssert.Contains("attackerSelfDamage", kamikaze);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", kamikaze);

        string takeOff = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack57Handler.cs");
        StringAssert.Contains("PlayTakeOffAnimation(attacker.transform)", takeOff);
        StringAssert.Contains("PlayTakeOffCrashAnimation(attacker.transform)", takeOff);
        StringAssert.Contains("attackerSelfDamage", takeOff);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", takeOff);

        string airStrike = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack58Handler.cs");
        StringAssert.Contains("PlayAirStrikeAnimation(attacker.transform, defender.transform, displayedHitCount)", airStrike);
        StringAssert.Contains("PlayAirStrikeCriticalAnimation(defender.transform)", airStrike);
        StringAssert.Contains("ParseAttackResult(attackResult, out int hitCount, out bool critical)", airStrike);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", airStrike);

        string justiceCrusade = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack59Handler.cs");
        StringAssert.Contains("PlayJusticeCrusadeAnimation(attacker.transform)", justiceCrusade);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", justiceCrusade);
        StringAssert.Contains("fights enemy for justice", justiceCrusade);

        string rapier = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack60Handler.cs");
        StringAssert.Contains("PlayRapierAnimation(attacker.transform, defender.transform)", rapier);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", rapier);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", rapier);
        StringAssert.Contains("is wounded", rapier);

        string expedition = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack61Handler.cs");
        StringAssert.Contains("PlayExpeditionaryAssaultAnimation(attacker.transform)", expedition);
        StringAssert.Contains("PlayExpeditionaryAssaultSuccessAnimation(attacker.transform, attacker.transform)", expedition);
        StringAssert.Contains("PlayExpeditionaryAssaultFailAnimation(attacker.transform)", expedition);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", expedition);
        StringAssert.Contains("almost died on sea", expedition);
    }

    [Test]
    public void SharedEffectPlayback_CoversElectricityTetherBlockadeDepressionAndOngoingActions()
    {
        string effectPlayback = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");
        StringAssert.Contains("effectType == 8", effectPlayback);
        StringAssert.Contains("PlayElectricityEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Electricity\";", effectPlayback);
        StringAssert.Contains("effectType == 9", effectPlayback);
        StringAssert.Contains("PlayTetherEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Tether\";", effectPlayback);
        StringAssert.Contains("effectType == 12", effectPlayback);
        StringAssert.Contains("PlayBlocadeEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Blockade\";", effectPlayback);
        StringAssert.Contains("effectType == 13", effectPlayback);
        StringAssert.Contains("PlayDepressionEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Depression\";", effectPlayback);

        string resultProcessor = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("case 8: // ELECTRICITY", resultProcessor);
        StringAssert.Contains("PlayElectricityAnimation(card.transform)", resultProcessor);
        StringAssert.Contains("ShowDialog($\"{card.cardName} cannot move\")", resultProcessor);
        StringAssert.Contains("case 9: // TETHER", resultProcessor);
        StringAssert.Contains("PlayTetherAnimation(card.transform)", resultProcessor);
        StringAssert.Contains("ShowDialog($\"{card.cardName} is locked\")", resultProcessor);
        StringAssert.Contains("case 12: // BLOCKADE", resultProcessor);
        StringAssert.Contains("PlayBlocadeWaitAnimation(card.transform)", resultProcessor);
        StringAssert.Contains("ShowDialog(\"The blockade holds strong\")", resultProcessor);
        StringAssert.Contains("!string.Equals(step.Source, \"attack\", StringComparison.OrdinalIgnoreCase)", resultProcessor);
        StringAssert.Contains("doubleEnvelopment", resultProcessor);
        StringAssert.Contains("PlayDoubleEnvelopmentWaitAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("PlayDoubleEnvelopAttackAnimation(target.transform)", resultProcessor);
        StringAssert.Contains("artInspiration", resultProcessor);
        StringAssert.Contains("PlayArtInspirationWaitAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("PlayArtInspirationEndAttackAnimation(actor.transform, target.transform)", resultProcessor);
        StringAssert.Contains("autoportrait", resultProcessor);
        StringAssert.Contains("PlayAutoportraitAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("PlayAutoportraitFinishAnimation(actor.transform)", resultProcessor);

        string kard = ReadProjectFile("Assets", "Scripts", "Kard.cs");
        StringAssert.Contains("effectName == \"Blockade\"", kard);
        StringAssert.Contains("\"Game/Animations/continentalblocade\"", kard);
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












