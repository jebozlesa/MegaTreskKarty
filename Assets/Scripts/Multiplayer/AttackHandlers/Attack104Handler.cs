using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 104: Retiarius
/// Starts a staged trident maneuver on success by applying Tether to the defender.
/// Ongoing aim and strike playback is handled by BattleResultProcessor.
/// </summary>
public class Attack104Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Retiarius");
        yield return animations.PlayRetiariusAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{attacker.cardName} throws the net");

        bool captured = attackResult == "captured"
            || (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "9"));

        if (captured)
        {
            yield return animations.PlayAnimationTiedUp(defender.transform);
            yield return showDialog($"{defender.cardName} was captured");
        }
        else
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog($"{defender.cardName} jumped away");
        }
    }
}
