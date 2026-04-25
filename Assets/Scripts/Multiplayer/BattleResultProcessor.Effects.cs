using System.Collections;
using UnityEngine;

public partial class BattleResultProcessor
{
    /// <summary>
    /// Play wake-up animation after a sleep effect is removed.
    /// </summary>
    private IEnumerator PlayWakeUpAnimation(Kard card, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning($"[WAKE_UP] {cardOwner} card ({card.cardName}) is waking up!");

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            yield return StartCoroutine(animations.PlaySleepEndAnimation(card.transform));
        }

        yield return StartCoroutine(ShowDialog($"{card.cardName} wakes up!"));
    }

    /// <summary>
    /// PrehrA recovery animAciu (Asceticism duration = 0)
    /// V10: PridanA pre Asceticism effect
    /// </summary>
    private IEnumerator PlayRecoveryAnimation(Kard card, string cardDescription)
    {
        Debug.LogWarning(
            $"[RECOVERY] {cardDescription} ({card.cardName}) recovered from Asceticism!"
        );

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            Debug.LogWarning("[RECOVERY] Playing PlayAscetismEndAnimation...");
            yield return StartCoroutine(animations.PlayAscetismEndAnimation(card.transform));
            Debug.LogWarning("[RECOVERY] PlayAscetismEndAnimation finished");
        }
        else
        {
            Debug.LogError("[RECOVERY] AttackAnimations is NULL!");
        }

        string asceticismEffectName = BattleEffectPlayback.GetEffectName(2);
        Debug.LogWarning($"[RECOVERY] Effect name for type 2: {asceticismEffectName}");

        if (!string.IsNullOrEmpty(asceticismEffectName))
        {
            Debug.LogWarning(
                $"[RECOVERY] Attempting to remove {asceticismEffectName} icon from {card.cardName}..."
            );
            yield return StartCoroutine(card.RemoveEffectIcon(asceticismEffectName));
            Debug.LogWarning(
                $"[RECOVERY] Removed {asceticismEffectName} icon from {card.cardName}"
            );
        }
        else
        {
            Debug.LogError("[RECOVERY] asceticismEffectName is NULL or EMPTY!");
        }

        yield return StartCoroutine(ShowDialog($"{card.cardName} feels blessed again!"));
    }

    /// <summary>
    /// PrehrA self-damage animAciu (Asceticism blocking penalty)
    /// V10: PridanA pre Asceticism effect
    /// </summary>
    private IEnumerator PlaySelfDamageAnimation(Kard card, int damage, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning(
            $"[SELF_DAMAGE] {cardOwner} card ({card.cardName}) takes {damage} self-damage!"
        );

        yield return StartCoroutine(
            BattleValuePlayback.PlayDamage(
                card,
                damage,
                isMyCard,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            )
        );

        yield return StartCoroutine(ShowDialog($"{card.cardName} suffers -{damage} HP!"));
    }

    /// <summary>
    /// PrehrA SINGLE Bleed continue animAciu + damage (pre jeden Bleed effect)
    /// V11.2: PRIORITY 1 - Individual Bleed damage processing (separate animations for each Bleed)
    /// </summary>
    private IEnumerator PlayBurnAnimation(Kard card, int damage, bool isMyCard)
    {
        if (card == null || damage <= 0)
        {
            yield break;
        }

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            yield return StartCoroutine(animations.PlayBurnContinueAnimation(card.transform));
        }

        yield return StartCoroutine(
            BattleValuePlayback.PlayDamage(
                card,
                damage,
                isMyCard,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            )
        );

        yield return StartCoroutine(ShowDialog($"{card.cardName} is on fire! -{damage} HP"));
    }

    private IEnumerator PlaySingleBleedAnimation(Kard card, int damage, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning(
            $"[SINGLE_BLEED] {cardOwner} card ({card.cardName}) takes {damage} Bleed damage!"
        );

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            yield return StartCoroutine(
                animations.PlayBleedContinueAnimation(card.transform, damage)
            );
        }

        yield return StartCoroutine(
            BattleValuePlayback.PlayDamage(
                card,
                damage,
                isMyCard,
                cardAnimator,
                playerLifeBar,
                enemyLifeBar
            )
        );

        yield return StartCoroutine(ShowDialog($"{card.cardName} is bleeding! -{damage} HP"));
    }

    /// <summary>
    /// PrehrA Exposure animAciu - damage alebo removal
    /// V11.2: PRIORITY 1 - Exposure processing (escalating radiation damage)
    /// </summary>
    private IEnumerator PlayExposureAnimation(Kard card, int damage, bool removed, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        AttackAnimations animations = attackComponent?.attackAnimations;

        if (removed)
        {
            Debug.LogWarning(
                $"adZ [EXPOSURE] {cardOwner} card ({card.cardName})'s irradiation is gone! (20% removal proc)"
            );

            if (animations != null)
            {
                yield return StartCoroutine(animations.PlayExposureEndAnimation(card.transform));
            }

            string exposureEffectName = BattleEffectPlayback.GetEffectName(4);
            if (!string.IsNullOrEmpty(exposureEffectName))
            {
                yield return StartCoroutine(card.RemoveEffectIcon(exposureEffectName));
                Debug.LogWarning(
                    $"adZ [EXPOSURE] Removed {exposureEffectName} icon from {card.cardName}"
                );
            }

            yield return StartCoroutine(ShowDialog($"{card.cardName}'s irradiation is gone"));
        }
        else if (damage > 0)
        {
            Debug.LogWarning(
                $"adZ [EXPOSURE] {cardOwner} card ({card.cardName}) is irradiated! -{damage} HP"
            );

            if (animations != null)
            {
                yield return StartCoroutine(animations.PlayExposureAnimation(card.transform));
            }

            yield return StartCoroutine(
                BattleValuePlayback.PlayDamage(
                    card,
                    damage,
                    isMyCard,
                    cardAnimator,
                    playerLifeBar,
                    enemyLifeBar
                )
            );

            yield return StartCoroutine(ShowDialog($"{card.cardName} is irradiated"));
        }
    }
}
