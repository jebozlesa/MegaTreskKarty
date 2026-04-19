using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 97: Passive Resistance
/// Shared stat playback owns the attacker defense buff and defender attack debuff.
/// This handler only resolves the cast animation and dialog.
/// </summary>
public class Attack97Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Passive Resistance");
        yield return animations.PlayPassiveResistanceAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} is resisting passively!!!");
    }
}
