using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 91: Calm
/// Shared stat/effect playback owns the ATT/STR debuffs and Calm effect icon.
/// This handler resolves only the cast animation and the branch-specific feedback.
/// </summary>
public class Attack91Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Calm");
        yield return animations.PlayCalmAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} trying to calm enemy");

        if (attackResult == "calmed")
        {
            yield return animations.PlayCalmStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is calm");
            yield break;
        }

        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog($"{defender.cardName} is calm already");
    }
}
