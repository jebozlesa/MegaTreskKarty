using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 63: Fire Ship
/// Fixed 3 damage with optional Burn application rendered through the standard effectsApplied flow.
/// </summary>
public class Attack63Handler
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
        yield return showDialog($"{attacker.cardName} uses FireShip");
        yield return animations.PlayFireShipAnimation(attacker.transform, defender.transform);

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

        yield return showDialog("Fire ship impacts");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var burnEffect = effectsApplied.Find(e => e["type"].ToString() == "16");
            if (burnEffect != null)
            {
                yield return animations.PlayBurnStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is burning");
            }
        }
    }
}
