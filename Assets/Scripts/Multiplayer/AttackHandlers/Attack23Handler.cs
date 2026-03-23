using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 23: Flaming Gun
/// Fixed damage with optional Burn application rendered through the standard effectsApplied flow.
/// </summary>
public class Attack23Handler
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
        yield return showDialog($"{attacker.cardName} uses Flaming Gun");
        yield return animations.PlayFlamingGunAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            defender.health -= damage;
            if (defender.health < 0)
            {
                defender.health = 0;
            }

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

        yield return showDialog($"{defender.cardName} is being caramelized");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var burnEffect = effectsApplied.Find(e => e["type"].ToString() == "16");
            if (burnEffect != null)
            {
                yield return animations.PlayBurnStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is burning");
            }
        }
    }
}
