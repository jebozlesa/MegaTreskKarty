using System;
using System.Collections.Generic;

/// <summary>
/// Jednoduchý výsledok z útoku pre multiplayer
/// </summary>
[Serializable]
public class BattleResult
{
    // Informácie o damage
    public int damageDealt;           // Koľko damage bolo udelené
    public bool didSleep;             // Či sa aplikoval sleep efekt (20% šanca pri Punch)
    public int sleepDuration;         // Ak didSleep=true, na koľko kôl
    
    // Finálne HP hodnoty
    public int attackerHealth;        // HP útočníka po útoku
    public int defenderHealth;        // HP obrancu po útoku
    
    // Ktorý hráč útočil prvý (podľa priority/speed)
    public string firstAttacker;      // "player1" alebo "player2"
    
    // Success flag
    public bool success;
}

/// <summary>
/// Dáta odosielané na server pre simuláciu útoku
/// ✅ V3 - MINIMÁLNY PAYLOAD (iba IDs)
/// Server trackuje HP v battleState.playerHealths
/// </summary>
[Serializable]
public class AttackSubmission
{
    public string playerId;
    public string roomCode;
    public string cardId;             // ID karty (server načíta stats z DB)
    public int attackId;              // ID útoku (1 = Punch)
    public int attackSlot;            // Slot útoku (1-4) - pre attack count decrement
    
    // DEPRECATED - server trackuje HP sám v room.battleState
    // Ponechané pre backward compatibility, ale server ich ignoruje
    public int currentHealth;         // IGNORED by server v3
    public int attackerHealth;        // IGNORED
    public int attackerMaxHealth;     // IGNORED
    public int attackerStrength;      // IGNORED
    public int attackerDefense;       // IGNORED
    public int attackerSpeed;         // IGNORED
    public int attackerMagic;         // IGNORED
    
    // Stats obrancu (budú potrebné pre výpočet)
    public int defenderHealth;
    public int defenderMaxHealth;
    public int defenderStrength;
    public int defenderDefense;
    public int defenderSpeed;
    public int defenderMagic;
}

/// <summary>
/// Jednoduchý wrapper pre attack selection data
/// </summary>
[Serializable]
public class SelectedAttackData
{
    public int attackType;     // 1-4 (ktoré tlačidlo)
    public int attackId;       // ID útoku (napr. 1 = Punch)
    public int attackCount;    // Zobrazovaný počet (damage preview)
    public string cardId;      // ID karty
}
