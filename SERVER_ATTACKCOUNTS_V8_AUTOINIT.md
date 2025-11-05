# Server V8: Attack Counts Auto-Init (KISS Optimization)

## 🎯 Cieľ
**Eliminovať zbytočné `calculateAttackCounts` volania** - counts sa počítajú iba raz a ukladajú do DB, klient ich potom iba číta.

---

## 🔄 Nový Flow

### Pred (V7 - NEEFEKTÍVNE):
```
Card selection → setSelectedCard
  ↓
LoadAttackCounts → calculateAttackCounts (VÝPOČET zakaždým!)
  ↓
Attack → executeBattle → (decrement?)
  ↓
LoadAttackCounts → calculateAttackCounts (VÝPOČET znova!)
```
**Problém:** `calculateAttackCounts` sa volá pri každom `LoadAttackCounts()` = zbytočné výpočty!

### Po (V8 - EFEKTÍVNE):
```
Card selection → setSelectedCard → AUTO-INIT counts (ak neexistujú)
  ↓
LoadAttackCounts → getAttackCounts (READ z DB - rýchle!)
  ↓
Attack → executeBattle → DECREMENT v DB
  ↓
LoadAttackCounts → getAttackCounts (READ decrementované hodnoty)
```
**Výhoda:** Counts sa počítajú iba raz pri prvom výbere karty, potom len READ!

---

## 📝 Server Changes Required

### 1. `setSelectedCard.js` - Pridaj Auto-Init Logic

**Kde:** Po uložení `selectedCards`, pred `return res.status(200)`

**Čo pridať:**

```javascript
// ✅ V8: AUTO-INIT attack counts if not exist
const attackCountsPath = `attackCounts.${playerId}.${selectedCard.cardId}`;
const existingCounts = room.attackCounts?.[playerId]?.[selectedCard.cardId];

if (!existingCounts) {
  console.log(`[setSelectedCard] Attack counts not found for cardId=${selectedCard.cardId}, initializing...`);
  
  // Import calculateAttackCountsLogic helper
  const { calculateAttackCountsLogic } = require('./calculateAttackCounts');
  
  const attackIds = [
    selectedCard.Attack1 || 0,
    selectedCard.Attack2 || 0,
    selectedCard.Attack3 || 0,
    selectedCard.Attack4 || 0
  ];
  
  const stats = {
    strength: selectedCard.Strength || 0,
    defense: selectedCard.Defense || 0,
    attack: selectedCard.Attack || 0,
    knowledge: selectedCard.Knowledge || 0,
    charisma: selectedCard.Charisma || 0,
    speed: selectedCard.Speed || 0
  };
  
  // Calculate counts using existing logic
  const counts = calculateAttackCountsLogic(attackIds, stats);
  
  console.log(`[setSelectedCard] Calculated counts: ${JSON.stringify(counts)}`);
  
  // Save to DB (atomic update using nested field paths)
  await collection.updateOne(
    { roomCode },
    {
      $set: {
        [`${attackCountsPath}.count1`]: counts.count1,
        [`${attackCountsPath}.count2`]: counts.count2,
        [`${attackCountsPath}.count3`]: counts.count3,
        [`${attackCountsPath}.count4`]: counts.count4
      }
    }
  );
  
  console.log(`[setSelectedCard] ✅ Attack counts initialized and saved to DB`);
} else {
  console.log(`[setSelectedCard] Attack counts already exist for cardId=${selectedCard.cardId}, skipping init`);
}
```

**Important Notes:**
- Pridaj `const { calculateAttackCountsLogic } = require('./calculateAttackCounts');` na začiatok súboru (s ostatnými imports)
- Táto logika beží iba ak `attackCounts` pre túto kartu NEexistujú
- Používa atomic `$set` s nested field paths (anti race-condition)
- Reusuje existujúcu `calculateAttackCountsLogic()` funkciu

---

### 2. `calculateAttackCounts.js` - Export Helper Function

**Overiť že tento súbor exportuje logiku:**

```javascript
// calculateAttackCounts.js

/**
 * Helper function pre výpočet attack counts
 * (reusable by setSelectedCard auto-init)
 */
export function calculateAttackCountsLogic(attackIds, stats) {
  const counts = { count1: 0, count2: 0, count3: 0, count4: 0 };
  
  for (let i = 0; i < 4; i++) {
    const attackId = attackIds[i];
    
    if (attackId === 0) {
      counts[`count${i + 1}`] = 0;
      continue;
    }
    
    // Attack count formula (existing logic)
    let baseCount = 10;
    
    // Knowledge-based attacks (IDs 8+)
    if (attackId >= 8 && attackId <= 50) {
      baseCount = Math.floor((stats.knowledge || 0) * 1.5) + 5;
    }
    // Charisma-based attacks (IDs 51+)
    else if (attackId >= 51) {
      baseCount = Math.floor((stats.charisma || 0) * 2) + 3;
    }
    // Basic attacks (IDs 1-7)
    else {
      baseCount = Math.floor(((stats.strength || 0) + (stats.attack || 0)) / 2) + 10;
    }
    
    counts[`count${i + 1}`] = baseCount;
  }
  
  return counts;
}

// Endpoint handler (unchanged)
export default async function handler(req, res) {
  // ... existing endpoint code uses calculateAttackCountsLogic()
}
```

**Note:** Ak už exportuješ `calculateAttackCountsLogic`, len overiť že je `export` (nie `const`).

---

### 3. `getAttackCounts.js` - Verify READ Logic

**Overiť že endpoint číta z DB (nie počíta!):**

```javascript
// getAttackCounts.js
import clientPromise from './mongodb';

function extractParam(req, key) {
  return req.body?.[key] 
    || req.body?.FunctionArgument?.[key] 
    || req.query?.[key];
}

export default async function handler(req, res) {
  const roomCode = extractParam(req, 'roomCode');
  const playerId = extractParam(req, 'playerId');
  const cardId = extractParam(req, 'cardId');
  
  if (!roomCode || !playerId || !cardId) {
    return res.status(400).json({ 
      success: false, 
      error: 'Missing required parameters: roomCode, playerId, cardId' 
    });
  }
  
  try {
    const client = await clientPromise;
    const db = client.db();
    const collection = db.collection('rooms');
    
    const room = await collection.findOne({ roomCode });
    
    if (!room) {
      return res.status(404).json({ success: false, error: 'Room not found' });
    }
    
    // ✅ READ from DB (NOT calculate!)
    const attackCounts = room.attackCounts?.[playerId]?.[cardId];
    
    if (!attackCounts) {
      console.log(`[getAttackCounts] Attack counts not found for playerId=${playerId}, cardId=${cardId}`);
      return res.status(404).json({ 
        success: false, 
        error: 'Attack counts not initialized - select card first!' 
      });
    }
    
    console.log(`[getAttackCounts] ✅ Returning counts: ${JSON.stringify(attackCounts)}`);
    
    return res.status(200).json({
      success: true,
      count1: attackCounts.count1 || 0,
      count2: attackCounts.count2 || 0,
      count3: attackCounts.count3 || 0,
      count4: attackCounts.count4 || 0
    });
    
  } catch (error) {
    console.error(`[getAttackCounts] Error:`, error);
    return res.status(500).json({ 
      success: false, 
      error: error.message 
    });
  }
}
```

**Key Points:**
- Endpoint iba **číta** z `room.attackCounts[playerId][cardId]`
- **NEPOČÍTA** - counts už existujú v DB (vytvorené v `setSelectedCard`)
- Ak counts neexistujú → error (user musí vybrať kartu znova)

---

### 4. `executeBattle.js` - Verify Decrement Logic

**Overiť že decrement beží správne:**

```javascript
// executeBattle.js - PO saveSelectedCards(), PRED return

// ✅ V8: DECREMENT attack counts after battle
const { decrementAttackCountLogic } = require('./decrementAttackCount');

// Decrement for both players
if (battleData.player1?.attackSlot) {
  await decrementAttackCountLogic(
    collection,
    roomCode,
    player1Id,
    battleData.player1.cardId,
    battleData.player1.attackSlot,  // ⚠️ MUST be attackSlot (1-4), NOT attackId!
    updatedRoom
  );
  console.log(`[executeBattle] ✅ Player1 attack slot ${battleData.player1.attackSlot} decremented`);
}

if (battleData.player2?.attackSlot) {
  await decrementAttackCountLogic(
    collection,
    roomCode,
    player2Id,
    battleData.player2.cardId,
    battleData.player2.attackSlot,
    updatedRoom
  );
  console.log(`[executeBattle] ✅ Player2 attack slot ${battleData.player2.attackSlot} decremented`);
}
```

**Critical:**
- Používaj `battleData.playerX.attackSlot` (hodnota 1-4)
- **NIE** `attackId` (hodnota 1-123)!
- Decrement musí byť PO `saveSelectedCards()` (aby mal updatedRoom)

---

## 🗄️ MongoDB Schema (Updated)

```javascript
{
  roomCode: "ABC123",
  players: ["player1_id", "player2_id"],
  
  // ✅ V8: Attack counts stored in DB
  attackCounts: {
    "player1_id": {
      "card-uuid-1": {
        count1: 15,  // Attack slot 1 (Attack1 ID=21)
        count2: 45,  // Attack slot 2 (Attack2 ID=1)
        count3: 5,   // Attack slot 3 (Attack3 ID=65)
        count4: 3    // Attack slot 4 (Attack4 ID=43)
      },
      "card-uuid-2": {
        count1: 10,
        count2: 28,
        count3: 28,
        count4: 9
      }
    },
    "player2_id": { ... }
  },
  
  selectedCards: { ... },
  battleData: { ... },
  battleState: { ... }
}
```

**Flow:**
1. **Card selected** → `setSelectedCard` → **AUTO-INIT** `attackCounts[player][card]` ak neexistujú
2. **Load counts** → `getAttackCounts` → **READ** z DB
3. **Attack used** → `executeBattle` → **DECREMENT** `attackCounts[player][card].countX`
4. **Next load** → `getAttackCounts` → **READ** decrementované hodnoty

---

## 🧪 Testing Checklist

### Test 1: First Card Selection (Auto-Init)
```bash
# Unity: Select card for first time
→ setSelectedCard called
→ Server log: "[setSelectedCard] Attack counts not found, initializing..."
→ Server log: "[setSelectedCard] ✅ Attack counts initialized"

# MongoDB check:
db.rooms.findOne({ roomCode: "ABC123" }, { attackCounts: 1 })
# Should see: attackCounts.player_id.card_id = { count1, count2, count3, count4 }
```

### Test 2: Load Counts (Read from DB)
```bash
# Unity: LoadAttackCounts() called
→ getAttackCounts called (NOT calculateAttackCounts!)
→ Server log: "[getAttackCounts] ✅ Returning counts: {...}"
→ Unity UI: Buttons show counts (6, 45, 5, 3)

# Vercel logs should NOT show:
❌ [calculateAttackCounts] Attack counts calculated...  # Should NOT appear!
```

### Test 3: Attack Decrement
```bash
# Unity: Player attacks with slot 2
→ executeBattle called
→ Server log: "[executeBattle] ✅ Player1 attack slot 2 decremented"

# MongoDB check:
db.rooms.findOne({ roomCode: "ABC123" }, { attackCounts: 1 })
# attackCounts.player_id.card_id.count2: 44 (was 45)
```

### Test 4: Second Card Selection (Reuse Counts)
```bash
# Unity: Select same card again (e.g., after opponent killed it)
→ setSelectedCard called
→ Server log: "[setSelectedCard] Attack counts already exist, skipping init"

# Counts are NOT recalculated (reused from DB)
```

---

## 📊 Expected Vercel Logs (After V8)

### ✅ CORRECT Logs:
```
/api/setSelectedCard
[setSelectedCard] Attack counts not found, initializing...
[setSelectedCard] ✅ Attack counts initialized

/api/getAttackCounts
[getAttackCounts] ✅ Returning counts: {"count1":6,"count2":45,"count3":5,"count4":3}

/api/executeBattle
[executeBattle] ✅ Player1 attack slot 2 decremented

/api/getAttackCounts
[getAttackCounts] ✅ Returning counts: {"count1":6,"count2":44,"count3":5,"count4":3}
```

### ❌ WRONG Logs (V7 - should NOT see):
```
/api/calculateAttackCounts  ← Should NOT appear anymore!
[calculateAttackCounts] Attack counts calculated...  ← Zbytočný výpočet!
```

---

## 🚀 Deployment Steps

1. **Edit `setSelectedCard.js`** - Pridaj auto-init logic (Section 1)
2. **Verify `calculateAttackCounts.js`** - Export helper (Section 2)
3. **Verify `getAttackCounts.js`** - READ only (Section 3)
4. **Verify `executeBattle.js`** - Decrement logic (Section 4)
5. **Git commit + push** → Vercel auto-deploy
6. **Test in Unity** - Check Vercel logs (should NOT see calculateAttackCounts)

---

## 🎯 Success Criteria

✅ `calculateAttackCounts` endpoint **nie je volaný** po card selection  
✅ `getAttackCounts` endpoint **číta z DB** (rýchle)  
✅ Attack counts **decrementujú** po útoku  
✅ Counts **persistent** across turns  
✅ Vercel logs **čisté** (len setSelectedCard → getAttackCounts → executeBattle)  

---

**Version:** V8 (Auto-Init + KISS Optimization)  
**Priority:** HIGH (performance improvement)  
**Breaking Changes:** None (backward compatible)
