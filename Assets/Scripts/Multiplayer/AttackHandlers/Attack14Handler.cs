using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 14: Up In Smoke
/// Damage: 0 (self-heal only)
/// Effect: Self-heal Random(1-2) + STR/4, +1 Charisma buff
/// Backfire: 10% chance to fall asleep (1-3 turns)
/// </summary>
public class Attack14Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int healAmount,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> attackerEffects)
    {
        yield return showDialog($"{attacker.cardName} uses Up In Smoke!");
        yield return animations.PlayUpInSmokeAnimation(attacker.transform);
        
        // Self-heal
        if (healAmount > 0)
        {
            Debug.LogWarning($" [UP_IN_SMOKE] {attacker.cardName} heals {healAmount} HP");
            attacker.health += healAmount;
            if (attacker.health > attacker.maxHealth) 
            {
                attacker.health = attacker.maxHealth;
            }
            
            // Green HP animation (self-heal)
            if (cardAnimator != null)
            {
                yield return cardAnimator.AnimateHeal(attacker, healAmount);
            }
            
            // Update HP bar
            if (isMyAttack)
            {
                playerLifeBar.SetHP(attacker.health);
            }
            else
            {
                enemyLifeBar.SetHP(attacker.health);
            }
        }
        
        yield return showDialog($"{attacker.cardName} smokes some s#!t and feels good");
        
        // Backfire check (attacker falls asleep)
        if (attackerEffects != null && attackerEffects.Count > 0)
        {
            var sleepEffect = attackerEffects.Find(e => e["type"].ToString() == "3");
            if (sleepEffect != null)
            {
                Debug.LogWarning($" [BACKFIRE] {attacker.cardName} BACKFIRE! Falls asleep!");
                
                // [OK] INITIAL ANIMATION (sucast utoku, nie efektu!)
                yield return animations.PlayDrunkAnimation(attacker.transform);
                yield return showDialog($"{attacker.cardName} falls asleep");
                
                // Effect icon sa prida automaticky neskor v DisplayMultipleEffects()
            }
        }
    }
}
