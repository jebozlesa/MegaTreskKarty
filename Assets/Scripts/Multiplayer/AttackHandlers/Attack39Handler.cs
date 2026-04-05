using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 39: Tesla Coil
/// Shared battle playback owns Electricity icon lifecycle and ongoing block ticks.
/// This handler renders cast, damage, and initial shock-start feedback.
/// </summary>
public class Attack39Handler
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
        List<Dictionary<string, object>> effectsApplied = null
    )
    {
        yield return showDialog($"{attacker.cardName} uses Tesla Coil");
        yield return animations.PlayTeslaCoilAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} shocks enemy");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "8"))
        {
            yield return animations.PlayElectricityStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is shocked");
        }
    }
}
