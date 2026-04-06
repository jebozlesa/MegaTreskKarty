using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 47: Great Army
/// Self-buff only. Shared battle playback applies ATT/STR and optional DEF changes.
/// </summary>
public class Attack47Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Great Army");
        yield return animations.PlayGreatArmyAnimation(attacker.transform);

        if (attackResult == "fortified")
        {
            yield return showDialog($"{attacker.cardName} is assembling an army and fortifying the ranks");
            yield break;
        }

        yield return showDialog($"{attacker.cardName} is assembling an army");
    }
}
