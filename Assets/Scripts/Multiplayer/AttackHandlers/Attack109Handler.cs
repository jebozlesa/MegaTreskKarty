using System.Collections;

/// <summary>
/// Attack ID 109: Arquebus
/// Server resolves exploded/hit/miss branch. Client renders the selected branch only.
/// </summary>
public class Attack109Handler
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
        yield return showDialog($"{attacker.cardName} uses Arquebus");

        if (attackResult == "exploded")
        {
            yield return animations.PlayArquebusExplosionAnimation(attacker.transform);
            yield return showDialog("Arquebus exploded");
            yield break;
        }

        if (attackResult == "hit")
        {
            yield return animations.PlayArquebusShotAnimation(attacker.transform, defender.transform, true);
            yield return showDialog($"Bang! {attacker.cardName} hits target");
            yield break;
        }

        yield return animations.PlayArquebusShotAnimation(attacker.transform, defender.transform, false);
        yield return showDialog("Bang! aaaand miss");
    }
}
