using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 43: Tie Up
/// No damage. Applies Tether through the standard effectsApplied flow on success.
/// </summary>
public class Attack43Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null)
    {
        yield return showDialog($"{attacker.cardName} uses Tie Up");
        yield return animations.PlayTieUpAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{attacker.cardName} trying to catch enemy");

        bool tetherApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "9");
        if (tetherApplied)
        {
            yield return animations.PlayAnimationTiedUp(defender.transform);
            yield return showDialog($"{defender.cardName} was captured");
        }
        else
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog($"{defender.cardName} runs away");
        }
    }
}
