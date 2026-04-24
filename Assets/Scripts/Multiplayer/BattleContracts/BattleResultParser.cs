using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using PlayFab;

public class ParsedBattleResult
{
    public string FirstAttackerCardId;
    public string SecondAttackerCardId;
    public bool IAmFirstAttacker;

    public Dictionary<string, object> MyAttackData;
    public Dictionary<string, object> EnemyAttackData;

    public int MyDamage;
    public int EnemyDamage;
    public int MyAttackId;
    public int EnemyAttackId;
    public int MyHealAmount;
    public int EnemyHealAmount;

    public string MyAttackResult;
    public string EnemyAttackResult;

    public AttackStatChanges MyStatChanges;
    public AttackStatChanges EnemyStatChanges;

    public Dictionary<string, object> MyEffectApplied;
    public Dictionary<string, object> EnemyEffectApplied;

    public List<Dictionary<string, object>> MyEffectsApplied;
    public List<Dictionary<string, object>> EnemyEffectsApplied;
    public List<Dictionary<string, object>> MyAttackerEffects;
    public List<Dictionary<string, object>> EnemyAttackerEffects;

    public int MyAttackerSelfDamage;
    public int EnemyAttackerSelfDamage;

    public bool MyAttackBlocked;
    public bool EnemyAttackBlocked;
    public bool MyWokeUp;
    public bool EnemyWokeUp;
    public bool MyRecovered;
    public bool EnemyRecovered;

    public int MySelfDamage;
    public int EnemySelfDamage;

    public int MyBleedDamage;
    public int EnemyBleedDamage;
    public List<int> MyBleedDamages;
    public List<int> EnemyBleedDamages;

    public int MyExposureDamage;
    public int EnemyExposureDamage;
    public bool MyExposureRemoved;
    public bool EnemyExposureRemoved;

    public int? MyBlockedBy;
    public int? EnemyBlockedBy;
}

public static class BattleResultParser
{
    public static bool TryParse(Dictionary<string, object> battleResult, string myCardId, out ParsedBattleResult parsed, out string error)
    {
        parsed = null;
        error = null;

        if (battleResult == null)
        {
            error = "battleResult is null";
            return false;
        }

        if (!battleResult.ContainsKey("firstAttacker") || !battleResult.ContainsKey("secondAttacker"))
        {
            error = "Missing 'firstAttacker' or 'secondAttacker' in battleResult";
            return false;
        }

        var firstAttackerData = ToDictionary(battleResult["firstAttacker"]);
        var secondAttackerData = ToDictionary(battleResult["secondAttacker"]);

        if (firstAttackerData == null || secondAttackerData == null)
        {
            error = "Could not parse attacker objects";
            return false;
        }

        if (!firstAttackerData.TryGetValue("cardId", out var firstCardIdObj) || firstCardIdObj == null)
        {
            error = "Missing firstAttacker.cardId";
            return false;
        }

        if (!secondAttackerData.TryGetValue("cardId", out var secondCardIdObj) || secondCardIdObj == null)
        {
            error = "Missing secondAttacker.cardId";
            return false;
        }

        string firstAttackerCardId = firstCardIdObj.ToString();
        string secondAttackerCardId = secondCardIdObj.ToString();

        bool iAmFirstAttacker = (firstAttackerCardId == myCardId);
        var myAttackData = iAmFirstAttacker ? firstAttackerData : secondAttackerData;
        var enemyAttackData = iAmFirstAttacker ? secondAttackerData : firstAttackerData;

        var myEffectApplied = GetEffectDict(myAttackData, "effectApplied");
        var enemyEffectApplied = GetEffectDict(enemyAttackData, "effectApplied");

        var myEffectsApplied = GetEffectsArray(myAttackData, "effectsApplied");
        var enemyEffectsApplied = GetEffectsArray(enemyAttackData, "effectsApplied");

        if (myEffectsApplied.Count == 0 && myEffectApplied != null)
        {
            myEffectsApplied.Add(myEffectApplied);
        }

        if (enemyEffectsApplied.Count == 0 && enemyEffectApplied != null)
        {
            enemyEffectsApplied.Add(enemyEffectApplied);
        }

        myEffectsApplied = DeduplicateEffects(myEffectsApplied);
        enemyEffectsApplied = DeduplicateEffects(enemyEffectsApplied);

        parsed = new ParsedBattleResult
        {
            FirstAttackerCardId = firstAttackerCardId,
            SecondAttackerCardId = secondAttackerCardId,
            IAmFirstAttacker = iAmFirstAttacker,

            MyAttackData = myAttackData,
            EnemyAttackData = enemyAttackData,

            MyDamage = GetInt(myAttackData, "damageReceived", 0),
            EnemyDamage = GetInt(enemyAttackData, "damageReceived", 0),
            MyAttackId = GetInt(myAttackData, "attackId", 1),
            EnemyAttackId = GetInt(enemyAttackData, "attackId", 1),
            MyHealAmount = GetInt(myAttackData, "healAmount", 0),
            EnemyHealAmount = GetInt(enemyAttackData, "healAmount", 0),

            MyAttackResult = GetStringOrNull(myAttackData, "attackResult"),
            EnemyAttackResult = GetStringOrNull(enemyAttackData, "attackResult"),

            MyStatChanges = BuildStatChanges(myAttackData),
            EnemyStatChanges = BuildStatChanges(enemyAttackData),

            MyEffectApplied = myEffectApplied,
            EnemyEffectApplied = enemyEffectApplied,

            MyEffectsApplied = myEffectsApplied,
            EnemyEffectsApplied = enemyEffectsApplied,
            MyAttackerEffects = GetEffectsArray(myAttackData, "attackerEffectsApplied"),
            EnemyAttackerEffects = GetEffectsArray(enemyAttackData, "attackerEffectsApplied"),

            MyAttackerSelfDamage = GetInt(myAttackData, "attackerSelfDamage", 0),
            EnemyAttackerSelfDamage = GetInt(enemyAttackData, "attackerSelfDamage", 0),

            MyAttackBlocked = GetBool(myAttackData, "blocked", false),
            EnemyAttackBlocked = GetBool(enemyAttackData, "blocked", false),
            MyWokeUp = GetBool(myAttackData, "wokeUp", false),
            EnemyWokeUp = GetBool(enemyAttackData, "wokeUp", false),
            MyRecovered = GetBool(myAttackData, "recovered", false),
            EnemyRecovered = GetBool(enemyAttackData, "recovered", false),

            MySelfDamage = GetInt(myAttackData, "selfDamage", 0),
            EnemySelfDamage = GetInt(enemyAttackData, "selfDamage", 0),

            MyBleedDamage = GetInt(myAttackData, "bleedDamage", 0),
            EnemyBleedDamage = GetInt(enemyAttackData, "bleedDamage", 0),
            MyBleedDamages = GetIntArray(myAttackData, "bleedDamages"),
            EnemyBleedDamages = GetIntArray(enemyAttackData, "bleedDamages"),

            MyExposureDamage = GetInt(myAttackData, "exposureDamage", 0),
            EnemyExposureDamage = GetInt(enemyAttackData, "exposureDamage", 0),
            MyExposureRemoved = GetBool(myAttackData, "exposureRemoved", false),
            EnemyExposureRemoved = GetBool(enemyAttackData, "exposureRemoved", false),

            MyBlockedBy = GetNullableInt(myAttackData, "blockedBy"),
            EnemyBlockedBy = GetNullableInt(enemyAttackData, "blockedBy")
        };

        return true;
    }

    private static AttackStatChanges BuildStatChanges(Dictionary<string, object> data)
    {
        var result = AttackStatChanges.Zero;
        string attackerCardId = GetStringOrNull(data, "cardId");
        var statChanges = GetObjectArray(data, "statChanges");

        foreach (var item in statChanges)
        {
            var change = ToDictionary(item);
            if (change == null)
            {
                continue;
            }

            string targetCardId = GetStringOrNull(change, "targetCardId");
            string statName = GetStringOrNull(change, "statName");
            int amount = GetInt(change, "amount", 0);

            if (string.IsNullOrEmpty(statName) || amount == 0)
            {
                continue;
            }

            bool targetsAttacker = targetCardId == attackerCardId;
            ApplyStatChange(ref result, targetsAttacker, statName, amount);
        }

        return result;
    }

    private static void ApplyStatChange(ref AttackStatChanges result, bool targetsAttacker, string statName, int amount)
    {
        switch (statName)
        {
            case "ATT":
                if (targetsAttacker) result.attackerAttack += amount;
                else result.defenderAttack += amount;
                break;
            case "STR":
                if (targetsAttacker) result.attackerStrength += amount;
                else result.defenderStrength += amount;
                break;
            case "DEF":
                if (targetsAttacker) result.attackerDefense += amount;
                else result.defenderDefense += amount;
                break;
            case "KNO":
                if (targetsAttacker) result.attackerKnowledge += amount;
                else result.defenderKnowledge += amount;
                break;
            case "SPD":
                if (targetsAttacker) result.attackerSpeed += amount;
                else result.defenderSpeed += amount;
                break;
            case "CHA":
                if (targetsAttacker) result.attackerCharisma += amount;
                else result.defenderCharisma += amount;
                break;
        }
    }

    private static Dictionary<string, object> GetEffectDict(Dictionary<string, object> data, string key)
    {
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return null;
        }

        return ToDictionary(data[key]);
    }

    private static List<Dictionary<string, object>> GetEffectsArray(Dictionary<string, object> data, string key)
    {
        var list = new List<Dictionary<string, object>>();
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return list;
        }

        var source = ToObjectList(data[key]);
        if (source == null)
        {
            return list;
        }

        foreach (var item in source)
        {
            var effectDict = ToDictionary(item);
            if (effectDict != null)
            {
                list.Add(effectDict);
            }
        }

        return list;
    }

    private static List<object> ToObjectList(object value)
    {
        if (value is List<object> list)
        {
            return list;
        }

        if (value is JArray array)
        {
            return array.ToObject<List<object>>();
        }

        if (value is object[] objectArray)
        {
            return objectArray.ToList();
        }

        return null;
    }

    private static Dictionary<string, object> ToDictionary(object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is Dictionary<string, object> dict)
        {
            return dict;
        }

        if (value is JObject jObj)
        {
            return jObj.ToObject<Dictionary<string, object>>();
        }

        var json = value.ToString();
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
            .DeserializeObject<Dictionary<string, object>>(json);
    }

    private static int GetInt(Dictionary<string, object> data, string key, int defaultValue)
    {
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return defaultValue;
        }

        return int.Parse(data[key].ToString());
    }

    private static int? GetNullableInt(Dictionary<string, object> data, string key)
    {
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return null;
        }

        return int.Parse(data[key].ToString());
    }

    private static bool GetBool(Dictionary<string, object> data, string key, bool defaultValue)
    {
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return defaultValue;
        }

        return bool.Parse(data[key].ToString());
    }

    private static string GetStringOrNull(Dictionary<string, object> data, string key)
    {
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return null;
        }

        return data[key].ToString();
    }

    private static List<object> GetObjectArray(Dictionary<string, object> data, string key)
    {
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return new List<object>();
        }

        return ToObjectList(data[key]) ?? new List<object>();
    }

    private static List<int> GetIntArray(Dictionary<string, object> data, string key)
    {
        var result = new List<int>();
        if (!data.ContainsKey(key) || data[key] == null)
        {
            return result;
        }

        var values = ToObjectList(data[key]);
        if (values == null)
        {
            return result;
        }

        foreach (var value in values)
        {
            result.Add(int.Parse(value.ToString()));
        }

        return result;
    }

    private static List<Dictionary<string, object>> DeduplicateEffects(List<Dictionary<string, object>> effects)
    {
        var unique = new List<Dictionary<string, object>>();
        var seen = new HashSet<string>();
        if (effects == null)
        {
            return unique;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            int effectType = GetInt(effect, "type", 0);
            int duration = GetInt(effect, "duration", 0);
            string source = GetStringOrNull(effect, "source") ?? string.Empty;
            string key = $"{effectType}:{duration}:{source}";

            if (seen.Add(key))
            {
                unique.Add(effect);
            }
        }

        return unique;
    }
}
