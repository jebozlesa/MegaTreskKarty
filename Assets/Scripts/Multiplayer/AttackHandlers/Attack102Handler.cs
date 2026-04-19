using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 102: Blitzkrieg
/// Implemented by design intent: attacker speed advantage increases the chance of a successful
/// blitz attack, while failure causes a small self-damage stumble.
/// </summary>
public class Attack102Handler
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
        bool tooSlow = attackResult == "too-slow";
        bool blitz = attackResult == "blitz";

        if (string.IsNullOrEmpty(attackResult))
        {
            blitz = damage > 0;
            tooSlow = attackerSelfDamage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Blitzkrieg");

        if (tooSlow)
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);

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

            yield return showDialog("Too slow");
            yield break;
        }

        yield return animations.PlayBlitzkriegAnimation(attacker.transform, defender.transform);

        if (blitz && damage > 0)
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

        yield return showDialog($"{attacker.cardName} uses Blitzkrieg tactics");
    }
}
