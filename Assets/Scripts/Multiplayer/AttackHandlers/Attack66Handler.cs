using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 66: Carcano M91
/// Hit chance is handled on the server.
/// Client renders the hit or miss branch from attackResult.
/// </summary>
public class Attack66Handler
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

        yield return showDialog($"{attacker.cardName} uses Carcano M91");
        yield return animations.PlayCarcanoAnimation(attacker.transform, defender.transform, hit);

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
