using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 106: Kusarigama
/// Shared playback handles damage. This handler owns cast animation, flavor dialog, and optional tether start.
/// </summary>
public class Attack106Handler
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
        yield return showDialog($"{attacker.cardName} uses Kusarigama");
        yield return animations.PlayKusarigamaAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName}'s Kusarigama strikes");

        bool tetherApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "9");
        if (tetherApplied)
        {
            yield return animations.PlayAnimationTiedUp(defender.transform);
            yield return showDialog($"{defender.cardName} is trapped in chain");
        }
    }
}
