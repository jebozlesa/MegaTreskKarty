using System.Collections;

/// <summary>
/// Attack ID 121: Shaolin Soccer
/// Server resolves the combo selection and sends the chosen sub-attacks in attackResult.
/// Client replays the chosen attacks through ComboAttackPlayback.
/// </summary>
public class Attack121Handler
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
        yield return showDialog($"{attacker.cardName} uses Shaolin Soccer");

        AttackExecutionContext comboContext = new AttackExecutionContext(
            attacker,
            defender,
            damage,
            0,
            isMyAttack,
            0,
            animations,
            cardAnimator,
            playerLifeBar,
            enemyLifeBar,
            showDialog,
            null,
            attackResult,
            null
        );

        yield return ComboAttackPlayback.PlaySequence(comboContext);
    }
}
