using System.Collections;

/// <summary>
/// Attack ID 110: Pirate Raid
/// Server resolves success or repelled branch. Client renders the matching branch with shared playback.
/// </summary>
public class Attack110Handler
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
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Pirate Raid");

        if (attackResult == "repelled")
        {
            yield return animations.PlayPirateRaidFailAnimation(attacker.transform, defender.transform);

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

            yield return showDialog("Raiding didn't go well");
            yield break;
        }

        yield return animations.PlayPirateRaidAnimation(attacker.transform, defender.transform);

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

        yield return showDialog($"{attacker.cardName} raids the enemy");
    }
}
