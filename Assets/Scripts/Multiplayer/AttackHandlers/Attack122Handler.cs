using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 122: Sport Skills
/// Shared stat playback owns the defender SPD/CHA changes.
/// This handler only replays the football flavor branch and optional sleep outcome.
/// </summary>
public class Attack122Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Sport Skills");
        yield return animations.PlayFootballAnimation(attacker.transform, defender.transform, true);
        yield return showDialog($"{attacker.cardName} plays football");

        if (attackResult == "liked")
        {
            yield return showDialog($"{defender.cardName} likes it");
            yield break;
        }

        if (attackResult == "bored" || attackResult == "bored_sleep" || attackResult == "bored_blocked")
        {
            yield return animations.PlayBoredomAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is bored by Football");

            bool sleepApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "3");
            if (sleepApplied || attackResult == "bored_sleep")
            {
                yield return showDialog($"{defender.cardName} falls asleep");
            }
        }
    }
}
