using System.Collections;

/// <summary>
/// Attack ID 52: Self Isolation
/// Attack/result visuals are orchestrated here; shared stat playback renders the DEF buff.
/// </summary>
public class Attack52Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses SelfIsolation");
        yield return animations.PlaySelfIsolationAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} feels happy alone");

        if (attackResult == "isolated_inspired")
        {
            yield return animations.PlayArtInspirationStartAnimation(attacker.transform);
            yield return showDialog($"{attacker.cardName} is empowered by muse");
        }
    }
}
