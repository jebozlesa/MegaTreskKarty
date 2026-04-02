using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 33: Peace Pipe
/// Stat changes are rendered by shared timeline playback; this handler only covers the attack visuals.
/// </summary>
public class Attack33Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Peace Pipe");
        animations.StartCoroutine(animations.PlayUpInSmokeAnimation(defender.transform));
        yield return animations.PlayUpInSmokeAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} offers peace pipe");
    }
}
