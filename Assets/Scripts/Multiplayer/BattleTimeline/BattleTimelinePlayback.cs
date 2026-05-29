using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class BattleTimelinePlaybackContext
{
    public MonoBehaviour CoroutineHost;
    public Kard PlayerCard;
    public Kard EnemyCard;
    public string PlayerCardId;
    public string EnemyCardId;
    public Attack AttackComponent;
    public MultiplayerCardAnimator CardAnimator;
    public HealthBar PlayerLifeBar;
    public HealthBar EnemyLifeBar;
    public TMP_Text DialogText;
    public GameObject PlayerBoard;
    public GameObject EnemyBoard;
    public bool ResetCardsAfterPlayback;
    public string CompletionDialog;
    public float DialogDelaySeconds = 1.5f;
    public float TickDelaySeconds = 0.3f;
    public float InterAttackDelaySeconds = 0.5f;
    public float FinalDelaySeconds = 1f;
}

public static class BattleTimelinePlayback
{
    public static IEnumerator Play(BattleTimelinePlaybackContext context, List<BattleStep> steps)
    {
        if (context == null || steps == null)
        {
            yield break;
        }

        int executedAttacks = 0;

        for (int i = 0; i < steps.Count; i++)
        {
            BattleStep step = steps[i];
            if (step == null)
            {
                continue;
            }

            Kard actor = GetCardById(context, step.ActorCardId);
            Kard target = GetCardById(context, step.TargetCardId);
            bool isPlayerActor = actor != null && actor.cardId == context.PlayerCardId;

            switch (step.Type)
            {
                case BattleStepType.BleedTick:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return PlaySingleBleedAnimation(context, actor, step.Amount, isPlayerActor);
                        yield return new WaitForSeconds(context.TickDelaySeconds);
                    }
                    else
                    {
                        LogEffectTickAnimation("skipped", "BleedTick", context, step, actor, step.Amount, null);
                    }
                    break;

                case BattleStepType.BurnTick:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return PlayBurnAnimation(context, actor, step.Amount, isPlayerActor);
                        yield return new WaitForSeconds(context.TickDelaySeconds);
                    }
                    else
                    {
                        LogEffectTickAnimation("skipped", "BurnTick", context, step, actor, step.Amount, null);
                    }
                    break;

                case BattleStepType.ExposureTick:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return PlayExposureAnimation(context, actor, step.Amount, isPlayerActor);
                        yield return new WaitForSeconds(context.TickDelaySeconds);
                    }
                    else
                    {
                        LogEffectTickAnimation("skipped", "ExposureTick", context, step, actor, step.Amount, null);
                    }
                    break;

                case BattleStepType.ExposureRemoved:
                    if (actor != null)
                    {
                        yield return RemoveEndedEffectIcon(context, actor, 4);
                        yield return ShowDialog(context, $"{actor.cardName}'s irradiation is gone");
                        yield return new WaitForSeconds(context.TickDelaySeconds);
                    }
                    break;

                case BattleStepType.Recovery:
                    if (actor != null)
                    {
                        yield return PlayRecoveryAnimation(context, actor, isPlayerActor ? "My card" : "Enemy card");
                        yield return new WaitForSeconds(0.5f);
                    }
                    break;

                case BattleStepType.WakeUp:
                    if (actor != null)
                    {
                        yield return PlayWakeUpAnimation(context, actor, step.EffectType);
                    }
                    break;

                case BattleStepType.Attack:
                    if (actor == null || target == null || step.Skipped || step.Blocked)
                    {
                        break;
                    }

                    if (actor.health <= 0 || target.health <= 0)
                    {
                        break;
                    }

                    if (executedAttacks > 0)
                    {
                        yield return new WaitForSeconds(context.InterAttackDelaySeconds);
                    }

                    if (context.DialogText != null)
                    {
                        context.DialogText.color = isPlayerActor ? Color.blue : Color.red;
                    }

                    yield return ExecuteAttackAnimation(
                        context,
                        actor,
                        target,
                        step.AttackId,
                        GetAttackDamageFromTimeline(steps, i, step.ActorCardId, step.TargetCardId),
                        GetAttackHealFromTimeline(steps, i, step.ActorCardId),
                        isPlayerActor,
                        step.EffectsApplied,
                        step.AttackResult,
                        step.AttackerEffectsApplied
                    );
                    executedAttacks++;
                    break;

                case BattleStepType.StatChange:
                    if (target != null && !string.IsNullOrEmpty(step.StatName) && step.Amount != 0)
                    {
                        yield return BattleStatPlayback.PlayTimelineStatChange(
                            target,
                            step.Amount,
                            step.StatName,
                            context.CardAnimator
                        );
                    }
                    break;

                case BattleStepType.Blocked:
                    if (actor != null)
                    {
                        yield return PlayBlockAnimation(context, actor, step.BlockedBy);
                    }
                    break;

                case BattleStepType.SelfDamage:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return PlaySelfDamageAnimation(context, actor, step.Amount, isPlayerActor);
                    }
                    break;

                case BattleStepType.EffectApplied:
                    if (target != null)
                    {
                        var effectData = new Dictionary<string, object>
                        {
                            { "type", step.EffectType },
                            { "duration", step.Duration },
                            { "source", step.Source },
                        };
                        yield return BattleEffectPlayback.AddEffectIconOnly(
                            target,
                            effectData,
                            target.cardId == context.PlayerCardId
                        );
                    }
                    break;

                case BattleStepType.EffectRemoved:
                    if (target != null)
                    {
                        yield return RemoveEndedEffectIcon(context, target, step.EffectType);
                        if (step.EffectType == 7)
                        {
                            yield return ShowDialog(context, $"{target.cardName} is starving no more");
                        }
                        yield return new WaitForSeconds(0.2f);
                    }
                    break;

                case BattleStepType.SatelliteTick:
                    if (actor != null)
                    {
                        AttackAnimations animations = context.AttackComponent?.attackAnimations;
                        LogEffectTickAnimation("start", "SatelliteTick", context, step, actor, step.Amount, animations);
                        if (animations != null)
                        {
                            yield return animations.PlaySatelliteAnimation(actor.transform);
                        }
                        else
                        {
                            LogEffectTickAnimation("missing-animations", "SatelliteTick", context, step, actor, step.Amount, animations);
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return ShowDialog(context, step.Note);
                        }

                        LogEffectTickAnimation("finish", "SatelliteTick", context, step, actor, step.Amount, animations);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        LogEffectTickAnimation("skipped", "SatelliteTick", context, step, actor, step.Amount, null);
                    }
                    break;

                case BattleStepType.OngoingActionStarted:
                    break;

                case BattleStepType.OngoingActionProgress:
                    yield return PlayOngoingActionProgress(context, step, actor, target);
                    break;

                case BattleStepType.OngoingActionResolved:
                    yield return PlayOngoingActionResolved(context, step, actor, target);
                    break;

                case BattleStepType.OngoingActionCancelled:
                    if (!string.IsNullOrEmpty(step.Note))
                    {
                        yield return ShowDialog(context, step.Note);
                    }
                    break;

                case BattleStepType.Damage:
                    if (target != null && ShouldPlayStandaloneDamage(step))
                    {
                        yield return BattleValuePlayback.PlayDamage(
                            target,
                            step.Amount,
                            target.cardId == context.PlayerCardId,
                            context.CardAnimator,
                            context.PlayerLifeBar,
                            context.EnemyLifeBar
                        );
                    }
                    break;

                case BattleStepType.Heal:
                    if (
                        target != null
                        && string.Equals(step.Source, "famine", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        AttackAnimations animations = context.AttackComponent?.attackAnimations;
                        if (animations != null)
                        {
                            yield return animations.PlayFamineContinueAnimation(target.transform);
                        }

                        yield return BattleValuePlayback.PlayHeal(
                            target,
                            step.Amount,
                            target.cardId == context.PlayerCardId,
                            context.CardAnimator,
                            context.PlayerLifeBar,
                            context.EnemyLifeBar
                        );

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return ShowDialog(context, step.Note);
                        }
                    }
                    break;

                case BattleStepType.Death:
                default:
                    break;
            }
        }

        if (context.DialogText != null)
        {
            context.DialogText.color = Color.black;
        }

        if (!string.IsNullOrEmpty(context.CompletionDialog))
        {
            yield return ShowDialog(context, context.CompletionDialog);
        }

        if (context.ResetCardsAfterPlayback)
        {
            ResetCardsAfterPlayback(context);
        }

        if (context.FinalDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(context.FinalDelaySeconds);
        }
    }

    private static IEnumerator ExecuteAttackAnimation(
        BattleTimelinePlaybackContext context,
        Kard attacker,
        Kard defender,
        int attackId,
        int damage,
        int healAmount,
        bool isPlayerAttack,
        List<Dictionary<string, object>> effectsApplied,
        string attackResult,
        List<Dictionary<string, object>> attackerEffects)
    {
        var attackContext = new AttackExecutionContext(
            attacker,
            defender,
            damage,
            healAmount,
            isPlayerAttack,
            0,
            context.AttackComponent != null ? context.AttackComponent.attackAnimations : null,
            context.CardAnimator,
            context.PlayerLifeBar,
            context.EnemyLifeBar,
            message => ShowDialog(context, message),
            effectsApplied,
            attackResult,
            attackerEffects
        );

        yield return AttackRegistry.ExecuteOrFallback(attackId, attackContext);
    }

    private static IEnumerator PlayWakeUpAnimation(BattleTimelinePlaybackContext context, Kard card, int effectType)
    {
        yield return RemoveEndedEffectIcon(context, card, effectType, 3);

        yield return ShowDialog(context, $"{card.cardName} wakes up!");
    }

    private static IEnumerator PlayRecoveryAnimation(BattleTimelinePlaybackContext context, Kard card, string cardDescription)
    {
        yield return RemoveEndedEffectIcon(context, card, 2);

        yield return ShowDialog(context, $"{card.cardName} feels blessed again!");
    }

    private static IEnumerator PlayBlockAnimation(BattleTimelinePlaybackContext context, Kard card, int? blockedBy)
    {
        AttackAnimations animations = context.AttackComponent?.attackAnimations;
        if (animations == null)
        {
            yield return ShowDialog(context, $"{card.cardName} cannot attack!");
            yield break;
        }

        switch (blockedBy)
        {
            case 2:
                yield return animations.PlayConfusionHurtItselfAnimation(card.transform);
                yield return ShowDialog(context, $"{card.cardName} practizes asceticism...");
                break;

            case 3:
            case 27:
                yield return animations.PlaySleepAnimation(card.transform);
                yield return ShowDialog(context, $"{card.cardName} is sleeping...");
                break;

            case 8:
                yield return animations.PlayElectricityAnimation(card.transform);
                yield return ShowDialog(context, $"{card.cardName} cannot move");
                break;

            case 9:
                yield return animations.PlayTetherAnimation(card.transform);
                yield return ShowDialog(context, $"{card.cardName} is locked");
                break;

            case 12:
                yield return animations.PlayBlocadeWaitAnimation(card.transform);
                yield return ShowDialog(context, "The blockade holds strong");
                break;

            default:
                yield return ShowDialog(context, $"{card.cardName} cannot attack!");
                break;
        }
    }

    private static IEnumerator PlaySelfDamageAnimation(BattleTimelinePlaybackContext context, Kard card, int damage, bool isPlayerCard)
    {
        yield return BattleValuePlayback.PlayDamage(
            card,
            damage,
            isPlayerCard,
            context.CardAnimator,
            context.PlayerLifeBar,
            context.EnemyLifeBar
        );

        yield return ShowDialog(context, $"{card.cardName} suffers -{damage} HP!");
    }

    private static IEnumerator PlayBurnAnimation(BattleTimelinePlaybackContext context, Kard card, int damage, bool isPlayerCard)
    {
        AttackAnimations animations = context.AttackComponent?.attackAnimations;
        LogEffectTickAnimation("start", "BurnTick", context, null, card, damage, animations);
        if (animations != null)
        {
            yield return animations.PlayBurnContinueAnimation(card.transform);
        }
        else
        {
            LogEffectTickAnimation("missing-animations", "BurnTick", context, null, card, damage, animations);
        }

        yield return BattleValuePlayback.PlayDamage(
            card,
            damage,
            isPlayerCard,
            context.CardAnimator,
            context.PlayerLifeBar,
            context.EnemyLifeBar
        );

        yield return ShowDialog(context, $"{card.cardName} is on fire! -{damage} HP");
        LogEffectTickAnimation("finish", "BurnTick", context, null, card, damage, animations);
    }

    private static IEnumerator PlaySingleBleedAnimation(BattleTimelinePlaybackContext context, Kard card, int damage, bool isPlayerCard)
    {
        AttackAnimations animations = context.AttackComponent?.attackAnimations;
        LogEffectTickAnimation("start", "BleedTick", context, null, card, damage, animations);
        if (animations != null)
        {
            yield return animations.PlayBleedContinueAnimation(card.transform, damage);
        }
        else
        {
            LogEffectTickAnimation("missing-animations", "BleedTick", context, null, card, damage, animations);
        }

        yield return BattleValuePlayback.PlayDamage(
            card,
            damage,
            isPlayerCard,
            context.CardAnimator,
            context.PlayerLifeBar,
            context.EnemyLifeBar
        );

        yield return ShowDialog(context, $"{card.cardName} is bleeding! -{damage} HP");
        LogEffectTickAnimation("finish", "BleedTick", context, null, card, damage, animations);
    }

    private static IEnumerator PlayExposureAnimation(BattleTimelinePlaybackContext context, Kard card, int damage, bool isPlayerCard)
    {
        AttackAnimations animations = context.AttackComponent?.attackAnimations;
        LogEffectTickAnimation("start", "ExposureTick", context, null, card, damage, animations);

        if (damage <= 0)
        {
            LogEffectTickAnimation("skipped", "ExposureTick", context, null, card, damage, animations);
            yield break;
        }

        if (animations != null)
        {
            yield return animations.PlayExposureAnimation(card.transform);
        }
        else
        {
            LogEffectTickAnimation("missing-animations", "ExposureTick", context, null, card, damage, animations);
        }

        yield return BattleValuePlayback.PlayDamage(
            card,
            damage,
            isPlayerCard,
            context.CardAnimator,
            context.PlayerLifeBar,
            context.EnemyLifeBar
        );

        yield return ShowDialog(context, $"{card.cardName} is irradiated");
        LogEffectTickAnimation("finish", "ExposureTick", context, null, card, damage, animations);
    }

    private static void LogEffectTickAnimation(
        string phase,
        string tickType,
        BattleTimelinePlaybackContext context,
        BattleStep step,
        Kard card,
        int amount,
        AttackAnimations animations)
    {
        string actorId = step != null ? step.ActorCardId : card?.cardId;
        string targetId = step != null ? step.TargetCardId : null;
        string cardName = card != null ? card.cardName : "none";
        string cardId = card != null ? card.cardId : (actorId ?? "none");
        bool hasAttackComponent = context?.AttackComponent != null;
        bool hasAnimations = animations != null;
        string boardSide = context != null && card != null && card.cardId == context.PlayerCardId ? "player" : "enemy";

        string message =
            $"[EFFECT_TICK_ANIM] phase={phase}, type={tickType}, card={cardName}({cardId}), side={boardSide}, amount={amount}, actorId={actorId ?? "none"}, targetId={targetId ?? "none"}, hasAttackComponent={hasAttackComponent}, hasAnimations={hasAnimations}";

        if (string.Equals(phase, "missing-animations", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    private static IEnumerator PlayOngoingActionProgress(
        BattleTimelinePlaybackContext context,
        BattleStep step,
        Kard actor,
        Kard target)
    {
        AttackAnimations animations = context.AttackComponent?.attackAnimations;
        string actionType = step.ActionType ?? string.Empty;

        if (string.Equals(actionType, "siege", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null && target != null)
            {
                yield return animations.PlaySiegeContinueAnimation(actor.transform, target.transform);
            }
        }
        else if (string.Equals(actionType, "doubleEnvelopment", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayDoubleEnvelopmentWaitAnimation(actor.transform);
            }
        }
        else if (string.Equals(actionType, "artInspiration", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayArtInspirationWaitAnimation(actor.transform);
            }
        }
        else if (string.Equals(actionType, "autoportrait", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayAutoportraitAnimation(actor.transform);
            }
        }
        else if (string.Equals(actionType, "buffaloHorns", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayBuffaloHornsContinueAnimation(actor.transform);
            }
        }
        else if (string.Equals(actionType, "flintlockPistol", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayFlintlockPistolLoadingAnimation(actor.transform);
            }
        }
        else if (string.Equals(actionType, "trident", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayTridentAimAnimation(actor.transform);
            }
        }

        if (!string.IsNullOrEmpty(step.Note))
        {
            yield return ShowDialog(context, step.Note);
        }
    }

    private static IEnumerator PlayOngoingActionResolved(
        BattleTimelinePlaybackContext context,
        BattleStep step,
        Kard actor,
        Kard target)
    {
        AttackAnimations animations = context.AttackComponent?.attackAnimations;
        string actionType = step.ActionType ?? string.Empty;

        if (string.Equals(actionType, "siege", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && target != null)
            {
                yield return animations.PlaySiegeEndAnimation(target.transform);
            }
        }
        else if (string.Equals(actionType, "doubleEnvelopment", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && target != null)
            {
                yield return animations.PlayDoubleEnvelopAttackAnimation(target.transform);
            }
        }
        else if (string.Equals(actionType, "artInspiration", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayArtInspirationEndAnimation(actor.transform);
                yield return ShowDialog(context, $"{actor.cardName} finished his creation");
            }

            if (animations != null && target != null)
            {
                yield return animations.PlayArtInspirationEndEnemyAnimation(target.transform);
                yield return ShowDialog(context, $"{target.cardName} is impressed by masterpiece");
            }

            if (animations != null && actor != null && target != null)
            {
                yield return animations.PlayArtInspirationEndAttackAnimation(actor.transform, target.transform);
            }
        }
        else if (string.Equals(actionType, "autoportrait", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null)
            {
                yield return animations.PlayAutoportraitFinishAnimation(actor.transform);
            }
        }
        else if (string.Equals(actionType, "buffaloHorns", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null && target != null)
            {
                yield return animations.PlayBuffaloHornsEndAnimation(actor.transform, target.transform);
            }
        }
        else if (string.Equals(actionType, "flintlockPistol", StringComparison.OrdinalIgnoreCase))
        {
            bool hit = string.Equals(step.AttackResult, "hit", StringComparison.OrdinalIgnoreCase);
            if (animations != null && actor != null && target != null)
            {
                yield return animations.PlayFlintlockPistolShotAnimation(actor.transform, target.transform, hit);
            }
        }
        else if (string.Equals(actionType, "trident", StringComparison.OrdinalIgnoreCase))
        {
            if (animations != null && actor != null && target != null)
            {
                yield return animations.PlayTridentHitAnimation(actor.transform, target.transform);
            }
        }

        if (!string.IsNullOrEmpty(step.Note))
        {
            yield return ShowDialog(context, step.Note);
        }
    }

    private static bool ShouldPlayStandaloneDamage(BattleStep step)
    {
        bool isTickDamage =
            string.Equals(step.Source, "bleed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(step.Source, "burn", StringComparison.OrdinalIgnoreCase)
            || string.Equals(step.Source, "exposure", StringComparison.OrdinalIgnoreCase);

        return !isTickDamage
            && !string.Equals(step.Source, "attack", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerator RemoveEndedEffectIcon(
        BattleTimelinePlaybackContext context,
        Kard card,
        int effectType,
        int fallbackEffectType = 0)
    {
        int resolvedEffectType = effectType != 0 ? effectType : fallbackEffectType;
        if (context == null || card == null || resolvedEffectType == 0)
        {
            yield break;
        }

        yield return BattleEffectPlayback.RemoveEffectIconOnly(
            card,
            resolvedEffectType,
            card.cardId == context.PlayerCardId,
            context.AttackComponent
        );
    }

    private static void ResetCardsAfterPlayback(BattleTimelinePlaybackContext context)
    {
        if (context.CardAnimator == null || context.PlayerCard == null || context.EnemyCard == null)
        {
            return;
        }

        Vector3 playerBoardPos =
            context.PlayerBoard != null
                ? context.PlayerBoard.transform.position
                : context.PlayerCard.transform.position;
        Vector3 enemyBoardPos =
            context.EnemyBoard != null
                ? context.EnemyBoard.transform.position
                : context.EnemyCard.transform.position;

        if (context.CoroutineHost != null)
        {
            context.CoroutineHost.StartCoroutine(
                context.CardAnimator.ResetCardPosition(context.PlayerCard, playerBoardPos, Quaternion.identity)
            );
            context.CoroutineHost.StartCoroutine(
                context.CardAnimator.ResetCardPosition(context.EnemyCard, enemyBoardPos, Quaternion.identity)
            );
            return;
        }
    }

    private static Kard GetCardById(BattleTimelinePlaybackContext context, string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
        {
            return null;
        }

        if (string.Equals(cardId, context.PlayerCardId, StringComparison.Ordinal))
        {
            return context.PlayerCard;
        }

        if (string.Equals(cardId, context.EnemyCardId, StringComparison.Ordinal))
        {
            return context.EnemyCard;
        }

        return null;
    }

    private static int GetAttackDamageFromTimeline(
        List<BattleStep> steps,
        int attackIndex,
        string attackerCardId,
        string defenderCardId)
    {
        for (int i = attackIndex + 1; i < steps.Count; i++)
        {
            BattleStep next = steps[i];
            if (next == null)
            {
                continue;
            }

            if (next.Type == BattleStepType.Attack)
            {
                break;
            }

            if (
                next.Type == BattleStepType.Damage
                && string.Equals(next.ActorCardId, attackerCardId, StringComparison.Ordinal)
                && string.Equals(next.TargetCardId, defenderCardId, StringComparison.Ordinal)
                && string.Equals(next.Source, "attack", StringComparison.OrdinalIgnoreCase)
            )
            {
                return next.Amount;
            }
        }

        return 0;
    }

    private static int GetAttackHealFromTimeline(List<BattleStep> steps, int attackIndex, string attackerCardId)
    {
        for (int i = attackIndex + 1; i < steps.Count; i++)
        {
            BattleStep next = steps[i];
            if (next == null)
            {
                continue;
            }

            if (next.Type == BattleStepType.Attack)
            {
                break;
            }

            if (
                next.Type == BattleStepType.Heal
                && string.Equals(next.TargetCardId, attackerCardId, StringComparison.Ordinal)
                && string.Equals(next.Source, "attack", StringComparison.OrdinalIgnoreCase)
            )
            {
                return next.Amount;
            }
        }

        return 0;
    }

    private static IEnumerator ShowDialog(BattleTimelinePlaybackContext context, string message)
    {
        if (context.DialogText != null)
        {
            context.DialogText.text = message ?? string.Empty;
        }

        if (context.DialogDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(context.DialogDelaySeconds);
        }
    }
}
