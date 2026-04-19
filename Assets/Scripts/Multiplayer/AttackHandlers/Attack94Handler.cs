using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 94: Moonshine
/// Shared stat playback applies the permanent +2/+2/-2 tradeoff, while Fury/Sleep
/// stay on the standard self-effect contract already used elsewhere.
/// </summary>
public class Attack94Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int healAmount,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> attackerEffects,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Moonshine");
        yield return animations.PlayMoonshineAnimation(attacker.transform);

        if (healAmount > 0)
        {
            yield return BattleValuePlayback.PlayHeal(
                attacker,
                healAmount,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} is getting pretty drunk");

        bool hasFury = attackerEffects != null && attackerEffects.Exists(e => e["type"].ToString() == "6");
        bool hasSleep = attackerEffects != null && attackerEffects.Exists(e => e["type"].ToString() == "3");

        if (hasFury)
        {
            yield return animations.PlayFuryAnimation(attacker.transform);
            yield return showDialog($"{attacker.cardName} is furious!");
        }

        if (hasSleep)
        {
            yield return animations.PlayDrunkAnimation(attacker.transform);
            yield return showDialog($"{attacker.cardName} falls asleep");
        }
    }
}
