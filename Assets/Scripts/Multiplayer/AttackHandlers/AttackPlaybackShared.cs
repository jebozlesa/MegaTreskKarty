using System.Collections;
using UnityEngine;

public static class AttackPlaybackShared
{
    public static IEnumerator PlayStandardTargetDamage(
        Kard defender,
        int damage,
        bool isMyAttack,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar)
    {
        if (defender == null || damage <= 0)
        {
            yield break;
        }

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
            enemyLifeBar?.SetHP(defender.health);
        }
        else
        {
            playerLifeBar?.SetHP(defender.health);
        }
    }
}
