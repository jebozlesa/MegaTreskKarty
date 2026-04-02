using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 36: Guerilla
/// Standard target damage uses AttackPlaybackShared; ATT debuff remains in shared timeline stat playback.
/// </summary>
public class Attack36Handler
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
        yield return showDialog($"{attacker.cardName} uses Guerilla");
        yield return animations.PlayGuerillaAnimation(attacker.transform, defender.transform);

        if (damage > 0)
        {
            yield return AttackPlaybackShared.PlayStandardTargetDamage(
                defender,
                damage,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} sends Guerillas");
    }
}
