using System.Collections.Generic;

/// <summary>
/// Shared Temptation compatibility rules.
/// Default male/female style groups match singleplayer.
/// Override table exists so future exceptions can be added without rewriting Attack27.
/// </summary>
public static class TemptationRules
{
    private static readonly HashSet<int> DefaultMaleStyleIds = new HashSet<int>
    {
        1, 2, 3, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 26,
        27, 28, 29, 31, 32, 33, 34, 37, 38, 39, 40, 42, 43, 46, 47, 48, 49
    };

    private static readonly HashSet<int> DefaultFemaleStyleIds = new HashSet<int>
    {
        4, 10, 25, 30, 35, 36, 41, 44, 45, 50
    };

    // Supported values: "male", "female", "all", "none"
    private static readonly Dictionary<int, string> StyleOverrides = new Dictionary<int, string>();

    public static string GetTemptationGroup(int styleId)
    {
        if (StyleOverrides.TryGetValue(styleId, out string overrideGroup))
        {
            return overrideGroup;
        }

        if (DefaultMaleStyleIds.Contains(styleId))
        {
            return "male";
        }

        if (DefaultFemaleStyleIds.Contains(styleId))
        {
            return "female";
        }

        return "unknown";
    }

    public static bool CanTemptTarget(int attackerStyleId, int defenderStyleId)
    {
        string attackerGroup = GetTemptationGroup(attackerStyleId);
        string defenderGroup = GetTemptationGroup(defenderStyleId);

        if (attackerGroup == "none" || defenderGroup == "none")
        {
            return false;
        }

        if (attackerGroup == "all")
        {
            return defenderGroup != "unknown";
        }

        if (defenderGroup == "all")
        {
            return attackerGroup != "unknown";
        }

        if (attackerGroup == "unknown" || defenderGroup == "unknown")
        {
            return false;
        }

        return attackerGroup != defenderGroup;
    }

    public static bool UseMaleSuccessAnimationVariant(int styleId)
    {
        string group = GetTemptationGroup(styleId);
        return group == "male" || group == "all";
    }
}
