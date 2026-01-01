using System.Collections;
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
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Punch!");
        yield return animations.PlayPunchAnimation(attacker.transform, defender.transform);
        
        // Damage attack - apply damage + HP bar update
        if (damage > 0)
        {
            Debug.LogWarning($"💥 [PUNCH] {attacker.cardName} → {defender.cardName}: {damage} damage");
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
            
            yield return showDialog($"Puf! punch from {attacker.cardName}");
        }
    }
}
