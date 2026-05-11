using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEffectPlayback
{
    public static IEnumerator AddEffectIconOnly(Kard card, Dictionary<string, object> effectData, bool isMyCard)
    {
        int effectType = int.Parse(effectData["type"].ToString());
        int duration = int.Parse(effectData["duration"].ToString());

        Debug.LogWarning(
            $"[EFFECT_ICON] Adding ICON ONLY on {card.cardName}: type={effectType}, duration={duration}"
        );

        string effectName = GetEffectName(effectType);
        if (!string.IsNullOrEmpty(effectName))
        {
            card.AddEffectIcon(effectName);
            card.RepositionEffectIcons();
            Debug.LogWarning($"[EFFECT_ICON] Added {effectName} icon to {card.cardName}");
        }

        yield return null;
    }

    public static IEnumerator DisplayMultipleEffects(
        Kard card,
        List<Dictionary<string, object>> effectsArray,
        bool isMyCard)
    {
        if (effectsArray == null || effectsArray.Count == 0)
        {
            yield break;
        }

        Debug.LogWarning(
            $"[MULTI-EFFECTS] Displaying {effectsArray.Count} effects on {card.cardName}"
        );

        foreach (var effect in effectsArray)
        {
            yield return DisplayEffectIcon(card, effect, isMyCard);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public static IEnumerator RemoveEffectIconOnly(
        Kard card,
        int effectType,
        bool isMyCard,
        Attack attackComponent)
    {
        string effectName = GetEffectName(effectType);
        if (string.IsNullOrEmpty(effectName))
        {
            yield break;
        }

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null && effectType == 21)
        {
            yield return animations.PlayCalmEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 6)
        {
            yield return animations.PlayFuryEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 7)
        {
            yield return animations.PlayFamineEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 8)
        {
            yield return animations.PlayElectricityEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 9)
        {
            yield return animations.PlayTetherEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 12)
        {
            yield return animations.PlayBlocadeEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 13)
        {
            yield return animations.PlayDepressionEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 19)
        {
            yield return animations.PlayFearEndAnimation(card.transform);
        }

        Debug.LogWarning(
            $"[EFFECT_ICON] Removing {effectName} from {(isMyCard ? "MY" : "ENEMY")} card {card.cardName}"
        );
        yield return card.RemoveEffectIcon(effectName);
    }

    public static string GetEffectName(int effectType)
    {
        switch (effectType)
        {
            case 1:
                return "Bleed";
            case 2:
                return "Asceticism";
            case 3:
                return "Sleep";
            case 27:
                return "Sleep";
            case 4:
                return "Exposure";
            case 5:
                return "Siege";
            case 6:
                return "Fury";
            case 7:
                return "Famine";
            case 8:
                return "Electricity";
            case 9:
                return "Tether";
            case 10:
                return "Starving";
            case 11:
                return "ScientificLecture";
            case 12:
                return "Blockade";
            case 13:
                return "Depression";
            case 14:
                return "ArtInspiration";
            case 15:
                return "Autoportrait";
            case 16:
                return "Burn";
            case 17:
                return "Confusion";
            case 18:
                return "Satellite";
            case 19:
                return "Fear";
            case 20:
                return null;
            case 21:
                return "Calm";
            case 22:
                return "Reloading";
            case 23:
                return "Trident";
            case 24:
                return "Poison";
            case 26:
                return "Curse";
            default:
                Debug.LogWarning($"[BattleEffectPlayback] Unknown effect type: {effectType}");
                return null;
        }
    }

    private static IEnumerator DisplayEffectIcon(
        Kard card,
        Dictionary<string, object> effectData,
        bool isMyCard)
    {
        int effectType = int.Parse(effectData["type"].ToString());
        int duration = int.Parse(effectData["duration"].ToString());

        Debug.LogWarning(
            $"[EFFECT_ICON] Adding effect icon to {card.cardName}: type={effectType}, duration={duration}"
        );

        string effectName = GetEffectName(effectType);
        if (!string.IsNullOrEmpty(effectName))
        {
            card.AddEffectIcon(effectName);
            card.RepositionEffectIcons();
            Debug.LogWarning($"[EFFECT_ICON] Added {effectName} icon to {card.cardName}");
        }

        yield break;
    }
}

