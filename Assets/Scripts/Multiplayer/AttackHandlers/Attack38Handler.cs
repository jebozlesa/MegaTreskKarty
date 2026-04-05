using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 38: Marxism
/// No damage. Shared battle playback applies the charisma stat changes.
/// </summary>
public class Attack38Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Marxism");
        yield return animations.PlayMarxismAnimation(attacker.transform);

        if (attackResult == "inspired")
        {
            yield return showDialog($"{attacker.cardName} explains Marxism and grows more persuasive");
            yield break;
        }

        yield return showDialog($"{attacker.cardName} explains Marxism");
    }
}
