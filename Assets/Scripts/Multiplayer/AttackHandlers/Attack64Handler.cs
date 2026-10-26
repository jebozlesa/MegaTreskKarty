using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 64: Handcuff Escape
/// Cast first; shared timeline cleanup precedes the registered effect-application visual.
/// </summary>
public class Attack64Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Danger Escape");
        yield return animations.PlayHandcuffEscapeAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} slips outta trouble");
    }

    public static IEnumerator PlayEffectApplication(
        Kard defender,
        int effectType,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        if (effectType != 17) yield break;
        yield return animations.PlayConfusionStartAnimation(defender.transform);
        yield return showDialog($"{defender.cardName} is confused");
    }
}
