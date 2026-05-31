using System;
using System.Collections.Generic;

[Serializable]
public class CampaignOnlineSessionEnvelopeDto
{
    public bool success;
    public string message;
    public string error;
    public CampaignOnlineSessionDto session;
    public CampaignOnlineSessionSummaryDto sessionSummary;
}

[Serializable]
public class CampaignOnlineBattleEnvelopeDto
{
    public bool success;
    public string message;
    public string error;
    public BattleResultDto battleResult;
    public CampaignOnlineBotAttackDto botAttack;
    public CampaignOnlineSessionDto session;
    public CampaignOnlineSessionSummaryDto sessionSummary;
    public bool playerNeedsReplacement;
    public bool enemyNeedsReplacement;
    public bool runEnded;
    public string runStatus;
}

[Serializable]
public class CampaignOnlineSessionDto
{
    public string sessionId;
    public int schemaVersion;
    public string mode;
    public string playerId;
    public string status;
    public CampaignOnlineDeckDto playerDeck;
    public CampaignOnlineDeckDto enemyDeck;
    public CampaignOnlineAttackCountsDto attackCounts;
    public CampaignOnlineActiveDto active;
    public CampaignOnlineProgressDto progress;
    public CampaignOnlineModeConfigDto modeConfig;
    public CampaignOnlineResultSummaryDto resultSummary;
    public BattleResultDto lastBattleResult;
    public string createdAt;
    public string updatedAt;
    public string abandonedAt;
}

[Serializable]
public class CampaignOnlineDeckDto
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
public class CampaignOnlineAttackCountsDto
{
    public Dictionary<string, CampaignOnlineAttackCountEntryDto> player;
    public Dictionary<string, CampaignOnlineAttackCountEntryDto> enemy;
}

[Serializable]
public class CampaignOnlineAttackCountEntryDto
{
    public int count1;
    public int count2;
    public int count3;
    public int count4;
}

[Serializable]
public class CampaignOnlineActiveDto
{
    public string playerCardId;
    public string enemyCardId;
}

[Serializable]
public class CampaignOnlineProgressDto
{
    public int turnNumber;
    public int battleCount;
    public int defeatedEnemyCount;
    public int playerDeaths;
    public string campaignId;
    public int? levelId;
}

[Serializable]
public class CampaignOnlineModeConfigDto
{
    public int playerDeckSize;
    public int enemyDeckSize;
    public string campaignId;
    public int missionId;
    public string botStrategy;
    public bool rewardFlowEnabled;
    public string persistenceScope;
}

[Serializable]
public class CampaignOnlineResultSummaryDto
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
public class CampaignOnlineSessionSummaryDto
{
    public string sessionId;
    public int schemaVersion;
    public string mode;
    public string playerId;
    public string status;
    public CampaignOnlineDeckSummaryDto playerDeck;
    public CampaignOnlineDeckSummaryDto enemyDeck;
    public CampaignOnlineAttackCountsDto attackCounts;
    public CampaignOnlineActiveDto active;
    public CampaignOnlineProgressDto progress;
    public CampaignOnlineModeConfigDto modeConfig;
    public BattleResultDto lastBattleResult;
    public CampaignOnlineResultSummaryDto resultSummary;
    public string createdAt;
    public string updatedAt;
    public string abandonedAt;
}

[Serializable]
public class CampaignOnlineDeckSummaryDto
{
    public string deckId;
    public string deckName;
    public int deckSize;
    public bool loaded;
}

[Serializable]
public class CampaignOnlineBotAttackDto
{
    public int attackSlot;
    public int attackId;
    public List<int> validSlots;
    public string reason;
}
