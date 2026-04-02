using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack 12: Chi Sau (Sticky Hands)
/// Bruce Lee's rapid hand technique with variable strikes
/// Damage: multiply - (defender speed / 4)
/// where multiply = random(1, attacker speed)
/// </summary>
public class Attack12Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} attacks with sticky hands!");

        int visualMultiply = Mathf.Clamp(damage, 1, 9);
        yield return animations.PlayChiSauAnimation(attacker.transform, defender.transform, visualMultiply);

        yield return AttackPlaybackShared.PlayStandardTargetDamage(
            defender,
            damage,
            isMyAttack,
            cardAnimator,
            playerLifeBar,
            enemyLifeBar
        );

        Debug.Log($"[Attack12] {attacker.cardName} -> Chi Sau ({visualMultiply} strikes) -> {defender.cardName}: {damage} damage");
    }
}
