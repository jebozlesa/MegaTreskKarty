using System;
using System.Collections.Generic;

public enum BattleStepType
{
    Attack,
    StatChange,
    Blocked,
    Damage,
    BleedTick,
    BurnTick,
    ExposureTick,
    ExposureRemoved,
    Heal,
    SelfDamage,
    WakeUp,
    Recovery,
    EffectApplied,
    EffectRemoved,
    Death
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
    public int Duration;
    public bool Blocked;
    public int? BlockedBy;
    public bool Skipped;
    public string Source;
    public string Note;
}



