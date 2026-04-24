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
