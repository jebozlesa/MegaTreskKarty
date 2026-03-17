using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 4: Forgiveness
/// Damage: NONE (peaceful attack)
/// Effect: 75% Asceticism (1-3 turns)
/// Debuff: -1 attack stat
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
        
        // Debuff effect - apply -1 attack
        Debug.LogWarning($" [FORGIVENESS] {attacker.cardName} -> {defender.cardName}: -1 attack");
        defender.HandleAttack(-1);
        yield return showDialog($"{attacker.cardName} forgives your heresy");
        
        // [OK] Initial effect animation (Asceticism)
        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var asceticismEffect = effectsApplied.Find(e => e["type"].ToString() == "2");
            if (asceticismEffect != null)
            {
                Debug.LogWarning($" [ASCETICISM_INIT] Playing ASCETICISM START animation");
                yield return animations.PlayAscetismStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} feels doomed!");
            }
        }
    }
}
