using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 118: Football
/// Server resolves hit/miss and optional knockout; client renders the chosen branch.
/// </summary>
public class Attack118Handler
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
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        bool hit = attackResult == "hit" || attackResult == "hit_ko";
        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Football");
        yield return animations.PlayFootballAnimation(attacker.transform, defender.transform, hit);

        if (!hit)
        {
            yield return showDialog("kick missed");
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

        yield return showDialog($"{attacker.cardName} kicks football");

        if (attackResult == "hit_ko")
        {
            yield return animations.PlayKnockoutAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is KO");
        }
    }
}

