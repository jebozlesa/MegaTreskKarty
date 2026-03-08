using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PlayFab;

public static class BattleTimelineBuilder
{
    public static bool TryBuild(Dictionary<string, object> battleResult, out List<BattleStep> steps, out string error)
    {
        steps = new List<BattleStep>();
        error = null;

        if (battleResult == null)
        {
            error = "battleResult is null";
            return false;
        }

        if (TryBuildFromTimelineV2(battleResult, out var timelineV2Steps, out var timelineV2Error))
        {
            steps = timelineV2Steps;
            return true;
        }

        var first = ToDict(GetValue(battleResult, "firstAttacker"));
        var second = ToDict(GetValue(battleResult, "secondAttacker"));
        if (first == null || second == null)
        {
            if (!string.IsNullOrEmpty(timelineV2Error))
            {
                error = $"timelineV2 invalid: {timelineV2Error}; legacy fallback failed: Missing firstAttacker or secondAttacker";
                return false;
            }
            error = "Missing firstAttacker or secondAttacker";
            return false;
        }

        string firstCardId = GetString(first, "cardId");
        string secondCardId = GetString(second, "cardId");
        if (string.IsNullOrWhiteSpace(firstCardId) || string.IsNullOrWhiteSpace(secondCardId))
        {
            error = "Missing attacker cardId";
            return false;
        }

        var deadCards = new HashSet<string>();

        BuildForAttacker(first, firstCardId, secondCardId, steps, deadCards);
        BuildForAttacker(second, secondCardId, firstCardId, steps, deadCards);

        AppendDeathIfNeeded(first, firstCardId, steps, deadCards);
        AppendDeathIfNeeded(second, secondCardId, steps, deadCards);

        return true;
    }

    private static bool TryBuildFromTimelineV2(
        Dictionary<string, object> battleResult,
        out List<BattleStep> steps,
        out string error)
    {
        steps = new List<BattleStep>();
        error = null;

        var timelineV2 = ToDict(GetValue(battleResult, "timelineV2"));
        if (timelineV2 == null)
        {
            return false;
        }

        var stepsValue = GetValue(timelineV2, "steps");
        if (stepsValue == null)
        {
            error = "timelineV2.steps missing";
            return false;
        }

        IEnumerable<object> rawSteps = null;
        if (stepsValue is List<object> objList)
        {
            rawSteps = objList;
        }
        else if (stepsValue is JArray jArray)
        {
            rawSteps = jArray;
        }

        if (rawSteps == null)
        {
            error = "timelineV2.steps is not array";
            return false;
        }

        foreach (var rawStep in rawSteps)
        {
            var dict = ToDict(rawStep);
            if (dict == null)
            {
                error = "timelineV2 step is not object";
                return false;
            }

            if (!TryParseTimelineV2Step(dict, out var step, out var stepError))
            {
                error = stepError;
                return false;
            }

            steps.Add(step);
        }

        return true;
    }

    private static bool TryParseTimelineV2Step(
        Dictionary<string, object> data,
        out BattleStep step,
        out string error)
    {
        step = new BattleStep();
        error = null;

        string typeRaw = GetStringAny(data, "type", "Type");
        if (!TryParseStepType(typeRaw, out var stepType))
        {
            error = $"Unknown step type: {typeRaw}";
            return false;
        }

        step.Type = stepType;
        step.ActorCardId = GetStringAny(data, "actorCardId", "ActorCardId");
        step.TargetCardId = GetStringAny(data, "targetCardId", "TargetCardId");
        step.AttackId = GetIntAny(data, 0, "attackId", "AttackId");
        step.AttackResult = GetStringAny(data, "attackResult", "AttackResult");
        step.Amount = GetIntAny(data, 0, "amount", "Amount");
        step.EffectType = GetIntAny(data, 0, "effectType", "EffectType");
        step.Duration = GetIntAny(data, 0, "duration", "Duration");
        step.Blocked = GetBoolAny(data, false, "blocked", "Blocked");
        step.BlockedBy = GetNullableIntAny(data, "blockedBy", "BlockedBy");
        step.Skipped = GetBoolAny(data, false, "skipped", "Skipped");
        step.Source = GetStringAny(data, "source", "Source");
        step.Note = GetStringAny(data, "note", "Note");
        step.EffectsApplied = GetEffectListAny(data, "effectsApplied", "EffectsApplied");
        step.AttackerEffectsApplied = GetEffectListAny(data, "attackerEffectsApplied", "AttackerEffectsApplied");

        return true;
    }

    private static bool TryParseStepType(string value, out BattleStepType type)
    {
        if (Enum.TryParse(value, true, out type))
        {
            return true;
        }

        if (int.TryParse(value, out var numeric) && Enum.IsDefined(typeof(BattleStepType), numeric))
        {
            type = (BattleStepType)numeric;
            return true;
        }

        type = default;
        return false;
    }

    private static void BuildForAttacker(
        Dictionary<string, object> attacker,
        string attackerCardId,
        string defenderCardId,
        List<BattleStep> steps,
        HashSet<string> deadCards)
    {
        int bleedDamage = GetInt(attacker, "bleedDamage", 0);
        if (bleedDamage > 0)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.BleedTick,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = bleedDamage,
                Source = "bleed"
            });

            steps.Add(new BattleStep
            {
                Type = BattleStepType.Damage,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = bleedDamage,
                Source = "bleed"
            });
        }

        int exposureDamage = GetInt(attacker, "exposureDamage", 0);
        if (exposureDamage > 0)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.ExposureTick,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = exposureDamage,
                Source = "exposure"
            });

            steps.Add(new BattleStep
            {
                Type = BattleStepType.Damage,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = exposureDamage,
                Source = "exposure"
            });
        }

        if (GetBool(attacker, "exposureRemoved", false))
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.ExposureRemoved,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Source = "exposure"
            });
        }

        if (GetBool(attacker, "recovered", false))
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.Recovery,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Source = "asceticism"
            });
        }

        if (GetBool(attacker, "wokeUp", false))
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.WakeUp,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Source = "sleep"
            });
        }

        bool isDead = GetBool(attacker, "isDead", false);
        int damageDealt = GetInt(attacker, "damageDealt", 0);
        bool blocked = GetBool(attacker, "blocked", false);
        bool deadBeforeAttack = isDead && !blocked && damageDealt <= 0;

        if (deadBeforeAttack && !deadCards.Contains(attackerCardId))
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.Death,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Source = "state"
            });
            deadCards.Add(attackerCardId);
        }

        var effectsApplied = GetEffectList(attacker, "effectsApplied");
        var singleEffect = ToDict(GetValue(attacker, "effectApplied"));
        if (singleEffect != null)
        {
            effectsApplied.Add(singleEffect);
        }
        effectsApplied = DeduplicateEffects(effectsApplied);
        var attackerEffectsApplied = DeduplicateEffects(GetEffectList(attacker, "attackerEffectsApplied"));

        steps.Add(new BattleStep
        {
            Type = BattleStepType.Attack,
            ActorCardId = attackerCardId,
            TargetCardId = defenderCardId,
            AttackId = GetInt(attacker, "attackId", 1),
            AttackResult = GetString(attacker, "attackResult"),
            EffectsApplied = effectsApplied.Count > 0 ? effectsApplied : null,
            AttackerEffectsApplied = attackerEffectsApplied.Count > 0 ? attackerEffectsApplied : null,
            Blocked = blocked,
            BlockedBy = GetNullableInt(attacker, "blockedBy"),
            Skipped = deadBeforeAttack,
            Note = deadBeforeAttack ? "skipped_dead" : null
        });

        if (blocked)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.Blocked,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Blocked = true,
                BlockedBy = GetNullableInt(attacker, "blockedBy"),
                EffectType = GetNullableInt(attacker, "blockedBy") ?? 0,
                Source = "effect"
            });
        }

        if (damageDealt > 0)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.Damage,
                ActorCardId = attackerCardId,
                TargetCardId = defenderCardId,
                Amount = damageDealt,
                Source = "attack"
            });
        }

        int healAmount = GetInt(attacker, "healAmount", 0);
        if (healAmount > 0)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.Heal,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = healAmount,
                Source = "attack"
            });
        }

        if (!deadBeforeAttack)
        {
            AppendAppliedEffects(effectsApplied, attackerCardId, defenderCardId, steps);
            AppendAppliedEffects(attackerEffectsApplied, attackerCardId, attackerCardId, steps);
        }

        int selfDamage = GetInt(attacker, "selfDamage", 0);
        if (selfDamage > 0)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.SelfDamage,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = selfDamage,
                Source = "effect"
            });
        }

        int attackerSelfDamage = GetInt(attacker, "attackerSelfDamage", 0);
        if (attackerSelfDamage > 0)
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.SelfDamage,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Amount = attackerSelfDamage,
                Source = "attack"
            });
        }

        if (isDead && !deadCards.Contains(attackerCardId))
        {
            steps.Add(new BattleStep
            {
                Type = BattleStepType.Death,
                ActorCardId = attackerCardId,
                TargetCardId = attackerCardId,
                Source = "state"
            });
            deadCards.Add(attackerCardId);
        }
    }

    private static void AppendDeathIfNeeded(
        Dictionary<string, object> attacker,
        string cardId,
        List<BattleStep> steps,
        HashSet<string> deadCards)
    {
        if (!GetBool(attacker, "isDead", false) || deadCards.Contains(cardId))
        {
            return;
        }

        steps.Add(new BattleStep
        {
            Type = BattleStepType.Death,
            ActorCardId = cardId,
            TargetCardId = cardId,
            Source = "state"
        });
        deadCards.Add(cardId);
    }

    private static void AppendAppliedEffects(
        List<Dictionary<string, object>> effects,
        string attackerCardId,
        string defenderCardId,
        List<BattleStep> steps)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i];
            int effectType = GetInt(effect, "type", 0);
            int duration = GetInt(effect, "duration", 0);
            string source = GetString(effect, "source");

            steps.Add(new BattleStep
            {
                Type = BattleStepType.EffectApplied,
                ActorCardId = attackerCardId,
                TargetCardId = defenderCardId,
                EffectType = effectType,
                Duration = duration,
                Source = string.IsNullOrWhiteSpace(source) ? "attack" : source
            });
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

    private static object GetValueAny(Dictionary<string, object> dict, params string[] keys)
    {
        if (dict == null || keys == null)
        {
            return null;
        }

        for (int i = 0; i < keys.Length; i++)
        {
            if (dict.ContainsKey(keys[i]))
            {
                return dict[keys[i]];
            }
        }

        return null;
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

        if (value is JObject token)
        {
            return token.ToObject<Dictionary<string, object>>();
        }

        return PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
            .DeserializeObject<Dictionary<string, object>>(value.ToString());
    }

    private static List<Dictionary<string, object>> GetEffectList(Dictionary<string, object> attacker, string key)
    {
        var result = new List<Dictionary<string, object>>();
        var value = GetValue(attacker, key);
        if (value == null)
        {
            return result;
        }

        if (value is List<object> objList)
        {
            foreach (var item in objList)
            {
                var dict = ToDict(item);
                if (dict != null)
                {
                    result.Add(dict);
                }
            }
            return result;
        }

        if (value is JArray jArray)
        {
            foreach (var token in jArray)
            {
                var dict = ToDict(token);
                if (dict != null)
                {
                    result.Add(dict);
                }
            }
        }

        return result;
    }

    private static List<Dictionary<string, object>> GetEffectListAny(Dictionary<string, object> data, params string[] keys)
    {
        var result = new List<Dictionary<string, object>>();
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return result;
        }

        if (value is List<object> objList)
        {
            foreach (var item in objList)
            {
                var dict = ToDict(item);
                if (dict != null)
                {
                    result.Add(dict);
                }
            }
            return result;
        }

        if (value is JArray jArray)
        {
            foreach (var token in jArray)
            {
                var dict = ToDict(token);
                if (dict != null)
                {
                    result.Add(dict);
                }
            }
        }

        return result;
    }

    private static int GetInt(Dictionary<string, object> data, string key, int defaultValue)
    {
        if (data == null || !data.ContainsKey(key) || data[key] == null)
        {
            return defaultValue;
        }

        return int.Parse(data[key].ToString());
    }

    private static int GetIntAny(Dictionary<string, object> data, int defaultValue, params string[] keys)
    {
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return defaultValue;
        }

        return int.Parse(value.ToString());
    }

    private static int? GetNullableInt(Dictionary<string, object> data, string key)
    {
        if (data == null || !data.ContainsKey(key) || data[key] == null)
        {
            return null;
        }

        return int.Parse(data[key].ToString());
    }

    private static int? GetNullableIntAny(Dictionary<string, object> data, params string[] keys)
    {
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return null;
        }

        return int.Parse(value.ToString());
    }

    private static bool GetBool(Dictionary<string, object> data, string key, bool defaultValue)
    {
        if (data == null || !data.ContainsKey(key) || data[key] == null)
        {
            return defaultValue;
        }

        return bool.Parse(data[key].ToString());
    }

    private static bool GetBoolAny(Dictionary<string, object> data, bool defaultValue, params string[] keys)
    {
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return defaultValue;
        }

        return bool.Parse(value.ToString());
    }

    private static string GetString(Dictionary<string, object> data, string key)
    {
        if (data == null || !data.ContainsKey(key) || data[key] == null)
        {
            return string.Empty;
        }

        return data[key].ToString();
    }

    private static string GetStringAny(Dictionary<string, object> data, params string[] keys)
    {
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return string.Empty;
        }

        return value.ToString();
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
            string source = GetString(effect, "source");
            string key = $"{effectType}:{duration}:{source}";

            if (seen.Add(key))
            {
                unique.Add(effect);
            }
        }

        return unique;
    }
}
