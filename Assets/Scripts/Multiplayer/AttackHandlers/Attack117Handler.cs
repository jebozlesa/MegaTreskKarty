using System.Collections;

/// <summary>
/// Attack ID 117: Act a fool
/// Shared stat playback owns all defender stat deltas; this handler renders only the cast branch visuals.
/// </summary>
public class Attack117Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Act a fool");
        yield return animations.PlayActAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} behaves like insane");

        if (attackResult == "scared")
        {
            yield return animations.PlayAnimationTerrified(defender.transform);
            yield return showDialog($"{defender.cardName} is scared");
            yield break;
        }

        yield return animations.PlayAnimationImpressed(defender.transform);
        yield return showDialog($"{defender.cardName} is impressed");
    }
}
