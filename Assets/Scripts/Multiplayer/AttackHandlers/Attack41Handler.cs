using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 41: Experiment
/// Bilateral random damage plus attacker KNO +2 via shared stat playback.
/// </summary>
public class Attack41Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        int attackerSelfDamage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog
    )
    {
        yield return showDialog($"{attacker.cardName} uses Experiment");
        yield return animations.PlayExperimentAnimation(attacker.transform);

        if (damage > 0 && cardAnimator != null)
        {
            attacker.StartCoroutine(cardAnimator.AnimateDamage(defender, damage));
        }

        if (attackerSelfDamage > 0 && cardAnimator != null)
        {
            attacker.StartCoroutine(cardAnimator.AnimateDamage(attacker, attackerSelfDamage));
        }

        if (damage > 0 || attackerSelfDamage > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (damage > 0)
        {
            BattleValuePlayback.ApplyDamage(defender, damage);
        }

        if (attackerSelfDamage > 0)
        {
            BattleValuePlayback.ApplyDamage(attacker, attackerSelfDamage);
        }

        if (damage > 0 || attackerSelfDamage > 0)
        {
            BattleValuePlayback.SyncHealthBar(attacker, isMyAttack, playerLifeBar, enemyLifeBar);
            BattleValuePlayback.SyncHealthBar(defender, !isMyAttack, playerLifeBar, enemyLifeBar);
        }

        yield return showDialog($"{attacker.cardName} is trying something");
    }
}
