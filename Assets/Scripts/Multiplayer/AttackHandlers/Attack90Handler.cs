using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 90: Philosophy
/// Shared stat/effect playback owns the defender knowledge buff and status application.
/// This handler only resolves the cast animation and branch-specific flavor playback.
/// </summary>
public class Attack90Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Philosophy");
        yield return animations.PlayPhilosophyAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} philosophizes!!!");

        switch (attackResult)
        {
            case "interested":
                yield return animations.PlayAnimationImpressed(defender.transform);
                yield return showDialog($"{defender.cardName} is interested in philosophy");
                yield break;

            case "bored":
                yield return showDialog($"{defender.cardName} is bored by Philosophy");
                yield return animations.PlayBoredomAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} falls asleep");
                yield break;

            case "confused":
                yield return showDialog($"{defender.cardName} doesn't understand that Philosophy");
                yield return animations.PlayConfusionStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is confused");
                yield break;

            default:
                yield return animations.PlayAnimationNotEffective(defender.transform);
                yield return showDialog("Attack has no effect");
                yield break;
        }
    }
}
