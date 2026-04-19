using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 89: Gamble
/// Shared stat playback owns the charisma transfer. This handler only resolves
/// the cast animation and outcome dialog.
/// </summary>
public class Attack89Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Gamble");
        yield return showDialog($"{attacker.cardName} is taking bets");
        yield return animations.PlayGambleAnimation(attacker.transform, defender.transform, attackResult == "win");

        if (attackResult == "win")
        {
            yield return showDialog("And wins");
            yield break;
        }

        yield return showDialog("And loses");
    }
}
