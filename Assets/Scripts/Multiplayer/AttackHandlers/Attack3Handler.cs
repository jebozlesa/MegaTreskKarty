using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 3: Heal
/// SELF-HEAL: Heals attacker (not defender!)
/// Heal amount: Random(1,2) + floor(knowledge/4)
/// Effect: Removes negative effects [1, 4, 13, 24]
/// </summary>
public class Attack3Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int healAmount,
        bool isMyAttack,
        AttackAnimations animations,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Heal!");
        yield return animations.PlayHealAnimation(attacker.transform);
        
        // Heal effect - apply heal + HP bar update
        if (healAmount > 0)
        {
            Debug.LogWarning($" [HEAL] {attacker.cardName} heals for {healAmount} HP!");
            attacker.Heal(healAmount);
            
            if (isMyAttack)
            {
                playerLifeBar.SetHP(attacker.health);
            }
            else
            {
                enemyLifeBar.SetHP(attacker.health);
            }
            
            yield return showDialog($"{attacker.cardName} Heals himself");
        }
    }
}
