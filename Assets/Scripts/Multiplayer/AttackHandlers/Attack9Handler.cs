using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 9: Radiation
/// Damage: NONE (pure effect attack)
/// Effect on Defender: 80% Exposure (permanent until 20% proc)
/// Self-Effect Risk: Knowledge-based chance for self-Exposure
///   - Low knowledge (0): 90% exposure chance (dangerous!)
///   - High knowledge (15+): 10% exposure chance (safe)
/// </summary>
public class Attack9Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Radiation!");
        yield return animations.PlayRadioactivityAnimation(attacker.transform);
        
        // No HP damage - pure effect attack!
        // Effects are handled in effect application section
        Debug.LogWarning($"☢️ [RADIATION] {attacker.cardName} uses Radiation on {defender.cardName} (no direct damage)");
    }
}
