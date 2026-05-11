using System;
using System.Collections.Generic;

/// <summary>
/// Jednoduchy vysledok z utoku pre multiplayer
/// </summary>
[Serializable]
public class BattleResult
{
    // Informacie o damage
    public int damageDealt;           // Kolko damage bolo udelene
    public bool didSleep;             // Ci sa aplikoval sleep efekt (20% sanca pri Punch)
    public int sleepDuration;         // Ak didSleep=true, na kolko kol
    
    // Finalne HP hodnoty
    public int attackerHealth;        // HP utocnika po utoku
    public int defenderHealth;        // HP obrancu po utoku
    
    // Ktory hrac utocil prvy (podla priority/speed)
    public string firstAttacker;      // "player1" alebo "player2"
    
    // Success flag
    public bool success;
}

/// <summary>
/// Data odosielane na server pre simulaciu utoku
/// Minimalny payload pre battle submit.
/// Server nacita card state a stats z room.selectedCards.
/// </summary>
[Serializable]
public class AttackSubmission
{
    public string playerId;
    public string roomCode;
    public string cardId;             // ID karty (server nacita stats z DB)
    public int attackId;              // ID utoku (1 = Punch)
    public int attackSlot;            // Slot utoku (1-4) - pre attack count decrement
}

/// <summary>
/// Jednoduchy wrapper pre attack selection data
/// </summary>
[Serializable]
public class SelectedAttackData
{
    public int attackType;     // 1-4 (ktore tlacidlo)
    public int attackId;       // ID utoku (napr. 1 = Punch)
    public int attackCount;    // Zobrazovany pocet (damage preview)
    public string cardId;      // ID karty
}

[Serializable]
public class PendingOngoingActionTurnData
{
    public string actionType;
    public int sourceAttackId;
    public string targetCardId;
    public int turnsRemaining;
}
