using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 29: Diplomacy
/// Server-driven DEF debuffs are rendered through shared timeline/stat playback.
/// Handler only plays attack animation and dialog.
/// </summary>
public class Attack29Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Diplomacy");
        yield return animations.PlayDiplomacyAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{attacker.cardName} made diplomatic gesture");
    }
}
