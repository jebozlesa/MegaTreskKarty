using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 103: Propaganda
/// Shared stat playback owns defender ATT/KNO debuffs.
/// This handler resolves only the cast animation and dialog.
/// </summary>
public class Attack103Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Propaganda");
        yield return animations.PlayPropagandaAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{defender.cardName} was hit by propaganda");
    }
}
