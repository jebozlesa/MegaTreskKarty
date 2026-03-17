using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Attack 11: Scientific Lecture
/// Effect: Knowledge check -> +1 Knowledge buff OR Sleep (boredom)
/// No damage
/// </summary>
public class Attack11Handler
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
        System.Func<string, IEnumerator> showDialog,
        string attackResult = null)  // [OK] Server tells us what happened
    {
        yield return showDialog($"{attacker.cardName} explaining science!!!");
        
        // Play Scientific Lecture animation on attacker
        yield return animations.PlayScientificLectureAnimation(attacker.transform);
        
        // [OK] If attackResult is null/empty, attacker was blocked BEFORE executing attack
        // (e.g. defender has Sleep/Asceticism) -> Show "no effect" after lecture animation
        if (string.IsNullOrEmpty(attackResult))
        {
            Debug.Log($"[Attack11] Attack blocked (defender has effect) -> No effect");
            yield return animations.PlayAnimationNotEffective(defender.transform);
            yield return showDialog("Attack has no effect");
            yield break;
        }
        
        // [OK] CLIENT IS DUMB RENDERER - Server decides which animation to play
        switch (attackResult)
        {
            case "impressed":
                // SUCCESS: Defender is interested, gains +1 Knowledge
                yield return animations.PlayAnimationImpressed(defender.transform);
                yield return showDialog($"{defender.cardName} is interested in science");
                Debug.Log($"[Attack11] {defender.cardName} is interested -> +1 Knowledge");
                break;
                
            case "bored":
                // FAILURE: Defender is bored, will fall asleep
                yield return animations.PlayBoredomAnimation(defender.transform);
                yield return showDialog($"{defender.cardName} falls asleep by boredom");
                Debug.Log($"[Attack11] {defender.cardName} is bored -> Sleep");
                break;
                
            case "blocked":
                // No effect (defender already has Sleep/Asceticism)
                yield return animations.PlayAnimationNotEffective(defender.transform);
                yield return showDialog("Attack has no effect");
                Debug.Log($"[Attack11] No effect (blocked by existing effect)");
                break;
                
            default:
                // Fallback (should never happen)
                Debug.LogWarning($"[Attack11] Unknown attackResult: {attackResult}");
                yield return animations.PlayAnimationNotEffective(defender.transform);
                yield return showDialog("Something went wrong...");
                break;
        }
    }
}
