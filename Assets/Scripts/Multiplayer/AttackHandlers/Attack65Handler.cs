using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attack ID 65: Illusion
/// No damage. Always grants the attacker CHA +1 and confuses the defender unless already confused.
/// Shared stat/effect playback owns the actual stat mutation and icon updates.
/// </summary>
public class Attack65Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Illusion");
        yield return animations.PlayIllusionAnimation(attacker.transform, Random.Range(0, 5));
        if (attackResult == "confused")
        {
            yield return animations.PlayConfusionStartAnimation(defender.transform);
            yield return showDialog($"{attacker.cardName}'s illusion confuses the enemy");
            yield break;
        }
        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog($"{defender.cardName} is already confused");
    }
}
