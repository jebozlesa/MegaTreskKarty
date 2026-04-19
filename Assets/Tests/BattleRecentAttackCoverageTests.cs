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
        int[] recentAttackIds =
        {
            38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
            49, 50, 51, 52, 53, 54, 55, 56, 57, 58,
            59, 60, 61, 62, 63, 64, 65, 66, 67, 68,
            69, 70, 71, 72, 73, 74, 75, 76, 77, 78,
            79, 80, 81, 82, 83, 84, 85, 86, 87, 88,
            89, 90, 91, 92, 93, 94, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 123,
        };

        foreach (int attackId in recentAttackIds)
        {
            AssertRegistryEntry(source, attackId);
        }
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

        string curse = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack123Handler.cs");
        StringAssert.Contains("PlayCurseAnimation(attacker.transform, defender.transform)", curse);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", curse);
        StringAssert.Contains("uses Curse", curse);
        StringAssert.Contains("curses the enemy", curse);

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

        string yumi = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack85Handler.cs");
        StringAssert.Contains("PlayYumiAnimation(attacker.transform, defender.transform, hit)", yumi);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", yumi);
        StringAssert.Contains("uses Yumi", yumi);
        StringAssert.Contains("arrow missed", yumi);
        StringAssert.Contains("shoots arrow", yumi);

        string jujutsu = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack86Handler.cs");
        StringAssert.Contains("PlayJujutsuAnimation(attacker.transform, defender.transform)", jujutsu);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", jujutsu);
        StringAssert.Contains("uses Jujutsu", jujutsu);
        StringAssert.Contains("Jujutsu Takedown", jujutsu);

        string espionage = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack87Handler.cs");
        StringAssert.Contains("PlayEspionageAnimation(attacker.transform, defender.transform)", espionage);
        StringAssert.Contains("uses Espionage", espionage);
        StringAssert.Contains("spies on enemy", espionage);

        string sabre = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack88Handler.cs");
        StringAssert.Contains("PlaySabreAnimation(attacker.transform, defender.transform)", sabre);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", sabre);
        StringAssert.Contains("uses Sabre", sabre);
        StringAssert.Contains("cuts with Sabre", sabre);

        string gamble = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack89Handler.cs");
        StringAssert.Contains("PlayGambleAnimation(attacker.transform, defender.transform, attackResult == \"win\")", gamble);
        StringAssert.Contains("uses Gamble", gamble);
        StringAssert.Contains("is taking bets", gamble);

        string philosophy = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack90Handler.cs");
        StringAssert.Contains("PlayPhilosophyAnimation(attacker.transform)", philosophy);
        StringAssert.Contains("PlayAnimationImpressed(defender.transform)", philosophy);
        StringAssert.Contains("PlayBoredomAnimation(defender.transform)", philosophy);
        StringAssert.Contains("PlayConfusionStartAnimation(defender.transform)", philosophy);
        StringAssert.Contains("uses Philosophy", philosophy);
        StringAssert.Contains("philosophizes!!!", philosophy);
        StringAssert.Contains("Attack has no effect", philosophy);

        string calm = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack91Handler.cs");
        StringAssert.Contains("PlayCalmAnimation(attacker.transform)", calm);
        StringAssert.Contains("PlayCalmStartAnimation(defender.transform)", calm);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", calm);
        StringAssert.Contains("uses Calm", calm);
        StringAssert.Contains("trying to calm enemy", calm);
        StringAssert.Contains("is calm already", calm);
        StringAssert.Contains("And wins", gamble);
        StringAssert.Contains("And loses", gamble);

        string honesty = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack92Handler.cs");
        StringAssert.Contains("PlayHonestyAnimation(attacker.transform)", honesty);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", honesty);
        StringAssert.Contains("PlayDepressionStartAnimation(defender.transform)", honesty);
        StringAssert.Contains("uses Honesty", honesty);
        StringAssert.Contains("is brutally honest", honesty);
        StringAssert.Contains("feels bad for enemy", honesty);

        string valaska = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack93Handler.cs");
        StringAssert.Contains("PlayValaskaAnimation(attacker.transform, defender.transform)", valaska);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", valaska);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", valaska);
        StringAssert.Contains("uses Valaska", valaska);
        StringAssert.Contains("valaska strikes", valaska);
        StringAssert.Contains("is wounded", valaska);

        string moonshine = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack94Handler.cs");
        StringAssert.Contains("PlayMoonshineAnimation(attacker.transform)", moonshine);
        StringAssert.Contains("BattleValuePlayback.PlayHeal(", moonshine);
        StringAssert.Contains("PlayFuryAnimation(attacker.transform)", moonshine);
        StringAssert.Contains("PlayDrunkAnimation(attacker.transform)", moonshine);
        StringAssert.Contains("uses Moonshine", moonshine);
        StringAssert.Contains("is getting pretty drunk", moonshine);
        StringAssert.Contains("is furious!", moonshine);
        StringAssert.Contains("falls asleep", moonshine);

        string flintlock = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack96Handler.cs");
        StringAssert.Contains("PlayFlintlockPistolLoadingAnimation(attacker.transform)", flintlock);
        StringAssert.Contains("uses Flintlock Pistol", flintlock);
        StringAssert.Contains("is loading the pistol", flintlock);

        string passiveResistance = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack97Handler.cs");
        StringAssert.Contains("PlayPassiveResistanceAnimation(attacker.transform)", passiveResistance);
        StringAssert.Contains("uses Passive Resistance", passiveResistance);
        StringAssert.Contains("is resisting passively!!!", passiveResistance);

        string hungerStrike = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack98Handler.cs");
        StringAssert.Contains("PlayHungerStrikeAnimation(attacker.transform)", hungerStrike);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", hungerStrike);
        StringAssert.Contains("PlayDepressionStartAnimation(defender.transform)", hungerStrike);
        StringAssert.Contains("uses Hunger Strike", hungerStrike);
        StringAssert.Contains("refuses to eat", hungerStrike);
        StringAssert.Contains("doesn't care", hungerStrike);

        string gladius = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack99Handler.cs");
        StringAssert.Contains("PlayGladiusAnimation(attacker.transform, defender.transform)", gladius);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", gladius);
        StringAssert.Contains("uses Gladius", gladius);
        StringAssert.Contains("attacks with gladius", gladius);

        string shieldBash = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack100Handler.cs");
        StringAssert.Contains("PlayShieldBashAnimation(attacker.transform, defender.transform)", shieldBash);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", shieldBash);
        StringAssert.Contains("uses Shield Bash", shieldBash);
        StringAssert.Contains("smashes with his shield", shieldBash);

        string yperit = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack101Handler.cs");
        StringAssert.Contains("PlayYperitSuccessAnimation(attacker.transform, defender.transform)", yperit);
        StringAssert.Contains("PlayYperitFailAnimation(attacker.transform)", yperit);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", yperit);
        StringAssert.Contains("uses Yperit", yperit);
        StringAssert.Contains("Yperit strikes", yperit);
        StringAssert.Contains("A breeze blows", yperit);

        string blitzkrieg = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack102Handler.cs");
        StringAssert.Contains("PlayBlitzkriegAnimation(attacker.transform, defender.transform)", blitzkrieg);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", blitzkrieg);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", blitzkrieg);
        StringAssert.Contains("uses Blitzkrieg", blitzkrieg);
        StringAssert.Contains("Too slow", blitzkrieg);
        StringAssert.Contains("uses Blitzkrieg tactics", blitzkrieg);

        string propaganda = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack103Handler.cs");
        StringAssert.Contains("PlayPropagandaAnimation(attacker.transform, defender.transform)", propaganda);
        StringAssert.Contains("uses Propaganda", propaganda);
        StringAssert.Contains("was hit by propaganda", propaganda);

        string retiarius = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack104Handler.cs");
        StringAssert.Contains("PlayRetiariusAnimation(attacker.transform, defender.transform)", retiarius);
        StringAssert.Contains("PlayAnimationTiedUp(defender.transform)", retiarius);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", retiarius);
        StringAssert.Contains("uses Retiarius", retiarius);
        StringAssert.Contains("throws the net", retiarius);
        StringAssert.Contains("was captured", retiarius);
        StringAssert.Contains("jumped away", retiarius);

        string shuriken = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack105Handler.cs");
        StringAssert.Contains("PlayShurikenAnimation(attacker.transform, defender.transform, hit)", shuriken);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", shuriken);
        StringAssert.Contains("uses Shuriken", shuriken);
        StringAssert.Contains("throw missed", shuriken);
        StringAssert.Contains("hrows stars", shuriken);

        string kusarigama = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack106Handler.cs");
        StringAssert.Contains("PlayKusarigamaAnimation(attacker.transform, defender.transform)", kusarigama);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", kusarigama);
        StringAssert.Contains("PlayAnimationTiedUp(defender.transform)", kusarigama);
        StringAssert.Contains("uses Kusarigama", kusarigama);
        StringAssert.Contains("Kusarigama strikes", kusarigama);
        StringAssert.Contains("is trapped in chain", kusarigama);

        string comboPlayback = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "ComboAttackPlayback.cs");
        StringAssert.Contains("JObject.Parse(parentContext.AttackResult)", comboPlayback);
        StringAssert.Contains("AttackRegistry.ExecuteOrFallback(subAttackId, subAttackContext)", comboPlayback);

        string ninjutsu = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack107Handler.cs");
        StringAssert.Contains("uses Ninjutsu", ninjutsu);
        StringAssert.Contains("ComboAttackPlayback.PlaySequence(comboContext)", ninjutsu);
        string shaolinSoccer = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack121Handler.cs");
        StringAssert.Contains("uses Shaolin Soccer", shaolinSoccer);
        StringAssert.Contains("ComboAttackPlayback.PlaySequence(comboContext)", shaolinSoccer);


        string orientalSpice = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack108Handler.cs");
        StringAssert.Contains("PlayOrientalSpiceAnimation(attacker.transform, defender.transform)", orientalSpice);
        StringAssert.Contains("offers some spices to enemy", orientalSpice);
        StringAssert.Contains("PlayPoisonStartAnimation(defender.transform)", orientalSpice);
        StringAssert.Contains("PlayAnimationNotEffective(defender.transform)", orientalSpice);
        StringAssert.Contains("Poison has no effect", orientalSpice);

        string arquebus = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack109Handler.cs");
        StringAssert.Contains("PlayArquebusExplosionAnimation(attacker.transform)", arquebus);
        StringAssert.Contains("PlayArquebusShotAnimation(attacker.transform, defender.transform, true)", arquebus);
        StringAssert.Contains("PlayArquebusShotAnimation(attacker.transform, defender.transform, false)", arquebus);
        StringAssert.Contains("Arquebus exploded", arquebus);
        StringAssert.Contains("Bang! aaaand miss", arquebus);

        string pirateRaid = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack110Handler.cs");
        StringAssert.Contains("PlayPirateRaidAnimation(attacker.transform, defender.transform)", pirateRaid);
        StringAssert.Contains("PlayPirateRaidFailAnimation(attacker.transform, defender.transform)", pirateRaid);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", pirateRaid);
        StringAssert.Contains("Raiding didn't go well", pirateRaid);
        StringAssert.Contains("raids the enemy", pirateRaid);

        string axe = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack111Handler.cs");
        StringAssert.Contains("PlayAxeAnimation(attacker.transform, defender.transform)", axe);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", axe);
        StringAssert.Contains("attacks with axe", axe);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", axe);
        StringAssert.Contains("is wounded", axe);

        string jaguarWarriors = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack112Handler.cs");
        StringAssert.Contains("PlayJaguarWarriorsAnimation(attacker.transform, defender.transform)", jaguarWarriors);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", jaguarWarriors);
        StringAssert.Contains("sends Jaguar Warriors", jaguarWarriors);

        string atlatl = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack113Handler.cs");
        StringAssert.Contains("PlayAtlatlAnimation(attacker.transform, defender.transform, hit)", atlatl);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", atlatl);
        StringAssert.Contains("throws spear", atlatl);
        StringAssert.Contains("spear missed", atlatl);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", atlatl);

        string macuahuitl = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack114Handler.cs");
        StringAssert.Contains("PlayMacuahuitlAnimation(attacker.transform, defender.transform)", macuahuitl);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", macuahuitl);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", macuahuitl);
        StringAssert.Contains("PlayKnockoutAnimation(defender.transform)", macuahuitl);
        StringAssert.Contains("smashes with Macuahuitl", macuahuitl);

        string cubism = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack115Handler.cs");
        StringAssert.Contains("PlayCubismAnimation(attacker.transform)", cubism);
        StringAssert.Contains("PlayConfusionStartAnimation(defender.transform)", cubism);
        StringAssert.Contains("BattleValuePlayback.PlayHeal(", cubism);
        StringAssert.Contains("uses Cubism", cubism);
        StringAssert.Contains("picture confuses the enemy", cubism);
        StringAssert.Contains("likes this art", cubism);

        string cosaNostra = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack116Handler.cs");
        StringAssert.Contains("PlayCosaNostraAnimation(attacker.transform, defender.transform)", cosaNostra);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", cosaNostra);
        StringAssert.Contains("uses Cosa Nostra", cosaNostra);
        StringAssert.Contains("orders mafia to attack", cosaNostra);

        string actAFool = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack117Handler.cs");
        StringAssert.Contains("PlayActAnimation(attacker.transform)", actAFool);
        StringAssert.Contains("PlayAnimationTerrified(defender.transform)", actAFool);
        StringAssert.Contains("PlayAnimationImpressed(defender.transform)", actAFool);
        StringAssert.Contains("uses Act a fool", actAFool);
        StringAssert.Contains("behaves like insane", actAFool);
        StringAssert.Contains("is scared", actAFool);
        StringAssert.Contains("is impressed", actAFool);

        string football = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack118Handler.cs");
        StringAssert.Contains("PlayFootballAnimation(attacker.transform, defender.transform, hit)", football);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", football);
        StringAssert.Contains("PlayKnockoutAnimation(defender.transform)", football);
        StringAssert.Contains("uses Football", football);
        StringAssert.Contains("kicks football", football);
        StringAssert.Contains("kick missed", football);
        StringAssert.Contains("is KO", football);

        string bicycleKick = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack119Handler.cs");
        StringAssert.Contains("PlayBicycleKickAnimation(attacker.transform, defender.transform)", bicycleKick);
        StringAssert.Contains("PlayBicycleKickFailAnimation(attacker.transform)", bicycleKick);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", bicycleKick);
        StringAssert.Contains("uses Bicycle Kick", bicycleKick);
        StringAssert.Contains("Plesk! Big kick from", bicycleKick);
        StringAssert.Contains("faces gravity", bicycleKick);

        string worldChampion = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack120Handler.cs");
        StringAssert.Contains("PlayWorldChampionAnimation(attacker.transform)", worldChampion);
        StringAssert.Contains("uses World Champion", worldChampion);
        StringAssert.Contains("is world champion", worldChampion);

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

        string katana = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack83Handler.cs");
        StringAssert.Contains("PlayKatanaAnimation(attacker.transform, defender.transform)", katana);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", katana);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", katana);
        StringAssert.Contains("uses Katana", katana);
        StringAssert.Contains("slashes with katana", katana);

        string nodachi = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", "Attack84Handler.cs");
        StringAssert.Contains("PlayNodachiAnimation(attacker.transform, defender.transform)", nodachi);
        StringAssert.Contains("BattleValuePlayback.PlayDamage(", nodachi);
        StringAssert.Contains("PlayBleedStartAnimation(defender.transform)", nodachi);
        StringAssert.Contains("uses Nodachi", nodachi);
        StringAssert.Contains("slashes with Nodachi", nodachi);
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
        StringAssert.Contains("flintlockPistol", resultProcessor);
        StringAssert.Contains("PlayFlintlockPistolLoadingAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("PlayFlintlockPistolShotAnimation(", resultProcessor);
        StringAssert.Contains("case BattleStepType.SatelliteTick:", resultProcessor);
        StringAssert.Contains("PlaySatelliteAnimation(actor.transform)", resultProcessor);
        StringAssert.Contains("!string.IsNullOrEmpty(step.Note)", resultProcessor);
        StringAssert.Contains("ShowDialog(step.Note)", resultProcessor);

        string kard = ReadProjectFile("Assets", "Scripts", "Kard.cs");
        StringAssert.Contains("effectName == \"Blockade\"", kard);
        StringAssert.Contains("\"Game/Animations/continentalblocade\"", kard);
    }

    private static void AssertRegistryEntry(string source, int attackId)
    {
        StringAssert.Contains($"[{attackId}] = new AttackDefinition(", source);
        StringAssert.Contains($"Attack{attackId}Handler.Execute(", source);
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





































