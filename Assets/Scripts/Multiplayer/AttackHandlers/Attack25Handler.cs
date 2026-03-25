using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 25: Pan
/// Damage already includes the optional extra hit from the server.
/// Knockout is rendered through the standard effectsApplied flow.
/// </summary>
public class Attack25Handler
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
        yield return showDialog($"{attacker.cardName} uses Pan");
        yield return animations.PlayPanAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"Tongggg!!! {defender.cardName} gets hit by the pan");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var knockoutEffect = effectsApplied.Find(e => e["type"].ToString() == "27");
            if (knockoutEffect != null)
            {
                yield return animations.PlayKnockoutAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} falls asleep");
            }
        }
    }
}
