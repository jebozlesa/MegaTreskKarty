using System;
using System.Collections;
using UnityEngine;

public static class BattleStatPlayback
{
    public static IEnumerator PlayCardStatChanges(
        Kard card,
        int attackChange,
        int strengthChange,
        int defenseChange,
        int knowledgeChange,
        int speedChange,
        int charismaChange,
        MultiplayerCardAnimator cardAnimator)
    {
        yield return PlaySingleStatChange(
            card,
            attackChange,
            "ATT",
            value => card.attack = Mathf.Max(1, card.attack + value),
            cardAnimator
        );
        yield return PlaySingleStatChange(
            card,
            strengthChange,
            "STR",
            value => card.strength = Mathf.Max(1, card.strength + value),
            cardAnimator
        );
        yield return PlaySingleStatChange(
            card,
            defenseChange,
            "DEF",
            value => card.defense = Mathf.Max(1, card.defense + value),
            cardAnimator
        );
        yield return PlaySingleStatChange(
            card,
            knowledgeChange,
            "KNO",
            value => card.knowledge = Mathf.Max(1, card.knowledge + value),
            cardAnimator
        );
        yield return PlaySingleStatChange(
            card,
            speedChange,
            "SPD",
            value => card.speed = Mathf.Max(1, card.speed + value),
            cardAnimator
        );
        yield return PlaySingleStatChange(
            card,
            charismaChange,
            "CHA",
            value => card.charisma = Mathf.Max(1, card.charisma + value),
            cardAnimator
        );
    }

    public static IEnumerator PlayTimelineStatChange(
        Kard card,
        int change,
        string statName,
        MultiplayerCardAnimator cardAnimator)
    {
        switch (statName)
        {
            case "ATT":
                yield return PlaySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.attack = Mathf.Max(1, card.attack + value),
                    cardAnimator
                );
                break;
            case "STR":
                yield return PlaySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.strength = Mathf.Max(1, card.strength + value),
                    cardAnimator
                );
                break;
            case "DEF":
                yield return PlaySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.defense = Mathf.Max(1, card.defense + value),
                    cardAnimator
                );
                break;
            case "KNO":
                yield return PlaySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.knowledge = Mathf.Max(1, card.knowledge + value),
                    cardAnimator
                );
                break;
            case "SPD":
                yield return PlaySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.speed = Mathf.Max(1, card.speed + value),
                    cardAnimator
                );
                break;
            case "CHA":
                yield return PlaySingleStatChange(
                    card,
                    change,
                    statName,
                    value => card.charisma = Mathf.Max(1, card.charisma + value),
                    cardAnimator
                );
                break;
            default:
                Debug.LogWarning(
                    $"[BattleStatPlayback] Unknown timeline stat change: stat={statName}, amount={change}, card={card.cardName}"
                );
                break;
        }
    }

    private static IEnumerator PlaySingleStatChange(
        Kard card,
        int change,
        string statName,
        Action<int> applyChange,
        MultiplayerCardAnimator cardAnimator)
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
