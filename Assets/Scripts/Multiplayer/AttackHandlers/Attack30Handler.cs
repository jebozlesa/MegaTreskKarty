using System.Collections;

/// <summary>
/// Attack ID 30: Siege
/// Delayed turns are handled by ongoing-action timeline steps; this handler only covers the initial cast.
/// </summary>
public class Attack30Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Siege");
        yield return animations.PlaySiegeAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} is building watch tower");
    }
}
