using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 7: CarHit
/// Damage: Random (receiver: 3-10, attacker self-damage: 0-5)
/// Effects on RECEIVER (20% each): Sleep (1-3t), Bleed (2-4t, stacks)
/// Effects on ATTACKER recoil (10% each): Sleep (1-3t), Bleed (2-4t, stacks)
/// </summary>
public class Attack7Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        int attackerSelfDamage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        List<Dictionary<string, object>> attackerEffects = null)
    {
        yield return showDialog($"{attacker.cardName} uses CarHit!");
        yield return animations.PlayCarHitAnimation(attacker.transform, defender.transform);
        
        // ✅ Play BOTH damage animations SIMULTANEOUSLY (parallel coroutines)
        if (damage > 0 && cardAnimator != null)
        {
            attacker.StartCoroutine(cardAnimator.AnimateDamage(defender, damage));
        }
        
        if (attackerSelfDamage > 0 && cardAnimator != null)
        {
            attacker.StartCoroutine(cardAnimator.AnimateDamage(attacker, attackerSelfDamage));
        }
        
        // Wait for animations to complete
        yield return new WaitForSeconds(0.5f);
        
        // ✅ Apply damage to BOTH cards SIMULTANEOUSLY (AFTER animations)
        if (damage > 0)
        {
            defender.health -= damage;
            if (defender.health < 0) defender.health = 0;
        }
        
        if (attackerSelfDamage > 0)
        {
            attacker.health -= attackerSelfDamage;
            if (attacker.health < 0) attacker.health = 0;
        }
        
        // ✅ Update HP bars AFTER damage application
        if (isMyAttack)
        {
            playerLifeBar.SetHP(attacker.health);
            enemyLifeBar.SetHP(defender.health);
        }
        else
        {
            playerLifeBar.SetHP(defender.health);
            enemyLifeBar.SetHP(attacker.health);
        }
        
        yield return showDialog($"Tresk! hit by {attacker.cardName}'s car");
        
        // ✅ Initial effect animations for DEFENDER (Sleep/Bleed from car crash)
        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            foreach (var effect in effectsApplied)
            {
                string effectType = effect["type"].ToString();
                
                if (effectType == "3" || effectType == "27") // Sleep/Knockout
                {
                    Debug.LogWarning($"⭐ [CARHIT_KO] Defender falls asleep from crash!");
                    yield return animations.PlayKnockoutAnimation(defender.transform);
                    yield return showDialog($"{defender.cardName} knocked out!");
                }
                else if (effectType == "1") // Bleed
                {
                    Debug.LogWarning($"🩸 [CARHIT_BLEED] Defender is bleeding from crash!");
                    yield return animations.PlayBleedStartAnimation(defender.transform);
                    yield return showDialog($"{defender.cardName} is bleeding!");
                }
                
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        // ✅ Initial effect animations for ATTACKER RECOIL (Sleep/Bleed from crash damage)
        if (attackerEffects != null && attackerEffects.Count > 0)
        {
            foreach (var effect in attackerEffects)
            {
                string effectType = effect["type"].ToString();
                
                if (effectType == "3" || effectType == "27") // Sleep/Knockout
                {
                    Debug.LogWarning($"⭐💥 [CARHIT_RECOIL_KO] Attacker knocked out from recoil!");
                    yield return animations.PlayKnockoutAnimation(attacker.transform);
                    yield return showDialog($"{attacker.cardName} knocked out by recoil!");
                }
                else if (effectType == "1") // Bleed
                {
                    Debug.LogWarning($"🩸💥 [CARHIT_RECOIL_BLEED] Attacker bleeding from recoil!");
                    yield return animations.PlayBleedStartAnimation(attacker.transform);
                    yield return showDialog($"{attacker.cardName} bleeding from crash!");
                }
                
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}
