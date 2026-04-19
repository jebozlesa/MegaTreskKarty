using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 114: Macuahuitl
/// Shared playback owns damage. This handler renders cast animation, flavor dialog,
/// and optional bleed / knockout start animations when the server applies them.
/// </summary>
public class Attack114Handler
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
        yield return showDialog($"{attacker.cardName} uses Macuahuitl");
        yield return animations.PlayMacuahuitlAnimation(attacker.transform, defender.transform);

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

        bool bleedApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "1");
        if (bleedApplied)
        {
            yield return animations.PlayBleedStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is wounded");
        }

        bool knockoutApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "27");
        if (knockoutApplied)
        {
            yield return animations.PlayKnockoutAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} falls asleep");
        }

        yield return showDialog($"{attacker.cardName} smashes with Macuahuitl");
    }
}
