using System;
using System.Collections.Generic;

public enum BattleStepType
{
    Attack = 0,
    StatChange = 1,
    Blocked = 2,
    Damage = 3,
    Heal = 8,
    SelfDamage = 9,
    EffectApplied = 12,
    EffectRemoved = 13,
    SatelliteTick = 14,
    OngoingActionStarted = 15,
    OngoingActionProgress = 16,
    OngoingActionResolved = 17,
    OngoingActionCancelled = 18,
    Death = 19,
    EffectTriggered = 22
}

[Serializable]
public class BattleStep
{
    public BattleStepType Type;
    public string ActorCardId;
    public string TargetCardId;
    public int AttackId;
    public string AttackResult;
    public List<Dictionary<string, object>> EffectsApplied;
    public List<Dictionary<string, object>> AttackerEffectsApplied;
    public int Amount;
    public string StatName;
    public int EffectType;
    public string RemovalReason;
    public int Duration;
    public bool Blocked;
    public int? BlockedBy;
    public bool Skipped;
    public string Source;
    public string Note;
    public string ActionType;
    public string ActionId;
    public int TurnsRemaining;
}



