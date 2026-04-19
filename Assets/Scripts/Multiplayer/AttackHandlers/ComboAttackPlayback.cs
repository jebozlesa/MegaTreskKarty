using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class ComboAttackPlayback
{
    public static IEnumerator PlaySequence(AttackExecutionContext parentContext)
    {
        if (string.IsNullOrEmpty(parentContext.AttackResult))
        {
            yield break;
        }

        JObject payload;
        try
        {
            payload = JObject.Parse(parentContext.AttackResult);
        }
        catch
        {
            Debug.LogWarning($"[ComboAttackPlayback] Failed to parse combo payload: {parentContext.AttackResult}");
            yield break;
        }

        JArray subAttacks = payload["subAttacks"] as JArray;
        if (subAttacks == null)
        {
            yield break;
        }

        foreach (JToken subAttackToken in subAttacks)
        {
            int subAttackId = subAttackToken.Value<int?>("attackId") ?? 0;
            if (subAttackId <= 0)
            {
                continue;
            }

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

            yield return AttackRegistry.ExecuteOrFallback(subAttackId, subAttackContext);
        }
    }
}
