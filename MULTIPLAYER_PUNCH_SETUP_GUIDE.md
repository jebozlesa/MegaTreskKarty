# Multiplayer Battle System - Punch (Attack ID 1) Implementation

## 🎯 Prehľad

Jednoduchý multiplayer battle systém implementujúci **Punch útok (ID 1)** s kompletným client-server flow.

## 📦 Vytvorené súbory

### Unity C# (Client)
1. **BattleResult.cs** - Dátové štruktúry pre battle výsledky
2. **ServerFunctionsManager.cs** - Pridané `ExecuteBattle()` metóda
3. **FightSystemMultiplayer.cs** - Kompletný battle flow
4. **AttackSelectionManager.cs** - Odstránená duplicita SelectedAttackData

### Vercel (Server)
5. **vercel-api/executeBattle.js** - Server-side battle simulácia

## 🚀 Ako to funguje (Flow)

```
KROK 1: Hráč vyberie útok
  ↓
  AttackSelectionManager.OnConfirmAttackClicked()
  ↓
  FightSystemMultiplayer.OnAttackConfirmed()

KROK 2: Odoslanie na server
  ↓
  FightSystemMultiplayer.SubmitAttackToServer()
  ↓
  ServerFunctionsManager.ExecuteBattle()
  ↓
  POST /api/executeBattle (Vercel funkcia)

KROK 3: Server simulácia
  ↓
  Uloží attack data pre player1
  ↓
  Čaká na player2
  ↓
  Keď obaja submitnú → simulatePunchBattle()
  ↓
  - Určí priority (speed)
  - Vypočíta damage: (strength/3) - (defense/3)
  - 20% šanca na sleep
  - Vráti výsledok

KROK 4: Client spracovanie
  ↓
  FightSystemMultiplayer.ProcessBattleResult()
  ↓
  - Aplikuje HP zmeny
  - Aktualizuje HealthBary
  ↓
  PlayBattleAnimations()
  ↓
  - Prehráva Punch animácie
  - Zobrazuje dialógy
  ↓
  CheckBattleOutcome()
  ↓
  - Určí víťaza/porazeného
```

## 📋 Setup (Krok za krokom)

### A) Unity Setup

#### 1. Skontroluj referencie v Inspectore

Otvor scénu **Multiplayer Fight** a vyber **FightSystemMultiplayer** GameObject:

```
FightSystemMultiplayer:
  ✓ player (Player component)
  ✓ enemy (Player component)
  ✓ playerBoard (GameObject)
  ✓ enemyBoard (GameObject)
  ✓ playerLifeBar (HealthBar)
  ✓ enemyLifeBar (HealthBar)
  ✓ dialogText (TMP_Text)
  ✓ attack (Attack component)
  ✓ serverFunctionsManager (ServerFunctionsManager)
  ✓ multiplayerUI (MultiplayerUI)
  ✓ multiplayerService (MultiplayerService)
  ✓ attackSelectionManager (AttackSelectionManager)
```

#### 2. Skontroluj Attack komponent

Uisti sa že `Attack` komponent má:
```
Attack:
  ✓ attackAnimations (AttackAnimations component)
```

### B) Vercel Setup

#### 1. Registruj funkciu v PlayFab

Ak používaš Vercel funkcie registrované cez PlayFab, musíš:

1. Otvor **PlayFab Dashboard**
2. Idi na **Automation** → **CloudScript** → **Functions**
3. Klikni **Register Function**
4. Zadaj:
   ```
   Function Name: executeBattle
   Function URL: https://tvoja-vercel-app.vercel.app/api/executeBattle
   ```

#### 2. Deploy Vercel funkciu

1. Skopíruj `vercel-api/executeBattle.js` do tvojho Vercel projektu
2. Umiestni ho do `/api/executeBattle.js`
3. Deploy:
   ```bash
   vercel --prod
   ```

#### 3. Test funkcie

Otestuj či funkcia beží:
```bash
curl -X POST https://tvoja-vercel-app.vercel.app/api/executeBattle \
  -H "Content-Type: application/json" \
  -d '{
    "roomCode": "TEST123",
    "playerId": "player1",
    "attackData": {
      "attackerStrength": 30,
      "attackerDefense": 20,
      "attackerSpeed": 25,
      "attackerHealth": 100,
      "defenderDefense": 18,
      "defenderHealth": 100
    }
  }'
```

Očakávaná odpoveď:
```json
{
  "success": true,
  "bothPlayersReady": false,
  "playersReady": 1
}
```

### C) Databáza/Storage pre Battle Data

Vercel funkcia potrebuje ukladať battle data. Máš 2 možnosti:

#### Možnosť 1: PlayFab TitleData (odporúčané)

Upravíš `executeBattle.js` aby volal PlayFab API:

```javascript
const axios = require('axios');

async function saveBattleData(roomCode, playerId, battleData) {
    const response = await axios.post(
        `https://${process.env.PLAYFAB_TITLE_ID}.playfabapi.com/Server/SetTitleData`,
        {
            Key: `Battle_${roomCode}`,
            Value: JSON.stringify(battleData)
        },
        {
            headers: {
                'X-SecretKey': process.env.PLAYFAB_SECRET_KEY
            }
        }
    );
    return response.data;
}

async function loadBattleData(roomCode) {
    const response = await axios.post(
        `https://${process.env.PLAYFAB_TITLE_ID}.playfabapi.com/Server/GetTitleData`,
        {
            Keys: [`Battle_${roomCode}`]
        },
        {
            headers: {
                'X-SecretKey': process.env.PLAYFAB_SECRET_KEY
            }
        }
    );
    
    const data = response.data.data.Data[`Battle_${roomCode}`];
    return data ? JSON.parse(data) : null;
}
```

Pridaj do Vercel environment variables:
```
PLAYFAB_TITLE_ID=tvoj_title_id
PLAYFAB_SECRET_KEY=tvoj_secret_key
```

#### Možnosť 2: Vercel KV (Redis)

```bash
npm install @vercel/kv
```

```javascript
import { kv } from '@vercel/kv';

async function saveBattleData(roomCode, playerId, battleData) {
    await kv.set(`Battle_${roomCode}`, battleData);
}

async function loadBattleData(roomCode) {
    return await kv.get(`Battle_${roomCode}`);
}
```

## 🧪 Testovanie

### Test 1: Solo submission (čakanie na oponenta)

1. Spusti Unity Editor (Player 1)
2. Pripoj sa do multiplayeru
3. Vyber kartu
4. Klikni na útok "Punch"
5. Potvrď výber

**Očakávaný výsledok:**
```
Console:
[FightSystemMultiplayer] Attack confirmed: Type=1, ID=1
[FightSystemMultiplayer] Submitting attack to server...
[FightSystemMultiplayer] Waiting for opponent... (1/2)

UI:
"Waiting for opponent..."
```

### Test 2: Obaja hráči submitnú

1. Spusti 2 Unity Editory (alebo build + editor)
2. Obaja sa pripoja do rovnakej room
3. Obaja vyberú karty
4. Obaja vyberú "Punch" útok
5. Obaja potvrdením

**Očakávaný výsledok:**
```
Console (oboch hráčov):
[FightSystemMultiplayer] Battle result received!
[FightSystemMultiplayer] Processing battle result
[FightSystemMultiplayer] {player1} uses Punch!
[FightSystemMultiplayer] Hit! X damage!
[FightSystemMultiplayer] {player2} uses Punch!
[FightSystemMultiplayer] Hit! Y damage!

Animácie:
- Punch animation prehraná
- HP bary aktualizované
- Dialógy zobrazené

Výsledok:
"You Won!" / "You Lost!" / "Next turn!"
```

### Test 3: Server simulácia

Otestuj damage calculation:

**Setup:**
- Player 1: strength=30, defense=20, speed=25
- Player 2: strength=28, defense=22, speed=20

**Očakávaný výsledok:**
```
Player 1 útočí prvý (vyššia speed)
Damage P1→P2: floor(30/3) - floor(22/3) = 10 - 7 = 3
Damage P2→P1: floor(28/3) - floor(20/3) = 9 - 6 = 3

Finálne HP:
Player 1: 97 (100 - 3)
Player 2: 97 (100 - 3)
```

## 🐛 Troubleshooting

### "Active cards not found!"
**Problém:** `player.cardInGame` alebo `enemy.cardInGame` je null  
**Riešenie:** Skontroluj či sa karty správne umiestňujú na board pomocou `Player.PlayCard()`

### "AttackAnimations component not found!"
**Problém:** `attack.attackAnimations` je null  
**Riešenie:** Priraď `AttackAnimations` komponent v Inspectore do `Attack` komponenta

### "ExecuteBattle: callback is null!"
**Problém:** Callback v `ServerFunctionsManager.ExecuteBattle()` nie je nastavený  
**Riešenie:** Skontroluj že `serverFunctionsManager` nie je null v `FightSystemMultiplayer`

### Server vráti error 500
**Problém:** Vercel funkcia crashne  
**Riešenie:** 
1. Pozri Vercel logs: `vercel logs`
2. Skontroluj či `saveBattleData/loadBattleData` funguje
3. Skontroluj že request obsahuje všetky required fields

### Polling neskončí
**Problém:** `PollForBattleResult()` beží donekonečna  
**Riešenie:**
1. Pridaj timeout (napr. max 60 pokusov)
2. Skontroluj či druhý hráč skutočne submitol útok
3. Pozri server logs či sa battleData ukladajú

## 📊 Dátové štruktúry

### AttackSubmission (Client → Server)
```csharp
{
    playerId: "abc123",
    roomCode: "ROOM001",
    attackId: 1,
    attackerHealth: 100,
    attackerMaxHealth: 100,
    attackerStrength: 30,
    attackerDefense: 20,
    attackerSpeed: 25,
    attackerMagic: 15,
    defenderHealth: 100,
    defenderMaxHealth: 100,
    defenderStrength: 28,
    defenderDefense: 22,
    defenderSpeed: 20,
    defenderMagic: 12
}
```

### BattleResult (Server → Client)
```json
{
    "success": true,
    "bothPlayersReady": true,
    "battleResult": {
        "firstAttacker": "player1",
        "player1Health": 97,
        "player2Health": 97,
        "player1Damage": 3,
        "player2Damage": 3,
        "player1DidSleep": false,
        "player2DidSleep": false,
        "player1SleepDuration": 0,
        "player2SleepDuration": 0
    }
}
```

## ✅ Checklist pred prvým testom

- [ ] `BattleResult.cs` pridaný do Unity projektu
- [ ] `ServerFunctionsManager.ExecuteBattle()` pridané
- [ ] `FightSystemMultiplayer` upravený s battle flow
- [ ] `AttackSelectionManager` - odstránená duplicita
- [ ] `executeBattle.js` deployed na Vercel
- [ ] Vercel funkcia registrovaná v PlayFab
- [ ] Storage/Database funguje (TitleData alebo KV)
- [ ] Inspector referencie skontrolované
- [ ] Test request na Vercel endpoint funguje

## 🎯 Ďalšie kroky

Po úspešnom otestovaní Punch útoku môžeš:

1. **Pridať viac útokov** - Skopíruj pattern z `executePunch()` pre Kick, Heal, atď.
2. **Pridať efekty** - Implementuj sleep detection a aplikáciu
3. **Pridať animácie** - Rozšír `PlayBattleAnimations()` pre rôzne útoky
4. **Optimalizovať polling** - Použiť webhooky namiesto pollingu
5. **Pridať error handling** - Timeout, disconnect handling

---

Ak toto všetko funguje, máš funkčný základ pre celý multiplayer battle systém! 🎉
