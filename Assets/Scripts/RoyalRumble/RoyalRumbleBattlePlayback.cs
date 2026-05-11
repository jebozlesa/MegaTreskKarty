using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoyalRumbleBattlePlayback : MonoBehaviour
{
    private static bool VerboseRoyalRumblePlaybackLogs => true;

    [Header("Dependencies")]
    public Attack attackComponent;
    public MultiplayerCardAnimator cardAnimator;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    public TMP_Text dialogText;

    public IEnumerator PlayBattleAsync(
        RoyalRumbleBattleEnvelopeDto envelope,
        Kard playerCard,
        Kard enemyCard,
        string playerSelectedCardId)
    {
        if (envelope?.battleResult == null || playerCard == null || enemyCard == null)
        {
            yield break;
        }

        if (VerboseRoyalRumblePlaybackLogs)
        {
            Debug.LogWarning(
                $"[RoyalRumbleBattlePlayback] Battle start: first={DescribeAttacker(envelope.battleResult.firstAttacker, playerSelectedCardId)}, " +
                $"second={DescribeAttacker(envelope.battleResult.secondAttacker, playerSelectedCardId)}, cardDied={envelope.battleResult.cardDied}, " +
                $"winner={envelope.battleResult.winnerCardId}, loser={envelope.battleResult.loserCardId}"
            );
        }

        if (envelope.battleResult.firstAttacker != null)
        {
            yield return StartCoroutine(PlayAttackAsync(envelope.battleResult.firstAttacker, playerCard, enemyCard, playerSelectedCardId));
        }

        if (envelope.battleResult.secondAttacker != null)
        {
            if (ShouldSkipSecondAttackPlayback(envelope.battleResult))
            {
                if (VerboseRoyalRumblePlaybackLogs)
                {
                    Debug.LogWarning(
                        $"[RoyalRumbleBattlePlayback] Skipping second attacker playback because loser={envelope.battleResult.loserCardId} died before their turn."
                    );
                }
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
            yield return StartCoroutine(PlayAttackAsync(envelope.battleResult.secondAttacker, playerCard, enemyCard, playerSelectedCardId));
        }
    }

    private static bool ShouldSkipSecondAttackPlayback(BattleResultDto battleResult)
    {
        if (battleResult?.secondAttacker == null || !battleResult.cardDied)
        {
            return false;
        }

        return string.Equals(battleResult.loserCardId, battleResult.secondAttacker.cardId, StringComparison.Ordinal);
    }

    private IEnumerator PlayAttackAsync(
        BattleAttackerDto attackerResult,
        Kard playerCard,
        Kard enemyCard,
        string playerSelectedCardId)
    {
        if (attackerResult == null)
        {
            yield break;
        }

        bool isPlayerAttack = string.Equals(attackerResult.cardId, playerSelectedCardId, StringComparison.Ordinal);
        Kard attacker = isPlayerAttack ? playerCard : enemyCard;
        Kard defender = isPlayerAttack ? enemyCard : playerCard;

        if (attacker == null || defender == null)
        {
            yield break;
        }

        if (VerboseRoyalRumblePlaybackLogs)
        {
            Debug.LogWarning(
                $"[RoyalRumbleBattlePlayback] Playback attack: side={(isPlayerAttack ? "player" : "enemy")}, attacker={attacker.cardName}({attacker.cardId}), " +
                $"defender={defender.cardName}({defender.cardId}), attackId={attackerResult.attackId}, attackName={AttackRegistry.GetAttackName(attackerResult.attackId)}, " +
                $"damage={attackerResult.damageDealt}, heal={attackerResult.healAmount}, selfDamage={attackerResult.attackerSelfDamage}, " +
                $"blocked={attackerResult.blocked}, blockedBy={attackerResult.blockedBy?.ToString() ?? "none"}, " +
                $"remainingDuration={attackerResult.remainingDuration}, wokeUp={attackerResult.wokeUp}, ongoingConsumed={attackerResult.ongoingActionConsumed}, " +
                $"attackResult={attackerResult.attackResult ?? "none"}, effectsApplied={DescribeBattleEffects(attackerResult.effectsApplied)}, " +
                $"attackerEffectsApplied={DescribeBattleEffects(attackerResult.attackerEffectsApplied)}, effectsRemoved={DescribeRemovedEffects(attackerResult.effectsRemoved)}"
            );
        }

        if (attackerResult.ongoingActionConsumed && attackerResult.ongoingActionSteps != null && attackerResult.ongoingActionSteps.Count > 0)
        {
            if (VerboseRoyalRumblePlaybackLogs)
            {
                Debug.LogWarning(
                    $"[RoyalRumbleBattlePlayback] Ongoing playback: actionSteps={attackerResult.ongoingActionSteps.Count}, attacker={attacker.cardName}, attackId={attackerResult.attackId}"
                );
            }
            yield return StartCoroutine(PlayOngoingActionAsync(attackerResult, attacker, defender, isPlayerAttack));
            SyncEffectIcons(attacker, attackerResult.attackerEffectsApplied);
            SyncEffectIcons(defender, attackerResult.effectsApplied);
            yield return new WaitForSeconds(0.15f);
            yield break;
        }

        if (attackerResult.attackId <= 0)
        {
            yield break;
        }

        var context = new AttackExecutionContext(
            attacker,
            defender,
            attackerResult.damageDealt,
            attackerResult.healAmount,
            isPlayerAttack,
            attackerResult.attackerSelfDamage,
            attackComponent != null ? attackComponent.attackAnimations : null,
            cardAnimator,
            playerLifeBar,
            enemyLifeBar,
            ShowDialog,
            ToEffectDictionaryList(attackerResult.effectsApplied),
            attackerResult.attackResult,
            ToEffectDictionaryList(attackerResult.attackerEffectsApplied)
        );

        if (attackerResult.blocked)
        {
            yield return StartCoroutine(ShowDialog($"{attacker.cardName}'s attack was blocked."));
        }
        else
        {
            yield return AttackRegistry.ExecuteOrFallback(attackerResult.attackId, context);
        }

        foreach (int removedEffectType in attackerResult.effectsRemoved)
        {
            yield return StartCoroutine(
                BattleEffectPlayback.RemoveEffectIconOnly(defender, removedEffectType, !isPlayerAttack, attackComponent)
            );
        }

        foreach (BattleStatChangeDto statChange in attackerResult.statChanges ?? new List<BattleStatChangeDto>())
        {
            Kard target = string.Equals(statChange.targetCardId, attacker.cardId, StringComparison.Ordinal)
                ? attacker
                : string.Equals(statChange.targetCardId, defender.cardId, StringComparison.Ordinal)
                    ? defender
                    : null;

            if (target == null || string.IsNullOrWhiteSpace(statChange.statName) || statChange.amount == 0)
            {
                continue;
            }

            yield return StartCoroutine(
                BattleStatPlayback.PlayTimelineStatChange(target, statChange.amount, statChange.statName, cardAnimator)
            );
        }

        SyncEffectIcons(attacker, attackerResult.attackerEffectsApplied);
        SyncEffectIcons(defender, attackerResult.effectsApplied);

        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator PlayOngoingActionAsync(
        BattleAttackerDto attackerResult,
        Kard attacker,
        Kard defender,
        bool isPlayerAttack)
    {
        foreach (BattleOngoingActionStepDto step in attackerResult.ongoingActionSteps)
        {
            if (step == null)
            {
                continue;
            }

            switch (step.type)
            {
                case "OngoingActionProgress":
                    LogOngoingStep(step, attacker, defender);
                    yield return StartCoroutine(PlayOngoingActionProgress(step, attacker, defender));
                    break;

                case "OngoingActionResolved":
                    LogOngoingStep(step, attacker, defender);
                    yield return StartCoroutine(PlayOngoingActionResolved(step, attacker, defender));
                    break;

                case "OngoingActionCancelled":
                    LogOngoingStep(step, attacker, defender);
                    if (!string.IsNullOrWhiteSpace(step.note))
                    {
                        yield return StartCoroutine(ShowDialog(step.note));
                    }
                    break;

                case "SelfDamage":
                    LogOngoingStep(step, attacker, defender);
                    yield return BattleValuePlayback.PlayDamage(
                        attacker,
                        step.amount,
                        isPlayerAttack,
                        cardAnimator,
                        playerLifeBar,
                        enemyLifeBar
                    );
                    break;

                case "Damage":
                    LogOngoingStep(step, attacker, defender);
                    yield return BattleValuePlayback.PlayDamage(
                        defender,
                        step.amount,
                        !isPlayerAttack,
                        cardAnimator,
                        playerLifeBar,
                        enemyLifeBar
                    );
                    break;

                case "EffectRemoved":
                    LogOngoingStep(step, attacker, defender);
                    yield return StartCoroutine(
                        BattleEffectPlayback.RemoveEffectIconOnly(
                            attacker,
                            step.effectType,
                            isPlayerAttack,
                            attackComponent
                        )
                    );
                    break;
            }
        }
    }

    private IEnumerator PlayOngoingActionProgress(BattleOngoingActionStepDto step, Kard attacker, Kard defender)
    {
        AttackAnimations animations = attackComponent?.attackAnimations;
        string actionType = step.actionType ?? string.Empty;

        if (string.Equals(actionType, "buffaloHorns", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayBuffaloHornsContinueAnimation(attacker.transform);
            }
        }
        else if (string.Equals(actionType, "artInspiration", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayArtInspirationWaitAnimation(attacker.transform);
            }
        }
        else if (string.Equals(actionType, "autoportrait", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayAutoportraitAnimation(attacker.transform);
            }
        }
        else if (string.Equals(actionType, "flintlockPistol", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayFlintlockPistolLoadingAnimation(attacker.transform);
            }
        }
        else if (string.Equals(actionType, "trident", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayTridentAimAnimation(attacker.transform);
            }
        }

        if (!string.IsNullOrWhiteSpace(step.note))
        {
            yield return StartCoroutine(ShowDialog(step.note));
        }
    }

    private IEnumerator PlayOngoingActionResolved(BattleOngoingActionStepDto step, Kard attacker, Kard defender)
    {
        AttackAnimations animations = attackComponent?.attackAnimations;
        string actionType = step.actionType ?? string.Empty;

        if (string.Equals(actionType, "buffaloHorns", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null && defender != null)
            {
                yield return animations.PlayBuffaloHornsEndAnimation(attacker.transform, defender.transform);
            }
        }
        else if (string.Equals(actionType, "siege", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && defender != null)
            {
                yield return animations.PlaySiegeEndAnimation(defender.transform);
            }
        }
        else if (string.Equals(actionType, "doubleEnvelopment", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && defender != null)
            {
                yield return animations.PlayDoubleEnvelopAttackAnimation(defender.transform);
            }
        }
        else if (string.Equals(actionType, "artInspiration", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayArtInspirationEndAnimation(attacker.transform);
            }

            if (animations != null && defender != null)
            {
                yield return animations.PlayArtInspirationEndEnemyAnimation(defender.transform);
            }

            if (animations != null && attacker != null && defender != null)
            {
                yield return animations.PlayArtInspirationEndAttackAnimation(attacker.transform, defender.transform);
            }
        }
        else if (string.Equals(actionType, "autoportrait", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null)
            {
                yield return animations.PlayAutoportraitFinishAnimation(attacker.transform);
            }
        }
        else if (string.Equals(actionType, "flintlockPistol", StringComparison.OrdinalIgnoreCase))
        {
            bool hit = string.Equals(step.attackResult, "hit", StringComparison.OrdinalIgnoreCase);
            if (animations != null && attacker != null && defender != null)
            {
                yield return animations.PlayFlintlockPistolShotAnimation(attacker.transform, defender.transform, hit);
            }
        }
        else if (string.Equals(actionType, "trident", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && attacker != null && defender != null)
            {
                yield return animations.PlayTridentHitAnimation(attacker.transform, defender.transform);
            }
        }

        if (!string.IsNullOrWhiteSpace(step.note))
        {
            yield return StartCoroutine(ShowDialog(step.note));
        }
    }

    private void SyncEffectIcons(Kard card, List<BattleEffectDto> effects)
    {
        if (card == null || effects == null)
        {
            return;
        }

        foreach (BattleEffectDto effect in effects)
        {
            string effectName = BattleEffectPlayback.GetEffectName(effect.type);
            if (string.IsNullOrEmpty(effectName))
            {
                continue;
            }

            bool alreadyPresent = false;
            foreach (Transform child in card.effectIconContainer)
            {
                if (child != null && child.name.StartsWith(effectName + "Icon", StringComparison.Ordinal))
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (!alreadyPresent)
            {
                card.AddEffectIcon(effectName);
                card.RepositionEffectIcons();
            }
        }
    }

    private IEnumerator ShowDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message ?? string.Empty;
        }

        yield return new WaitForSeconds(0.75f);
    }

    private static List<Dictionary<string, object>> ToEffectDictionaryList(List<BattleEffectDto> effects)
    {
        var result = new List<Dictionary<string, object>>();
        if (effects == null)
        {
            return result;
        }

        foreach (BattleEffectDto effect in effects)
        {
            if (effect == null)
            {
                continue;
            }

            result.Add(
                new Dictionary<string, object>
                {
                    ["type"] = effect.type,
                    ["duration"] = effect.duration,
                    ["source"] = effect.source ?? string.Empty,
                    ["intensity"] = effect.intensity,
                }
            );
        }

        return result;
    }

    private static string DescribeAttacker(BattleAttackerDto attacker, string playerSelectedCardId)
    {
        if (attacker == null)
        {
            return "none";
        }

        string side = string.Equals(attacker.cardId, playerSelectedCardId, StringComparison.Ordinal) ? "player" : "enemy";
        return $"{side}:{attacker.cardId}/attackId={attacker.attackId}/attackName={AttackRegistry.GetAttackName(attacker.attackId)}/damage={attacker.damageDealt}/heal={attacker.healAmount}/blocked={attacker.blocked}/ongoing={attacker.ongoingActionConsumed}";
    }

    private static string DescribeBattleEffects(List<BattleEffectDto> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            return "[]";
        }

        return "[" + string.Join(", ", effects
            .FindAll(effect => effect != null)
            .ConvertAll(effect => $"{effect.type}(dur={effect.duration},src={effect.source ?? "none"},int={effect.intensity})")) + "]";
    }

    private static string DescribeRemovedEffects(List<int> effectsRemoved)
    {
        if (effectsRemoved == null || effectsRemoved.Count == 0)
        {
            return "[]";
        }

        return "[" + string.Join(", ", effectsRemoved) + "]";
    }

    private static void LogOngoingStep(BattleOngoingActionStepDto step, Kard attacker, Kard defender)
    {
        if (!VerboseRoyalRumblePlaybackLogs || step == null)
        {
            return;
        }

        Debug.LogWarning(
            $"[RoyalRumbleBattlePlayback] Ongoing step: type={step.type}, actionType={step.actionType}, amount={step.amount}, effectType={step.effectType}, " +
            $"attacker={attacker?.cardName}, defender={defender?.cardName}, note={step.note ?? "none"}, attackResult={step.attackResult ?? "none"}"
        );
    }
}
