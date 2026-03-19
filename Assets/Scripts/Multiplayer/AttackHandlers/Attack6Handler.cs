using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 6: Water To Wine
/// Damage: 0 (self-buff only)
/// Stat changes are applied by shared battle playback/timeline flow.
/// </summary>
public class Attack6Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Water To Wine!");
        yield return animations.PlayWaterToWineAnimation(attacker.transform);
        Debug.LogWarning($"[WINE] [WATER TO WINE] {attacker.cardName} transforms water to wine!");
        yield return showDialog($"{attacker.cardName} changes his water to wine");
    }
}
