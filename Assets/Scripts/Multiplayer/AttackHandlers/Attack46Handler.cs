using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 46: Mortar
/// Server decides whether the shot hits or backfires.
/// Client renders the correct branch via shared damage playback.
/// </summary>
public class Attack46Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        int attackerSelfDamage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        bool backfire = attackResult == "backfire";
        bool hit = attackResult == "hit";
        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
            backfire = attackerSelfDamage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Mortar");

        if (backfire)
        {
            yield return animations.PlayMortarAnimation(attacker.transform, attacker.transform, true);
            if (attackerSelfDamage > 0)
            {
                yield return BattleValuePlayback.PlayDamage(
                    attacker,
                    attackerSelfDamage,
                    isMyAttack,
                    cardAnimator,
                    playerLifeBar,
                    enemyLifeBar
                );
            }

            yield return showDialog("Mortar exploded");
            yield break;
        }

        yield return animations.PlayMortarAnimation(attacker.transform, defender.transform, hit);

        if (hit && damage > 0)
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

        yield return showDialog(hit ? "Mortar fires" : "Mortar missed");
    }
}
