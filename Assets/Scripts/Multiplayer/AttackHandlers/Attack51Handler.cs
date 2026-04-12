using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attack ID 51: Depression
/// Attack/result visuals are orchestrated here; stat/effect playback stays in shared timeline playback.
/// </summary>
public class Attack51Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        AttackAnimations animations,
        System.Func<string, IEnumerator> showDialog,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null)
    {
        yield return showDialog($"{attacker.cardName} uses Depression");
        yield return animations.PlayDepressionAnimation(attacker.transform);
        yield return showDialog($"{attacker.cardName} is depressed");

        bool depressionApplied = effectsApplied != null && effectsApplied.Exists(e => e["type"].ToString() == "13");
        bool artStarted = attackResult == "depressed_inspired" || attackResult == "inspired";

        if (depressionApplied)
        {
            yield return animations.PlayDepressionStartAnimation(defender.transform);
            yield return showDialog($"{defender.cardName} feels bad for enemy");
        }
        else
        {
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog("No effect on enemy");
        }

        if (artStarted)
        {
            yield return animations.PlayArtInspirationStartAnimation(attacker.transform);
            yield return showDialog($"{attacker.cardName} is empowered by muse");
        }
    }
}
