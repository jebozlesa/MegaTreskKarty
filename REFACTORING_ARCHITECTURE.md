# Refaktoring: Clean Architecture + Database-driven Stats

## 🎯 Zmeny

### 1. **Separation of Concerns** - Rozdelenie logiky

**Pred (❌ Zlé):**
```
FightSystemMultiplayer.cs (500+ riadkov)
├─ OnAttackConfirmed()
├─ SubmitAttackToServer()
├─ PollForBattleResult()
├─ ProcessBattleResult()
├─ PlayBattleAnimations()
├─ CheckBattleOutcome()
└─ ... všetko v jednom súbore
```

**Po (✅ Dobré):**
```
FightSystemMultiplayer.cs (200 riadkov)
├─ OnAttackConfirmed() → deleguje na BattleSubmitter
└─ Iba koordinácia

BattleSubmitter.cs
├─ SubmitAttack()
├─ OnBattleResponse()
└─ PollForBattleResult()

BattleResultProcessor.cs
├─ ProcessBattleResult()
├─ PlayBattleAnimations()
└─ CheckBattleOutcome()
```

**Výhody:**
- ✅ Single Responsibility Principle
- ✅ Jednoduchšie testovanie
- ✅ Lepšia čitateľnosť
- ✅ Znovupoužiteľnosť

---

### 2. **Client-side Stats vs Database Stats**

## ❌ Starý prístup (Client posiela stats)

```csharp
// Unity Client
var submission = new AttackSubmission {
    attackId = 1,
    attackerStrength = 30,  // ⚠️ Klient posiela stats
    attackerDefense = 20,
    attackerSpeed = 25,
    defenderStrength = 28,
    defenderDefense = 22,
    defenderSpeed = 20
};
```

```javascript
// Server
function simulateBattle(player1Data, player2Data) {
    // Použije stats priamo z client requestu
    let damage = Math.floor(player1Data.attackerStrength / 3);
    // ⚠️ CHEATING MOŽNÝ - klient môže poslať strength: 9999
}
```

### Problémy:
| Problém | Popis |
|---------|-------|
| 🔓 **Cheating** | Hráč môže upraviť packet inspector a poslať strength: 9999 |
| 🔄 **Nekonzistencia** | Rôzne verzie hry môžu mať rôzne stats |
| 📦 **Veľký payload** | Zbytočne veľké dáta po sieti (10+ properties) |
| 🐛 **Ťažké debugovanie** | Nevieš či stats sú z DB alebo upravené klientom |
| 🔧 **Údržba** | Zmena stats = update na klientovi aj serveri |

---

## ✅ Nový prístup (Server číta z DB)

```csharp
// Unity Client
battleSubmitter.SubmitAttack(
    roomCode: "ABC123",
    playerId: "player1_id",
    cardId: "card_123",      // ✅ Iba ID!
    attackId: 1               // ✅ Iba ID!
);
```

```javascript
// Vercel Server
async function simulateBattle(player1Data, player2Data) {
    // ✅ Server si načíta stats z databázy
    const p1Card = await getCardStatsFromDB(player1Data.cardId);
    const p2Card = await getCardStatsFromDB(player2Data.cardId);
    
    // ✅ Používame verified stats z DB
    let damage = Math.floor(p1Card.strength / 3);
}
```

### Výhody:
| Výhoda | Popis |
|---------|-------|
| 🔒 **Anti-cheat** | Server je single source of truth, klient nemôže podvádzať |
| 🎯 **Konzistencia** | Všetci hráči používajú rovnaké stats z DB |
| 📉 **Menší payload** | Request: `{ cardId: "123", attackId: 1 }` namiesto 10+ properties |
| 🐛 **Jednoduchšie debug** | Stats sú vždy z DB, nie sú ambiguity |
| 🔧 **Centrálna údržba** | Zmena stats = update iba v DB |
| 📊 **Analytics** | Server vidí všetky card usage statistics |

---

## 📊 Porovnanie Payloadov

### Starý prístup:
```json
{
  "roomCode": "ABC123",
  "playerId": "player1_id",
  "attackId": 1,
  "attackerHealth": 100,
  "attackerMaxHealth": 100,
  "attackerStrength": 30,
  "attackerDefense": 20,
  "attackerSpeed": 25,
  "attackerMagic": 15,
  "defenderHealth": 100,
  "defenderMaxHealth": 100,
  "defenderStrength": 28,
  "defenderDefense": 22,
  "defenderSpeed": 20,
  "defenderMagic": 12
}
```
**Veľkosť:** ~350 bytes

### Nový prístup:
```json
{
  "roomCode": "ABC123",
  "playerId": "player1_id",
  "cardId": "card_123",
  "attackId": 1,
  "currentHealth": 100
}
```
**Veľkosť:** ~120 bytes

**Úspora:** 65% redukcia veľkosti!

---

## 🏗️ Nová Architektúra

```
┌─────────────────────────────────────────────────────────────┐
│                      UNITY CLIENT                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  FightSystemMultiplayer.cs                                  │
│       │                                                      │
│       └─> OnAttackConfirmed(attackData)                    │
│              │                                               │
│              ├─> BattleSubmitter.SubmitAttack()            │
│              │        │                                      │
│              │        │ Payload: { cardId, attackId }      │
│              │        │                                      │
│              │        v                                      │
│              │   ServerFunctionsManager                     │
│              │        │                                      │
└──────────────┼────────┼──────────────────────────────────────┘
               │        │
               │        v
┌──────────────┼────────┼──────────────────────────────────────┐
│              │   VERCEL SERVER                              │
├──────────────┼────────┼──────────────────────────────────────┤
│              │        v                                      │
│         executeBattle.js                                    │
│              │                                               │
│              ├─> loadBattleData(roomCode)                   │
│              ├─> saveBattleData(roomCode, data)             │
│              │                                               │
│              ├─> getCardStatsFromDB(cardId)  ← ─ ─ ─ ─ ─   │
│              │        │                              │       │
│              │        v                              │       │
│              │   ┌─────────────────────────┐        │       │
│              │   │   PlayFab TitleData     │ ← ─ ─ ┘       │
│              │   │                          │               │
│              │   │ Card_123 = {            │               │
│              │   │   strength: 30,         │               │
│              │   │   defense: 20,          │               │
│              │   │   speed: 25             │               │
│              │   │ }                        │               │
│              │   └─────────────────────────┘               │
│              │                                               │
│              ├─> simulateBattle()                           │
│              │     - Načíta stats z DB                      │
│              │     - Vypočíta damage                        │
│              │     - Aplikuje efekty                        │
│              │                                               │
│              └─> Return BattleResult                        │
│                        │                                     │
└────────────────────────┼─────────────────────────────────────┘
                         │
                         v
┌────────────────────────┼─────────────────────────────────────┐
│              UNITY CLIENT                                    │
├────────────────────────┼─────────────────────────────────────┤
│                        v                                     │
│         BattleResultProcessor.cs                            │
│              │                                               │
│              ├─> ProcessBattleResult()                      │
│              │     - Aplikuje HP zmeny                      │
│              │     - Aktualizuje UI                         │
│              │                                               │
│              └─> PlayBattleAnimations()                     │
│                    - Prehrá animácie                        │
│                    - Zobrazí dialógy                        │
│                    - Určí víťaza                            │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## 📝 Setup Kroky

### 1. Unity Setup (už hotové)

✅ **BattleSubmitter.cs** - odosielanie útokov  
✅ **BattleResultProcessor.cs** - spracovanie výsledkov  
✅ **FightSystemMultiplayer.cs** - jednoduchá koordinácia  

**Inspector references:**
```
FightSystemMultiplayer:
  - battleSubmitter (BattleSubmitter)
  - battleResultProcessor (BattleResultProcessor)
  
BattleSubmitter:
  - serverFunctionsManager
  - fightSystem
  - resultProcessor
  
BattleResultProcessor:
  - fightSystem
  - attackComponent
  - dialogText
  - playerLifeBar
  - enemyLifeBar
```

### 2. Vercel Setup

📄 **Skopíruj `executeBattle_v2.js` do `/api/executeBattle.js`**

**Environment variables:**
```bash
PLAYFAB_TITLE_ID=tvoj_title_id
PLAYFAB_SECRET_KEY=tvoj_secret_key
```

### 3. PlayFab Database Setup

Musíš nahrať card stats do **TitleData**:

**Formát:**
```
Key: Card_123
Value: {
  "cardId": "123",
  "cardName": "Warrior",
  "strength": 30,
  "defense": 20,
  "speed": 25,
  "magic": 15
}
```

**Bulk import script** (môžem vytvoriť ak chceš):
```csharp
// Unity Editor script na upload všetkých kariet do TitleData
foreach (var card in allCards) {
    PlayFabServerAPI.SetTitleData(new SetTitleDataRequest {
        Key = $"Card_{card.cardId}",
        Value = JsonUtility.ToJson(card.stats)
    });
}
```

### 4. Register v PlayFab

**PlayFab Dashboard:**
1. Automation → CloudScript → Functions
2. Register Function
3. Name: `executeBattle`
4. URL: `https://tvoja-app.vercel.app/api/executeBattle`

---

## 🧪 Testing

### Test 1: Card stats loading
```bash
curl -X POST https://tvoja-app.vercel.app/api/test-card-loading \
  -d '{"cardId": "123"}'
```

### Test 2: Battle simulation
```bash
# Player 1 submission
curl -X POST https://tvoja-app.vercel.app/api/executeBattle \
  -d '{
    "roomCode": "TEST",
    "playerId": "p1",
    "attackData": {
      "cardId": "123",
      "attackId": 1,
      "currentHealth": 100
    }
  }'
```

---

## 🎓 Čo si sa naučil

1. **Separation of Concerns** - každá trieda má jednu zodpovednosť
2. **Server Authority** - server je single source of truth
3. **Database-driven Logic** - stats z DB, nie z klienta
4. **Payload Optimization** - posielaj iba IDs
5. **Anti-cheat** - client nemôže upravovať stats

---

## 🔜 Ďalšie Kroky

1. ✅ Refaktoring hotový
2. ⏳ Nahrať card stats do PlayFab TitleData
3. ⏳ Otestovať Vercel funkciu
4. ⏳ Prvý multiplayer test

Chceš aby som vytvoril Unity Editor script na bulk upload kariet do PlayFab?
