using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 28: Shamshir
/// Damage already includes the optional extra cut from the server.
/// Bleed is rendered through the standard effectsApplied flow.
/// </summary>
public class Attack28Handler
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
        List<Dictionary<string, object>> effectsApplied = null)
    {
        yield return showDialog($"{attacker.cardName} uses Shamshir");
        yield return animations.PlayShamshirAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            yield return AttackPlaybackShared.PlayStandardTargetDamage(
                defender,
                damage,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} cuts with shamshir");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var bleedEffect = effectsApplied.Find(e => e["type"].ToString() == "1");
            if (bleedEffect != null)
            {
                yield return animations.PlayBleedStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is wounded");
            }
        }
    }
}
