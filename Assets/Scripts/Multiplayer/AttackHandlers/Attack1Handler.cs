using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 1: Punch
/// Damage: (strength/3) - (defense/3), min 1
/// Effect: 20% Sleep (1-2 turns)
/// </summary>
public class Attack1Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null)
    {
        yield return showDialog($"{attacker.cardName} uses Punch!");
        yield return animations.PlayPunchAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            Debug.LogWarning($"[HIT] [PUNCH] {attacker.cardName} -> {defender.cardName}: {damage} damage");
            yield return AttackPlaybackShared.PlayStandardTargetDamage(
                defender,
                damage,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
            yield return showDialog($"Puf! punch from {attacker.cardName}");
        }

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var sleepEffect = effectsApplied.Find(e => e["type"].ToString() == "27");
            if (sleepEffect != null)
            {
                Debug.LogWarning($"[STAR] [PUNCH_KO] Playing KNOCKOUT animation");
                yield return animations.PlayKnockoutAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} falls asleep!");
            }
        }
    }
}
