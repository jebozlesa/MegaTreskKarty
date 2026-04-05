using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 16: Revolver
/// Hit chance handled on server.
/// Client only renders hit/miss based on attackResult (fallback by damage).
/// </summary>
public class Attack16Handler
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

        yield return showDialog($"{attacker.cardName} uses Revolver");
        yield return animations.PlayRevolverAnimation(attacker.transform, defender.transform, hit);

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

        yield return showDialog($"Bang! {attacker.cardName} hits target");
    }
}


