# 💀 Card Death System - Implementation Guide

**Created:** 2025-10-30  
**Version:** V5 (CardID-based + Server Selective Clear)

---

## 🎯 Účel

Tento systém sa postará o:
1. **Detekciu smrti karty** - keď `health <= 0`
2. **Odstránenie z UI** - zničenie GameObjectu na boarde
3. **Odstránenie zo servera** - vymazanie z `room.selectedCards` v MongoDB

---

## 🏗️ Architektúra

### Flow Diagram:
```
Battle Complete (executeBattle.js)
         ↓
BattleResultProcessor.ProcessBattleResult()
         ↓
CheckBattleOutcome(myCard, enemyCard)
         ↓
  if (card.health <= 0)
         ↓
HandleCardDeath(deadCard, isMyCard) ← ✅ NOVÝ SYSTÉM
         ↓
    ┌────────────┴────────────┐
    ↓                         ↓
Player.RemoveCardFromBoard()  ServerFunctionsManager.ClearDeadCard()
(UI - zničí GameObject)       (Server - vymaže z selectedCards)
    ↓                         ↓
 Destroy(card.gameObject)  clearSelectedCards.js (selective clear)
```

---

## 📁 Súbory

### 1. **BattleResultProcessor.cs** (Unity Client)
**Lokácia:** `Assets/Scripts/Multiplayer/BattleResultProcessor.cs`

**Upravené metódy:**
```csharp
// UPDATED: Volá HandleCardDeath pri health <= 0
private void CheckBattleOutcome(Kard myCard, Kard enemyCard)
{
    if (myCard.health <= 0 && enemyCard.health <= 0)
    {
        // Obe karty zomreli
        StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
        StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));
    }
    else if (myCard.health <= 0)
    {
        // Len moja karta zomrela
        StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
    }
    else if (enemyCard.health <= 0)
    {
        // Len enemy karta zomrela
        StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));
    }
    // ...
}
```

**Nová metóda:**
```csharp
// NEW: Vymaže kartu z boardu + servera
private IEnumerator HandleCardDeath(Kard deadCard, bool isMyCard)
{
    // 1. Animácia (1s delay pre dramatický efekt)
    yield return new WaitForSeconds(1f);
    
    // 2. Remove z boardu (UI)
    Player owner = isMyCard ? fightSystem.player : fightSystem.enemy;
    owner.RemoveCardFromBoard(deadCard); // Destroy GameObject
    
    // 3. Server call - vymaž zo selectedCards
    serverFunctions.ClearDeadCard(roomCode, cardId, callback);
    
    // 4. Wait for server response (max 5s timeout)
    // ...
}
```

---

### 2. **ServerFunctionsManager.cs** (Unity - API Wrapper)
**Lokácia:** `Assets/Scripts/Networking/ServerFunctionsManager.cs`

**Nová metóda:**
```csharp
/// <summary>
/// Vymaže mŕtvu kartu zo selectedCards na serveri (selective clear)
/// </summary>
public void ClearDeadCard(string roomCode, string cardIdToClear, Action<ExecuteFunctionResult> callback)
{
    Debug.LogWarning($"[ServerFunctionsManager] ClearDeadCard called - roomCode: {roomCode}, cardId: {cardIdToClear}");
    var parameters = new
    {
        roomCode = roomCode,
        cardIdToClear = cardIdToClear
    };
    CallFunction("clearSelectedCards", parameters, callback ?? (_ => { }));
}
```

**Použitie:**
- Volá PlayFab → Vercel endpoint `clearSelectedCards` s `cardIdToClear` parametrom
- Server vymaže len túto kartu, nie všetky selectedCards

---

### 3. **Player.cs** (Unity - Player Model)
**Lokácia:** `Assets/Scripts/Player.cs`

**Upravená metóda:**
```csharp
public void RemoveCardFromBoard(Kard card)
{
    dialogText.text = card.cardName + " failed";
    
    cardsInGame.Remove(card);
    
    // ✅ NEW: Clear cardInGame reference
    if (cardInGame == card)
    {
        cardInGame = null;
    }
    
    Destroy(card.gameObject);
}
```

**Zmeny:**
- Pridané: `cardInGame = null` ak zomrela aktívna karta
- Zabráni NullReferenceException pri prístupe k `player.cardInGame`

---

### 4. **clearSelectedCards.js** (Vercel Server) ✅ ALREADY EXISTS
**Lokácia:** `c:\Zlozka\mega-tresk-server\api\clearSelectedCards.js`

**Funkčnosť:**
```javascript
// V3: Supports selective clear
export default async function handler(req, res) {
  const roomCode = extractParam(req, 'roomCode');
  const cardIdToClear = extractParam(req, 'cardIdToClear'); // ✅ Optional
  
  if (cardIdToClear) {
    // SELECTIVE CLEAR: Find player with this cardId
    const selectedCards = room.selectedCards || {};
    let playerToRemove = null;
    
    for (const [playerId, cardData] of Object.entries(selectedCards)) {
      if (cardData && cardData.cardId === cardIdToClear) {
        playerToRemove = playerId;
        break;
      }
    }
    
    if (playerToRemove) {
      // Remove only this player from selectedCards
      await collection.updateOne(
        { roomCode },
        { $unset: { [`selectedCards.${playerToRemove}`]: "" } }
      );
    }
  } else {
    // LEGACY CLEAR: Clear all selectedCards
    await collection.updateOne(
      { roomCode },
      { $set: { selectedCards: {} } }
    );
  }
}
```

**Key features:**
- ✅ Selective clear: `cardIdToClear` → vymaže len jedného playera
- ✅ Legacy support: bez `cardIdToClear` → vymaže všetko (backward compatibility)
- ✅ Atomic update: `$unset` namiesto `$set` (safe concurrency)

---

## 🔄 Flow Example

### Scenario: Player's card dies (HP = 0)

**1. Battle Complete:**
```javascript
// Server (executeBattle.js)
player1Health = 0;  // Henry Ford zomrel
player2Health = 15; // Dezider prežil

battleResult = {
  player1Health: 0,   // ← Server zistil smrť
  player2Health: 15,
  player1Damage: 22,
  player2Damage: 8
};
```

**2. Client Receives Result:**
```csharp
// Unity (BattleResultProcessor.cs)
myCard.health = 0;  // Sync z servera
enemyCard.health = 15;

CheckBattleOutcome(myCard, enemyCard);
// → myCard.health <= 0 → HandleCardDeath()
```

**3. UI Remove:**
```csharp
// Player.cs
RemoveCardFromBoard(myCard);
// → Destroy(myCard.gameObject);
// → Board je teraz prázdny
```

**4. Server Clear:**
```csharp
// ServerFunctionsManager.cs
ClearDeadCard(roomCode: "ABC123", cardIdToClear: "05b120d1-...")
// → PlayFab → Vercel
```

```javascript
// clearSelectedCards.js
// Find player with cardId = "05b120d1-..."
selectedCards = {
  "player1_id": { cardId: "05b120d1-...", name: "Henry Ford" }, ← Remove this
  "player2_id": { cardId: "dc8f40b2-...", name: "Dezider" }     ← Keep
}

// Update MongoDB
db.rooms.updateOne(
  { roomCode: "ABC123" },
  { $unset: { "selectedCards.player1_id": "" } }
);

// Result:
selectedCards = {
  "player2_id": { cardId: "dc8f40b2-...", name: "Dezider" }
}
```

**5. Complete:**
```
✅ GameObject zničený (board je prázdny)
✅ selectedCards na serveri vymazaný
✅ Ready for PLAYERDEATH state (player chooses new card)
```

---

## 🎮 Unity Setup (Inspector)

### FightSystemMultiplayer GameObject:
- `BattleResultProcessor` component:
  - ✅ `multiplayerService` → MultiplayerService GameObject
  - ✅ `fightSystem` → FightSystemMultiplayer (self reference)
  - ✅ `attackComponent` → Attack component
  - ✅ `dialogText` → TMP_Text UI element
  - ✅ `playerLifeBar` → HealthBar component (player)
  - ✅ `enemyLifeBar` → HealthBar component (enemy)

### Player GameObject:
- `Player` component:
  - ✅ `dialogText` → TMP_Text UI element
  - ✅ `hand` → List<Kard> (inicializovaný empty)
  - ✅ `cardsInGame` → List<Kard>
  - ✅ `isEnemy` → false (pre player), true (pre enemy)

---

## 🐛 Debugging

### Unity Console Logs:
```
[BattleResultProcessor] 💀 Card died: Henry Ford (ID: 05b120d1-..., isMyCard: true)
[BattleResultProcessor] Removing Henry Ford from player board
[BattleResultProcessor] Calling server to clear dead card - roomCode: ABC123, cardId: 05b120d1-...
[ServerFunctionsManager] ClearDeadCard called - roomCode: ABC123, cardId: 05b120d1-...
[BattleResultProcessor] ✅ Dead card cleared from server: Henry Ford (ID: 05b120d1-...)
[BattleResultProcessor] Card death handling complete for Henry Ford
```

### Server Logs (Vercel):
```
[clearSelectedCards] Request received
[clearSelectedCards] Processing roomCode: ABC123, cardIdToClear: 05b120d1-...
[clearSelectedCards] Room found, current selectedCards: { player1_id: {...}, player2_id: {...} }
[clearSelectedCards] 🎯 Selective clear: removing player player1_id with card 05b120d1-...
[clearSelectedCards] Update result - matched: 1, modified: 1
[clearSelectedCards] ✅ Selective clear completed for card: 05b120d1-...
```

### MongoDB Check:
```javascript
db.rooms.findOne({ roomCode: "ABC123" }, { selectedCards: 1 })

// Before:
{
  selectedCards: {
    "player1_id": { cardId: "05b120d1-...", name: "Henry Ford", health: 0 },
    "player2_id": { cardId: "dc8f40b2-...", name: "Dezider", health: 15 }
  }
}

// After:
{
  selectedCards: {
    "player2_id": { cardId: "dc8f40b2-...", name: "Dezider", health: 15 }
  }
}
```

---

## ⚠️ Common Issues

### 1. "ServerFunctionsManager not found!"
**Problém:** `fightSystem.serverFunctionsManager` je null  
**Fix:** Nastav v Unity Inspector - FightSystemMultiplayer → serverFunctionsManager field

### 2. "Owner not found for card!"
**Problém:** `fightSystem.player` alebo `fightSystem.enemy` je null  
**Fix:** Skontroluj či `FightSystemMultiplayer.Start()` správne inicializuje Player/Enemy

### 3. Server call timeout (5s)
**Problém:** Server neodpovedá včas  
**Fix:** 
- Skontroluj Vercel logs - je endpoint deployed?
- Skontroluj PlayFab functions - je `clearSelectedCards` registrovaný?
- Zvýš timeout v `HandleCardDeath` (momentálne 5s)

### 4. Card nie je vymazaná z selectedCards
**Problém:** Server našiel kartu ale neupdate-ovalsa  
**Fix:**
- Skontroluj MongoDB logs - bol `$unset` úspešný?
- Skontroluj `cardId` - zhoduje sa s `selectedCards[playerId].cardId`?

---

## 🚀 Future Enhancements

### PLAYERDEATH State:
```csharp
// TODO: Po smrti karty umožni výber novej karty z ruky
if (myCard.health <= 0 && fightSystem.player.hand.Count > 0)
{
    fightSystem.state = FightStateMultiplayer.PLAYERDEATH;
    dialogText.text = "Choose new fighter";
    // Show hand UI
    // Wait for player to select new card
    // Call selectCardForBattle.js with new cardId
}
```

### Death Animations:
```csharp
// TODO: Lepšie animácie pred zničením
IEnumerator PlayDeathAnimation(Kard card)
{
    // Fade out
    CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
    for (float t = 1f; t >= 0f; t -= Time.deltaTime)
    {
        canvasGroup.alpha = t;
        yield return null;
    }
    
    // Shake
    yield return card.ShakeCard(50f);
    
    // Destroy
    Destroy(card.gameObject);
}
```

### Multi-card Support:
```csharp
// TODO: Keď hráč má viac kariet na boarde (budúcnosť)
List<Kard> deadCards = GetAllDeadCards(player);
foreach (Kard card in deadCards)
{
    StartCoroutine(HandleCardDeath(card, isMyCard: true));
}
```

---

## 📞 Related Files

- `REFACTORING_ARCHITECTURE.md` - Clean architecture overview
- `SERVER_HP_TRACKING.md` - Server-authoritative HP system
- `UNITY_BATTLE_SETUP.md` - Unity Inspector setup guide
- `MULTIPLAYER_PUNCH_SETUP_GUIDE.md` - Initial multiplayer setup

---

**Last Updated:** 2025-10-30  
**Status:** ✅ Implemented, Ready for Testing
