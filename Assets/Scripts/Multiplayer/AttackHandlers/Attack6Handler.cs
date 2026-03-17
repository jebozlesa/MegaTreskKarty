using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 6: Water To Wine
/// Damage: 0 (self-buff only)
/// Effect: Permanent stat changes to attacker
///   - Attack: +2
///   - Strength: +1
///   - Defense: -1
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
        
        // Buff effect - apply stat changes
        Debug.LogWarning($"[WINE] [WATER TO WINE] {attacker.cardName} transforms water to wine!");
        attacker.HandleAttack(2);   // +2 attack
        attacker.HandleStrength(1); // +1 strength
        attacker.HandleDefense(-1); // -1 defense
        yield return showDialog($"{attacker.cardName} changes his water to wine");
    }
}
