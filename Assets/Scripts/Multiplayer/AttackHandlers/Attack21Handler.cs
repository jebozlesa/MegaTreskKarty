using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 21: Terrify
/// No damage. Server decides whether the defender is terrified or resists.
/// Stat changes are applied by shared battle playback/timeline flow.
/// </summary>
public class Attack21Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Terrify");
        yield return animations.PlayTerrifyAnimation(attacker.transform);

        if (string.IsNullOrEmpty(attackResult) || attackResult == "resisted")
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog($"{defender.cardName} does not fear you");
            yield break;
        }

        if (attackResult == "terrified")
        {
            yield return animations.PlayAnimationTerrified(defender.transform);
            yield return showDialog($"{defender.cardName} is terribly frightened");
            yield break;
        }

        Debug.LogWarning($"[Attack21] Unknown attackResult: {attackResult}");
        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog("Something went wrong...");
    }
}
