using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Attack ID 115: Cubism
/// Shared effect playback owns Confusion application; this handler resolves the cast branch visuals
/// and the defender-heal fallback.
/// </summary>
public class Attack115Handler
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
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Cubism");
        yield return animations.PlayCubismAnimation(attacker.transform);
        if (attackResult == "confused")
        {
            yield return animations.PlayConfusionStartAnimation(defender.transform);
            yield return showDialog($"{attacker.cardName}'s picture confuses the enemy");
            yield break;
        }
        if (healAmount > 0)
        {
            yield return BattleValuePlayback.PlayHeal(
                defender,
                healAmount,
                !isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }
        yield return showDialog($"{defender.cardName} likes this art");
    }
}
