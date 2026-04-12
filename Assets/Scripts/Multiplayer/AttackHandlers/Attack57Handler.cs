using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 57: Take Off
/// Server decides whether the launch succeeds or crashes.
/// </summary>
public class Attack57Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        int attackerSelfDamage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        bool tookOff = attackResult == "takeoff";
        if (string.IsNullOrEmpty(attackResult))
        {
            tookOff = attackerSelfDamage <= 0;
        }

        yield return showDialog($"{attacker.cardName} uses Take Off");

        if (tookOff)
        {
            yield return animations.PlayTakeOffAnimation(attacker.transform);
            yield return showDialog($"{attacker.cardName} took off");
            yield break;
        }

        yield return animations.PlayTakeOffCrashAnimation(attacker.transform);

        if (attackerSelfDamage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                attacker,
                attackerSelfDamage,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} crashes");
    }
}
