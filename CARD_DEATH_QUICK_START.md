# 🚀 Card Death System - Quick Start Guide

**TL;DR:** Karty ktoré zomrú (HP ≤ 0) sa automaticky vymazú z boardu a zo servera.

---

## ✅ Čo Je Hotové

1. **Unity Client:**
   - ✅ `BattleResultProcessor.HandleCardDeath()` - vymaže kartu z boardu + zavolá server
   - ✅ `ServerFunctionsManager.ClearDeadCard()` - API wrapper pre server call
   - ✅ `Player.RemoveCardFromBoard()` - zničí GameObject + clear cardInGame reference

2. **Server:**
   - ✅ `clearSelectedCards.js` - podporuje selective clear s `cardIdToClear` parametrom
   - ✅ Atomic update: `$unset` na konkrétny player field

---

## 🎮 Ako To Funguje

### Krok 1: Battle Complete (HP = 0)
```csharp
// Server vráti výsledok
myCard.health = 0;  // ← Karta zomrela
enemyCard.health = 15;
```

### Krok 2: Client Detekuje Smrť
```csharp
// BattleResultProcessor.CheckBattleOutcome()
if (myCard.health <= 0)
{
    StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
}
```

### Krok 3: Remove z Boardu
```csharp
// Player.RemoveCardFromBoard()
Destroy(card.gameObject);  // ← GameObject zmizne z UI
cardInGame = null;          // ← Clear reference
```

### Krok 4: Clear zo Servera
```javascript
// clearSelectedCards.js
db.rooms.updateOne(
  { roomCode: "ABC123" },
  { $unset: { "selectedCards.player1_id": "" } }
);
// ← Karta vymazaná z MongoDB
```

---

## 🧪 Testovanie

### Test Case 1: Player's Card Dies
```
1. Hraj battle až kým myCard.health <= 0
2. Očakávaj:
   - ✅ GameObject karty zmizne z boardu
   - ✅ Unity log: "💀 Card died: Henry Ford"
   - ✅ Unity log: "✅ Dead card cleared from server"
   - ✅ MongoDB: selectedCards.player1_id = undefined
```

### Test Case 2: Enemy's Card Dies
```
1. Hraj battle až kým enemyCard.health <= 0
2. Očakávaj:
   - ✅ Enemy karta zmizne
   - ✅ Unity log: "You Won!"
   - ✅ MongoDB: selectedCards.player2_id = undefined
```

### Test Case 3: Both Cards Die (Draw)
```
1. Obe karty majú HP = 1-2, simultánne zomrú
2. Očakávaj:
   - ✅ Obe karty zmiznú z boardu
   - ✅ Unity log: "Draw!"
   - ✅ MongoDB: selectedCards = {}
```

---

## 🐛 Debug Checklist

Ak karta nezomiera správne:

### 1. Unity Console Check:
```
❓ Vidíš: "[BattleResultProcessor] 💀 Card died: ..."?
   ✅ YES → HandleCardDeath() sa volá
   ❌ NO → CheckBattleOutcome() nekontroluje health <= 0

❓ Vidíš: "✅ Dead card cleared from server"?
   ✅ YES → Server call úspešný
   ❌ NO → Skontroluj server logs
```

### 2. Vercel Logs Check:
```
❓ Vidíš: "[clearSelectedCards] 🎯 Selective clear: removing player ..."?
   ✅ YES → Server našiel kartu
   ❌ NO → cardIdToClear parameter nebol poslaný

❓ Vidíš: "Update result - matched: 1, modified: 1"?
   ✅ YES → MongoDB update úspešný
   ❌ NO → Room nebol nájdený alebo už vymazaný
```

### 3. MongoDB Check:
```javascript
db.rooms.findOne({ roomCode: "ABC123" }, { selectedCards: 1 })

// Mŕtva karta by NEMALA byť v selectedCards
{
  selectedCards: {
    "player2_id": { cardId: "...", health: 15 }  // ← Len živá karta
    // "player1_id" by tu NEMAL byť!
  }
}
```

---

## ⚙️ Unity Inspector Setup

### BattleResultProcessor Component:
```
FightSystemMultiplayer GameObject
└─ BattleResultProcessor (script)
   ├─ multiplayerService → MultiplayerService GameObject
   ├─ fightSystem → FightSystemMultiplayer (self)
   ├─ attackComponent → Attack component
   ├─ dialogText → TMP_Text UI
   ├─ playerLifeBar → HealthBar (player)
   └─ enemyLifeBar → HealthBar (enemy)
```

### Player Component:
```
Player GameObject
└─ Player (script)
   ├─ dialogText → TMP_Text UI
   └─ isEnemy → false (pre player), true (pre enemy)
```

**⚠️ CRITICAL:** Ak nejaká referencia chýba → NullReferenceException!

---

## 📊 Performance

- **Server call latency:** ~200-500ms (Vercel serverless)
- **Timeout:** 5s (max wait pre server response)
- **UI destroy:** Instant (Destroy() je immediate)

---

## 🔮 Budúcnosť

### PLAYERDEATH State (TODO):
```csharp
// Keď karta zomrie a hráč má ďalšie karty v ruke
if (myCard.health <= 0 && player.hand.Count > 0)
{
    fightSystem.state = FightStateMultiplayer.PLAYERDEATH;
    dialogText.text = "Choose new fighter";
    // Show hand UI
    // Player vyberie novú kartu
    // selectCardForBattle.js
}
```

### Death Animations (TODO):
```csharp
// Fade out + shake pred zničením
IEnumerator PlayDeathAnimation(Kard card)
{
    // Fade out (1s)
    // Shake effect
    // Destroy
}
```

---

## 📚 Full Documentation

Pre podrobnejšiu dokumentáciu:
- **CARD_DEATH_SYSTEM.md** - Kompletná implementácia guide
- **.github/copilot-instructions.md** - Common issues section

---

**Created:** 2025-10-30  
**Status:** ✅ Ready for Testing
