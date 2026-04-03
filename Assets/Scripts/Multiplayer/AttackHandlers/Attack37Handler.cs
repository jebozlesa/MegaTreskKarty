using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 37: Famine
/// Shared timeline playback owns the ongoing harvest heal and expiry visuals.
/// This handler only renders the cast branch based on attackResult.
/// </summary>
public class Attack37Handler
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
        yield return showDialog($"{attacker.cardName} uses Famine");

        if (attackResult == "no_effect")
        {
            yield return animations.PlayAnimationNotImpressed(defender.transform);
            yield return showDialog($"Be a human, {attacker.cardName}!");
            yield break;
        }

        yield return animations.PlayFamineAnimation(defender.transform);

        if (damage > 0)
        {
            yield return BattleValuePlayback.PlayDamage(
                defender,
                damage,
                isMyAttack,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            );
        }

        yield return showDialog($"{attacker.cardName} caused a famine");
    }
}

