using System.Collections.Generic;

/// <summary>
/// Shared Revelation animation variant rules.
/// Keep style-group mapping here so future characters or whole revelation groups
/// can be updated without touching attack flow logic.
/// </summary>
public static class RevelationRules
{
    private static readonly Dictionary<int, HashSet<int>> VariantStyleIds =
        new Dictionary<int, HashSet<int>>
        {
            { 2, new HashSet<int> { 6 } },
            { 3, new HashSet<int> { 43, 10001 } },
            { 4, new HashSet<int> { 12 } },
            { 5, new HashSet<int> { 46 } },
        };

    public static int GetVariantIndex(int styleId)
    {
        foreach (KeyValuePair<int, HashSet<int>> entry in VariantStyleIds)
        {
            if (entry.Value.Contains(styleId))
            {
                return entry.Key;
            }
        }

        return 0;
    }
}
