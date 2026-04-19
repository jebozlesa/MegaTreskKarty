using System.Collections;

/// <summary>
/// Attack ID 108: Oriental Spice
/// Server resolves poison application. Client renders poison start or not-effective branch.
/// </summary>
public class Attack108Handler
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
        yield return showDialog($"{attacker.cardName} uses Oriental Spice");
        yield return animations.PlayOrientalSpiceAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{attacker.cardName} offers some spices to enemy");

        if (attackResult == "poisoned")
        {
            yield return animations.PlayPoisonStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} is poisoned by spices");
            yield break;
        }

        yield return animations.PlayAnimationNotEffective(defender.transform);
        yield return showDialog("Poison has no effect");
    }
}
