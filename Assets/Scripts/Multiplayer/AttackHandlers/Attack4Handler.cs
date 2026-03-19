using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 4: Forgiveness
/// Damage: NONE (peaceful attack)
/// Debuff/stat changes are applied by shared battle playback/timeline flow.
/// </summary>
public class Attack4Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null)
    {
        yield return showDialog($"{attacker.cardName} uses Forgiveness!");
        yield return animations.PlayForgivenessAnimation(attacker.transform);
        Debug.LogWarning($"[FORGIVENESS] {attacker.cardName} -> {defender.cardName}: pending stat/effect playback from server contract");
        yield return showDialog($"{attacker.cardName} forgives your heresy");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var asceticismEffect = effectsApplied.Find(e => e["type"].ToString() == "2");
            if (asceticismEffect != null)
            {
                Debug.LogWarning($"[ASCETICISM_INIT] Playing ASCETICISM START animation");
                yield return animations.PlayAscetismStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} feels doomed!");
            }
        }
    }
}
