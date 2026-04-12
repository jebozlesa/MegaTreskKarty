using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 58: Air Strike
/// Server decides hit-count and crit outcome; the handler replays the same sequence in multiplayer.
/// </summary>
public class Attack58Handler
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
        yield return showDialog($"{attacker.cardName} uses Air Strike");

        ParseAttackResult(attackResult, out int hitCount, out bool critical);
        int displayedHitCount = Mathf.Max(0, hitCount);
        yield return animations.PlayAirStrikeAnimation(attacker.transform, defender.transform, displayedHitCount);

        int baseDamage = Mathf.Max(0, hitCount);
        if (baseDamage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                defender,
                baseDamage,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        int criticalDamage = Mathf.Max(0, damage - baseDamage);
        if (critical)
        {
            yield return animations.PlayAirStrikeCriticalAnimation(defender.transform);

            if (criticalDamage > 0)
            {
                yield return BattleValuePlayback.PlayDamage(
                    defender,
                    criticalDamage,
                    !isMyAttack,
                    cardAnimator,
                    playerLifeBar,
                    enemyLifeBar
                );
            }
        }

        yield return showDialog($"{attacker.cardName} strikes from air");
    }

    private static void ParseAttackResult(string attackResult, out int hitCount, out bool critical)
    {
        hitCount = 0;
        critical = false;

        if (string.IsNullOrEmpty(attackResult))
        {
            return;
        }

        string[] parts = attackResult.Split(':');
        if (parts.Length >= 3 && parts[0] == "hits")
        {
            int.TryParse(parts[1], out hitCount);
            critical = parts[2] == "crit";
        }
    }
}
