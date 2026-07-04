using System;
using System.Collections.Generic;

[Serializable]
public class RoyalRumbleSessionEnvelopeDto
{
    public bool success;
    public string message;
    public string error;
    public RoyalRumbleSessionDto session;
    public RoyalRumbleSessionSummaryDto sessionSummary;
}

[Serializable]
public class RoyalRumbleBattleEnvelopeDto
{
    public bool success;
    public string message;
    public string error;
    public BattleResultDto battleResult;
    public RoyalRumbleBotAttackDto botAttack;
    public RoyalRumbleSessionDto session;
    public RoyalRumbleSessionSummaryDto sessionSummary;
    public bool playerNeedsReplacement;
    public bool enemyNeedsReplacement;
    public bool runEnded;
    public string runStatus;
    public RoyalRumbleRecordSyncDto recordSync;
    public CardProgressionDto cardProgression;
}

[Serializable]
public class RoyalRumbleSessionDto
{
    public string sessionId;
    public int schemaVersion;
    public string mode;
    public string playerId;
    public RoyalRumblePlayerInfoDto playerInfo;
    public string status;
    public RoyalRumbleDeckDto playerDeck;
    public RoyalRumbleDeckDto enemyDeck;
    public RoyalRumbleAttackCountsDto attackCounts;
    public RoyalRumbleActiveDto active;
    public RoyalRumbleProgressDto progress;
    public RoyalRumbleModeConfigDto modeConfig;
    public RoyalRumbleResultSummaryDto resultSummary;
    public BattleResultDto lastBattleResult;
    public string createdAt;
    public string updatedAt;
    public string abandonedAt;
}

[Serializable]
public class RoyalRumblePlayerInfoDto
{
    public string playerId;
    public string username;
}

[Serializable]
public class RoyalRumbleDeckDto
{
    public string playerId;
    public string deckId;
    public string deckName;
    public int deckSize;
    public string loadedAt;
    public string generatedAt;
    public List<SelectedCardData> cards;
}

[Serializable]
public class RoyalRumbleAttackCountsDto
{
    public Dictionary<string, RoyalRumbleAttackCountEntryDto> player;
    public Dictionary<string, RoyalRumbleAttackCountEntryDto> enemy;
}

[Serializable]
public class RoyalRumbleAttackCountEntryDto
{
    public int count1;
    public int count2;
    public int count3;
    public int count4;
}

[Serializable]
public class RoyalRumbleActiveDto
{
    public string playerCardId;
    public string enemyCardId;
}

[Serializable]
public class RoyalRumbleProgressDto
{
    public int turnNumber;
    public int battleCount;
    public int defeatedEnemyCount;
    public int playerDeaths;
    public int bestSubmittedScore;
    public int? pendingRecordScore;
    public string lastRecordError;
    public string campaignId;
    public int? levelId;
}

[Serializable]
public class RoyalRumbleModeConfigDto
{
    public int playerDeckSize;
    public int enemyDeckSize;
    public string botStrategy;
    public bool rewardFlowEnabled;
    public string persistenceScope;
}

[Serializable]
public class RoyalRumbleResultSummaryDto
{
    public string lastBattleAt;
    public bool playerNeedsReplacement;
    public bool enemyNeedsReplacement;
    public bool playerEliminated;
    public bool enemyEliminated;
    public int playerAttackSlot;
    public int enemyAttackSlot;
}

[Serializable]
public class RoyalRumbleSessionSummaryDto
{
    public string sessionId;
    public int schemaVersion;
    public string mode;
    public string playerId;
    public string status;
    public RoyalRumbleDeckSummaryDto playerDeck;
    public RoyalRumbleDeckSummaryDto enemyDeck;
    public RoyalRumbleAttackCountsDto attackCounts;
    public RoyalRumbleActiveDto active;
    public RoyalRumbleProgressDto progress;
    public RoyalRumbleModeConfigDto modeConfig;
    public BattleResultDto lastBattleResult;
    public RoyalRumbleResultSummaryDto resultSummary;
    public string createdAt;
    public string updatedAt;
    public string abandonedAt;
}

[Serializable]
public class RoyalRumbleDeckSummaryDto
{
    public string deckId;
    public string deckName;
    public int deckSize;
    public bool loaded;
}

[Serializable]
public class RoyalRumbleBotAttackDto
{
    public int attackSlot;
    public int attackId;
    public List<int> validSlots;
    public string reason;
}

[Serializable]
public class RoyalRumbleRecordSyncDto
{
    public bool submitted;
    public bool pending;
    public bool skipped;
    public int score;
    public string error;
}
