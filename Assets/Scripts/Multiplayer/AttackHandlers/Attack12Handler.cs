using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack 12: Chi Sau (Sticky Hands)
/// Bruce Lee's rapid hand technique with variable strikes
/// Damage: multiply - (defender speed / 4)
/// where multiply = random(1, attacker speed)
/// </summary>
public class Attack12Handler
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
        yield return showDialog($"{attacker.cardName} attacks with sticky hands!");
        
        // ✅ Use damage as visual multiply - higher damage = more strikes shown
        // Clamp to reasonable range (1-9) for animation
        int visualMultiply = Mathf.Clamp(damage, 1, 9);
        
        // Play Chi Sau animation with rapid strikes (count = damage dealt)
        yield return animations.PlayChiSauAnimation(attacker.transform, defender.transform, visualMultiply);
        
        // Apply server-calculated damage
        defender.health -= damage;
        defender.health = Mathf.Max(0, defender.health);
        
        // Update HP bar
        if (isMyAttack)
        {
            enemyLifeBar.SetHP(defender.health);
        }
        else
        {
            playerLifeBar.SetHP(defender.health);
        }
        
        // Animate damage
        yield return cardAnimator.AnimateDamage(defender, damage);
        
        Debug.Log($"[Attack12] {attacker.cardName} → Chi Sau ({visualMultiply} strikes) → {defender.cardName}: {damage} damage");
    }
}
