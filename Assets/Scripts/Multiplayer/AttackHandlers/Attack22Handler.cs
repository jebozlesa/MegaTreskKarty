using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 22: Drink Wine
/// Self-heal for 1..2 + CHA/4, self-buff STR +1, 10% backfire Sleep on attacker.
/// Stat changes are applied by shared battle playback/timeline flow.
/// </summary>
public class Attack22Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int healAmount,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> attackerEffects)
    {
        yield return showDialog($"{attacker.cardName} uses Drink Wine");
        yield return animations.PlayDrinkWineAnimation(attacker.transform);

        if (healAmount > 0)
        {
            Debug.LogWarning($"[DRINK_WINE] {attacker.cardName} heals {healAmount} HP");
            yield return BattleValuePlayback.PlayHeal(
                attacker,
                healAmount,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} is getting slightly drunk");

        if (attackerEffects != null && attackerEffects.Count > 0)
        {
            var sleepEffect = attackerEffects.Find(e => e["type"].ToString() == "3");
            if (sleepEffect != null)
            {
                Debug.LogWarning($"[DRINK_WINE] BACKFIRE! {attacker.cardName} falls asleep");
                yield return animations.PlayDrunkAnimation(attacker.transform);
                yield return showDialog($"{attacker.cardName} falls asleep");
            }
        }
    }
}
