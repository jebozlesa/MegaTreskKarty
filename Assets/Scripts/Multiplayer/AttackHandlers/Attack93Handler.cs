using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 93: Valaska
/// Standard damage playback stays shared. This handler owns only the cast animation,
/// flavor dialog, and bleed start animation when the server applies Bleed.
/// </summary>
public class Attack93Handler
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
        yield return showDialog($"{attacker.cardName} uses Valaska");
        yield return animations.PlayValaskaAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName}'s valaska strikes");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "1"))
        {
            yield return animations.PlayBleedStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is wounded");
        }
    }
}
