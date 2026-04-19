using System.Collections;
using UnityEngine;

/// <summary>
/// Attack ID 87: Espionage
/// Shared stat playback owns the DEF/SPD debuffs; the handler only drives cast animation and dialog.
/// </summary>
public class Attack87Handler
{
    public static IEnumerator Execute(
        Kard attacker,
        Kard defender,
        int damage,
        bool isMyAttack,
        AttackAnimations animations,
        MultiplayerCardAnimator cardAnimator,
        HealthBar playerLifeBar,
        HealthBar enemyLifeBar,
        System.Func<string, IEnumerator> showDialog)
    {
        yield return showDialog($"{attacker.cardName} uses Espionage");
        yield return animations.PlayEspionageAnimation(attacker.transform, defender.transform);
        yield return showDialog($"{attacker.cardName} spies on enemy");
    }
}
