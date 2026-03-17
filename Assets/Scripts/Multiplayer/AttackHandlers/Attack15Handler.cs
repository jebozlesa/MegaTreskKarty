using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack ID 15: Sing
/// Base effect: defender Attack -1, defender Defense -1
/// Extra: charisma chance can apply Calm (effect 21) and additional debuffs
/// </summary>
public class Attack15Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied)
    {
        yield return showDialog($"{attacker.cardName} uses Sing!");
        yield return animations.PlaySingAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} sings a banger");

        if (effectsApplied != null && effectsApplied.Count > 0)
        {
            var calmEffect = effectsApplied.Find(e =>
                e.ContainsKey("type") && e["type"] != null && e["type"].ToString() == "21");

            if (calmEffect != null)
            {
                yield return animations.PlayCalmStartAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} is calm");
            }
        }
    }
}
