using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 100: Shield Bash
/// Shared battle playback owns the defender damage and attacker defense buff.
/// This handler only resolves the cast animation and dialog.
/// </summary>
public class Attack100Handler
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
        yield return showDialog($"{attacker.cardName} uses Shield Bash");
        yield return animations.PlayShieldBashAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} smashes with his shield");
    }
}
