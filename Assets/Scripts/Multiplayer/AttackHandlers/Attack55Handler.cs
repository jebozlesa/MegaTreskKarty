using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 55: Gravity Pull
/// Variant and damage remain server-driven; this handler maps the server attackResult to the matching object animation.
/// </summary>
public class Attack55Handler
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
        yield return showDialog($"{attacker.cardName} uses Gravity Pull");

        int variantIndex = attackResult switch
        {
            "piano" => 0,
            "boiler" => 1,
            "anvil" => 2,
            _ => 3,
        };

        string objectName = attackResult switch
        {
            "piano" => "piano",
            "boiler" => "boiler",
            "anvil" => "anvil",
            _ => "gases",
        };

        yield return animations.PlayGravityPullAnimation(attacker.transform, defender.transform, variantIndex);

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

        yield return showDialog($"Gravity pulled {objectName} to {defender.cardName}");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "27"))
        {
            yield return animations.PlayKnockoutAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} falls asleep");
        }
    }
}
