using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 44: Corruption
/// No direct damage. Shared battle playback applies the random stat changes.
/// </summary>
public class Attack44Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null
    )
    {
        int multiplier = attackResult == "corrupting_1" ? 1 : 2;
        yield return showDialog($"{attacker.cardName} uses Corruption");
        yield return animations.PlayCorruptionAnimation(attacker.transform, multiplier);
        yield return showDialog($"{attacker.cardName} corrupted everybody around");
    }
}
