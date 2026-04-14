using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attack ID 75: Iambic Pentameter
/// No direct damage. Can confuse the defender, reduce defender KNO, or heal the defender for 1.
/// Shared stat/effect playback owns the debuff/effect mutation; this handler adds the special heal branch.
/// </summary>
public class Attack75Handler
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
        yield return showDialog($"{attacker.cardName} uses Iambic Pentameter");
        yield return animations.PlayIambicPentameterAnimation(attacker.transform, defender.transform);

        if (attackResult == "confused")
        {
            yield return animations.PlayConfusionStartAnimation(defender.transform);
            yield return showDialog($"{attacker.cardName}'s poetry confuses the enemy");
            yield break;
        }

        if (attackResult == "liked-it")
        {
            yield return BattleValuePlayback.PlayHeal(
                defender,
                1,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
            yield return showDialog($"{defender.cardName} likes this poetry");
            yield break;
        }

        yield return showDialog($"{defender.cardName} does not understand");
    }
}
