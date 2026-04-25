using System.Collections;
using UnityEngine;

public partial class BattleResultProcessor
{
    /// <summary>
    /// PrehrA blocking animAciu podAla typu effectu
    /// VOLA SA keAZ karta mA aktAvny blocking effect (Sleep, Stun, Freeze, atAZ.)
    /// </summary>
    private IEnumerator PlayBlockAnimation(Kard card, int? blockedBy, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        string effectName = blockedBy.HasValue
            ? BattleEffectPlayback.GetEffectName(blockedBy.Value)
            : "unknown effect";
        Debug.LogWarning(
            $"zdZ [BLOCK] {cardOwner} card ({card.cardName}) blocked by effect type {blockedBy} ({effectName})!"
        );

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations == null)
        {
            Debug.LogError("[BattleResultProcessor] AttackAnimations not found!");
            yield break;
        }

        switch (blockedBy)
        {
            case 2: // ASCETICISM
                Debug.LogWarning(
                    "[ASCETICISM_ONGOING] Playing HURT ITSELF animation (ongoing Asceticism blocking)"
                );
                yield return StartCoroutine(
                    animations.PlayConfusionHurtItselfAnimation(card.transform)
                );
                yield return StartCoroutine(ShowDialog($"{card.cardName} practizes asceticism..."));
                break;

            case 3: // SLEEP
                Debug.LogWarning(
                    "[SLEEP_ONGOING] Playing sleep animation (ongoing Sleep, not initial)"
                );
                yield return StartCoroutine(animations.PlaySleepAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is sleeping..."));
                break;

            case 27: // KNOCKOUT
                Debug.LogWarning(
                    "[KNOCKOUT_ONGOING_AS_SLEEP] Playing sleep animation (ongoing Knockout)"
                );
                yield return StartCoroutine(animations.PlaySleepAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is sleeping..."));
                break;

            case 8: // ELECTRICITY
                Debug.LogWarning("[ELECTRICITY_ONGOING] Playing electricity block animation");
                yield return StartCoroutine(animations.PlayElectricityAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} cannot move"));
                break;

            case 9: // TETHER
                Debug.LogWarning("[TETHER_ONGOING] Playing tether block animation");
                yield return StartCoroutine(animations.PlayTetherAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is locked"));
                break;

            case 12: // BLOCKADE
                Debug.LogWarning("[BLOCKADE_ONGOING] Playing blockade hold animation");
                yield return StartCoroutine(animations.PlayBlocadeWaitAnimation(card.transform));
                yield return StartCoroutine(ShowDialog("The blockade holds strong"));
                break;

            default:
                Debug.LogWarning(
                    $"[PlayBlockAnimation] Unknown effect type {blockedBy}, using default message"
                );
                yield return StartCoroutine(ShowDialog($"{card.cardName} cannot attack!"));
                break;
        }
    }

    /// <summary>
    /// Returns the attack name for an attackId (for dialog text).
    /// </summary>
    private string GetAttackName(int attackId)
    {
        return AttackRegistry.GetAttackName(attackId);
    }

    /// <summary>
    /// ZobrazA dialAg
    /// </summary>
    private IEnumerator ShowDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message;
        }

        yield return new WaitForSeconds(1.5f);
    }
}
