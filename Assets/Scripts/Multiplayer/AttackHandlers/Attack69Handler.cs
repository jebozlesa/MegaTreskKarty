using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 69: Space Rocket
/// Shared battle playback owns the self stat changes and Satellite icon application.
/// </summary>
public class Attack69Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        bool exploded = attackResult == "exploded";

        yield return showDialog($"{attacker.cardName} uses Space Rocket");
        yield return animations.PlaySpaceRocketAnimation(attacker.transform, !exploded);

        if (exploded)
        {
            yield return showDialog("Rocket exploded");
            yield break;
        }

        yield return showDialog($"{attacker.cardName} launches satellite");
    }
}
