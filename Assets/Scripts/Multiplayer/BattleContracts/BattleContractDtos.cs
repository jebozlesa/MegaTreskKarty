using System;
using System.Collections.Generic;

[Serializable]
public class ExecuteBattleEnvelopeDto
{
    public bool success;
    public bool bothPlayersReady;
    public int playersReadyCount;
    public string message;
    public BattleResultDto battleResult;
}

[Serializable]
public class BattleResultDto
{
    public BattleAttackerDto firstAttacker;
    public BattleAttackerDto secondAttacker;

    public bool cardDied;
    public string winnerCardId;
    public string loserCardId;
}

[Serializable]
public class BattleAttackerDto
{
    public string cardId;
    public int attackId;

    public int damageReceived;
    public int damageDealt;
    public int healAmount;

    public bool blocked;
    public int? blockedBy;
    public int remainingDuration;
    public bool wokeUp;
    public bool recovered;
    public int selfDamage;

    public int bleedDamage;
    public List<int> bleedDamages;
    public int exposureDamage;
    public bool exposureRemoved;

    public List<int> effectsRemoved;
    public BattleEffectDto effectApplied;
    public List<BattleEffectDto> effectsApplied;

    public int attackerSelfDamage;
    public List<BattleEffectDto> attackerEffectsApplied;

    public string attackResult;

    public List<BattleStatChangeDto> statChanges;

    public List<BattleEffectDto> effects;
    public bool isDead;
}

[Serializable]
public class BattleEffectDto
{
    public int type;
    public int duration;
    public string source;
    public int intensity;
}

[Serializable]
public class BattleStatChangeDto
{
    public string statName;
    public int amount;
    public string actorCardId;
    public string targetCardId;
    public string source;
}

[Serializable]
public class MatchStateEnvelopeDto
{
    public bool success;
    public string message;
    public string error;
    public MatchStateDto matchState;
}

[Serializable]
public class MatchStateDto
{
    public string roomCode;
    public string status;
    public string phase;
    public string inferredPhase;
    public int schemaVersion;
    public int version;
    public string updatedAt;
    public string lastTransitionAt;
    public string createdAt;
    public int playersCount;
    public string viewerPlayerId;
    public string opponentPlayerId;
    public List<MatchSeatDto> seats;
    public MatchDeckStateDto decks;
    public Dictionary<string, SelectedCardData> selectedCards;
    public MatchBattleStateDto battle;
    public MatchNextTurnReadyDto nextTurnReady;
    public List<string> replacementRequiredPlayerIds;
    public List<string> warnings;
}

[Serializable]
public class MatchSeatDto
{
    public string seat;
    public string playerId;
    public string username;
    public string joinedAt;
    public string lastActivity;
    public bool connected;
    public bool deckLoaded;
    public int deckSize;
    public string selectedCardId;
    public string selectedCardName;
    public bool submittedAttack;
    public bool readyForNextTurn;
}

[Serializable]
public class MatchDeckStateDto
{
    public int loadedCount;
    public bool allLoaded;
    public List<string> loadedPlayerIds;
}

[Serializable]
public class MatchBattleStateDto
{
    public int submittedCount;
    public Dictionary<string, bool> submittedByPlayerId;
    public bool hasLastResult;
    public string lastBattleTime;
}

[Serializable]
public class MatchNextTurnReadyDto
{
    public Dictionary<string, bool> readyByPlayerId;
    public List<string> readyPlayerIds;
    public int readyCount;
    public bool bothReady;
}
