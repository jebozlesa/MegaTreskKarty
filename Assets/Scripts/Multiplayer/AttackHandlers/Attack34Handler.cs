using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 34: Recurve Bow
/// Hit chance handled on server.
/// Client only renders hit or miss based on attackResult (fallback by damage).
/// </summary>
public class Attack34Handler
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
        string attackResult = null)
    {
        bool hit = attackResult == "hit";
        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Recurve Bow");
        yield return animations.PlayRecurveBowAnimation(attacker.transform, defender.transform, hit);

        if (!hit)
        {
            yield return showDialog("arrow missed");
            yield break;
        }

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

        yield return showDialog($"{attacker.cardName} shoots arrow");
    }
}
