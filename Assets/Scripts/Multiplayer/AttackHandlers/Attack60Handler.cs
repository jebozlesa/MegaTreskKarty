using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 60: Rapier
/// Server drives total damage, bonus-hit branch and bleed application.
/// </summary>
public class Attack60Handler
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
        yield return showDialog($"{attacker.cardName} uses Rapier");
        yield return animations.PlayRapierAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} cuts with Rapier");

        bool bleedApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "1");
        if (bleedApplied)
        {
            yield return animations.PlayBleedStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is wounded");
        }
    }
}
