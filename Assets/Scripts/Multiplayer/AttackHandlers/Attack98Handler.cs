using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 98: Hunger Strike
/// Shared damage and stat/effect playback own the self-damage and Depression application.
/// This handler resolves the cast animation and branch-specific dialog feedback.
/// </summary>
public class Attack98Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int attackerSelfDamage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Hunger Strike");
        yield return animations.PlayHungerStrikeAnimation(attacker.transform);

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

        yield return showDialog($"{attacker.cardName} refuses to eat");

        bool depressionApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "13");
        if (depressionApplied)
        {
            yield return animations.PlayDepressionStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} feels bad for enemy");
            yield break;
        }

        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog($"{defender.cardName} doesn't care");
    }
}
