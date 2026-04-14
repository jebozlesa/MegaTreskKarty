using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 79: Iwisa
/// Standard damage playback stays shared. This handler owns the cast animation,
/// flavor dialog, and knockout start animation when the server applies effect 27.
/// </summary>
public class Attack79Handler
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
        yield return showDialog($"{attacker.cardName} uses Iwisa");
        yield return animations.PlayIwisaAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} attacks with Iwisa");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "27"))
        {
            yield return animations.PlayKnockoutAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} falls asleep");
        }
    }
}
