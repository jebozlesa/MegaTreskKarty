using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 76: Ghost
/// No direct damage. Shared stat/effect playback owns Fear stat mutation and expiry rollback.
/// </summary>
public class Attack76Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Ghost");
        yield return animations.PlayGhostAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{attacker.cardName} summons ghost");

        if (attackResult == "feared")
        {
            yield return animations.PlayFearStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} fears");
            yield break;
        }

        if (string.IsNullOrEmpty(attackResult) || attackResult == "unafraid")
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog($"{defender.cardName} does not fear");
            yield break;
        }

        Debug.LogWarning($"[Attack76] Unknown attackResult: {attackResult}");
        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog("Something went wrong...");
    }
}
