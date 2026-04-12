using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 61: Expeditionary Assault
/// Success/fail branch is server-driven; shared stat playback renders the self-buffs.
/// </summary>
public class Attack61Handler
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
        bool succeeded = attackResult == "loot";

        yield return showDialog($"{attacker.cardName} uses Expeditionary Assault");
        yield return animations.PlayExpeditionaryAssaultAnimation(attacker.transform);

        if (succeeded)
        {
            yield return animations.PlayExpeditionaryAssaultSuccessAnimation(attacker.transform, attacker.transform);
            yield return showDialog($"{attacker.cardName} gets useful stuff");
            yield break;
        }

        yield return animations.PlayExpeditionaryAssaultFailAnimation(attacker.transform);

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

        yield return showDialog($"{attacker.cardName} almost died on sea");
    }
}
