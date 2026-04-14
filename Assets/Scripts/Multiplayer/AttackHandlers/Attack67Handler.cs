using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 67: Winchester
/// Hit chance is handled on the server.
/// Client renders the hit or miss branch from attackResult.
/// </summary>
public class Attack67Handler
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

        yield return showDialog($"{attacker.cardName} uses Winchester");
        yield return animations.PlayWinchesterAnimation(attacker.transform, defender.transform, hit);

        if (!hit)
        {
            yield return showDialog("Bang! aaaand miss");
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

        yield return showDialog($"Bang! {attacker.cardName} shoots");
    }
}
