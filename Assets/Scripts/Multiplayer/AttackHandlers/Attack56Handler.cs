using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 56: Kamikaze
/// Server decides hit or miss; the attacker always self-destructs for its current HP.
/// </summary>
public class Attack56Handler
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
        bool hit = attackResult == "hit";
        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Kamikaze");
        yield return animations.PlayKamikazeAnimation(attacker.transform, defender.transform, hit);

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

        yield return showDialog(hit ? "Kamikaze strikes" : "Ou! Nasty miss");
    }
}
