using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 26: Boost
/// Stat changes and wake-up/removal are rendered by shared timeline playback.
/// This handler only owns the attack animation and dialog.
/// </summary>
public class Attack26Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Boost");
        yield return animations.PlayBoostAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} gets some good boost");
    }
}
