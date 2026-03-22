using System.Collections.Generic;
using UnityEngine;

public static class BattleTimelinePilotPolicy
{
    public const string TimelineV2EnabledPrefKey = "battle.timeline.v2.enabled";

    public static bool ShouldRunTimeline(
        Dictionary<string, object> battleResult,
        ParsedBattleResult parsed,
        out string mode)
    {
        mode = "disabled";
        if (parsed == null)
        {
            mode = "parsed_missing";
            return false;
        }

        bool timelineV2Enabled = ReadBoolPref(TimelineV2EnabledPrefKey, true);
        bool hasTimelineV2 = battleResult != null &&
                             battleResult.ContainsKey("timelineV2") &&
                             battleResult["timelineV2"] != null;

        if (!timelineV2Enabled)
        {
            mode = "v2_disabled";
            return false;
        }

        if (!hasTimelineV2)
        {
            mode = "v2_missing";
            return false;
        }

        mode = "v2";
        return true;
    }

    public static bool IsEligible(ParsedBattleResult parsed)
    {
        if (parsed == null)
        {
            return false;
        }

        if (!IsSupportedAttackId(parsed.MyAttackId) || !IsSupportedAttackId(parsed.EnemyAttackId))
        {
            return false;
        }

        if (parsed.MyAttackerSelfDamage > 0 || parsed.EnemyAttackerSelfDamage > 0)
        {
            return false;
        }

        if (HasEffects(parsed.MyAttackerEffects) || HasEffects(parsed.EnemyAttackerEffects))
        {
            return false;
        }

        if (HasAnyStatChanges(parsed.MyStatChanges) || HasAnyStatChanges(parsed.EnemyStatChanges))
        {
            return false;
        }

        return true;
    }

    private static bool IsSupportedAttackId(int attackId)
    {
        return attackId == 1 || attackId == 2 || attackId == 3 || attackId == 4 || attackId == 5 || attackId == 8 || attackId == 9 || attackId == 11;
    }

    private static bool HasEffects(List<Dictionary<string, object>> effects)
    {
        return effects != null && effects.Count > 0;
    }

    private static bool HasAnyStatChanges(AttackStatChanges statChanges)
    {
        return statChanges.attackerAttack != 0 ||
               statChanges.attackerStrength != 0 ||
               statChanges.attackerDefense != 0 ||
               statChanges.attackerKnowledge != 0 ||
               statChanges.attackerSpeed != 0 ||
               statChanges.attackerCharisma != 0 ||
               statChanges.defenderAttack != 0 ||
               statChanges.defenderStrength != 0 ||
               statChanges.defenderDefense != 0 ||
               statChanges.defenderKnowledge != 0 ||
               statChanges.defenderSpeed != 0 ||
               statChanges.defenderCharisma != 0;
    }

    private static bool ReadBoolPref(string key, bool defaultValue)
    {
        int defaultInt = defaultValue ? 1 : 0;
        return PlayerPrefs.GetInt(key, defaultInt) != 0;
    }

    public static bool IsHardModeEnabled()
    {
        return true;
    }
}
