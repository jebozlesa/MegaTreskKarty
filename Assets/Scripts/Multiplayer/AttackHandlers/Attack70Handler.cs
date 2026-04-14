using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 70: V-2
/// Hit chance is handled on the server.
/// Client renders the hit or miss branch from attackResult.
/// </summary>
public class Attack70Handler
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

        yield return showDialog($"{attacker.cardName} uses V-2");
        yield return animations.PlayV2Animation(attacker.transform, defender.transform, hit);

        if (!hit)
        {
            yield return showDialog("Missile missed");
            yield break;
        }

        if (damage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                defender,
                damage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog("V2 hits target");
    }
}
