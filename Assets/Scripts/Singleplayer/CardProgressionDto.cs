using System;

[Serializable]
public class CardProgressionDto
{
    public bool applied;
    public string reason;
    public bool pending;
    public bool duplicate;
    public string eventId;
    public string playerCardId;
    public string enemyCardId;
    public int enemyLevel;
    public int xpGained;
    public int experienceBefore;
    public int experienceAfter;
    public int levelBefore;
    public int levelAfter;
    public bool leveledUp;
    public int levelUps;
    public bool writeVerified;
    public bool persistedCardFound;
    public int persistedExperienceAfter;
    public int persistedLevelAfter;
    public string writeVerificationReason;
    public string writeVerificationError;
}

public static class OnlineCardExperienceCurve
{
    public static int RequiredXpForLevel(int level)
    {
        int normalizedLevel = Math.Max(1, level);
        if (normalizedLevel <= 1)
        {
            return 0;
        }

        double rawRequiredXp = 3.2 * Math.Pow(normalizedLevel - 1, 1.65);
        int roundedRequiredXp = (int)Math.Floor(rawRequiredXp + 0.5);
        return Math.Max(5, roundedRequiredXp);
    }
}
