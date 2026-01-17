using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 13: One Inch Punch
/// Damage: 3 + (speed/4) - (defense/4)
/// Effect: Critical hit based on knowledge (knowledge/20 = crit chance, max 100%)
/// Crit damage: +strength/2
/// </summary>
public class Attack13Handler
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
        yield return showDialog($"{attacker.cardName} uses One Inch Punch!");
        yield return animations.PlayOneInchPunchAnimation(attacker.transform, defender.transform);
        
        // Apply damage
        if (damage > 0)
        {
            Debug.LogWarning($"💥 [ONE_INCH_PUNCH] {attacker.cardName} → {defender.cardName}: {damage} damage");
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
            
            yield return showDialog($"{attacker.cardName} pokes enemy with finger");
        }
    }
}
