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

        error = string.IsNullOrEmpty(timelineV2Error)
            ? "timelineV2 missing"
            : $"timelineV2 invalid: {timelineV2Error}";
        return false;
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
        step.StatName = GetStringAny(data, "statName", "StatName");
        step.EffectType = GetIntAny(data, 0, "effectType", "EffectType");
        step.Duration = GetIntAny(data, 0, "duration", "Duration");
        step.Blocked = GetBoolAny(data, false, "blocked", "Blocked");
        step.BlockedBy = GetNullableIntAny(data, "blockedBy", "BlockedBy");
        step.Skipped = GetBoolAny(data, false, "skipped", "Skipped");
        step.Source = GetStringAny(data, "source", "Source");
        step.Note = GetStringAny(data, "note", "Note");
        step.ActionType = GetStringAny(data, "actionType", "ActionType");
        step.ActionId = GetStringAny(data, "actionId", "ActionId");
        step.TurnsRemaining = GetIntAny(data, 0, "turnsRemaining", "TurnsRemaining");
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

    private static int GetIntAny(Dictionary<string, object> data, int defaultValue, params string[] keys)
    {
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return defaultValue;
        }

        return int.Parse(value.ToString());
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

    private static bool GetBoolAny(Dictionary<string, object> data, bool defaultValue, params string[] keys)
    {
        var value = GetValueAny(data, keys);
        if (value == null)
        {
            return defaultValue;
        }

        return bool.Parse(value.ToString());
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
}
