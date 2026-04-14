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
        StringAssert.Contains("[62] = new AttackDefinition(\"Culverin\"", source);
        StringAssert.Contains("Attack62Handler.Execute(", source);
        StringAssert.Contains("[63] = new AttackDefinition(\"Fire Ship\"", source);
        StringAssert.Contains("Attack63Handler.Execute(", source);
        StringAssert.Contains("[64] = new AttackDefinition(\"Handcuff Escape\"", source);
        StringAssert.Contains("Attack64Handler.Execute(", source);
        StringAssert.Contains("[65] = new AttackDefinition(\"Illusion\"", source);
        StringAssert.Contains("Attack65Handler.Execute(", source);
        StringAssert.Contains("[66] = new AttackDefinition(\"Carcano M91\"", source);
        StringAssert.Contains("Attack66Handler.Execute(", source);
        StringAssert.Contains("[67] = new AttackDefinition(\"Winchester\"", source);
        StringAssert.Contains("Attack67Handler.Execute(", source);
        StringAssert.Contains("[82] = new AttackDefinition(\"Iaijutsu\"", source);
        StringAssert.Contains("Attack82Handler.Execute(", source);
        StringAssert.Contains("[68] = new AttackDefinition(\"Ambush\"", source);
        StringAssert.Contains("Attack68Handler.Execute(", source);
        StringAssert.Contains("[69] = new AttackDefinition(\"Space Rocket\"", source);
        StringAssert.Contains("Attack69Handler.Execute(", source);
        StringAssert.Contains("[70] = new AttackDefinition(\"V-2\"", source);
        StringAssert.Contains("Attack70Handler.Execute(", source);
        StringAssert.Contains("[71] = new AttackDefinition(\"Battle Cry\"", source);
        StringAssert.Contains("Attack71Handler.Execute(", source);
        StringAssert.Contains("[72] = new AttackDefinition(\"Revelation\"", source);
        StringAssert.Contains("Attack72Handler.Execute(", source);
        StringAssert.Contains("[73] = new AttackDefinition(\"Standard\"", source);
        StringAssert.Contains("Attack73Handler.Execute(", source);
        StringAssert.Contains("[74] = new AttackDefinition(\"Pen\"", source);
        StringAssert.Contains("Attack74Handler.Execute(", source);
        StringAssert.Contains("[75] = new AttackDefinition(\"Iambic Pentameter\"", source);
        StringAssert.Contains("Attack75Handler.Execute(", source);
        StringAssert.Contains("[76] = new AttackDefinition(\"Ghost\"", source);
        StringAssert.Contains("Attack76Handler.Execute(", source);
        StringAssert.Contains("[77] = new AttackDefinition(\"Buffalo Horns\"", source);
        StringAssert.Contains("Attack77Handler.Execute(", source);
        StringAssert.Contains("[78] = new AttackDefinition(\"Iklwa\"", source);
        StringAssert.Contains("Attack78Handler.Execute(", source);
        StringAssert.Contains("[79] = new AttackDefinition(\"Iwisa\"", source);
        StringAssert.Contains("Attack79Handler.Execute(", source);
        StringAssert.Contains("[80] = new AttackDefinition(\"Niten Ichi-ryū\"", source);
        StringAssert.Contains("Attack80Handler.Execute(", source);
        StringAssert.Contains("[81] = new AttackDefinition(\"Tessenjutsu\"", source);
        StringAssert.Contains("Attack81Handler.Execute(", source);
        StringAssert.Contains("[68] = new AttackDefinition(\"Ambush\"", source);
        StringAssert.Contains("Attack68Handler.Execute(", source);
        StringAssert.Contains("[69] = new AttackDefinition(\"Space Rocket\"", source);
        StringAssert.Contains("Attack69Handler.Execute(", source);
        StringAssert.Contains("[70] = new AttackDefinition(\"V-2\"", source);
        StringAssert.Contains("Attack70Handler.Execute(", source);
        StringAssert.Contains("[71] = new AttackDefinition(\"Battle Cry\"", source);
        StringAssert.Contains("Attack71Handler.Execute(", source);
        StringAssert.Contains("[72] = new AttackDefinition(\"Revelation\"", source);
        StringAssert.Contains("Attack72Handler.Execute(", source);
        StringAssert.Contains("[73] = new AttackDefinition(\"Standard\"", source);
        StringAssert.Contains("Attack73Handler.Execute(", source);
        StringAssert.Contains("[74] = new AttackDefinition(\"Pen\"", source);
        StringAssert.Contains("Attack74Handler.Execute(", source);
        StringAssert.Contains("[75] = new AttackDefinition(\"Iambic Pentameter\"", source);
        StringAssert.Contains("Attack75Handler.Execute(", source);
        StringAssert.Contains("[76] = new AttackDefinition(\"Ghost\"", source);
        StringAssert.Contains("Attack76Handler.Execute(", source);
        StringAssert.Contains("[77] = new AttackDefinition(\"Buffalo Horns\"", source);
        StringAssert.Contains("Attack77Handler.Execute(", source);
        StringAssert.Contains("[78] = new AttackDefinition(\"Iklwa\"", source);
        StringAssert.Contains("Attack78Handler.Execute(", source);
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

        string culverin = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack62Handler.cs");
        StringAssert.Contains("PlayCulverinAnimation(attacker.transform, defender.transform, hit)", culverin);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", culverin);
        StringAssert.Contains("Bang! aaaand miss", culverin);
        StringAssert.Contains("cannon fires", culverin);

        string fireShip = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack63Handler.cs");
        StringAssert.Contains("PlayFireShipAnimation(attacker.transform, defender.transform)", fireShip);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", fireShip);
        StringAssert.Contains("PlayBurnStartAnimation(defender.transform)", fireShip);
        StringAssert.Contains("Fire ship impacts", fireShip);
        StringAssert.Contains("is burning", fireShip);

        string handcuffEscape = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack64Handler.cs");
        StringAssert.Contains("PlayHandcuffEscapeAnimation(attacker.transform)", handcuffEscape);
        StringAssert.Contains("PlayConfusionStartAnimation(defender.transform)", handcuffEscape);
        StringAssert.Contains("slips outta trouble", handcuffEscape);
        StringAssert.Contains("is confused", handcuffEscape);

        string illusion = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack65Handler.cs");
        StringAssert.Contains("PlayIllusionAnimation(attacker.transform, Random.Range(0, 5))", illusion);
        StringAssert.Contains("PlayConfusionStartAnimation(defender.transform)", illusion);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", illusion);
        StringAssert.Contains("uses Illusion", illusion);
        StringAssert.Contains("is already confused", illusion);

        string carcano = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack66Handler.cs");
        StringAssert.Contains("PlayCarcanoAnimation(attacker.transform, defender.transform, hit)", carcano);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", carcano);
        StringAssert.Contains("Bang! aaaand miss", carcano);
        StringAssert.Contains("Bang! {attacker.cardName} shoots", carcano);

        string winchester = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack67Handler.cs");
        StringAssert.Contains("PlayWinchesterAnimation(attacker.transform, defender.transform, hit)", winchester);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", winchester);
        StringAssert.Contains("Bang! aaaand miss", winchester);
        StringAssert.Contains("Bang! {attacker.cardName} shoots", winchester);

        string ambush = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack68Handler.cs");
        StringAssert.Contains("PlayAmbushAnimation(defender.transform)", ambush);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", ambush);
        StringAssert.Contains("{defender.cardName} ambushed with surprise", ambush);

        string jupiter = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack69Handler.cs");
        StringAssert.Contains("PlaySpaceRocketAnimation(attacker.transform, !exploded)", jupiter);
        StringAssert.Contains("Rocket exploded", jupiter);
        StringAssert.Contains("{attacker.cardName} launches satellite", jupiter);

        string v2 = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack70Handler.cs");
        StringAssert.Contains("PlayV2Animation(attacker.transform, defender.transform, hit)", v2);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", v2);
        StringAssert.Contains("Missile missed", v2);
        StringAssert.Contains("V2 hits target", v2);

        string battleCry = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack71Handler.cs");
        StringAssert.Contains("PlayBattleCryAnimation(attacker.transform)", battleCry);
        StringAssert.Contains("uses BattleCry", battleCry);
        StringAssert.Contains("roared into battle", battleCry);

        string revelation = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack72Handler.cs");
        StringAssert.Contains("PlayRevelationAnimation(attacker.transform, variantIndex)", revelation);
        StringAssert.Contains("uses Revelation", revelation);
        StringAssert.Contains("God is with {attacker.cardName}", revelation);

        string revelationRules = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "RevelationRules.cs");
        StringAssert.Contains("GetVariantIndex", revelationRules);
        StringAssert.Contains("10001", revelationRules);
        StringAssert.Contains("46", revelationRules);

        string standard = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack73Handler.cs");
        StringAssert.Contains("PlayStandardAnimation(attacker.transform)", standard);
        StringAssert.Contains("uses Standard", standard);
        StringAssert.Contains("Banner Raised Morale", standard);

        string pen = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack74Handler.cs");
        StringAssert.Contains("PlayPenAnimation(attacker.transform, defender.transform)", pen);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", pen);
        StringAssert.Contains("PlayPoisonStartAnimation(defender.transform)", pen);
        StringAssert.Contains("The pen is mightier than the sword", pen);

        string iambicPentameter = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack75Handler.cs");
        StringAssert.Contains("PlayIambicPentameterAnimation(attacker.transform, defender.transform)", iambicPentameter);
        StringAssert.Contains("PlayConfusionStartAnimation(defender.transform)", iambicPentameter);
        StringAssert.Contains("BattleValuePlayback.PlayHeal(", iambicPentameter);
        StringAssert.Contains("does not understand", iambicPentameter);
        StringAssert.Contains("likes this poetry", iambicPentameter);

        string ghost = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack76Handler.cs");
        StringAssert.Contains("PlayGhostAnimation(attacker.transform, defender.transform)", ghost);
        StringAssert.Contains("PlayFearStartAnimation(defender.transform)", ghost);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", ghost);
        StringAssert.Contains("summons ghost", ghost);
        StringAssert.Contains("does not fear", ghost);

        string buffaloHorns = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack77Handler.cs");
        StringAssert.Contains("PlayBuffaloHornsAnimation(attacker.transform)", buffaloHorns);
        StringAssert.Contains("uses Buffalo Horns", buffaloHorns);
        StringAssert.Contains("Launching the maneuver!", buffaloHorns);

        string iklwa = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack78Handler.cs");
        StringAssert.Contains("PlayIklwaAnimation(attacker.transform, defender.transform, hit)", iklwa);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", iklwa);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", iklwa);
        StringAssert.Contains("uses Iklwa", iklwa);
        StringAssert.Contains("throw missed", iklwa);

        string iwisa = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack79Handler.cs");
        StringAssert.Contains("PlayIwisaAnimation(attacker.transform, defender.transform)", iwisa);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", iwisa);
        StringAssert.Contains("PlayKnockoutAnimation(defender.transform)", iwisa);
        StringAssert.Contains("uses Iwisa", iwisa);
        StringAssert.Contains("attacks with Iwisa", iwisa);

        string nitenIchiRyu = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack80Handler.cs");
        StringAssert.Contains("PlayNitenIchiRyuKatanaAnimation(attacker.transform, defender.transform)", nitenIchiRyu);
        StringAssert.Contains("PlayNitenIchiRyuWakizashiAnimation(attacker.transform, defender.transform)", nitenIchiRyu);
        StringAssert.Contains("ParseSplitDamage", nitenIchiRyu);
        StringAssert.Contains("uses Niten Ichi-Ryu", nitenIchiRyu);
        StringAssert.Contains("attacks wit Katana and Wakizashi", nitenIchiRyu);

        string tessenjutsu = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack81Handler.cs");
        StringAssert.Contains("PlayTessenjutsuAnimation(attacker.transform)", tessenjutsu);
        StringAssert.Contains("uses Tessenjutsu", tessenjutsu);
        StringAssert.Contains("moves like Kitana", tessenjutsu);

        string iaijutsu = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack82Handler.cs");
        StringAssert.Contains("PlayIaijutsuAnimation(attacker.transform, defender.transform)", iaijutsu);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", iaijutsu);
        StringAssert.Contains("uses Iaijutsu", iaijutsu);
        StringAssert.Contains("Flash of steel by", iaijutsu);
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
        StringAssert.Contains("effectType == 19", effectPlayback);
        StringAssert.Contains("PlayFearEndAnimation(card.transform)", effectPlayback);
        StringAssert.Contains("return \"Fear\";", effectPlayback);

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
        StringAssert.Contains("buffaloHorns", resultProcessor);
        StringAssert.Contains("PlayBuffaloHornsContinueAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("PlayBuffaloHornsEndAnimation(", resultProcessor);
        StringAssert.Contains("case BattleStepType.SatelliteTick:", resultProcessor);
        StringAssert.Contains("PlaySatelliteAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("!string.IsNullOrEmpty(step.Note)", resultProcessor);
        StringAssert.Contains("ShowDialog(step.Note)", resultProcessor);

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




















