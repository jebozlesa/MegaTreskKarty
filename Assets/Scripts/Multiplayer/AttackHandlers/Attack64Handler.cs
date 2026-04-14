using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 64: Handcuff Escape
/// Self-cleanses Tether and Blockade, then can apply Confusion to the defender.
/// Effect icon removal stays in the shared battle-result playback flow.
/// </summary>
public class Attack64Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Danger Escape");
        yield return animations.PlayHandcuffEscapeAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} slips outta trouble");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var confusionEffect = effectsApplied.Find(e => e["type"].ToString() == "17");
            if (confusionEffect != null)
            {
                yield return animations.PlayConfusionStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is confused");
            }
        }
    }
}
