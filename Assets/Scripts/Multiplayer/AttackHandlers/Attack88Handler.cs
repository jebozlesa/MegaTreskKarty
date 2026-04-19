using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 88: Sabre
/// Standard damage playback stays shared. This handler owns only the cast animation,
/// flavor dialog, and bleed start animation when the server applies Bleed.
/// </summary>
public class Attack88Handler
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
        yield return showDialog($"{attacker.cardName} uses Sabre");
        yield return animations.PlaySabreAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} cuts with Sabre");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "1"))
        {
            yield return animations.PlayBleedStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is wounded");
        }
    }
}
