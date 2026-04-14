using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 81: Tessenjutsu
/// Shared battle playback owns the self defense buff. This handler only resolves
/// the cast animation and dialog.
/// </summary>
public class Attack81Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Tessenjutsu");
        yield return animations.PlayTessenjutsuAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} moves like Kitana");
    }
}
