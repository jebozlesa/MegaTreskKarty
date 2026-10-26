using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ComboAttackPlayback
{
    public static bool TryParseSequence(string attackResult, out JArray subAttacks, out string error)
    {
        subAttacks = null;
        error = null;
        if (string.IsNullOrWhiteSpace(attackResult))
        {
            error = "Combo attackResult is missing.";
            return false;
        }

        JObject payload;
        try
        {
            payload = JObject.Parse(attackResult);
        }
        catch (JsonException exception)
        {
            error = $"Invalid combo JSON: {exception.Message}";
            return false;
        }

        var sequence = payload["subAttacks"] as JArray;
        if (sequence == null || sequence.Count == 0)
        {
            error = "Combo requires a non-empty subAttacks array.";
            return false;
        }

        foreach (JToken token in sequence)
        {
            if (!(token is JObject subAttack) || subAttack["attackId"]?.Type != JTokenType.Integer
                || !int.TryParse(subAttack["attackId"].ToString(), out int subAttackId))
            {
                error = "Combo sub-attack requires an integer attackId.";
                return false;
            }

            JToken nestedResult = subAttack["attackResult"];
            if (nestedResult != null && nestedResult.Type != JTokenType.Null && nestedResult.Type != JTokenType.String)
            {
                error = $"Invalid combo sub-attack {subAttackId}: attackResult must be a string or null.";
                return false;
            }
            if (!AttackRegistry.TryValidate(subAttackId, nestedResult?.Value<string>(), true, out error))
                return false;
        }

        subAttacks = sequence;
        return true;
    }

    public static IEnumerator PlaySequence(AttackExecutionContext parentContext)
    {
        // Validate the whole sequence before yielding any sub-attack.
        if (!TryParseSequence(parentContext.AttackResult, out var subAttacks, out string error))
            throw new InvalidOperationException(error);

        foreach (JToken subAttackToken in subAttacks)
        {
            int subAttackId = subAttackToken.Value<int>("attackId");
            List<Dictionary<string, object>> effectsApplied =
                (subAttackToken["effectsApplied"] as JArray)?.ToObject<List<Dictionary<string, object>>>()
                ?? new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> attackerEffectsApplied =
                (subAttackToken["attackerEffectsApplied"] as JArray)?.ToObject<List<Dictionary<string, object>>>()
                ?? new List<Dictionary<string, object>>();

            AttackExecutionContext subAttackContext = new AttackExecutionContext(
                parentContext.Attacker,
                parentContext.Defender,
                subAttackToken.Value<int?>("damage") ?? 0,
                subAttackToken.Value<int?>("healAmount") ?? 0,
                parentContext.IsMyAttack,
                subAttackToken.Value<int?>("attackerSelfDamage") ?? 0,
                parentContext.Animations,
                parentContext.CardAnimator,
                parentContext.PlayerLifeBar,
                parentContext.EnemyLifeBar,
                parentContext.ShowDialog,
                effectsApplied,
                subAttackToken.Value<string>("attackResult"),
                attackerEffectsApplied
            );

            yield return AttackRegistry.Execute(subAttackId, subAttackContext);
        }
    }
}
