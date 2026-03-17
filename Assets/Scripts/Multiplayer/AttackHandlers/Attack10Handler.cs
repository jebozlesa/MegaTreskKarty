using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 10: Scratch
/// Damage: (attack/3) - (defense/3), min 1
/// Effect: 20% Bleed (1-2 turns, STACKS!)
/// </summary>
public class Attack10Handler
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
        yield return showDialog($"{attacker.cardName} uses Scratch!");
        yield return animations.PlayScratchAnimation(defender.transform);
        
        // Damage attack - apply damage + HP bar update
        if (damage > 0)
        {
            Debug.LogWarning($"[HIT] [SCRATCH] {attacker.cardName} -> {defender.cardName}: {damage} damage");
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
        }
        
        yield return showDialog($"{attacker.cardName} scratches opponent!");
        
        // [OK] Initial effect animation (Bleed)
        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var bleedEffect = effectsApplied.Find(e => e["type"].ToString() == "1");
            if (bleedEffect != null)
            {
                Debug.LogWarning($"[BLEED] [BLEED_INIT] Playing BLEED START animation");
                yield return animations.PlayBleedStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is bleeding!");
            }
        }
    }
}
