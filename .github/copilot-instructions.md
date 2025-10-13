# Copilot Instructions for MegaTreskKarty# Copilot Instructions for MegaTreskKarty



## 🎯 Hlavný Cieľ Projektu## 🎯 Hlavný Cieľ Projektu



**Prerobenie singleplayer card game na multiplayer** - zachovanie existujúceho gameplay, adaptácia na server-authoritative architektúru.**Prerobenie singleplayer card game na multiplayer** - zachovanie existujúceho gameplay, adaptácia na server-authoritative architektúru.



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

   - Načíta card stats z MongoDB (room.playerDecks)   - Načíta card stats z MongoDB (room.playerDecks)

   - Načíta HP z MongoDB (room.battleState.playerHealths)   - Načíta HP z MongoDB (room.battleState.playerHealths)

   - Simuluje battle (rovnaké vzorce ako Attack.cs)   - Simuluje battle (rovnaké vzorce ako Attack.cs)

   - Uloží nové HP do battleState   - Uloží nové HP do battleState

      

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

    

  battleState: {  battleState: {

    playerHealths: { "player1_id": 85, "player2_id": 72 },    playerHealths: { "player1_id": 85, "player2_id": 72 },

    turnNumber: 3    turnNumber: 3

  },  },

    

  battleData: {  battleData: {

    player1: { cardId, attackId, submitted: true },    player1: { cardId, attackId, submitted: true },

    player2: { cardId, attackId, submitted: false },    player2: { cardId, attackId, submitted: false },

    lastResult: { ... }    lastResult: { ... }

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



**HP sa resetujú každý turn**  **HP sa resetujú každý turn**  

→ Používaš V2, potrebuješ V3 (server-side HP tracking)→ Používaš V2, potrebuješ V3 (server-side HP tracking)



------



**Version:** V3 (Server-Authoritative)  **Version:** V3 (Server-Authoritative)  

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

---

## 📞 Support & Contact

- GitHub Issues: [MegaTreskKarty/issues](https://github.com/jebozlesa/MegaTreskKarty/issues)
- Documentation: Viď README files v roote projektu
- Server logs: Vercel Dashboard → Functions → Logs
- MongoDB: Atlas Dashboard → Browse Collections

---

**Last Updated:** 2025-10-13
**Version:** V3 (Server-Authoritative HP Tracking)
**Current Branch:** Multiplayer