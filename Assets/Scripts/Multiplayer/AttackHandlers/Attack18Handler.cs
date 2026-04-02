using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 18: Bloodthirst
/// Server computes both damage dealt and the real heal amount.
/// Client renders drain damage first, then self-heal.
/// </summary>
public class Attack18Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        int healAmount,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Bloodthirst");
        yield return animations.PlayBloodSuckingAnimation(defender.transform, attacker.transform);

        if (damage > 0)
        {
            yield return AttackPlaybackShared.PlayStandardTargetDamage(
                defender,
                damage,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        if (healAmount > 0)
        {
            attacker.health += healAmount;
            if (attacker.health > attacker.maxHealth)
            {
                attacker.health = attacker.maxHealth;
            }

            if (cardAnimator != null)
            {
                yield return cardAnimator.AnimateHeal(attacker, healAmount);
            }

            if (isMyAttack)
            {
                playerLifeBar.SetHP(attacker.health);
            }
            else
            {
                enemyLifeBar.SetHP(attacker.health);
            }
        }

        yield return showDialog($"{defender.cardName} was drained");
    }
}
