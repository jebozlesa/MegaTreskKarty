using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 119: Bicycle Kick
/// Server resolves whether the kick lands cleanly, lands big, or fails into self-damage.
/// </summary>
public class Attack119Handler
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
        bool hit = attackResult == "hit" || attackResult == "hit_big";
        bool failed = attackResult == "fall";

        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
            failed = attackerSelfDamage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Bicycle Kick");

        if (failed)
        {
            yield return animations.PlayBicycleKickFailAnimation(attacker.transform);

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

            yield return showDialog($"{attacker.cardName} faces gravity");
            yield break;
        }

        yield return animations.PlayBicycleKickAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"Plesk! Big kick from {attacker.cardName}");
    }
}
