using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attack ID 53: Knife
/// Damage and bleed remain server-driven; this handler only owns presentation.
/// </summary>
public class Attack53Handler
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
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Knife");
        yield return animations.PlayKnifeAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} cuts enemy with knife");

        if (effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "1"))
        {
            yield return animations.PlayBleedStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is wounded");
        }
    }
}
