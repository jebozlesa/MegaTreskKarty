using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 92: Honesty
/// Shared damage and stat/effect playback own the raw HP loss and Depression application.
/// This handler resolves the cast animation and branch-specific dialog feedback.
/// </summary>
public class Attack92Handler
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
        yield return showDialog($"{attacker.cardName} uses Honesty");
        yield return animations.PlayHonestyAnimation(attacker.transform);

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

        yield return showDialog($"{attacker.cardName} is brutally honest");

        bool depressionApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "13");
        if (!depressionApplied)
        {
            yield break;
        }

        yield return animations.PlayDepressionStartAnimation(defender.transform);

        if (attackResult == "empathy")
        {
            yield return showDialog($"{attacker.cardName} feels bad for enemy");
            yield break;
        }

        yield return showDialog($"{defender.cardName} feels bad");
    }
}
