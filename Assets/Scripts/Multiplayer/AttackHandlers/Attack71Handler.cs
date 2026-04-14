using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 71: Battle Cry
/// Self-buff only. Shared battle playback applies ATT/STR changes.
/// </summary>
public class Attack71Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses BattleCry");
        yield return animations.PlayBattleCryAnimation(attacker.transform);

        if (attackResult == "inspired")
        {
            yield return showDialog($"{attacker.cardName} roared into battle with renewed fury");
            yield break;
        }

        yield return showDialog($"{attacker.cardName} roared into battle");
    }
}
