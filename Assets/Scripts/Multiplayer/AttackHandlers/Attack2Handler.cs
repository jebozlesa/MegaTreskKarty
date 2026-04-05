using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack ID 2: Kick
/// Damage: (speed/3) - (defense/3), min 1
/// Effect: 20% +4 extra damage
/// </summary>
public class Attack2Handler
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
        yield return showDialog($"{attacker.cardName} uses Kick!");
        yield return animations.PlayKickAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            Debug.LogWarning($"[HIT] [KICK] {attacker.cardName} -> {defender.cardName}: {damage} damage");
            yield return BattleValuePlayback.PlayDamage(
                defender,
                damage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
            yield return showDialog($"Plesk! kick from {attacker.cardName}");
        }
    }
}


