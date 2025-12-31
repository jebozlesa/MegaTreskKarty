# Copilot Instructions for MegaTreskKarty# Copilot Instructions for MegaTreskKarty



## 🎯 Hlavný Cieľ Projektu## 🎯 Hlavný Cieľ Projektu



**Prerobenie singleplayer card game na multiplayer** - zachovanie existujúceho gameplay, adaptácia na server-authoritative architektúru.**Prerobenie singleplayer card game na multiplayer** - zachovanie existujúceho gameplay, adaptácia na server-authoritative architektúru.



---

## 🏗️ CRITICAL: Development Philosophy (KISS Principle)

### ❌ **NEVER Do Quick Fixes or Temporary Solutions**

**User Mandate:**
> "nechceme vobec robit rychle riesenia... chceme najlepsie riesenia... podla principu KISS... nech nespustam zbytocnu funkcionalitu"

> "ked toho budem mat vela tak tam bude do pana boha chyb" (If we accumulate many quick fixes, there will be tons of bugs)

### ✅ **ALWAYS Follow These Principles:**

1. **Quality Over Speed** - Proper architecture beats fast hacks
   - ❌ Temporary workarounds that "work for now"
   - ✅ Clean, maintainable solutions from the start

2. **KISS (Keep It Simple, Stupid)** - Reuse existing code
   - ❌ Duplicate functionality "because it's faster"
   - ✅ Use dedicated endpoints/methods designed for specific purpose
   - Example: Reuse `MultiplayerBoardManager.RevealCards()` instead of writing new card reveal logic

3. **No Unnecessary Functionality** - "nech nespustam zbytocnu funkcionalitu"
   - ❌ Generic "reset everything" functions with side effects
   - ✅ Dedicated methods with single responsibility
   - Example: `ClearBattleData()` only clears battle data, nothing else

4. **Error-Prone Prevention** - "je to potom nachylne na chyby"
   - ❌ Quick fixes accumulate technical debt
   - ✅ Proper error handling, null checks, timeout handling

**When User Suggests Quick Fix:**
- ❌ DON'T implement it immediately
- ✅ Propose proper solution following KISS principle
- ✅ Explain why proper solution is better long-term

---

## 🆔 CRITICAL: ID-Based Player & Card Identification (V11)

**User Mandate:**
> "dopln do instrukcii aby sa hraci a karty v hre definovali na zaklade ich id, nikde nie ako first second, 1 alebo 2 alebo ekvivalenty"

### ✅ **ALWAYS Use ID-Based Identification:**

1. **Cards** - Identify by `cardId` (unique UUID)
   - ✅ `firstAttackerCardId`, `secondAttackerCardId`, `myCardId`, `enemyCardId`
   - ✅ `attackerCard.cardId`, `defenderCard.cardId`
   - ❌ NEVER: `card1`, `card2`, `firstCard`, `secondCard` (ambiguous positions)
   - ❌ NEVER: `isCard1First`, `card1Speed > card2Speed` (confusing)

2. **Players** - Identify by `playerId` (unique string)
   - ✅ `player1.playerId`, `player2.playerId`, `myPlayerId`, `enemyPlayerId`
   - ❌ NEVER: `player1`, `player2`, `firstPlayer`, `secondPlayer` (ambiguous positions)

3. **Battle Roles** - Use semantic role names with IDs
   - ✅ `firstAttacker: { cardId: "uuid", ... }` (role object with ID)
   - ✅ `secondAttacker: { cardId: "uuid", ... }` (role object with ID)
   - ✅ `iAmFirstAttacker = (firstAttackerCardId == myCardId)` (one boolean for role determination)
   - ❌ NEVER: Ternary hell like `isCard1First ? card1.damage : card2.damage`

4. **Response Format** - V11 ID-based structure
   ```javascript
   // ✅ CORRECT (V11)
   {
     firstAttacker: {
       cardId: "uuid",
       damageReceived: 5,  // Clear: what I took
       damageDealt: 2,     // Clear: what I gave
       blocked: true,
       blockedBy: 3
     },
     secondAttacker: { cardId: "uuid", ... }
   }
   
   // ❌ WRONG (DEPRECATED V10)
   {
     firstAttacker: "uuid_string",  // Just an ID, no data
     attacks: {
       "card1_uuid": { damage: 5, blocked: false },  // Confusing!
       "card2_uuid": { damage: 0, blocked: true }
     }
   }
   ```

### 🚫 **Forbidden Patterns:**

```csharp
// ❌ BAD - Position-based (ambiguous)
var card1 = selectedCards[0];
var card2 = selectedCards[1];
if (card1.speed > card2.speed) { ... }

// ✅ GOOD - Role-based with IDs
var myCard = selectedCards.Find(c => c.cardId == myCardId);
var enemyCard = selectedCards.Find(c => c.cardId == enemyCardId);
var iAmFaster = (myCard.speed > enemyCard.speed);
```

```javascript
// ❌ BAD - isCard1First ternary hell
const damage = isCard1First ? firstResult.damage : secondResult.damage;
const blocked = isCard1First ? false : (secondResult.blocked || false);

// ✅ GOOD - Direct role assignment
firstAttacker: {
  damageDealt: firstResult.damage,
  blocked: false  // First attacker never blocked
},
secondAttacker: {
  damageDealt: secondResult.damage,
  blocked: secondResult.blocked  // May be blocked by fresh/existing effects
}
```

### 📚 **Why This Matters:**

1. **Eliminates Confusion** - No mental mapping of positions to roles
2. **Prevents Bugs** - Sleep animation bug was caused by position-based logic
3. **Clear Semantics** - `damageReceived` vs `damageDealt` is self-documenting
4. **Easier Debugging** - Logs show IDs, not ambiguous positions
5. **KISS Principle** - Simpler code = fewer bugs = easier maintenance

**Reference:** See `mega-tresk-server/docs/V11_ID_BASED_RESPONSE_REFACTOR.md` for complete refactoring details.

---

## 📖 CRITICAL: Always Study Attached Documentation

**User Mandate:**
> "dopln do instrukcii ak to tam nieje aby si pri svojich odpovediach studoval aj prilozenu dokumentaciu, nech to aj vyuzijeme ked to robime"

### ✅ **Documentation-First Approach:**

1. **Before Answering** - Check attached documentation:
   - User may attach `.md` files with context
   - Study them BEFORE proposing solutions
   - Documentation contains architecture decisions, patterns, fixes

2. **Available Documentation in Repo:**
   - `CLEANUP_V8_AttackCountRefactor.md` - V8 attack count cleanup reasoning
   - `SERVER_ATTACKCOUNTS_V8_AUTOINIT.md` - Server auto-init system
   - `SERVER_V8_FIX_DECREMENT_MISSING.md` - attackSlot vs attackId fix
   - `REFACTORING_ARCHITECTURE.md` - Overall clean architecture
   - `NETWORK_RETRY_SYSTEM.md` - V7 retry mechanism (3-attempt retry)
   - `KILL_COUNTER_SYSTEM.md` - V7 kill counter & win condition
   - `CARD_REPLACEMENT_SYSTEM.md` - Card death → selection → reveal flow
   - `SERVER_HP_TRACKING.md` - Server-authoritative HP tracking
   - `SERVER_NEXT_TURN_SPEC.md` - Turn system specification

3. **When User Asks Question:**
   - ✅ First check if relevant `.md` doc exists in repo
   - ✅ Read documentation to understand context
   - ✅ Base your answer on documented patterns
   - ❌ Don't reinvent solutions already documented

4. **Why This Matters:**
   - Documentation captures **why** decisions were made
   - Prevents repeating past mistakes
   - Ensures consistency across features
   - Saves time (no re-explaining architecture)

**Example:**
```
User: "How do attack counts work?"
❌ BAD: Explain from scratch
✅ GOOD: "Based on CLEANUP_V8_AttackCountRefactor.md and SERVER_ATTACKCOUNTS_V8_AUTOINIT.md, 
         attack counts use server auto-init + auto-decrement. Let me explain..."
```

---

## 🐛 CRITICAL: Debug Logging Policy

**VŽDY použi Debug.LogWarning alebo Debug.LogError pre dôležité logy!**

**User Mandate:**
> "DOLEZITE! ked pises debug spravy, vzdy nech je to warning, tie bezne info logy mam vypnuty"

### ✅ **Logging Guidelines:**

1. **Debug.LogWarning** - Pre všetky informačné logy ktoré chceš vidieť
   - ❌ `Debug.Log("Attack submitted")` - NEVIDITEĽNÉ v Unity Console (Info logs vypnuté)
   - ✅ `Debug.LogWarning("Attack submitted")` - VIDITEĽNÉ vždy

2. **Debug.LogError** - Pre kritické chyby
   - ✅ `Debug.LogError("❌ executeBattle failed after 3 retries!")`
   - ✅ `Debug.LogError("NullReferenceException: battleSubmitter is null")`

3. **Debug.Log** - NEPOUŽÍVAJ pre debugging
   - ❌ User má Info logs disabled v Unity Console
   - ✅ Použij iba pre ultra-verbose logs ktoré user nemusí vidieť

**Príklad správneho logovania:**
```csharp
// ✅ SPRÁVNE - viditeľné v Console
Debug.LogWarning($"[BattleSubmitter] Submitting attack: attackId={attackId}");
Debug.LogWarning($"[KillCounterManager] Player killed enemy! Count: {playerKillCount}/3");

// ❌ ZLÉ - user to neuvidí!
Debug.Log("Battle submitted");  // Info logs disabled!
```

**Prečo:**
- User má vypnuté Info logs v Unity Console (performance, clarity)
- Debug.Log správy sú kompletne neviditeľné
- Nemožno debugovať bez viditeľných logov
- Warnings sú vždy viditeľné a dobre označené

---

## ⚠️ CRITICAL: Unity Inspector Setup Policy

**VŽDY sa SPÝTAJ pred automatickým riešením missing references!**

Keď chýba referencia (napr. NullReferenceException):
1. ✅ **PREFEROVANÁ METÓDA**: Spýtaj sa užívateľa či nastaviť v Unity Inspector alebo pridať auto-find v kóde
2. ❌ **NErob AUTOMATICKY**: `FindFirstObjectByType<>()` v `Start()` BEZ súhlasu užívateľa
3. ✅ **Inspector setup je čistejší** než runtime auto-discovery (performance, clarity)

**Príklad:**  
```csharp
// ❌ ZLÉ - pridané bez opýtania sa
void Start() {
    multiplayerService = FindFirstObjectByType<MultiplayerService>();
}

// ✅ SPRÁVNE - spýtať sa: "Chceš nastaviť multiplayerService v Inspector alebo pridať auto-find?"
// Užívateľ preferuje Inspector setup
```

------



## 📂 Repository Structure## 📂 Repository Structure



``````

C:\Zlozka\MegaTresk\C:\Zlozka\MegaTresk\

├── MegaTreskKarty\              ← Unity projekt (TENTO REPO)├── MegaTreskKarty\              ← Unity projekt (TENTO REPO)

│   └── Assets\Scripts\│   └── Assets\Scripts\

│       ├── Attack.cs            ← Singleplayer battle logic (123 attacks)│       ├── Attack.cs            ← Singleplayer battle logic (123 attacks)

│       ├── Kard.cs              ← Card model│       ├── Kard.cs              ← Card model

│       ├── Multiplayer\         ← Multiplayer implementation│       ├── Multiplayer\         ← Multiplayer implementation

│       └── Networking\          ← Server communication│       └── Networking\          ← Server communication

││

└── mega-tresk-server\           ← Vercel server (SEPARÁTNY REPO)└── mega-tresk-server\           ← Vercel server (SEPARÁTNY REPO)

    └── api\                     ← Server endpoints    └── api\                     ← Server endpoints

        ├── executeBattle.js     ← Battle simulation        ├── executeBattle.js     ← Battle simulation

        ├── getBattleStatus.js        ├── getBattleStatus.js

        ├── clearBattleData.js        ├── clearBattleData.js

        └── mongodb.js           ← DB helper        └── mongodb.js           ← DB helper

``````



**⚠️ Dôležité:****⚠️ Dôležité:**

- Server zmeny ROB V: `C:\Zlozka\mega-tresk-server\api\`- Server zmeny ROB V: `C:\Zlozka\mega-tresk-server\api\`

- `vercel-api/` v Unity projekte je len READ-ONLY reference- `vercel-api/` v Unity projekte je len READ-ONLY reference

- Deploy: git push → Vercel auto-deploy (~2 min)- Deploy: git push → Vercel auto-deploy (~2 min)



------



## 🎮 Singleplayer Gameplay (Základ)## � Singleplayer Gameplay (Základ)



### Card Model (Kard.cs):### Card Model (Kard.cs):

```csharp```csharp

// Základné stats každej karty:// Základné stats každej karty:

int health          // Aktuálne HPint health          // Aktuálne HP

int strength        // Fyzická silaint strength        // Fyzická sila

int speed           // Priorita útokuint speed           // Priorita útoku

int defense         // Obranaint defense         // Obrana

int knowledge       // Špeciálne útokyint knowledge       // Špeciálne útoky

int attack1-4       // Attack IDs (1-123)int attack1-4       // Attack IDs (1-123)

``````



### Battle System (Attack.cs):### Battle System (Attack.cs):

```csharp```csharp

// Singleplayer battle loop:// Singleplayer battle loop:

1. Player vyberie útok (attack1-4)1. Player vyberie útok (attack1-4)

2. Attack.ExecuteAttack(attacker, receiver, attackId)2. Attack.ExecuteAttack(attacker, receiver, attackId)

3. Vypočíta damage podľa vzorca (napr. Punch: strength/3 - defense/3)3. Vypočíta damage podľa vzorca (napr. Punch: strength/3 - defense/3)

4. Aplikuje efekty (sleep, burn, stun...)4. Aplikuje efekty (sleep, burn, stun...)

5. Animácie cez AttackAnimations.cs5. Animácie cez AttackAnimations.cs

6. Kontrola víťazstva6. Kontrola víťazstva



// Príklad: Punch (Attack ID 1)// Príklad: Punch (Attack ID 1)

damage = (attacker.strength / 3) - (receiver.defense / 3)damage = (attacker.strength / 3) - (receiver.defense / 3)

20% šanca na sleep (1-2 turns)20% šanca na sleep (1-2 turns)

``````



### Turn System:### Turn System:

- Turn-based (hráč → nepriateľ → hráč)- Turn-based (hráč → nepriateľ → hráč)

- Priority útoky (napr. AttackID 82) idú vždy prvé- Priority útoky (napr. AttackID 82) idú vždy prvé

- Attack count limiting (každý útok má max. použití)- Attack count limiting (každý útok má max. použití)



------



## 🌐 Multiplayer Implementation## 🌐 Multiplayer Implementation



### Architektúra (Clean Separation):### Architektúra (Clean Separation):



``````

FightSystemMultiplayer.cs      → Coordinator (deleguje úlohy)FightSystemMultiplayer.cs      → Coordinator (deleguje úlohy)

├─ BattleSubmitter.cs          → Odošle útok na server├─ BattleSubmitter.cs          → Odošle útok na server

├─ BattleResultProcessor.cs    → Spracuje výsledok, animácie├─ BattleResultProcessor.cs    → Spracuje výsledok, animácie

└─ ServerFunctionsManager.cs   → PlayFab API wrapper└─ ServerFunctionsManager.cs   → PlayFab API wrapper

``````



### Client → Server Flow:### Client → Server Flow:



``````

1. Hráč vyberie útok (attackId)1. Hráč vyberie útok (attackId)

2. BattleSubmitter.SubmitAttack(roomCode, playerId, cardId, attackId)2. BattleSubmitter.SubmitAttack(roomCode, playerId, cardId, attackId)

   → Payload: { cardId: "05b120d1...", attackId: 1 }  // Minimálne!   → Payload: { cardId: "05b120d1...", attackId: 1 }  // Minimálne!

      

3. Server dostane request cez PlayFab → Vercel3. Server dostane request cez PlayFab → Vercel

4. executeBattle.js:4. executeBattle.js:

   - ATOMIC UPDATE: Uloží attackData pomocou nested field path (anti race-condition)   - ATOMIC UPDATE: Uloží attackData pomocou nested field path (anti race-condition)

   - Načíta card stats z MongoDB (room.selectedCards)   - Načíta card stats z MongoDB (room.selectedCards)

   - Simuluje battle (rovnaké vzorce ako Attack.cs)   - Simuluje battle (rovnaké vzorce ako Attack.cs)

   - Uloží výsledok do battleData.lastResult   - Uloží výsledok do battleData.lastResult

   - RESET: Nastaví nextTurnReady = {p1:false, p2:false} (fix timeout bug)   - RESET: Nastaví nextTurnReady = {p1:false, p2:false} (fix timeout bug)

      

5. BattleResultProcessor.ProcessBattleResult()5. BattleResultProcessor.ProcessBattleResult()

   - Aktualizuje HP bars   - Aktualizuje HP bars

   - Prehrá animácie (reuse Attack.cs)   - Prehrá animácie (reuse Attack.cs)

   - Skontroluje víťazstvo   - Skontroluje víťazstvo

``````



### Server-Authoritative (V3):### Server-Authoritative (V3):

```javascript```javascript

// Server trackuje HP v MongoDB, NIE klient!// Server trackuje HP v MongoDB, NIE klient!

room.battleState.playerHealths = {room.battleState.playerHealths = {

  "player1_id": 85,  // Server má HP  "player1_id": 85,  // Server má HP

  "player2_id": 72  "player2_id": 72

}}



// Anti-cheat: Klient posiela len IDs, server validuje všetko// Anti-cheat: Klient posiela len IDs, server validuje všetko

``````



------



### 🔢 V8: Attack Count System (Server Auto-Init + Auto-Decrement):### 🔢 V8: Attack Count System (Server Auto-Init + Auto-Decrement):



```javascript```javascript

// ✅ Server auto-initializes on first card selection (setSelectedCard.js)// ✅ Server auto-initializes on first card selection (setSelectedCard.js)

if (!room.attackCounts[playerId][cardId]) {if (!room.attackCounts[playerId][cardId]) {

  counts = calculateAttackCountsLogic(attackIds, stats);  counts = calculateAttackCountsLogic(attackIds, stats);

  await collection.updateOne({ roomCode }, { $set: { [`attackCounts.${playerId}.${cardId}`]: counts } });  await collection.updateOne({ roomCode }, { $set: { [`attackCounts.${playerId}.${cardId}`]: counts } });

}}



// ✅ Server auto-decrements after battle (executeBattle.js)// ✅ Server auto-decrements after battle (executeBattle.js)

const p1Slot = battleData.player1?.attackSlot || battleData.player1?.attackId;  // ⚠️ Use attackSlot (1-4), NOT attackId (1-123)const p1Slot = battleData.player1?.attackSlot || battleData.player1?.attackId;  // ⚠️ Use attackSlot (1-4), NOT attackId (1-123)

await decrementAttackCountLogic(collection, roomCode, player1Id, battleData.player1.cardId, p1Slot, updatedRoom);await decrementAttackCountLogic(collection, roomCode, player1Id, battleData.player1.cardId, p1Slot, updatedRoom);



// ✅ Unity reads counts (AttackCountLoader.cs - READ ONLY)// ✅ Unity reads counts (AttackCountLoader.cs - READ ONLY)

serverFunctionsManager.GetAttackCounts(roomCode, playerId, cardId, result => {serverFunctionsManager.GetAttackCounts(roomCode, playerId, cardId, result => {

  DisplayAttackCounts(result); // { count1, count2, count3, count4 }  DisplayAttackCounts(result); // { count1, count2, count3, count4 }

});});

``````



**⚠️ CRITICAL: attackSlot vs attackId Distinction:**  **⚠️ CRITICAL: attackSlot vs attackId Distinction:**  

- `attackId` = Database ID of attack ability (1-123, varies by attack type: 1=Punch, 2=Kick, 8=Fireball, etc.)- `attackId` = Database ID of attack ability (1-123, varies by attack type: 1=Punch, 2=Kick, 8=Fireball, etc.)

- `attackSlot` = UI button position (1-4, fixed slots on card UI)- `attackSlot` = UI button position (1-4, fixed slots on card UI)

- **Decrement MUST use `attackSlot`** to update correct `count1`/`count2`/`count3`/`count4` field in DB- **Decrement MUST use `attackSlot`** to update correct `count1`/`count2`/`count3`/`count4` field in DB

- Example: Card has Attack2 (Kick) in slot 2 → `attackId=2, attackSlot=2` → decrement `count2`- Example: Card has Attack2 (Kick) in slot 2 → `attackId=2, attackSlot=2` → decrement `count2`

- Example: Card has Attack1 (Punch) in slot 2 → `attackId=1, attackSlot=2` → decrement `count2` (NOT count1!)- Example: Card has Attack1 (Punch) in slot 2 → `attackId=1, attackSlot=2` → decrement `count2` (NOT count1!)



**📚 Documentation:**  **📚 Documentation:**  

- `CLEANUP_V8_AttackCountRefactor.md` - Why V8 cleanup happened, deleted components- `CLEANUP_V8_AttackCountRefactor.md` - Why V8 cleanup happened, deleted components

- `SERVER_ATTACKCOUNTS_V8_AUTOINIT.md` - Server auto-init implementation- `SERVER_ATTACKCOUNTS_V8_AUTOINIT.md` - Server auto-init implementation

- `SERVER_V8_FIX_DECREMENT_MISSING.md` - attackSlot vs attackId bug fix- `SERVER_V8_FIX_DECREMENT_MISSING.md` - attackSlot vs attackId bug fix



------



## 🗄️ MongoDB Schema## 🗄️ MongoDB Schema



```javascript```javascript

rooms: {rooms: {

  roomCode: "ABC123",  roomCode: "ABC123",

  players: ["player1_id", "player2_id"],  players: ["player1_id", "player2_id"],

    

  playerDecks: {  playerDecks: {

    "player1_id": {    "player1_id": {

      cards: [{ CardID, PersonName, Strength, Defense, Speed, ... }]      cards: [{ CardID, PersonName, Strength, Defense, Speed, ... }]

    }    }

  },  },

    

  selectedCards: {  selectedCards: {

    "player1_id": { cardId, name, health, maxHealth, strength, defense, speed, ... },    "player1_id": { cardId, name, health, maxHealth, strength, defense, speed, ... },

    "player2_id": { ... }    "player2_id": { ... }

  },  },

    

  battleData: {  battleData: {

    player1: { cardId, attackId, attackSlot, submitted: true },    player1: { cardId, attackId, attackSlot, submitted: true },

    player2: { cardId, attackId, attackSlot, submitted: false },    player2: { cardId, attackId, attackSlot, submitted: false },

    lastResult: { ... }    lastResult: { ... }

  },  },

    

  // ✅ V8: Attack Counts (Server-side tracking)  // ✅ V8: Attack Counts (Server-side tracking)

  attackCounts: {  attackCounts: {

    "player1_id": {    "player1_id": {

      "card_uuid_1": {      "card_uuid_1": {

        count1: 15,  // Attack slot 1 remaining uses        count1: 15,  // Attack slot 1 remaining uses

        count2: 44,  // Attack slot 2 remaining uses (decrements each use)        count2: 44,  // Attack slot 2 remaining uses (decrements each use)

        count3: 5,   // Attack slot 3 remaining uses        count3: 5,   // Attack slot 3 remaining uses

        count4: 3    // Attack slot 4 remaining uses        count4: 3    // Attack slot 4 remaining uses

      }      }

    },    },

    "player2_id": { ... }    "player2_id": { ... }

  },  },

    

  // ✅ V5: Next turn ready tracking  // ✅ V5: Next turn ready tracking

  nextTurnReady: {  nextTurnReady: {

    "player1_id": false,    "player1_id": false,

    "player2_id": true    "player2_id": true

  }  }

}}

``````



------



## 🔄 Workflow: Pridanie Nového Útoku## 🔄 Workflow: Pridanie Nového Útoku



``````

1. SINGLEPLAYER (Attack.cs):1. SINGLEPLAYER (Attack.cs):

   public IEnumerator MyAttack(Kard attacker, Kard receiver, TMP_Text dialogText) {   public IEnumerator MyAttack(Kard attacker, Kard receiver, TMP_Text dialogText) {

       // Implementuj damage vzorec + animácie       // Implementuj damage vzorec + animácie

   }   }



2. SERVER (mega-tresk-server/api/executeBattle.js):2. SERVER (mega-tresk-server/api/executeBattle.js):

   case X:  // Nové attack ID   case X:  // Nové attack ID

     result = executeMyAttack(p1Stats, p2Stats);     result = executeMyAttack(p1Stats, p2Stats);

     // Použij ROVNAKÝ damage vzorec!     // Použij ROVNAKÝ damage vzorec!



3. MULTIPLAYER (BattleResultProcessor.cs):3. MULTIPLAYER (BattleResultProcessor.cs):

   case X:   case X:

     yield return attackComponent.MyAttack(...);     yield return attackComponent.MyAttack(...);

     // Reuse singleplayer animácie     // Reuse singleplayer animácie



4. UI (AttackSelectionManager.cs):4. UI (AttackSelectionManager.cs):

   // Pridaj button pre attackId = X   // Pridaj button pre attackId = X

``````



------



## 📡 Server Projekt (mega-tresk-server)## 📡 Server Projekt (mega-tresk-server)



### Tech Stack:### Tech Stack:

- Vercel serverless functions (Node.js)- Vercel serverless functions (Node.js)

- MongoDB Atlas driver- MongoDB Atlas driver

- Environment vars: MONGODB_URI, MONGODB_DB- Environment vars: MONGODB_URI, MONGODB_DB



### Dostupné Endpoints:### Dostupné Endpoints:



```javascript```javascript

/api/executeBattle      // Main battle simulation (V3 - server HP tracking)/api/executeBattle      // Main battle simulation (V3 - server HP tracking)

/api/getBattleStatus    // Polling pre výsledok (1s interval)/api/getBattleStatus    // Polling pre výsledok (1s interval)

/api/clearBattleData    // Reset po kole/api/clearBattleData    // Reset po kole

/api/getSelectedCards   // Load player decks/api/getSelectedCards   // Load player decks

/api/loadPlayerDecksIntoRoom  // Init game/api/loadPlayerDecksIntoRoom  // Init game

/api/heartbeat          // Keep-alive/api/heartbeat          // Keep-alive

/api/mongodb            // DB connection helper (shared)/api/mongodb            // DB connection helper (shared)

``````



### Reusable Functions:### Reusable Functions:



```javascript```javascript

// mongodb.js - použiteľné pre všetky endpoints// mongodb.js - použiteľné pre všetky endpoints

import clientPromise from './mongodb';import clientPromise from './mongodb';

const client = await clientPromise;const client = await clientPromise;

const db = client.db();const db = client.db();

const collection = db.collection('rooms');const collection = db.collection('rooms');



// extractParam() - parsovanie PlayFab requestov// extractParam() - parsovanie PlayFab requestov

function extractParam(req, key) {function extractParam(req, key) {

  return req.body?.[key]   return req.body?.[key] 

    || req.body?.FunctionArgument?.[key]    || req.body?.FunctionArgument?.[key]

    || req.query?.[key];    || req.query?.[key];

}}



// getCardStatsFromRoom() - load stats z room.playerDecks// getCardStatsFromRoom() - load stats z room.playerDecks

async function getCardStatsFromRoom(roomCode, playerId, cardId) {async function getCardStatsFromRoom(roomCode, playerId, cardId) {

  const room = await collection.findOne({ roomCode });  const room = await collection.findOne({ roomCode });

  return room.playerDecks[playerId].cards.find(c => c.CardID === cardId);  return room.playerDecks[playerId].cards.find(c => c.CardID === cardId);

}}

``````



### Ako Pridať Nový Endpoint:### Ako Pridať Nový Endpoint:



```javascript```javascript

// 1. Vytvor súbor: api/myNewEndpoint.js// 1. Vytvor súbor: api/myNewEndpoint.js

import clientPromise from './mongodb';import clientPromise from './mongodb';



export default async function handler(req, res) {export default async function handler(req, res) {

  const roomCode = extractParam(req, 'roomCode');  const roomCode = extractParam(req, 'roomCode');

    

  const client = await clientPromise;  const client = await clientPromise;

  const db = client.db();  const db = client.db();

  // ... tvoja logika  // ... tvoja logika

    

  return res.status(200).json({ success: true, data: ... });  return res.status(200).json({ success: true, data: ... });

}}



// 2. Register v PlayFab:// 2. Register v PlayFab:

//    Dashboard → Functions → Register//    Dashboard → Functions → Register

//    URL: https://tvoja-app.vercel.app/api/myNewEndpoint//    URL: https://tvoja-app.vercel.app/api/myNewEndpoint



// 3. Unity volá cez ServerFunctionsManager:// 3. Unity volá cez ServerFunctionsManager:

serverFunctionsManager.CallFunction("myNewEndpoint", params, callback);serverFunctionsManager.CallFunction("myNewEndpoint", params, callback);

``````



------



## 🎯 Naming Conventions## 🎯 Naming Conventions



```csharp```csharp

// Unity C#// Unity C#

public class MyClass { }           // PascalCasepublic class MyClass { }           // PascalCase

private int myField;               // camelCaseprivate int myField;               // camelCase

public void MyMethod() { }         // PascalCasepublic void MyMethod() { }         // PascalCase

const int MAX_VALUE = 100;         // UPPER_SNAKE_CASEconst int MAX_VALUE = 100;         // UPPER_SNAKE_CASE

``````



```javascript```javascript

// Server JavaScript// Server JavaScript

async function myFunction() { }    // camelCaseasync function myFunction() { }    // camelCase

const MY_CONSTANT = 100;           // UPPER_SNAKE_CASEconst MY_CONSTANT = 100;           // UPPER_SNAKE_CASE

const myVariable = { ... };        // camelCaseconst myVariable = { ... };        // camelCase

``````



------



## 🚀 Quick Commands## 🚀 Quick Commands



### Server Development:### Server Development:

```powershell```powershell

cd C:\Zlozka\mega-tresk-servercd C:\Zlozka\mega-tresk-server



# Local test# Local test

vercel dev  # http://localhost:3000vercel dev  # http://localhost:3000



# Deploy# Deploy

git add .git add .

git commit -m "Update logic"git commit -m "Update logic"

git push  # Auto-deploy na Vercelgit push  # Auto-deploy na Vercel



# Test endpoint# Test endpoint

curl -X POST http://localhost:3000/api/executeBattle `curl -X POST http://localhost:3000/api/executeBattle `

  -H "Content-Type: application/json" `  -H "Content-Type: application/json" `

  -d '{"roomCode":"TEST","playerId":"p1","attackData":{"cardId":"c1","attackId":1}}'  -d '{"roomCode":"TEST","playerId":"p1","attackData":{"cardId":"c1","attackId":1}}'

``````



### MongoDB Debug:### MongoDB Debug:

```powershell```powershell

mongosh "mongodb+srv://..."mongosh "mongodb+srv://..."

use megatreskuse megatresk

db.rooms.findOne({ roomCode: "ABC123" })db.rooms.findOne({ roomCode: "ABC123" })

``````



------



## 📚 Dokumentácia## 📚 Dokumentácia



- `REFACTORING_ARCHITECTURE.md` - Prečo clean architecture- `REFACTORING_ARCHITECTURE.md` - Prečo clean architecture

- `SERVER_HP_TRACKING.md` - Prečo server-side HP (anti-cheat)- `SERVER_HP_TRACKING.md` - Prečo server-side HP (anti-cheat)

- `UNITY_BATTLE_SETUP.md` - Inspector setup guide- `UNITY_BATTLE_SETUP.md` - Inspector setup guide

- `DEPLOY_VERCEL_FIX.md` - Troubleshooting- `DEPLOY_VERCEL_FIX.md` - Troubleshooting



------



## 🐛 Common Issues## 🐛 Common Issues



**"Missing required attack data fields"**  **"Missing required attack data fields"**  

→ Server má starú verziu, redeploy `executeBattle.js`→ Server má starú verziu, redeploy `executeBattle.js`



**"NullReferenceException: battleSubmitter"**  **"NullReferenceException: battleSubmitter"**  

→ Unity Inspector: drag GameObjecty do fields→ Unity Inspector: drag GameObjecty do fields



**"Card stats not found in room data"**  **"Card stats not found in room data"**  

→ Zavolaj `loadPlayerDecksIntoRoom` pred hrou→ Zavolaj `loadPlayerDecksIntoRoom` pred hrou



**"Timeout waiting for opponent after first turn"** ⚠️ **CRITICAL****"Timeout waiting for opponent after first turn"** ⚠️ **CRITICAL**  

→ Race condition v `nextTurnReady` - server neresetuje flags po battle  → Race condition v `nextTurnReady` - server neresetuje flags po battle  

→ Fix: `executeBattle.js` musí resetovať `nextTurnReady = {p1:false, p2:false}` po každom battle  → Fix: `executeBattle.js` musí resetovať `nextTurnReady = {p1:false, p2:false}` po každom battle  

→ Riešenie: Pozri `HOTFIX_CRITICAL_NextTurnRace.md`→ Riešenie: Pozri `HOTFIX_CRITICAL_NextTurnRace.md`



**"Súbežné attack submity failujú (race condition)"** ⚠️**"Súbežné attack submity failujú (race condition)"** ⚠️  

→ `battleData` update prepíše celý objekt namiesto atomic update  → `battleData` update prepíše celý objekt namiesto atomic update  

→ Fix: Použiť `$set: { [\`battleData.\${playerKey}.field\`]: value }` namiesto `$set: { battleData: obj }`  → Fix: Použiť `$set: { [\`battleData.\${playerKey}.field\`]: value }` namiesto `$set: { battleData: obj }`  

→ Riešenie: Pozri `SNIPPET_FIX2_AtomicUpdates.js`→ Riešenie: Pozri `SNIPPET_FIX2_AtomicUpdates.js`



------



**Version:** V5 (CardID-based + Next Turn System)  **Version:** V5 (CardID-based + Next Turn System)  

**Branch:** Multiplayer  **Branch:** Multiplayer  

**Updated:** 2025-10-13**Updated:** 2025-10-13

```
FightSystemMultiplayer.cs (Coordinator)
├─ Orchestrates battle flow
├─ Minimal logic - deleguje na komponenty
└─ Referencie:
   ├─ BattleSubmitter - attack submission
   ├─ BattleResultProcessor - result handling
   ├─ ServerFunctionsManager - API wrapper
   └─ MultiplayerService, MultiplayerUI, AttackSelectionManager

BattleSubmitter.cs (Networking Layer)
├─ SubmitAttack(roomCode, playerId, cardId, attackId)
├─ PollForBattleResult() - polling každých 1s (max 60s)
└─ Posiela MINIMÁLNY payload: { cardId, attackId }

BattleResultProcessor.cs (Presentation Layer)
├─ ProcessBattleResult() - parsuje server response
├─ PlayBattleAnimations() - reusuje Attack.cs animácie
├─ UpdateHealthBars() - sync UI s server HP
└─ CheckBattleOutcome() - WIN/LOSS/DRAW detection

ServerFunctionsManager.cs (API Client)
├─ ExecuteBattle(roomCode, playerId, attackData, callback)
├─ GetBattleStatus(roomCode, playerId, callback)
└─ Wrapper pre PlayFabCloudScriptAPI.ExecuteFunction()
```

#### Data Structures:
```csharp
// BattleResult.cs
AttackSubmission {
    string cardId;       // ✅ Server načíta stats z DB
    int attackId;        // ✅ 1 = Punch, 2 = Kick, ...
    // HP fieldy sú DEPRECATED (server ignoruje)
}

BattleResult {
    int player1Health;   // ✅ Authoritative HP zo servera
    int player2Health;   // ✅ Authoritative HP zo servera
    int player1Damage;
    int player2Damage;
    string firstAttacker; // "player1" / "player2"
    bool player1DidSleep;
    int player1SleepDuration;
    // ... same for player2
}
```

### Server-Side (Vercel + MongoDB)

#### executeBattle.js (V3 - Server-Authoritative):
```javascript
// FLOW:
1. Client posiela { cardId, attackId } - BEZ HP!
2. Server načíta card stats z room.playerDecks[playerId]
3. Server načíta/inicializuje HP z room.battleState.playerHealths
4. Simuluje battle s authoritative stats & HP
5. Uloží nové HP do room.battleState.playerHealths
6. Vráti battleResult s novými HP

// KEY FUNCTIONS:
getCardStatsFromRoom(roomCode, playerId, cardId)
  → Načíta { strength, defense, speed, magic, maxHealth } z MongoDB

initializeBattleState(room, p1Id, p2Id, p1Stats, p2Stats)
  → Prvé kolo: HP = maxHealth z DB
  → Ďalšie kolá: HP = room.battleState.playerHealths (persistent)

simulatePunchBattle(player1Data, player2Data)
  → Priority: vyšší speed útočí prvý
  → Damage: (strength/3) - (defense/3), min 1
  → Sleep: 20% šanca, 1-2 turns
```

#### MongoDB Schema (rooms collection):
```javascript
{
  _id: ObjectId("..."),
  roomCode: "ABC123",
  players: ["player1_id", "player2_id"],
  
  // ✅ Card stats načítané z PlayFab pri game start
  playerDecks: {
    "player1_id": {
      cards: [
        {
          CardID: "05b120d1-...",
          PersonName: "Henry Ford",
          Strength: 5,
          Defense: 3,
          Speed: 8,
          Knowledge: 6,
          MaxHealth: 27,
          Attack1: 1,  // Punch
          Attack2: 2,  // Kick
          Attack3: 8,
          Attack4: 41
        }
      ]
    },
    "player2_id": { ... }
  },
  
  // ✅ V3 - Server-side HP tracking
  battleState: {
    playerHealths: {
      "player1_id": 27,  // Aktuálne HP (persistent across turns)
      "player2_id": 22
    },
    turnNumber: 1,
    gameStartTime: ISODate("2025-10-13T10:00:00Z"),
    lastBattleTime: ISODate("2025-10-13T10:05:00Z")
  },
  
  // ✅ Turn-based attack submissions
  battleData: {
    player1: {
      playerId: "player1_id",
      cardId: "05b120d1-...",
      attackId: 1,
      submitted: true,
      timestamp: ISODate("...")
    },
    player2: {
      playerId: "player2_id",
      cardId: "dc8f40b2-...",
      attackId: 1,
      submitted: false
    },
    lastResult: {
      success: true,
      player1Health: 20,
      player2Health: 15,
      player1Damage: 7,
      player2Damage: 2,
      firstAttacker: "player2",
      player1DidSleep: false,
      player2DidSleep: true,
      player2SleepDuration: 2
    }
  },
  
  status: "playing",
  lastActivity: ISODate("...")
}
```

---

## 🔄 Developer Workflows

### Battle System Development:

#### Client-Side Changes:
```
Lokácia: C:\Zlozka\MegaTresk\MegaTreskKarty\Assets\Scripts\Multiplayer\

Workflow:
1. Edituj C# súbory v Unity
2. Test v Unity Editor (Play Mode)
3. Git commit + push (branch: Multiplayer)
4. Build Android APK (ak potrebné)

Common Files:
- FightSystemMultiplayer.cs - main coordinator
- BattleSubmitter.cs - networking
- BattleResultProcessor.cs - presentation
- BattleResult.cs - data structures
```

#### Server-Side Changes:
```
Lokácia: C:\Zlozka\mega-tresk-server\api\

Workflow:
1. Edituj .js súbory v VS Code
2. Test lokálne: vercel dev (http://localhost:3000)
3. Git commit + push → Vercel auto-deploy (~2 min)
4. Test production: Unity → connect to live server

Common Files:
- executeBattle.js - battle simulation (V3)
- getBattleStatus.js - polling endpoint
- clearBattleData.js - turn cleanup
- mongodb.js - DB connection helper
```

#### Synchronizácia Unity ↔ Server:
```
Po zmene servera:
1. Update executeBattle.js v mega-tresk-server/api/
2. Git push → Vercel deploy
3. VOLITEĽNE: Copy do MegaTreskKarty/vercel-api/ (pre reference)
4. Test v Unity s live serverom

Po zmene Unity data structures:
1. Update BattleResult.cs (C#)
2. Updatni server validation v executeBattle.js
3. Deploy oboje
```

### Attack Implementation Workflow:

```
Nový útok (napr. Attack ID 5 - Fireball):

1. SINGLEPLAYER (Attack.cs):
   - Implementuj: public IEnumerator Fireball(Kard attacker, Kard receiver, TMP_Text dialogText)
   - Definuj damage vzorec, efekty, animácie

2. SERVER (executeBattle.js):
   - Pridaj case 5: do simulateBattle()
   - Použij ten istý vzorec ako v Attack.cs
   - Test s curl/Postman

3. CLIENT (BattleResultProcessor.cs):
   - Pridaj case 5: do PlayBattleAnimations()
   - Reusuj Attack.cs animácie: attackComponent.Fireball(...)

4. UI (AttackSelectionManager.cs):
   - Pridaj button/UI pre attack 5
   - Nastav attackId = 5 v OnAttackConfirmed()
```

---

## 🎨 Conventions & Patterns

### Naming Conventions:
```csharp
// Unity C#
public class BattleSubmitter { }        // PascalCase classes
private bool isWaitingForBattle;       // camelCase private fields
public void SubmitAttack() { }          // PascalCase public methods
const int MAX_POLL_ATTEMPTS = 60;      // UPPER_SNAKE_CASE constants
```

```javascript
// Server JavaScript
async function getCardStatsFromRoom() { } // camelCase functions
const DAMAGE_MULTIPLIER = 3;              // UPPER_SNAKE_CASE constants
const battleData = { ... };               // camelCase variables
```

### Architecture Patterns:

#### ✅ CORRECT - Separation of Concerns:
```csharp
// FightSystemMultiplayer - COORDINATOR ONLY
public void OnAttackConfirmed(SelectedAttackData attackData) {
    battleSubmitter.SubmitAttack(roomCode, playerId, cardId, attackId);
    // Žiadna business logic tu!
}

// BattleSubmitter - NETWORKING ONLY
public void SubmitAttack(...) {
    serverFunctionsManager.ExecuteBattle(...);
    StartCoroutine(PollForBattleResult());
}

// BattleResultProcessor - PRESENTATION ONLY
public void ProcessBattleResult(Dictionary<string, object> result) {
    UpdateHealthBars();
    PlayBattleAnimations();
    CheckBattleOutcome();
}
```

#### ❌ WRONG - God Object:
```csharp
// NIE - všetko v jednom súbore
public class FightSystemMultiplayer {
    void OnAttackConfirmed() {
        // 500 riadkov networking kódu
        // 300 riadkov animation kódu
        // 200 riadkov UI update kódu
        // ZLOOOO!
    }
}
```

### Security Patterns:

#### ✅ Server-Authoritative (V3):
```csharp
// Unity CLIENT - posiela iba IDs
var submission = new AttackSubmission {
    cardId = "05b120d1-...",  // ID only
    attackId = 1               // ID only
    // ❌ BEZ HP, strength, defense - server ich má v DB!
};
```

```javascript
// Server - načíta všetko z DB
const cardStats = await getCardStatsFromRoom(cardId);  // strength, defense, speed
const currentHP = room.battleState.playerHealths[playerId];  // HP z DB
// ✅ Klient NEMÔŽE cheatiť!
```

#### ❌ Client-Authoritative (DEPRECATED):
```csharp
// NEPOUŽÍVAJ - vulnerability!
var submission = new AttackSubmission {
    cardId = "...",
    currentHealth = 9999,    // ❌ Klient môže poslať fake HP!
    strength = 9999          // ❌ Klient môže poslať fake stats!
};
```

---

## 🔌 Integration Points

### PlayFab Integration:
```
Role: FUNKCIA REGISTRÁCIA ONLY (nie data storage!)

Setup:
1. PlayFab Dashboard → Automation → CloudScript → Functions
2. Register Function:
   - Name: executeBattle
   - URL: https://tvoja-app.vercel.app/api/executeBattle
3. Unity volá: PlayFabCloudScriptAPI.ExecuteFunction("executeBattle", ...)
4. PlayFab forwarduje request na Vercel
5. Vercel procesuje a vráti response cez PlayFab

❌ NEPOUŽÍVAME:
- PlayFab TitleData (príliš pomalé, nahradené MongoDB)
- PlayFab CloudScript (nahradené Vercel functions)
```

### MongoDB Atlas:
```
Connection: C:\Zlozka\mega-tresk-server\api\mongodb.js

Collections:
- rooms: battle state, player decks, HP tracking
- users: (ak existuje) player profiles
- matches: (budúcnosť) match history

Environment Variables (Vercel):
- MONGODB_URI=mongodb+srv://...
- MONGODB_DB=megatresk
```

### Vercel Deployment:
```
Auto-deploy z Git:
1. Git push do mega-tresk-server repo
2. Vercel detekuje zmenu → build → deploy (~2 min)
3. Funkcie dostupné na: https://tvoja-app.vercel.app/api/*

Environment Variables:
- MONGODB_URI
- MONGODB_DB
- (PLAYFAB_TITLE_ID, PLAYFAB_SECRET_KEY - ak potrebné v budúcnosti)
```

---

## 💡 Examples

### Example 1: Pridanie nového útoku

```csharp
// 1. Attack.cs (Unity Singleplayer)
public IEnumerator Fireball(Kard attacker, Kard receiver, TMP_Text dialogText)
{
    yield return StartCoroutine(ShowAttackDialog(dialogText, attacker.cardName + " casts Fireball!"));
    yield return StartCoroutine(attackAnimations.PlayFireballAnimation(attacker.transform, receiver.transform));
    
    // Damage: (magic/2) - (defense/4)
    int damage = (attacker.magic / 2) - (receiver.defense / 4);
    if (damage < 1) damage = 1;
    
    receiver.TakeDamage(damage);
    yield return StartCoroutine(ShowDialog(dialogText, $"Boom! {damage} fire damage!"));
    
    // 30% burn chance (5 damage per turn, 2 turns)
    if (UnityEngine.Random.value <= 0.3f && !receiver.CheckEffect(4)) // 4 = burn
    {
        yield return StartCoroutine(receiver.AddEffect(4, 2)); // burn, 2 turns
        yield return StartCoroutine(ShowDialog(dialogText, receiver.cardName + " is burning!"));
    }
}
```

```javascript
// 2. executeBattle.js (Server)
function executeFireball(attackerMagic, attackerDefense, attackerHealth, defenderDefense, defenderHealth) {
  // Rovnaký vzorec ako v Unity!
  let damage = Math.floor(attackerMagic / 2) - Math.floor(defenderDefense / 4);
  if (damage < 1) damage = 1;
  
  defenderHealth -= damage;
  if (defenderHealth < 0) defenderHealth = 0;
  
  // 30% burn chance
  let didBurn = false;
  let burnDuration = 0;
  
  if (Math.random() <= 0.3 && defenderHealth > 0) {
    didBurn = true;
    burnDuration = 2;
  }
  
  return {
    damageDealt: damage,
    defenderHealth: defenderHealth,
    attackerHealth: attackerHealth,
    didBurn: didBurn,
    burnDuration: burnDuration
  };
}

// V simulateBattle() pridaj:
case 5: // Fireball
  firstAttackResult = executeFireball(
    firstData.stats.magic,
    firstData.stats.defense,
    firstData.currentHealth,
    secondData.stats.defense,
    secondData.currentHealth
  );
  break;
```

```csharp
// 3. BattleResultProcessor.cs (Unity Multiplayer)
private IEnumerator PlayBattleAnimations(...)
{
    // ...
    if (iAmFirstAttacker)
    {
        yield return StartCoroutine(ShowDialog($"{myCard.cardName} casts Fireball!"));
        
        // Reusuj singleplayer animáciu!
        AttackAnimations animations = attackComponent.attackAnimations;
        yield return StartCoroutine(animations.PlayFireballAnimation(myCard.transform, enemyCard.transform));
        
        yield return StartCoroutine(ShowDialog($"Boom! {damage} fire damage!"));
        
        if (didBurn)
        {
            yield return StartCoroutine(ShowDialog($"{enemyCard.cardName} is burning!"));
        }
    }
    // ...
}
```

### Example 2: Debugging Battle Flow

```csharp
// Unity - pridaj logy
[BattleSubmitter] Submitting attack: roomCode=V4Y4VC, cardId=05b120d1-..., attackId=1
[ServerFunctionsManager] CallFunction: executeBattle
[BattleSubmitter] Polling attempt 1/60
[BattleSubmitter] Both players ready! Processing result...
[BattleResultProcessor] P1=85HP, P2=72HP, firstAttacker=player2
```

```javascript
// Server - zodpovedajúce logy
[executeBattle] Request received: { roomCode: 'V4Y4VC', playerId: 'A36FA...', attackData: {...} }
[Room] Loaded Henry Ford (STR:5, DEF:3, SPD:8, MaxHP:27)
[BattleState] Turn 1: P1=27HP, P2=22HP
[simulatePunchBattle] PLAYER2 attacks first (speed: 9 vs 8)
[executePunch] Calculated damage: 2 (str:8/3 - def:3/3)
[executeBattle] Battle complete! New HP: P1=25, P2=20
```

### Example 3: Testing Server Lokálne

```powershell
# 1. Spusti Vercel dev server
cd C:\Zlozka\mega-tresk-server
vercel dev

# 2. Test executeBattle endpoint
curl -X POST http://localhost:3000/api/executeBattle `
  -H "Content-Type: application/json" `
  -d '{
    "roomCode": "TEST123",
    "playerId": "player1",
    "attackData": {
      "cardId": "test-card-id",
      "attackId": 1
    }
  }'

# 3. Skontroluj MongoDB
mongosh "mongodb+srv://..."
use megatresk
db.rooms.findOne({ roomCode: "TEST123" })
```

---

## 📁 Key Files & Directories

### Unity Project Structure:
```
C:\Zlozka\MegaTresk\MegaTreskKarty\
├── Assets\
│   ├── Scripts\
│   │   ├── Multiplayer\              ← MULTIPLAYER BATTLE SYSTEM
│   │   │   ├── FightSystemMultiplayer.cs (coordinator)
│   │   │   ├── BattleSubmitter.cs (networking)
│   │   │   ├── BattleResultProcessor.cs (presentation)
│   │   │   ├── BattleResult.cs (data structures)
│   │   │   ├── MultiplayerService.cs
│   │   │   ├── MultiplayerUI.cs
│   │   │   ├── MultiplayerHandManager.cs
│   │   │   └── MultiplayerBoardManager.cs
│   │   │
│   │   ├── Networking\               ← SERVER COMMUNICATION
│   │   │   └── ServerFunctionsManager.cs (PlayFab/Vercel wrapper)
│   │   │
│   │   ├── Attack.cs                 ← SINGLEPLAYER ATTACKS (reused v MP)
│   │   ├── AttackAnimations.cs       ← Animácie (shared SP+MP)
│   │   ├── AttackDescriptions.cs
│   │   ├── AttackNamesLoader.cs
│   │   ├── AttackCountLoader.cs
│   │   ├── AttackSelectionManager.cs ← UI pre výber útokov
│   │   │
│   │   ├── Kard.cs                   ← Card model
│   │   ├── Player.cs                 ← Player model
│   │   ├── HealthBar.cs              ← HP UI component
│   │   └── Effects.cs                ← Status effects (sleep, burn, ...)
│   │
│   ├── Scenes\
│   │   ├── Multiplayer.unity         ← Multiplayer battle scéna
│   │   ├── Game.unity                ← Singleplayer scéna
│   │   └── ...
│   │
│   └── Prefabs\
│       ├── Card.prefab
│       └── ...
│
├── vercel-api\                       ← LOCAL REFERENCE (read-only!)
│   └── *.js                          ← Kópie z mega-tresk-server (pre viewing)
│
├── REFACTORING_ARCHITECTURE.md       ← Architecture decision log
├── SERVER_HP_TRACKING.md             ← V3 server-authoritative guide
├── UNITY_BATTLE_SETUP.md             ← Unity inspector setup guide
└── DEPLOY_VERCEL_FIX.md              ← Deployment troubleshooting
```

### Server Project Structure:
```
C:\Zlozka\mega-tresk-server\
├── api\                              ← PRODUCTION SERVER FUNCTIONS
│   ├── executeBattle.js              ← ⭐ V3 battle simulation (server-authoritative)
│   ├── getBattleStatus.js            ← Battle status polling
│   ├── clearBattleData.js            ← Turn cleanup
│   ├── getSelectedCards.js           ← Load selected cards from room
│   ├── loadPlayerDecksIntoRoom.js    ← Initialize player decks from PlayFab
│   ├── heartbeat.js                  ← Keep-alive endpoint
│   ├── mongodb.js                    ← MongoDB connection helper
│   └── ...                           ← Ostatné endpoints
│
├── vercel.json                       ← Vercel config
├── package.json                      ← Dependencies (mongodb, axios, ...)
└── .env                              ← Environment variables (local only, NEcommituj!)
```

### Important Documentation:
```
MegaTreskKarty\
├── REFACTORING_ARCHITECTURE.md
│   ├─ Client-side vs Database-driven stats
│   ├─ Separation of Concerns pattern
│   ├─ Payload optimization (350B → 120B)
│   └─ Setup steps
│
├── SERVER_HP_TRACKING.md
│   ├─ V3 server-authoritative HP tracking
│   ├─ battleState.playerHealths schema
│   ├─ Anti-cheat benefits
│   └─ Spectator/Replay support
│
├── UNITY_BATTLE_SETUP.md
│   ├─ Unity Inspector setup
│   ├─ GameObject references
│   ├─ Attack.cs reuse guide
│   └─ Common errors & fixes
│
├── DEPLOY_VERCEL_FIX.md
│   ├─ Deployment troubleshooting
│   ├─ Version mismatch fixes
│   └─ Testing procedures
│
├── ATTACK_COUNT_SYSTEM_README.md
│   └─ Attack count tracking system
│
├── SERVER_ATTACK_COUNT_IMPLEMENTATION.md
│   └─ Server-side attack count logic
│
└── MULTIPLAYER_PUNCH_SETUP_GUIDE.md
    └─ Initial multiplayer setup (deprecated - see newer docs)
```

---

## 🚀 Quick Start Commands

### Unity Development:
```powershell
# Otvor Unity projekt
cd C:\Zlozka\MegaTresk\MegaTreskKarty
# Otvor v Unity Editor

# Build Android
# Unity → File → Build Settings → Android → Build
```

### Server Development:
```powershell
# Setup
cd C:\Zlozka\mega-tresk-server
npm install

# Local development
vercel dev
# Server runs on http://localhost:3000

# Deploy
git add .
git commit -m "Update executeBattle logic"
git push
# Vercel auto-deploy (~2 min)

# Test production
curl https://tvoja-app.vercel.app/api/executeBattle -X POST -d '{"roomCode":"TEST"}'
```

### MongoDB Operations:
```powershell
# Connect
mongosh "mongodb+srv://user:pass@cluster.mongodb.net/megatresk"

# Query rooms
db.rooms.find({ roomCode: "ABC123" })

# Check battleState
db.rooms.findOne({ roomCode: "ABC123" }, { battleState: 1, battleData: 1 })

# Clear battle data
db.rooms.updateOne(
  { roomCode: "ABC123" },
  { $unset: { battleData: "", battleState: "" } }
)
```

---

## 🎓 Learning Resources

### Študuj tieto súbory v tomto poradí:
1. `REFACTORING_ARCHITECTURE.md` - Pochop architektúru
2. `SERVER_HP_TRACKING.md` - Pochop server-authoritative pattern
3. `BattleResult.cs` - Pozri data structures
4. `BattleSubmitter.cs` - Pozri networking layer
5. `executeBattle.js` - Pozri server-side simulation
6. `Attack.cs` (lines 489-520) - Pozri Punch implementation

### Best Practices:
✅ **Vždy synchronizuj Unity ↔ Server:**
   - Rovnaké damage vzorce
   - Rovnaké effect šance
   - Rovnaké konstant

✅ **Server je single source of truth:**
   - HP iba v `room.battleState.playerHealths`
   - Stats iba v `room.playerDecks[playerId]`
   - Klient akceptuje čo server povie

✅ **Testuj oboje:**
   - Unity Editor (Play Mode)
   - Vercel local dev (`vercel dev`)
   - Production deployment

✅ **Dokumentuj zmeny:**
   - Update README files
   - Komentuj nový kód
   - Log important operations

---

## 🐛 Common Issues & Fixes

### "Missing required attack data fields"
**Problém:** Server očakáva iné fieldy ako klient posiela
**Fix:** Skontroluj či máš najnovšiu verziu executeBattle.js deployed

### "NullReferenceException: battleSubmitter"
**Problém:** Unity Inspector referencie nie sú nastavené
**Fix:** Drag GameObjecty do Inspector fields (viď UNITY_BATTLE_SETUP.md)

### "Card stats not found in room data"
**Problém:** `playerDecks` nie je inicializovaný v room
**Fix:** Zavolaj `loadPlayerDecksIntoRoom.js` pred začiatkom hry

### HP sa "resetujú" každý turn
**Problém:** Používaš starú verziu servera (V2)
**Fix:** Deploy executeBattle_v3.js (server-side HP tracking)

### "Card nie je vymazaná z boardu po smrti"
**Problém:** `HandleCardDeath` nie je volaný v `CheckBattleOutcome`
**Fix:** Skontroluj či `BattleResultProcessor.CheckBattleOutcome` obsahuje `StartCoroutine(HandleCardDeath(...))`

### "Dead card still in selectedCards on server"
**Problém:** `ClearDeadCard` server call failol alebo timeout
**Symptom:** Opponent vidí mŕtvu kartu (health=0) namiesto novej karty
**Log Example:**
```
/CloudScript/ExecuteFunction: connection timed out
[ServerFunctionsManager] 🔴 Network error shown: Server error: clearSelectedCards
[BattleResultProcessor] ❌ Server call returned null result!
```
**Fix:** ✅ **V7 Retry System** - ClearDeadCard má teraz 3-attempt retry!
```
Timeout → 🔄 Retry (3 left) → 🔄 Retry (2 left) → ✅ Success!
Mŕtva karta vymazaná → Opponent vidí novú kartu
```
**Manual Fix:** Skontroluj Vercel logs pre `clearSelectedCards` endpoint, overiť že `cardIdToClear` parameter je správny

### "Network errors during card selection"
**Problém:** Race condition - opponent leaves room počas tvojho card selection
**Log Example:**
```
/CloudScript/ExecuteFunction: {"success":false,"error":"Room not found or player not part of the room"}
[ServerFunctionsManager] 🔴 Network error shown: Server error: setSelectedCard
```
**Fix:** ✅ **V7 Retry System** - SetSelectedCard má teraz retry protection!
```
Room deleted → Retry za 1s → Room recreated → ✅ Card selection success
```

### "Battle submission fails randomly"
**Problém:** MongoDB timeout alebo Vercel cold start
**Log Example:**
```
/CloudScript/ExecuteFunction: connection timed out
[ServerFunctionsManager] 🔴 Network error shown: Server error: executeBattle
```
**Fix:** ✅ **V7 Retry System** - ExecuteBattle má 3 retries, väčšina failov sa opraví automaticky
```
Timeout → 🔄 Retry → 🔄 Retry → ✅ Battle executed
Network indicator shows → Hides after success
```

### "Missing attack data for cards!" after card replacement
**Problém:** Server vracia battle result s MŔTVOU kartou (nie s novou kartou)
**Príčina:** `battleData.lastResult` nie je vyčistený po card replacement
**Fix:** ✅ `ClearBattleData()` sa volá v `HandleEnemyCardDeath` po `RevealCards()` (už implementované)

---

## 📚 Version History & System Updates

### V11: Card Generation System Refactor (December 2025)

**Purpose:** Separate AI cards (Series 1) from player-obtainable cards (Series 2+)

**Changes:**
- **Card Series Logic:** Removed hardcoded "5x Series 1 + 1x Series 2" pack generation
- **New System:** All marketplace pack cards are **Series 2+ ONLY** (no Series 1)
- **Series 1 Reserved:** Series 1 is now **AI-exclusive** (Royal Battle enemies, Campaign)
- **Royal Battle Rewards:** Generate Series 2+ cards (not Series 1)

**Files Modified:**
- `Assets/Scripts/CardGenerator.cs`:
  - `GenerateCardPack()`: Removed `if (i == 5) series = 2` special 6th card logic
  - `AddRandomCardCoroutine()`: Removed `WHERE Series = 1` SQL filter
  - Added `GetRandomAvailableSeries()`: Returns Series 2+ for all player cards

**Key Function:**
```csharp
private int GetRandomAvailableSeries()
{
    return 2; // Current: All player cards are Series 2
    // Future expansion: Random selection from {2, 3, 4...} for rarity tiers
}
```

**Database Structure:**
- **CardDatabase:** Main card definitions (StyleID, PersonName, stats, Series)
- **CardVisuals:** Visual variants per series (Color, Image path)
- **CharacterAttacks:** Attack pool assignments per character (4-8 attacks)
- **Location:** `Assets/StreamingAssets/MyDatabase.db` (source)
- **Runtime:** `Application.persistentDataPath/MyDatabase.db` (copied on first launch)

**Adding New Cards:**
1. Add image to `Assets/Resources/Cards/[name].png`
2. Insert into `CardDatabase` table (StyleID, stats, **Series 2+**)
3. Insert into `CardVisuals` table (CharacterID, Series, Color, Image)
4. Insert into `CharacterAttacks` table (CharacterID, AttackID pool)
5. Delete runtime DB or reinstall to copy new version

**Documentation:** See [CARD_CREATION_GUIDE.md](CARD_CREATION_GUIDE.md) for complete walkthrough

---

## ⚔️ Attack & Effect System (V9)

### Implemented Attacks:

#### Attack ID 1: Punch
- **Damage:** 5 (base) - 2 (receiver defense)
- **Effect:** 50% chance Sleep (duration=1)
- **Notes:** Basic melee attack

#### Attack ID 2: Kick
- **Damage:** 7 (base) - 2 (receiver defense)
- **Effect:** None
- **Notes:** Higher damage, no effects

#### Attack ID 3: Heal
- **Damage:** 0 (no damage dealt)
- **Effect:** Heals self for 4 HP
- **Notes:** Cannot overheal (max = maxHealth)

#### Attack ID 4: Forgiveness (NEW - V9)
- **Damage:** 0 (no HP damage)
- **Effect:** Asceticism with charisma-based chance + Attack stat debuff (-1)
  - Chance: `min(75%, charisma/30)` - Max 75%, scaled by charisma stat
  - Duration: 1-3 turns (random)
- **Notes:** Peaceful attack, applies mental discipline effect
- **Implementation:**
  - Server: `executeForgiveness()` in `attackFunctions.js`
  - Client: Uses existing `Attack.cs` animations
  - No stacking: Ignores if target already has Asceticism or Sleep

#### Attack ID 5: Crusade (NEW - V11)
- **Damage:** `5 + (strength/4) - (defense/4)`, min 1
- **Effect:** `(attack/50)%` chance for permanent -2 defense debuff
- **Notes:** Balanced damage + stat debuff mechanic
- **Implementation:**
  - Server: `executeCrusade()` in `attackFunctions.js`
  - Client: `PlayCrusadeAnimation()` in `AttackAnimations.cs`
  - Debuff is permanent (saved to MongoDB card stats)
  - Defense cannot go below 0

#### Attack ID 6: Water To Wine (NEW - V11)
- **Damage:** 0 (self-buff only)
- **Effect:** Permanent stat changes to ATTACKER
  - Attack stat: +2
  - Strength stat: +1
  - Defense stat: -1
- **Notes:** Self-buff attack (transforms water to wine)
- **Implementation:**
  - Server: `executeWaterToWine()` in `attackFunctions.js`
  - Client: `PlayWaterToWineAnimation()` in `AttackAnimations.cs` (self-animation)
  - All stat changes are permanent (saved to MongoDB)
  - Defense cannot go below 0

### Effect System:

#### Effect Type 2: Asceticism (NEW - V9)
- **Duration:** 2 turns (decrements when blocking: 2→1→0)
- **Effect:** Blocks attacks + 1 HP self-damage per blocked attack
- **Check Timing:** ONLY when card tries to attack (not at turn start)
- **Recovery:** duration=0 → recovered=true, show END animation, icon removed, attack proceeds
- **No Stacking:** Cannot have multiple Asceticism effects
- **Mutual Exclusion:** Cannot coexist with Sleep (whichever applied first stays)
- **Implementation:**
  - Server: `checkAsceticismBlocking()` in `effectManager.js`
  - Server: `willBeBlockedByAsceticism()` helper for Case selection (non-destructive check)
  - Client: `PlayAsceticismStartAnimation()`, `PlayAsceticismBlockAnimation()`, `PlayAsceticismEndAnimation()`
  - Fresh Effect Handling: Asceticism applied in same turn blocks counter-attack immediately

**Asceticism Flow Example:**
```
Turn 1: Player uses Forgiveness → Enemy gets Asceticism (duration=2)
Turn 2: Enemy tries to attack → BLOCKED, self-damage=1, duration 2→1
Turn 3: Enemy tries to attack → BLOCKED, self-damage=1, duration 1→0
Turn 4: Enemy tries to attack → RECOVERED (END animation, icon removed), attack proceeds
```

**Critical Implementation Details:**
- Sleep checks ALL cards at turn start (decrements even if not attacking)
- Asceticism checks ONLY when card attacks (per-attacker timing)
- `willBeBlockedByAsceticism()` used for Case selection (doesn't modify effects)
- `checkAsceticismBlocking()` called when card actually attacks (modifies effects: decrement, removal, self-damage)
- Fresh Asceticism blocks counter-attack in same turn (duration decremented)

#### Effect Type 3: Sleep
- **Duration:** 1-2 turns (decrements: 2→1→0)
- **Effect:** Blocks attack, no self-damage
- **Check Timing:** At turn start for ALL cards
- **Wake Up:** duration=0 → wokeUp=true, show wake animation, attack proceeds
- **No Stacking:** Cannot stack Sleep effects
- **Mutual Exclusion:** Cannot coexist with Asceticism

### Effect Stacking Rules:

**Design Philosophy:** Effects mirror reality - players should understand WHY effects behave as they do.

#### Blocking Effects (NO STACKING):
- **Sleep (Type 3):** Cannot stack - môžeš spať len raz naraz
- **Asceticism (Type 2):** Cannot stack - jeden duchovný stav
- **Rule:** First blocking effect applied wins, blocks new applications
- **Mutual Exclusion:** Sleep ↔ Asceticism cannot coexist

```javascript
// Server (executeForgiveness):
if (hasEffect(target, EffectTypes.ASCETICISM)) {
  console.log("Already has Asceticism, IGNORING new application!");
  return { damage: 0, effectApplied: null };  // Attack debuff still applied
}

// Server (executePunch/executeKick for Sleep):
if (hasEffect(target, EffectTypes.SLEEP)) {
  console.log("Already has Sleep, IGNORING new application!");
  return { damage: calculatedDamage, didSleep: false };
}
```

#### Damage-Over-Time Effects (YES STACKING):
- **Bleed (Type 1):** CAN stack - viac rán = viac krvácajúcich rán
- **Reason:** Realistic - each wound bleeds independently
- **Implementation:** `applyEffect(card, EffectTypes.BLEED, { allowStacking: true })`
- **Effect Behavior:** Each bleed effect decrements separately, damage = duration value

```javascript
// Server (executeCarHit):
const bleedEffect = applyEffect(defenderCard, EffectTypes.BLEED, {
  duration: 4,
  allowStacking: true  // ✅ Multiple bleeds can coexist
});

// Result: Card can have effects = [
//   { type: 1, duration: 4 },  // First bleed (4 turns)
//   { type: 1, duration: 2 }   // Second bleed (2 turns)
// ]
// Turn X: Takes 4+2=6 damage, both decrement → 3+1=4 damage next turn
```

**Priority:** EXISTING blocking effect prevents NEW blocking effect (no replacement)

### Server Battle Flow (executeBattle.js):

**V11 Note:** Server internally uses `firstCard`/`secondCard` for battle simulation (speed-based), but **response format** uses ID-based `firstAttacker`/`secondAttacker` objects. Client never sees internal simulation variables.

```javascript
// 1. Determine attack order by speed (internal simulation only)
const firstCard = (card1.speed > card2.speed) ? card1 : card2;
const secondCard = (firstCard === card1) ? card2 : card1;

// 2. Sleep check - ALL cards at turn start
const card1SleepCheck = checkSleepBlocking(card1, attackId1);
const card2SleepCheck = checkSleepBlocking(card2, attackId2);

// 3. Asceticism pre-check - determine Cases WITHOUT modifying effects
const firstCardWillBeBlocked = willBeBlockedByAsceticism(firstCard);
const secondCardWillBeBlocked = willBeBlockedByAsceticism(secondCard);

// 4. Case selection:
// Case 1: Both blocked → call checkAsceticismBlocking() for BOTH
// Case 2: First blocked → call checkAsceticismBlocking() for first only
// Case 4: Normal battle → 
//   - Check firstCard Asceticism if duration=0 (recovery)
//   - First attack executes
//   - Check fresh effects on secondCard
//   - Check existing Asceticism on secondCard (if not fresh)
//   - Second attack executes (if not blocked)

// 5. Result object includes:
// - recovered: true/false (Asceticism recovery happened)
// - selfDamage: 0 or 1 (Asceticism self-damage)
// - blocked: true/false
// - blockedBy: 2 (Asceticism) or 3 (Sleep)
```

---

## 📞 Support & Contact

- GitHub Issues: [MegaTreskKarty/issues](https://github.com/jebozlesa/MegaTreskKarty/issues)
- Documentation: Viď README files v roote projektu
  - `NETWORK_RETRY_SYSTEM.md` - **V7 NEW!** Complete retry mechanism documentation (3-attempt retry, visual feedback)
  - `KILL_COUNTER_SYSTEM.md` - **V7 NEW!** Kill counter & win condition system (3 kills = win)
  - `CARD_REPLACEMENT_SYSTEM.md` - Complete card replacement guide (death → selection → reveal → ClearBattleData)
  - `CARD_DEATH_SYSTEM.md` - Card death handling (remove from board + server)
  - `REFACTORING_ARCHITECTURE.md` - Clean architecture overview (KISS principle)
  - `SERVER_HP_TRACKING.md` - Server-authoritative HP tracking
  - `CLEANUP_V8_AttackCountRefactor.md` - **V8 NEW!** Attack count cleanup & refactoring (server auto-init + auto-decrement)
  - `SERVER_ATTACKCOUNTS_V8_AUTOINIT.md` - **V8 NEW!** Server attack count auto-initialization system
  - `SERVER_V8_FIX_DECREMENT_MISSING.md` - **V8 NEW!** attackSlot vs attackId bug fix documentation
- Server logs: Vercel Dashboard → Functions → Logs
- MongoDB: Atlas Dashboard → Browse Collections

---

**Last Updated:** 2025-12-30  
**Version:** V11 (ID-Based Response Format Refactor)  
**Current Branch:** Multiplayer  

**Key Changes in V11 (CURRENT):**
- ✅ **ID-Based Identification** - Players & cards identified by IDs, not positions (first/second/1/2)
- ✅ **Response Format Refactor** - `firstAttacker`/`secondAttacker` objects with cardId, damageReceived, damageDealt
- ✅ **Sleep Animation Bug Fix** - Blocked attacks now show correct blocking animation (not 0 damage attack)
- ✅ **Eliminated isCard1First Logic** - No more position-based ternary confusion
- ✅ **Clear Semantics** - `damageReceived` (what I took) vs `damageDealt` (what I gave)
- ✅ **Client Role Determination** - One boolean `iAmFirstAttacker` determines all data access
- ✅ **Server Simplification** - Direct role assignment instead of card1/card2 mapping
- ✅ **KISS Principle Applied** - Simpler code, fewer bugs, easier debugging
- ✅ **Documentation** - Complete refactoring guide in V11_ID_BASED_RESPONSE_REFACTOR.md

**Key Changes in V9 (Previous):**
- ✅ **Attack ID 4: Forgiveness** - Peaceful attack (0 HP damage, -1 attack debuff, 100% Asceticism)
- ✅ **Asceticism Effect** - Blocking effect with self-damage (duration=2, per-attacker timing)
- ✅ **Effect No-Stacking** - Same effect types never stack (first applied wins)
- ✅ **Mutual Exclusion** - Sleep ↔ Asceticism cannot coexist
- ✅ **Fresh Effect Handling** - Effects applied in same turn block counter-attacks
- ✅ **Recovery System** - duration=0 triggers END animation, icon removal, attack proceeds
- ✅ **Per-Attacker Timing** - Asceticism checked ONLY when card attacks (unlike Sleep's global check)
- ✅ **willBeBlockedByAsceticism()** - Non-destructive helper for Case selection
- ✅ **Complete Animation System** - START, BLOCK, END animations for Asceticism
- ✅ **Server Architecture** - Proper Case handling for battle scenarios (both blocked, first blocked, normal)

**Key Changes in V8 (Previous):**
- ✅ **Critical Bug Fix** - Attack counts now decrement correctly (attackSlot vs attackId fix)
- ✅ **Server Auto-Decrement** - executeBattle.js automatically decrements attack counts after battles
- ✅ **Comprehensive Cleanup** - Removed 6 deprecated files/components (client-side decrement system)
- ✅ **Simplified Architecture** - Server-only attack count management (no client-side calculation/decrement)
- ✅ **Unity Read-Only** - AttackCountLoader.cs now only reads from server (GetAttackCounts)
- ✅ **KISS Principle Applied** - Eliminated duplicate functionality, single source of truth
- ✅ **MongoDB Schema Updated** - Added attackCounts field to Copilot instructions
- ✅ **Documentation Complete** - 3 new V8 docs + updated Copilot instructions with V8 architecture

**Key Changes in V7 (Previous):**
- ✅ Network Retry Mechanism (3-attempt retry with exponential backoff)
- ✅ Kill Counter System (visual tracking, 3 kills = win)
- ✅ Network Error Indicator (visual feedback during errors)
- ✅ Debug Logging Policy (Debug.LogWarning for important logs)
- ✅ Dead Card Bug Fix (retry prevents zombie cards)