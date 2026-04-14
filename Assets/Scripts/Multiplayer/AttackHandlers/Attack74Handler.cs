using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 74: Pen
/// Standard damage playback stays shared. This handler owns only the cast animation,
/// flavor dialog, and poison start animation when the server applies effect 24.
/// </summary>
public class Attack74Handler
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
        yield return showDialog($"{attacker.cardName} uses Pen");
        yield return animations.PlayPenAnimation(attacker.transform, defender.transform);

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

        yield return showDialog("The pen is mightier than the sword");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "24"))
        {
            yield return animations.PlayPoisonStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is poisoned by ink");
        }
    }
}
