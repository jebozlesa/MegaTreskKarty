using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 19: Sword
/// Damage already includes any extra hit from the server.
/// Bleed is rendered through the standard effectsApplied flow.
/// </summary>
public class Attack19Handler
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
        yield return showDialog($"{attacker.cardName} uses Sword");
        yield return animations.PlaySwordAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
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

        yield return showDialog($"{attacker.cardName} cuts enemy with sword");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var bleedEffect = effectsApplied.Find(e => e["type"].ToString() == "1");
            if (bleedEffect != null)
            {
                yield return animations.PlayBleedStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is wounded");
            }
        }
    }
}
