using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 8: MonkeyWrench
/// Damage: 3 + (strength/2) - (defense/2), min 1
/// Critical: 50% chance for +10 damage (speed-based)
/// Effect: 10% Sleep (2 turns, knowledge-based)
/// </summary>
public class Attack8Handler
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
        yield return showDialog($"{attacker.cardName} uses MonkeyWrench!");
        yield return animations.PlayMonkeyWrenchAnimation(attacker.transform, defender.transform);
        
        // Damage attack - apply damage + HP bar update
        if (damage > 0)
        {
            Debug.LogWarning($"💥 [MONKEYWRENCH] {attacker.cardName} → {defender.cardName}: {damage} damage");
            defender.health -= damage;
            if (defender.health < 0) defender.health = 0;
            
            if (cardAnimator != null)
            {
                yield return cardAnimator.AnimateDamage(defender, damage);
            }
            
            if (isMyAttack)
            {
                enemyLifeBar.SetHP(defender.health);
            }
            else
            {
                playerLifeBar.SetHP(defender.health);
            }
            
            yield return showDialog($"{attacker.cardName} hits with Monkey Wrench");
        }

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var sleepEffect = effectsApplied.Find(e => e["type"].ToString() == "27");
            if (sleepEffect != null)
            {
                Debug.LogWarning($"⭐ [MONKEYWRENCH_KO] Playing KNOCKOUT animation");
                yield return animations.PlayKnockoutAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} falls asleep!");
            }
        }
    }
}
