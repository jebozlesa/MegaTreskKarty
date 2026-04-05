using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 5: Crusade
/// Damage: 5 + (strength/4) - (defense/4), min 1
/// Effect: (attack/50)% chance for -2 defense debuff
/// </summary>
public class Attack5Handler
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
        yield return showDialog($"{attacker.cardName} uses Crusade!");
        yield return animations.PlayCrusadeAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            Debug.LogWarning($"[HIT] [CRUSADE] {attacker.cardName} -> {defender.cardName}: {damage} damage");
            yield return BattleValuePlayback.PlayDamage(
                defender,
                damage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
            yield return showDialog("In the name of Christ!!! damage was done");
        }
    }
}


