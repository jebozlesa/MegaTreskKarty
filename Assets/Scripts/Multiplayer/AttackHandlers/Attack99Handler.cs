using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 99: Gladius
/// Shared damage playback handles the HP update. This handler owns the cast animation and dialog.
/// </summary>
public class Attack99Handler
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
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null
    )
    {
        yield return showDialog($"{attacker.cardName} uses Gladius");
        yield return animations.PlayGladiusAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                defender,
                damage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} attacks with gladius");
    }
}
