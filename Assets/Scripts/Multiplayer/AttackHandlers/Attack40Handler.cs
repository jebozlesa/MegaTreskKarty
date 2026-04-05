using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 40: Wireless Charger
/// Self-heal and self ATT buff are applied by shared battle playback.
/// </summary>
public class Attack40Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        int healAmount,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog
    )
    {
        yield return showDialog($"{attacker.cardName} uses Wireless Charger");
        yield return animations.PlayWirelessChargerAnimation(attacker.transform);

        if (healAmount > 0)
        {
            yield return BattleValuePlayback.PlayHeal(
                attacker,
                healAmount,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} is charging wirelessly!!!");
    }
}
