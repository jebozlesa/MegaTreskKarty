using System.Collections;
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
        System.Func<string, IEnumerator> showDialog)
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
    }
}
