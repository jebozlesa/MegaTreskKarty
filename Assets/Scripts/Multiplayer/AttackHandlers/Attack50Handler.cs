using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Attack ID 50: Continental Blockade
/// Applies Blockade through the standard effectsApplied flow; shared battle playback owns the ongoing damage/stat/block steps.
/// </summary>
public class Attack50Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Continental Blockade");
        yield return animations.PlayContinentalBlockadeAnimation(attacker.transform, defender.transform);
        bool blockadeApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "12");
        if (blockadeApplied)
        {
            yield return showDialog($"{attacker.cardName} Enforcing the blockade");
            yield break;
        }
        if (attackResult == "blocked_trade")
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog($"The blockade is already in place");
        }
    }
}
