using System.Collections;

/// <summary>
/// Attack ID 96: Flintlock Pistol
/// Initial cast only; reload progress and final shot are handled through ongoing-action timeline steps.
/// </summary>
public class Attack96Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Flintlock Pistol");
        yield return animations.PlayFlintlockPistolLoadingAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} is loading the pistol");
    }
}
