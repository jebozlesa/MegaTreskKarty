using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 105: Shuriken
/// Server resolves hit/miss and applies the speed debuff on hit. Client renders the selected branch.
/// </summary>
public class Attack105Handler
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
        string attackResult = null)
    {
        bool hit = attackResult == "hit";
        if (string.IsNullOrEmpty(attackResult))
        {
            hit = damage > 0;
        }

        yield return showDialog($"{attacker.cardName} uses Shuriken");
        yield return animations.PlayShurikenAnimation(attacker.transform, defender.transform, hit);

        if (!hit)
        {
            yield return showDialog("throw missed");
            yield break;
        }

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

        yield return showDialog($"{attacker.cardName} hrows stars");
    }
}
