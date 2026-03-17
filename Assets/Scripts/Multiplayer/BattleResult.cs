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
/// [OK] V3 - MINIMALNY PAYLOAD (iba IDs)
/// Server trackuje HP v battleState.playerHealths
/// </summary>
[Serializable]
public class AttackSubmission
{
    public string playerId;
    public string roomCode;
    public string cardId;             // ID karty (server nacita stats z DB)
    public int attackId;              // ID utoku (1 = Punch)
    public int attackSlot;            // Slot utoku (1-4) - pre attack count decrement
    
    // DEPRECATED - server trackuje HP sam v room.battleState
    // Ponechane pre backward compatibility, ale server ich ignoruje
    public int currentHealth;         // IGNORED by server v3
    public int attackerHealth;        // IGNORED
    public int attackerMaxHealth;     // IGNORED
    public int attackerStrength;      // IGNORED
    public int attackerDefense;       // IGNORED
    public int attackerSpeed;         // IGNORED
    public int attackerMagic;         // IGNORED
    
    // Stats obrancu (budu potrebne pre vypocet)
    public int defenderHealth;
    public int defenderMaxHealth;
    public int defenderStrength;
    public int defenderDefense;
    public int defenderSpeed;
    public int defenderMagic;
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
