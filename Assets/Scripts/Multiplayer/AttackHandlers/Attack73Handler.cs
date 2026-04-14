using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 73: Standard
/// Shared battle playback owns the self stat changes. This handler only resolves
/// the cast animation and the morale dialog branch.
/// </summary>
public class Attack73Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Standard");
        yield return animations.PlayStandardAnimation(attacker.transform);

        if (attackResult == "morale")
        {
            yield return showDialog($"{attacker.cardName}'s Banner Raised Morale");
            yield break;
        }

        yield return showDialog($"{attacker.cardName}'s Banner Raised Morale");
    }
}
