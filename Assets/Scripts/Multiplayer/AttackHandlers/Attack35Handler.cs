using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 35: Fury
/// Shared battle playback owns the stat changes and effect icons.
/// This handler only renders the cast branch based on attackResult.
/// </summary>
public class Attack35Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Fury");

        if (attackResult == "no_effect")
        {
            animations.StartCoroutine(animations.PlayAnimationNotEffective(attacker.transform));
            yield return showDialog($"No effect on {attacker.cardName}");
            yield break;
        }

        yield return animations.PlayFuryAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} is furious!");
    }
}
