using System.Collections;

/// <summary>
/// Attack ID 77: Buffalo Horns
/// Initial cast only; follow-up self-damage and strike turns are handled through ongoing-action timeline steps.
/// </summary>
public class Attack77Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Buffalo Horns");
        yield return animations.PlayBuffaloHornsAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} Launching the maneuver!");
    }
}
