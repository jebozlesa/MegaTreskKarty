using System;
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

    public static void SyncEffectIconsFromSnapshot(Kard card, SelectedCardData.EffectData[] effects)
    {
        if (card?.effectIconContainer == null)
        {
            return;
        }

        var missingCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        if (effects != null)
        {
            foreach (var effect in effects)
            {
                if (effect == null || !int.TryParse(effect.type, out int effectType))
                {
                    continue;
                }

                string effectName = GetEffectName(effectType);
                if (!string.IsNullOrEmpty(effectName))
                {
                    missingCounts.TryGetValue(effectName, out int count);
                    missingCounts[effectName] = count + 1;
                }
            }
        }

        var names = new List<string>(missingCounts.Keys);
        var excessIcons = new List<Transform>();
        foreach (Transform icon in card.effectIconContainer)
        {
            string matchedName = names.Find(name => icon.name.StartsWith(name + "Icon", StringComparison.Ordinal));
            if (matchedName != null && missingCounts[matchedName] > 0)
            {
                missingCounts[matchedName]--;
            }
            else
            {
                excessIcons.Add(icon);
            }
        }

        foreach (Transform icon in excessIcons)
        {
            // Destroy is deferred; detach now so another sync in this frame sees exact counts.
            icon.SetParent(null, false);
            UnityEngine.Object.Destroy(icon.gameObject);
        }

        int countBeforeAdd = card.effectIconContainer.childCount;
        foreach (string name in names)
        {
            for (int i = 0; i < missingCounts[name]; i++)
            {
                card.AddEffectIcon(name);
            }
        }

        int added = card.effectIconContainer.childCount - countBeforeAdd;
        if (excessIcons.Count > 0 || added > 0)
        {
            card.RepositionEffectIcons();
            Debug.LogWarning($"[EFFECT_ICON] Snapshot synced: card={card.cardName}({card.cardId}), added={added}, removed={excessIcons.Count}, total={card.effectIconContainer.childCount}");
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
        if (animations != null && effectType == 2)
        {
            yield return animations.PlayAscetismEndAnimation(card.transform);
        }
        else if (animations != null && (effectType == 3 || effectType == 27))
        {
            yield return animations.PlaySleepEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 4)
        {
            yield return animations.PlayExposureEndAnimation(card.transform);
        }
        else if (animations != null && effectType == 21)
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
        else if (animations != null && effectType == 17)
        {
            yield return animations.PlayConfusionEndAnimation(card.transform);
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
            case 6:
                return "Fury";
            case 7:
                return "Famine";
            case 8:
                return "Electricity";
            case 9:
                return "Tether";
            case 12:
                return "Blockade";
            case 13:
                return "Depression";
            case 16:
                return "Burn";
            case 17:
                return "Confusion";
            case 18:
                return "Satellite";
            case 19:
                return "Fear";
            case 21:
                return "Calm";
            case 24:
                return "Poison";
            default:
                Debug.LogWarning($"[BattleEffectPlayback] Unknown effect type: {effectType}");
                return null;
        }
    }

}

