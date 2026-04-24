using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BattleResultParserTests
{
    [Test]
    public void TryParse_ReturnsFalse_WhenRequiredAttackersAreMissing()
    {
        var invalid = new Dictionary<string, object>();

        bool ok = TryParseWithReflection(invalid, "any_card", out var parsed, out var error);

        Assert.IsFalse(ok);
        Assert.IsNull(parsed);
        Assert.IsFalse(string.IsNullOrWhiteSpace(error));
    }

    [Test]
    public void TryParse_AllBattleFixtures_ParityWithFixtureData()
    {
        string fixturesDir = ResolveFixturesDir();
        Assert.IsTrue(Directory.Exists(fixturesDir), $"Fixtures directory not found: {fixturesDir}");

        var files = Directory.GetFiles(fixturesDir, "*.json").OrderBy(Path.GetFileName).ToList();
        Assert.Greater(files.Count, 0, $"No fixture files in: {fixturesDir}");

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            var envelope = JsonUtility.FromJson<FixtureEnvelope>(json);
            Assert.IsNotNull(envelope, $"Envelope parse failed for fixture: {Path.GetFileName(file)}");

            if (envelope.battleResult == null)
            {
                continue;
            }

            var battleResult = ToBattleResultDict(envelope.battleResult);
            var first = ToAttackerDict(envelope.battleResult.firstAttacker);
            var second = ToAttackerDict(envelope.battleResult.secondAttacker);

            // Waiting/polling fixtures can carry partial battleResult without attacker payloads.
            if (first == null || second == null)
            {
                continue;
            }

            string firstCardId = GetString(first, "cardId");
            string secondCardId = GetString(second, "cardId");
            Assert.IsFalse(string.IsNullOrWhiteSpace(firstCardId), $"Missing firstAttacker.cardId in fixture: {Path.GetFileName(file)}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(secondCardId), $"Missing secondAttacker.cardId in fixture: {Path.GetFileName(file)}");

            ValidatePerspective(file, battleResult, firstCardId, first, second);
            ValidatePerspective(file, battleResult, secondCardId, second, first);
        }
    }

    private static void ValidatePerspective(
        string fixturePath,
        Dictionary<string, object> battleResult,
        string myCardId,
        Dictionary<string, object> expectedMine,
        Dictionary<string, object> expectedEnemy)
    {
        bool ok = TryParseWithReflection(battleResult, myCardId, out var parsed, out var error);
        Assert.IsTrue(ok, $"Parser failed for fixture={Path.GetFileName(fixturePath)}, myCardId={myCardId}, error={error}");
        Assert.IsNotNull(parsed, $"Parsed result is null for fixture={Path.GetFileName(fixturePath)}, myCardId={myCardId}");

        var myAttackData = ReadField<Dictionary<string, object>>(parsed, "MyAttackData");
        var enemyAttackData = ReadField<Dictionary<string, object>>(parsed, "EnemyAttackData");

        Assert.AreEqual(GetString(expectedMine, "cardId"), myCardId, $"Unexpected perspective setup in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetString(expectedMine, "cardId"), GetCardId(myAttackData), $"MyAttackData.cardId mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetString(expectedEnemy, "cardId"), GetCardId(enemyAttackData), $"EnemyAttackData.cardId mismatch in {Path.GetFileName(fixturePath)}");

        Assert.AreEqual(GetInt(expectedMine, "damageReceived", 0), ReadField<int>(parsed, "MyDamage"), $"MyDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "damageReceived", 0), ReadField<int>(parsed, "EnemyDamage"), $"EnemyDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedMine, "attackId", 1), ReadField<int>(parsed, "MyAttackId"), $"MyAttackId mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "attackId", 1), ReadField<int>(parsed, "EnemyAttackId"), $"EnemyAttackId mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedMine, "healAmount", 0), ReadField<int>(parsed, "MyHealAmount"), $"MyHealAmount mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "healAmount", 0), ReadField<int>(parsed, "EnemyHealAmount"), $"EnemyHealAmount mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetString(expectedMine, "attackResult"), ReadField<string>(parsed, "MyAttackResult") ?? string.Empty, $"MyAttackResult mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetString(expectedEnemy, "attackResult"), ReadField<string>(parsed, "EnemyAttackResult") ?? string.Empty, $"EnemyAttackResult mismatch in {Path.GetFileName(fixturePath)}");

        AssertStatChanges(ReadField<object>(parsed, "MyStatChanges"), expectedMine, $"MyStatChanges mismatch in {Path.GetFileName(fixturePath)}");
        AssertStatChanges(ReadField<object>(parsed, "EnemyStatChanges"), expectedEnemy, $"EnemyStatChanges mismatch in {Path.GetFileName(fixturePath)}");

        AssertEffect(ReadField<Dictionary<string, object>>(parsed, "MyEffectApplied"), ToDict(GetValue(expectedMine, "effectApplied")), $"MyEffectApplied mismatch in {Path.GetFileName(fixturePath)}");
        AssertEffect(ReadField<Dictionary<string, object>>(parsed, "EnemyEffectApplied"), ToDict(GetValue(expectedEnemy, "effectApplied")), $"EnemyEffectApplied mismatch in {Path.GetFileName(fixturePath)}");

        var expectedMyEffectsApplied = ToDictList(GetValue(expectedMine, "effectsApplied"));
        if (expectedMyEffectsApplied.Count == 0 && GetValue(expectedMine, "effectApplied") != null)
        {
            expectedMyEffectsApplied.Add(ToDict(GetValue(expectedMine, "effectApplied")));
        }

        var expectedEnemyEffectsApplied = ToDictList(GetValue(expectedEnemy, "effectsApplied"));
        if (expectedEnemyEffectsApplied.Count == 0 && GetValue(expectedEnemy, "effectApplied") != null)
        {
            expectedEnemyEffectsApplied.Add(ToDict(GetValue(expectedEnemy, "effectApplied")));
        }

        AssertEffects(ReadField<List<Dictionary<string, object>>>(parsed, "MyEffectsApplied"), expectedMyEffectsApplied, $"MyEffectsApplied mismatch in {Path.GetFileName(fixturePath)}");
        AssertEffects(ReadField<List<Dictionary<string, object>>>(parsed, "EnemyEffectsApplied"), expectedEnemyEffectsApplied, $"EnemyEffectsApplied mismatch in {Path.GetFileName(fixturePath)}");
        AssertEffects(ReadField<List<Dictionary<string, object>>>(parsed, "MyAttackerEffects"), ToDictList(GetValue(expectedMine, "attackerEffectsApplied")), $"MyAttackerEffects mismatch in {Path.GetFileName(fixturePath)}");
        AssertEffects(ReadField<List<Dictionary<string, object>>>(parsed, "EnemyAttackerEffects"), ToDictList(GetValue(expectedEnemy, "attackerEffectsApplied")), $"EnemyAttackerEffects mismatch in {Path.GetFileName(fixturePath)}");

        Assert.AreEqual(GetInt(expectedMine, "attackerSelfDamage", 0), ReadField<int>(parsed, "MyAttackerSelfDamage"), $"MyAttackerSelfDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "attackerSelfDamage", 0), ReadField<int>(parsed, "EnemyAttackerSelfDamage"), $"EnemyAttackerSelfDamage mismatch in {Path.GetFileName(fixturePath)}");

        Assert.AreEqual(GetBool(expectedMine, "blocked", false), ReadField<bool>(parsed, "MyAttackBlocked"), $"MyAttackBlocked mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedEnemy, "blocked", false), ReadField<bool>(parsed, "EnemyAttackBlocked"), $"EnemyAttackBlocked mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedMine, "wokeUp", false), ReadField<bool>(parsed, "MyWokeUp"), $"MyWokeUp mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedEnemy, "wokeUp", false), ReadField<bool>(parsed, "EnemyWokeUp"), $"EnemyWokeUp mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedMine, "recovered", false), ReadField<bool>(parsed, "MyRecovered"), $"MyRecovered mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedEnemy, "recovered", false), ReadField<bool>(parsed, "EnemyRecovered"), $"EnemyRecovered mismatch in {Path.GetFileName(fixturePath)}");

        Assert.AreEqual(GetInt(expectedMine, "selfDamage", 0), ReadField<int>(parsed, "MySelfDamage"), $"MySelfDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "selfDamage", 0), ReadField<int>(parsed, "EnemySelfDamage"), $"EnemySelfDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedMine, "bleedDamage", 0), ReadField<int>(parsed, "MyBleedDamage"), $"MyBleedDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "bleedDamage", 0), ReadField<int>(parsed, "EnemyBleedDamage"), $"EnemyBleedDamage mismatch in {Path.GetFileName(fixturePath)}");
        CollectionAssert.AreEqual(GetIntList(expectedMine, "bleedDamages"), ReadField<List<int>>(parsed, "MyBleedDamages"), $"MyBleedDamages mismatch in {Path.GetFileName(fixturePath)}");
        CollectionAssert.AreEqual(GetIntList(expectedEnemy, "bleedDamages"), ReadField<List<int>>(parsed, "EnemyBleedDamages"), $"EnemyBleedDamages mismatch in {Path.GetFileName(fixturePath)}");

        Assert.AreEqual(GetInt(expectedMine, "exposureDamage", 0), ReadField<int>(parsed, "MyExposureDamage"), $"MyExposureDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetInt(expectedEnemy, "exposureDamage", 0), ReadField<int>(parsed, "EnemyExposureDamage"), $"EnemyExposureDamage mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedMine, "exposureRemoved", false), ReadField<bool>(parsed, "MyExposureRemoved"), $"MyExposureRemoved mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetBool(expectedEnemy, "exposureRemoved", false), ReadField<bool>(parsed, "EnemyExposureRemoved"), $"EnemyExposureRemoved mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetNullableInt(expectedMine, "blockedBy"), ReadField<int?>(parsed, "MyBlockedBy"), $"MyBlockedBy mismatch in {Path.GetFileName(fixturePath)}");
        Assert.AreEqual(GetNullableInt(expectedEnemy, "blockedBy"), ReadField<int?>(parsed, "EnemyBlockedBy"), $"EnemyBlockedBy mismatch in {Path.GetFileName(fixturePath)}");
    }

    private static string ResolveFixturesDir()
    {
        string env = Environment.GetEnvironmentVariable("MEGA_TRESK_FIXTURES_DIR");
        if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
        {
            return env;
        }

        var candidates = new List<string>
        {
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "mega-tresk-server", "docs", "multiplayer", "fixtures")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "mega-tresk-server", "docs", "multiplayer", "fixtures")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "docs", "multiplayer", "fixtures"))
        };

        foreach (string candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }

    private static Dictionary<string, object> ToBattleResultDict(FixtureBattleResult result)
    {
        return new Dictionary<string, object>
        {
            { "firstAttacker", ToAttackerDict(result.firstAttacker) },
            { "secondAttacker", ToAttackerDict(result.secondAttacker) },
            { "cardDied", result.cardDied },
            { "winnerCardId", result.winnerCardId },
            { "loserCardId", result.loserCardId }
        };
    }

    private static Dictionary<string, object> ToAttackerDict(FixtureAttacker attacker)
    {
        if (attacker == null)
        {
            return null;
        }

        return new Dictionary<string, object>
        {
            { "cardId", attacker.cardId },
            { "attackId", attacker.attackId },
            { "damageReceived", attacker.damageReceived },
            { "damageDealt", attacker.damageDealt },
            { "healAmount", attacker.healAmount },
            { "blocked", attacker.blocked },
            { "blockedBy", attacker.blockedBy },
            { "remainingDuration", attacker.remainingDuration },
            { "wokeUp", attacker.wokeUp },
            { "recovered", attacker.recovered },
            { "selfDamage", attacker.selfDamage },
            { "bleedDamage", attacker.bleedDamage },
            { "bleedDamages", attacker.bleedDamages == null ? new List<object>() : attacker.bleedDamages.Cast<object>().ToList() },
            { "exposureDamage", attacker.exposureDamage },
            { "exposureRemoved", attacker.exposureRemoved },
            { "effectsRemoved", attacker.effectsRemoved == null ? new List<object>() : attacker.effectsRemoved.Cast<object>().ToList() },
            { "effectApplied", ToEffectDict(attacker.effectApplied) },
            { "effectsApplied", ToEffectDictList(attacker.effectsApplied) },
            { "attackerSelfDamage", attacker.attackerSelfDamage },
            { "attackerEffectsApplied", ToEffectDictList(attacker.attackerEffectsApplied) },
            { "attackResult", attacker.attackResult },
            { "statChanges", ToStatChangeDictList(attacker.statChanges) },
            { "effects", ToEffectDictList(attacker.effects) },
            { "isDead", attacker.isDead }
        };
    }

    private static Dictionary<string, object> ToEffectDict(FixtureEffect effect)
    {
        if (effect == null)
        {
            return null;
        }

        return new Dictionary<string, object>
        {
            { "type", effect.type },
            { "duration", effect.duration },
            { "source", effect.source },
            { "intensity", effect.intensity }
        };
    }

    private static List<object> ToEffectDictList(List<FixtureEffect> effects)
    {
        if (effects == null)
        {
            return new List<object>();
        }

        return effects.Select(e => (object)ToEffectDict(e)).ToList();
    }

    private static List<object> ToStatChangeDictList(List<FixtureStatChange> statChanges)
    {
        if (statChanges == null)
        {
            return new List<object>();
        }

        return statChanges.Select(s => (object)new Dictionary<string, object>
        {
            { "type", s.type },
            { "statName", s.statName },
            { "amount", s.amount },
            { "actorCardId", s.actorCardId },
            { "targetCardId", s.targetCardId },
            { "source", s.source }
        }).ToList();
    }

    private static void AssertStatChanges(object actualStats, Dictionary<string, object> expected, string messagePrefix)
    {
        var expectedStats = BuildExpectedStatChanges(expected);

        Assert.AreEqual(expectedStats["attackerAttack"], ReadIntField(actualStats, "attackerAttack"), $"{messagePrefix}: attackerAttack");
        Assert.AreEqual(expectedStats["attackerStrength"], ReadIntField(actualStats, "attackerStrength"), $"{messagePrefix}: attackerStrength");
        Assert.AreEqual(expectedStats["attackerDefense"], ReadIntField(actualStats, "attackerDefense"), $"{messagePrefix}: attackerDefense");
        Assert.AreEqual(expectedStats["attackerKnowledge"], ReadIntField(actualStats, "attackerKnowledge"), $"{messagePrefix}: attackerKnowledge");
        Assert.AreEqual(expectedStats["attackerSpeed"], ReadIntField(actualStats, "attackerSpeed"), $"{messagePrefix}: attackerSpeed");
        Assert.AreEqual(expectedStats["attackerCharisma"], ReadIntField(actualStats, "attackerCharisma"), $"{messagePrefix}: attackerCharisma");
        Assert.AreEqual(expectedStats["defenderAttack"], ReadIntField(actualStats, "defenderAttack"), $"{messagePrefix}: defenderAttack");
        Assert.AreEqual(expectedStats["defenderStrength"], ReadIntField(actualStats, "defenderStrength"), $"{messagePrefix}: defenderStrength");
        Assert.AreEqual(expectedStats["defenderDefense"], ReadIntField(actualStats, "defenderDefense"), $"{messagePrefix}: defenderDefense");
        Assert.AreEqual(expectedStats["defenderKnowledge"], ReadIntField(actualStats, "defenderKnowledge"), $"{messagePrefix}: defenderKnowledge");
        Assert.AreEqual(expectedStats["defenderSpeed"], ReadIntField(actualStats, "defenderSpeed"), $"{messagePrefix}: defenderSpeed");
        Assert.AreEqual(expectedStats["defenderCharisma"], ReadIntField(actualStats, "defenderCharisma"), $"{messagePrefix}: defenderCharisma");
    }

    private static Dictionary<string, int> BuildExpectedStatChanges(Dictionary<string, object> expected)
    {
        var result = new Dictionary<string, int>
        {
            { "attackerAttack", 0 },
            { "attackerStrength", 0 },
            { "attackerDefense", 0 },
            { "attackerKnowledge", 0 },
            { "attackerSpeed", 0 },
            { "attackerCharisma", 0 },
            { "defenderAttack", 0 },
            { "defenderStrength", 0 },
            { "defenderDefense", 0 },
            { "defenderKnowledge", 0 },
            { "defenderSpeed", 0 },
            { "defenderCharisma", 0 }
        };

        string attackerCardId = GetString(expected, "cardId");
        foreach (var change in ToDictList(GetValue(expected, "statChanges")))
        {
            string prefix = GetString(change, "targetCardId") == attackerCardId ? "attacker" : "defender";
            string key = prefix + StatNameToFieldSuffix(GetString(change, "statName"));
            if (result.ContainsKey(key))
            {
                result[key] += GetInt(change, "amount");
            }
        }

        return result;
    }

    private static string StatNameToFieldSuffix(string statName)
    {
        switch (statName)
        {
            case "ATT": return "Attack";
            case "STR": return "Strength";
            case "DEF": return "Defense";
            case "KNO": return "Knowledge";
            case "SPD": return "Speed";
            case "CHA": return "Charisma";
            default: return string.Empty;
        }
    }

    private static void AssertEffect(Dictionary<string, object> actual, Dictionary<string, object> expected, string messagePrefix)
    {
        if (expected == null)
        {
            Assert.IsNull(actual, messagePrefix);
            return;
        }

        Assert.IsNotNull(actual, messagePrefix);
        Assert.AreEqual(GetInt(expected, "type", 0), GetInt(actual, "type"), $"{messagePrefix}: type");
        Assert.AreEqual(GetInt(expected, "duration", 0), GetInt(actual, "duration"), $"{messagePrefix}: duration");
        Assert.AreEqual(GetString(expected, "source"), GetString(actual, "source"), $"{messagePrefix}: source");
    }

    private static void AssertEffects(List<Dictionary<string, object>> actual, List<Dictionary<string, object>> expected, string messagePrefix)
    {
        Assert.AreEqual(expected.Count, actual.Count, $"{messagePrefix}: count");
        for (int i = 0; i < expected.Count; i++)
        {
            AssertEffect(actual[i], expected[i], $"{messagePrefix}[{i}]");
        }
    }

    private static object GetValue(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.ContainsKey(key))
        {
            return null;
        }

        return dict[key];
    }

    private static Dictionary<string, object> ToDict(object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is Dictionary<string, object> dict)
        {
            return dict;
        }

        return null;
    }

    private static List<Dictionary<string, object>> ToDictList(object value)
    {
        var result = new List<Dictionary<string, object>>();
        if (value == null)
        {
            return result;
        }

        if (value is List<object> list)
        {
            foreach (var item in list)
            {
                var dict = ToDict(item);
                if (dict != null)
                {
                    result.Add(dict);
                }
            }

            return result;
        }

        if (value is List<Dictionary<string, object>> dictList)
        {
            result.AddRange(dictList);
        }

        return result;
    }

    private static int GetInt(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
        {
            return 0;
        }

        return int.Parse(dict[key].ToString());
    }

    private static int GetInt(Dictionary<string, object> dict, string key, int defaultValue)
    {
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
        {
            return defaultValue;
        }

        return int.Parse(dict[key].ToString());
    }

    private static bool GetBool(Dictionary<string, object> dict, string key, bool defaultValue)
    {
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
        {
            return defaultValue;
        }

        return bool.Parse(dict[key].ToString());
    }

    private static int? GetNullableInt(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
        {
            return null;
        }

        return int.Parse(dict[key].ToString());
    }

    private static List<int> GetIntList(Dictionary<string, object> dict, string key)
    {
        var result = new List<int>();
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
        {
            return result;
        }

        if (dict[key] is List<object> list)
        {
            foreach (var item in list)
            {
                result.Add(int.Parse(item.ToString()));
            }
        }

        return result;
    }

    private static string GetString(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
        {
            return string.Empty;
        }

        return dict[key].ToString();
    }

    private static string GetCardId(Dictionary<string, object> attackData)
    {
        if (attackData == null || !attackData.ContainsKey("cardId") || attackData["cardId"] == null)
        {
            return string.Empty;
        }

        return attackData["cardId"].ToString();
    }

    private static int ReadIntField(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName);
        Assert.IsNotNull(field, $"Missing field '{fieldName}' on {obj.GetType().Name}");
        return (int)field.GetValue(obj);
    }

    private static T ReadField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName);
        Assert.IsNotNull(field, $"Missing field '{fieldName}' on {obj.GetType().Name}");
        return (T)field.GetValue(obj);
    }

    private static bool TryParseWithReflection(
        Dictionary<string, object> battleResult,
        string myCardId,
        out object parsed,
        out string error)
    {
        parsed = null;
        error = null;

        var parserType = Type.GetType("BattleResultParser, Assembly-CSharp");
        Assert.IsNotNull(parserType, "Type BattleResultParser was not found in Assembly-CSharp.");

        var method = parserType.GetMethod(
            "TryParse",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Dictionary<string, object>), typeof(string), parserType.Assembly.GetType("ParsedBattleResult").MakeByRefType(), typeof(string).MakeByRefType() },
            null);
        Assert.IsNotNull(method, "Method BattleResultParser.TryParse was not found.");

        object[] args = { battleResult, myCardId, null, null };
        bool ok = (bool)method.Invoke(null, args);
        parsed = args[2];
        error = args[3] as string;
        return ok;
    }

    [Serializable]
    private class FixtureEnvelope
    {
        public bool success;
        public bool bothPlayersReady;
        public int playersReadyCount;
        public string message;
        public FixtureBattleResult battleResult;
    }

    [Serializable]
    private class FixtureBattleResult
    {
        public FixtureAttacker firstAttacker;
        public FixtureAttacker secondAttacker;
        public bool cardDied;
        public string winnerCardId;
        public string loserCardId;
    }

    [Serializable]
    private class FixtureAttacker
    {
        public string cardId;
        public int attackId;
        public int damageReceived;
        public int damageDealt;
        public int healAmount;
        public bool blocked;
        public int? blockedBy;
        public int remainingDuration;
        public bool wokeUp;
        public bool recovered;
        public int selfDamage;
        public int bleedDamage;
        public List<int> bleedDamages;
        public int exposureDamage;
        public bool exposureRemoved;
        public List<int> effectsRemoved;
        public FixtureEffect effectApplied;
        public List<FixtureEffect> effectsApplied;
        public int attackerSelfDamage;
        public List<FixtureEffect> attackerEffectsApplied;
        public string attackResult;
        public List<FixtureStatChange> statChanges;
        public List<FixtureEffect> effects;
        public bool isDead;
    }

    [Serializable]
    private class FixtureStatChange
    {
        public string type;
        public string statName;
        public int amount;
        public string actorCardId;
        public string targetCardId;
        public string source;
    }

    [Serializable]
    private class FixtureEffect
    {
        public int type;
        public int duration;
        public string source;
        public int intensity;
    }
}
