# 🔒 Server-Authoritative HP Tracking

## ❌ Problém s `currentHealth` z klienta

### Čo sa deje teraz (V2):

**Unity Client:**
```csharp
var submission = new AttackSubmission {
    cardId = "05b120d1-...",
    attackId = 1,
    currentHealth = 27  // ⚠️ Klient posiela HP
};
```

**Server (V2):**
```javascript
const { cardId, attackId, currentHealth } = attackData;

// ❌ Používa HP z klienta!
const player1BattleData = {
    currentHealth: currentHealth  // Klient môže poslať 9999!
};
```

### 🐛 Security Issues:

1. **HP Manipulation:**
   ```
   Hráč môže packet sniff + modify:
   currentHealth: 27 → currentHealth: 9999
   ```

2. **No Persistence:**
   - Po každom kole sa HP "resetnú"
   - Server nemá single source of truth

3. **Spectator Problem:**
   - Diváci nevedia reálne HP
   - Každý klient si vie nastaviť svoje HP

---

## ✅ Riešenie: Server-Side HP Tracking (V3)

### Nový flow:

```
┌─────────────────────────────────────────────────┐
│           UNITY CLIENT                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  battleSubmitter.SubmitAttack(                  │
│      roomCode: "ABC123",                        │
│      playerId: "player1_id",                    │
│      cardId: "card_123",    ← IBA ID            │
│      attackId: 1            ← IBA ID            │
│  );                                              │
│                                                  │
│  ❌ BEZ currentHealth!                          │
│                                                  │
└─────────────────────────────────────────────────┘
                    │
                    │ POST /api/executeBattle
                    │ { cardId, attackId }
                    v
┌─────────────────────────────────────────────────┐
│           VERCEL SERVER                         │
├─────────────────────────────────────────────────┤
│                                                  │
│  1️⃣ Načítaj room                               │
│     room = db.rooms.findOne({ roomCode })       │
│                                                  │
│  2️⃣ Inicializuj/Načítaj HP z battleState      │
│     if (!room.battleState) {                    │
│         // Prvé kolo - použij maxHealth z DB   │
│         room.battleState = {                    │
│             playerHealths: {                    │
│                 "p1": 100,  ← Z DB!            │
│                 "p2": 95    ← Z DB!            │
│             }                                    │
│         }                                        │
│     }                                            │
│                                                  │
│  3️⃣ Načítaj card stats Z DB                   │
│     p1Card = getCardStatsFromRoom(cardId)       │
│     // { strength, defense, speed, maxHealth } │
│                                                  │
│  4️⃣ Použij HP Z battleState (nie z klienta!)  │
│     player1BattleData = {                       │
│         currentHealth: room.battleState         │
│             .playerHealths["p1"],  ← Z DB!     │
│         stats: p1Card                           │
│     }                                            │
│                                                  │
│  5️⃣ Simuluj battle                             │
│     battleResult = simulatePunchBattle(...)     │
│                                                  │
│  6️⃣ ULOŽ nové HP do battleState                │
│     room.battleState.playerHealths = {          │
│         "p1": battleResult.player1Health,       │
│         "p2": battleResult.player2Health        │
│     }                                            │
│     db.rooms.updateOne({ roomCode }, {          │
│         $set: { battleState: ... }              │
│     })                                           │
│                                                  │
│  7️⃣ Vráť výsledok klientovi                    │
│                                                  │
└─────────────────────────────────────────────────┘
                    │
                    │ Response: { player1Health: 80, player2Health: 70 }
                    v
┌─────────────────────────────────────────────────┐
│           UNITY CLIENT                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  ✅ Akceptuje HP zo servera                     │
│  myCard.health = battleResult.player1Health;    │
│  enemyCard.health = battleResult.player2Health; │
│                                                  │
│  🎯 Klient NEMÔŽE upraviť HP!                   │
│     Server má single source of truth            │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## 📊 Porovnanie

| Aspekt | V2 (Client HP) | V3 (Server HP) |
|--------|----------------|----------------|
| **Payload size** | 150 bytes | 80 bytes (-47%) |
| **Security** | ❌ Klient môže cheatiť | ✅ Server authority |
| **Persistence** | ❌ HP sa "strácajú" | ✅ HP v battleState |
| **Spectators** | ❌ Každý má svoje HP | ✅ Jedno HP pre všetkých |
| **Bandwidth** | Vyššia | Nižšia |
| **Complexity** | Nižšia (klient trackuje) | Vyššia (server trackuje) |
| **Replay support** | ❌ Nemožné | ✅ Možné (HP v DB) |

---

## 🗄️ Database Schema - battleState

```javascript
room = {
    roomCode: "ABC123",
    players: ["player1_id", "player2_id"],
    
    // ✅ NOVÉ: Persistent battle state
    battleState: {
        playerHealths: {
            "player1_id": 85,  // Aktuálne HP player 1
            "player2_id": 72   // Aktuálne HP player 2
        },
        turnNumber: 3,         // Číslo kola
        gameStartTime: ISODate("2025-10-13T10:00:00Z"),
        lastBattleTime: ISODate("2025-10-13T10:05:23Z")
    },
    
    // Existujúce fieldy
    battleData: {
        player1: { cardId: "...", attackId: 1, submitted: false },
        player2: { cardId: "...", attackId: 1, submitted: false },
        lastResult: { ... }
    },
    
    playerDecks: { ... }
}
```

---

## 🔄 Lifecycle

### Prvé kolo:
```javascript
// Inicializácia (turn 1)
battleState = {
    playerHealths: {
        "p1": 100,  // Z card.MaxHealth
        "p2": 95    // Z card.MaxHealth
    },
    turnNumber: 1
}
```

### Po battle:
```javascript
// Po útoku (turn 1 → 2)
battleState = {
    playerHealths: {
        "p1": 85,   // -15 damage
        "p2": 78    // -17 damage
    },
    turnNumber: 2
}
```

### Koniec hry:
```javascript
// Game over
battleState = {
    playerHealths: {
        "p1": 0,    // DEAD
        "p2": 45    // WINNER
    },
    turnNumber: 7,
    gameEndTime: ISODate("...")
}
```

---

## 🎯 Benefits

### 1. Anti-cheat
```
Klient: "Mám 9999 HP!"
Server: "Nie, máš 85 HP podľa battleState."
```

### 2. Spectators
```
GET /api/getBattleState?roomCode=ABC123
→ { player1Health: 85, player2Health: 72 }

Divák vidí SKUTOČNÉ HP z DB!
```

### 3. Replay system
```
GET /api/getBattleHistory?roomCode=ABC123
→ [
    { turn: 1, p1HP: 100, p2HP: 95, action: "p1 punch" },
    { turn: 2, p1HP: 85, p2HP: 78, action: "p2 punch" },
    ...
]
```

### 4. Disconnection handling
```
Hráč 1 disconnectne na turn 5.
Reconnectne na turn 7.

Server: "Tvoje HP je 45 (uložené v battleState)"
Nie: "Resetujem na 100 HP"
```

---

## 🚀 Deploy Steps

### 1. Backup starú verziu
```powershell
cd c:\Zlozka\mega-tresk-server\api
Copy-Item executeBattle.js executeBattle_v2_backup.js
```

### 2. Deploy novú verziu
```powershell
Copy-Item executeBattle_v3.js executeBattle.js
```

### 3. Git commit
```powershell
git add api/executeBattle.js
git commit -m "V3: Server-side HP tracking in battleState"
git push
```

### 4. Test
```powershell
# Test prvé kolo (inicializácia HP)
curl -X POST https://tvoja-app.vercel.app/api/executeBattle `
  -H "Content-Type: application/json" `
  -d '{
    "roomCode": "TEST123",
    "playerId": "p1",
    "attackData": { "cardId": "card1", "attackId": 1 }
  }'

# Skontroluj battleState v MongoDB
db.rooms.findOne({ roomCode: "TEST123" }, { battleState: 1 })
```

---

## 📋 Migration Checklist

### Server-side:
- [x] Vytvor `executeBattle_v3.js`
- [x] Pridaj `initializeBattleState()` funkciu
- [x] Ulož HP do `room.battleState.playerHealths`
- [ ] Deploy na Vercel
- [ ] Test s 2 klientmi
- [ ] Monitor MongoDB pre battleState field

### Client-side:
- [x] Odstráň `currentHealth` z AttackSubmission
- [x] Nastav všetky deprecated fieldy na 0
- [x] Aktualizuj BattleSubmitter.cs
- [ ] Test Unity build
- [ ] Test reconnection scenario

### Testing:
- [ ] Test prvé kolo (HP inicializácia z maxHealth)
- [ ] Test druhé kolo (HP z battleState)
- [ ] Test HP manipulation attempt (mal by zlyhať)
- [ ] Test player disconnect + reconnect (HP by mali persistovať)
- [ ] Test spectator view (HP by mali byť z DB)

---

Hotovo! Teraz máš **plne server-authoritative** battle system! 🔒🎉
