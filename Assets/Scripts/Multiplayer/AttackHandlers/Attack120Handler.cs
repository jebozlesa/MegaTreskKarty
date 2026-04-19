using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 120: World Champion
/// Self-buff only. Shared battle playback applies CHA/STR changes.
/// </summary>
public class Attack120Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses World Champion");
        yield return animations.PlayWorldChampionAnimation(attacker.transform);

        if (attackResult == "celebrated")
        {
            yield return showDialog($"{attacker.cardName} is world champion and steals the spotlight");
            yield break;
        }

        yield return showDialog($"{attacker.cardName} is world champion");
    }
}
