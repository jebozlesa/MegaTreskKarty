using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 27: Temptation
/// No damage. Server decides whether the target is tempted or stays unmoved.
/// Stat changes are applied by shared battle playback/timeline flow.
/// </summary>
public class Attack27Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Temptation");
        yield return animations.PlayTemptationAnimation(attacker.transform, defender.transform);

        if (attackResult == "tempted")
        {
            bool useMaleVariant = TemptationRules.UseMaleSuccessAnimationVariant(defender.styleId);
            yield return animations.PlayTemptationSuccessAnimation(defender.transform, useMaleVariant);
            yield return showDialog($"{defender.cardName} feels awkward and confused");
            yield break;
        }

        if (string.IsNullOrEmpty(attackResult) || attackResult == "unmoved")
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog($"No effect on {defender.cardName}");
            yield break;
        }

        Debug.LogWarning($"[Attack27] Unknown attackResult: {attackResult}");
        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog("Something went wrong...");
    }
}
