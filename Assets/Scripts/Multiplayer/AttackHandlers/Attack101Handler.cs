using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 101: Yperit
/// Server decides whether the gas strike lands or backfires.
/// Client renders the matching branch via shared damage playback.
/// </summary>
public class Attack101Handler
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
        string attackResult = null
    )
    {
        bool backfire = attackResult == "backfire";
        bool hit = attackResult == "hit";

        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
            backfire = attackerSelfDamage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Yperit");

        if (backfire)
        {
            yield return animations.PlayYperitFailAnimation(attacker.transform);

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

            yield return showDialog("A breeze blows");
            yield break;
        }

        yield return animations.PlayYperitSuccessAnimation(attacker.transform, defender.transform);

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

        yield return showDialog("Yperit strikes");
    }
}
