using System;
using System.Collections;
using UnityEngine;

public sealed class BattleStatApplier
{
    private readonly MultiplayerCardAnimator cardAnimator;

    public BattleStatApplier(MultiplayerCardAnimator cardAnimator)
    {
        this.cardAnimator = cardAnimator;
    }

    public IEnumerator ApplyCardStatChanges(
        Kard card,
        int attackChange,
        int strengthChange,
        int defenseChange,
        int knowledgeChange,
        int speedChange,
        int charismaChange)
    {
        yield return ApplySingleStatChange(
            card,
            attackChange,
            "ATT",
            value => card.attack = Mathf.Max(1, card.attack + value));
        yield return ApplySingleStatChange(
            card,
            strengthChange,
            "STR",
            value => card.strength = Mathf.Max(1, card.strength + value));
        yield return ApplySingleStatChange(
            card,
            defenseChange,
            "DEF",
            value => card.defense = Mathf.Max(1, card.defense + value));
        yield return ApplySingleStatChange(
            card,
            knowledgeChange,
            "KNO",
            value => card.knowledge = Mathf.Max(1, card.knowledge + value));
        yield return ApplySingleStatChange(
            card,
            speedChange,
            "SPD",
            value => card.speed = Mathf.Max(1, card.speed + value));
        yield return ApplySingleStatChange(
            card,
            charismaChange,
            "CHA",
            value => card.charisma = Mathf.Max(1, card.charisma + value));
    }

    public IEnumerator ApplyTimelineStatChange(Kard card, int change, string statName)
    {
        switch (statName)
        {
            case "ATT":
                yield return ApplySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.attack = Mathf.Max(1, card.attack + value));
                break;
            case "STR":
                yield return ApplySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.strength = Mathf.Max(1, card.strength + value));
                break;
            case "DEF":
                yield return ApplySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.defense = Mathf.Max(1, card.defense + value));
                break;
            case "KNO":
                yield return ApplySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.knowledge = Mathf.Max(1, card.knowledge + value));
                break;
            case "SPD":
                yield return ApplySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.speed = Mathf.Max(1, card.speed + value));
                break;
            case "CHA":
                yield return ApplySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.charisma = Mathf.Max(1, card.charisma + value));
                break;
            default:
                Debug.LogWarning(
                    $"[BattleStatApplier] Unknown timeline stat change: stat={statName}, amount={change}, card={card.cardName}");
                break;
        }
    }

    private IEnumerator ApplySingleStatChange(
        Kard card,
        int change,
        string statName,
        Action<int> applyChange)
    {
        if (change == 0)
        {
            yield break;
        }

        applyChange(change);

        if (cardAnimator != null)
        {
            yield return cardAnimator.AnimateStatChange(card, change, statName);
        }
    }
}
