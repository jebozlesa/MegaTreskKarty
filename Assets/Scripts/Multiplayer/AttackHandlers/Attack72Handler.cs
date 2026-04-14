using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 72: Revelation
/// Shared battle playback owns the self stat changes. This handler only resolves
/// the cast dialog and the server-driven revelation animation variant.
/// </summary>
public class Attack72Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Revelation");

        int variantIndex = ParseVariantIndex(attackResult, attacker.styleId);
        yield return animations.PlayRevelationAnimation(attacker.transform, variantIndex);

        yield return showDialog($"God is with {attacker.cardName}");
    }

    private static int ParseVariantIndex(string attackResult, int fallbackStyleId)
    {
        if (!string.IsNullOrEmpty(attackResult) && attackResult.StartsWith("variant:"))
        {
            string rawValue = attackResult.Substring("variant:".Length);
            if (int.TryParse(rawValue, out int parsedVariant))
            {
                return parsedVariant;
            }
        }

        return RevelationRules.GetVariantIndex(fallbackStyleId);
    }
}
