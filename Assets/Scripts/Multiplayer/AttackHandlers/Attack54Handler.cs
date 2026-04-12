using System.Collections;

/// <summary>
/// Attack ID 54: Autoportrait
/// Initial cast only; repeated self-painting turns are handled through ongoing-action timeline steps.
/// </summary>
public class Attack54Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Autoportrait");
        yield return animations.PlayAutoportraitAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} discovers himself through art");
    }
}
