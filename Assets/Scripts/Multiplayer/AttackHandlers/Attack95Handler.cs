using System.Collections;

/// <summary>
/// Attack ID 95: Outlaw Band
/// Server resolves either the raid branch or the fail/self-damage branch. Client only
/// replays the matching animation and shared damage playback.
/// </summary>
public class Attack95Handler
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
        yield return showDialog($"{attacker.cardName} uses Outlaw Band");

        if (attackResult == "fled")
        {
            yield return animations.PlayOutlawBandFailAnimation(defender.transform);

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

            yield return showDialog("The outlaw band run in fear");
            yield break;
        }

        yield return animations.PlayOutlawBandAnimation(defender.transform);

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

        yield return showDialog($"{attacker.cardName}'s band attacks");
    }
}
