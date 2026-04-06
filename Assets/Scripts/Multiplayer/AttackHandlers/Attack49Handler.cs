using System.Collections;

/// <summary>
/// Attack ID 49: Double Envelopment
/// Delayed turns are handled by ongoing-action timeline steps; this handler only covers the initial cast.
/// </summary>
public class Attack49Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Double Envelopment");
        yield return animations.PlayDoubleEnvelopmentAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} Launching the maneuver!");
    }
}
