# CRITICAL FIX: executeBattle.js Not Calling Decrement Logic

## 🐛 Problem Diagnosed

**Vercel logs show:** Attack counts are **NOT being decremented** after battle!

```
❌ MISSING LOGS:
[executeBattle] ✅ Player1 attack slot 2 decremented
[decrementAttackCountLogic] ...

✅ EXISTING LOGS:
[executeBattle] Battle complete, nextTurnReady reset to: {...}
```

**Root Cause:** `executeBattle.js` **NEVER CALLS** the decrement function!

---

## 🔧 Fix Required

### File: `mega-tresk-server/api/executeBattle.js`

**Location:** AFTER `saveSelectedCards()` call, BEFORE `nextTurnReady` reset

**Add this code:**

```javascript
// ============================================================
// ✅ V8: DECREMENT ATTACK COUNTS AFTER BATTLE
// ============================================================
console.log('[executeBattle] 🔄 Attempting to decrement attack counts...');

try {
  // Import decrement logic
  const { decrementAttackCountLogic } = require('./decrementAttackCount');
  
  // ✅ Decrement Player 1's attack count
  if (battleData.player1?.attackSlot && battleData.player1.attackSlot > 0) {
    console.log(`[executeBattle] Decrementing Player1: cardId=${battleData.player1.cardId}, slot=${battleData.player1.attackSlot}`);
    
    await decrementAttackCountLogic(
      collection,
      roomCode,
      player1Id,
      battleData.player1.cardId,
      battleData.player1.attackSlot,  // ⚠️ MUST be attackSlot (1-4), NOT attackId!
      updatedRoom
    );
    
    console.log(`[executeBattle] ✅ Player1 attack slot ${battleData.player1.attackSlot} decremented`);
  } else {
    console.log(`[executeBattle] ⚠️ Player1 attackSlot missing or invalid: ${battleData.player1?.attackSlot}`);
  }
  
  // ✅ Decrement Player 2's attack count
  if (battleData.player2?.attackSlot && battleData.player2.attackSlot > 0) {
    console.log(`[executeBattle] Decrementing Player2: cardId=${battleData.player2.cardId}, slot=${battleData.player2.attackSlot}`);
    
    await decrementAttackCountLogic(
      collection,
      roomCode,
      player2Id,
      battleData.player2.cardId,
      battleData.player2.attackSlot,
      updatedRoom
    );
    
    console.log(`[executeBattle] ✅ Player2 attack slot ${battleData.player2.attackSlot} decremented`);
  } else {
    console.log(`[executeBattle] ⚠️ Player2 attackSlot missing or invalid: ${battleData.player2?.attackSlot}`);
  }
  
} catch (error) {
  console.error('[executeBattle] ❌ Failed to decrement attack counts:', error);
  // Don't fail the battle - just log error and continue
}

console.log('[executeBattle] 🔄 Attack count decrement completed');
```

---

## 📍 Exact Placement in executeBattle.js

**Find this section:**

```javascript
// Save updated HP back to selectedCards
await saveSelectedCards(
  collection,
  roomCode,
  player1Id,
  updatedRoom.selectedCards[player1Id],
  player2Id,
  updatedRoom.selectedCards[player2Id]
);

// ============================================================
// ✅ ADD DECREMENT CODE HERE (BETWEEN saveSelectedCards AND nextTurnReady reset)
// ============================================================

// Reset nextTurnReady flags
await collection.updateOne(
  { roomCode },
  { 
    $set: { 
      nextTurnReady: { 
        [player1Id]: false, 
        [player2Id]: false 
      } 
    } 
  }
);
```

**After adding, it should look like:**

```javascript
// Save updated HP back to selectedCards
await saveSelectedCards(/* ... */);

// ✅ V8: DECREMENT ATTACK COUNTS
console.log('[executeBattle] 🔄 Attempting to decrement attack counts...');
try {
  const { decrementAttackCountLogic } = require('./decrementAttackCount');
  
  if (battleData.player1?.attackSlot && battleData.player1.attackSlot > 0) {
    await decrementAttackCountLogic(/* ... */);
    console.log(`[executeBattle] ✅ Player1 attack slot ${battleData.player1.attackSlot} decremented`);
  }
  
  if (battleData.player2?.attackSlot && battleData.player2.attackSlot > 0) {
    await decrementAttackCountLogic(/* ... */);
    console.log(`[executeBattle] ✅ Player2 attack slot ${battleData.player2.attackSlot} decremented`);
  }
} catch (error) {
  console.error('[executeBattle] ❌ Failed to decrement attack counts:', error);
}

// Reset nextTurnReady flags
await collection.updateOne(/* ... */);
```

---

## ⚠️ Critical Notes

### 1. Use `attackSlot` NOT `attackId`

```javascript
// ✅ CORRECT:
battleData.player1.attackSlot  // Value: 1-4 (button position)

// ❌ WRONG:
battleData.player1.attackId    // Value: 1-123 (attack database ID)
```

**Why:**
- `attackSlot` = Which button was pressed (1-4)
- `attackId` = Which attack from database (1-123)
- Decrement needs slot number to update `count1`, `count2`, `count3`, or `count4`

### 2. Don't Fail Battle on Decrement Error

```javascript
try {
  // Decrement logic
} catch (error) {
  console.error('[executeBattle] ❌ Failed to decrement:', error);
  // ✅ Don't throw - let battle complete even if decrement fails
}
```

**Why:**
- Battle HP updates are more critical than count tracking
- If decrement fails, player can still play (counts reload from DB)
- Error logged for debugging

### 3. Import at Top of Function (or File)

```javascript
// Option A: Import at top of executeBattle function
export default async function handler(req, res) {
  const { decrementAttackCountLogic } = require('./decrementAttackCount');
  // ... rest of function
}

// Option B: Import inline (as shown in code above)
try {
  const { decrementAttackCountLogic } = require('./decrementAttackCount');
  // ... use it
}
```

**Either works** - inline is cleaner if only used in one place.

---

## 🧪 Testing After Deploy

### 1. Deploy Changes

```bash
cd C:\Zlozka\mega-tresk-server
git add api/executeBattle.js
git commit -m "Fix: Add missing attack count decrement in executeBattle"
git push
# Wait ~2 min for Vercel auto-deploy
```

### 2. Test in Unity

```
1. Start game
2. Both players select cards
3. Player 1 uses Attack 2 (attackSlot=2, initial count=45)
4. Player 2 uses Attack 1 (attackSlot=1, initial count=8)
5. Battle executes
6. Check Vercel logs
7. Check MongoDB
8. Next turn - counts should be decremented in UI
```

### 3. Expected Vercel Logs

```
/api/executeBattle
[executeBattle] Request: { roomCode: '97JN9N', playerId: 'A36FA8EC0F782700', ... }
[executeBattle] Battle executing...
[executeBattle] 🔄 Attempting to decrement attack counts...
[executeBattle] Decrementing Player1: cardId=35ca1ba1-..., slot=2
[decrementAttackCountLogic] Room 97JN9N: Decrementing attack 2 for player A36FA8EC0F782700, card 35ca1ba1-...
[decrementAttackCountLogic] Current count: 45, new count: 44
[executeBattle] ✅ Player1 attack slot 2 decremented
[executeBattle] Decrementing Player2: cardId=dc8f40b2-..., slot=1
[decrementAttackCountLogic] Room 97JN9N: Decrementing attack 1 for player 2060B1F370923AE8, card dc8f40b2-...
[decrementAttackCountLogic] Current count: 8, new count: 7
[executeBattle] ✅ Player2 attack slot 1 decremented
[executeBattle] 🔄 Attack count decrement completed
[executeBattle] Battle complete, nextTurnReady reset to: { ... }
```

### 4. Expected MongoDB State

**Before battle:**
```json
"attackCounts": {
  "A36FA8EC0F782700": {
    "35ca1ba1-325f-47cd-9d93-d3a328b069c7": {
      "count1": 5,
      "count2": 45,  // ← Player 1 uses this
      "count3": 5,
      "count4": 3
    }
  },
  "2060B1F370923AE8": {
    "dc8f40b2-3ae2-476e-9ea3-574ded6e658a": {
      "count1": 8,   // ← Player 2 uses this
      "count2": 28,
      "count3": 28,
      "count4": 9
    }
  }
}
```

**After battle:**
```json
"attackCounts": {
  "A36FA8EC0F782700": {
    "35ca1ba1-325f-47cd-9d93-d3a328b069c7": {
      "count1": 5,
      "count2": 44,  // ✅ Decremented from 45!
      "count3": 5,
      "count4": 3
    }
  },
  "2060B1F370923AE8": {
    "dc8f40b2-3ae2-476e-9ea3-574ded6e658a": {
      "count1": 7,   // ✅ Decremented from 8!
      "count2": 28,
      "count3": 28,
      "count4": 9
    }
  }
}
```

### 5. Expected Unity Console (Next Turn)

```
[AttackCountLoader] Loading attack counts for card: Harry Houdini (cardId: 35ca1ba1-...)
[ServerFunctionsManager] GetAttackCounts: roomCode=97JN9N, ...
[AttackCountLoader] Received attack counts: 5, 44, 5, 3  // ✅ count2 = 44 (was 45)
```

**UI Buttons should show:**
```
[Attack 1: Chakra Blast]  5  ← count1 (unchanged)
[Attack 2: Punch]        44  ← count2 (DECREMENTED!)
[Attack 3: Magic Shield]  5  ← count3 (unchanged)
[Attack 4: Fireball]      3  ← count4 (unchanged)
```

---

## ❌ If Still Not Working

### Check 1: Verify `decrementAttackCount.js` Exists

```bash
ls mega-tresk-server/api/decrementAttackCount.js
# Should exist and export decrementAttackCountLogic
```

### Check 2: Verify Export in `decrementAttackCount.js`

```javascript
// decrementAttackCount.js should have:
export async function decrementAttackCountLogic(collection, roomCode, playerId, cardId, attackSlot, room) {
  // ... logic
}

// OR (if using module.exports):
module.exports.decrementAttackCountLogic = async function(collection, roomCode, playerId, cardId, attackSlot, room) {
  // ... logic
};
```

### Check 3: Verify `battleData` Has `attackSlot`

**In Vercel logs, search for:**
```
[executeBattle] Request: { ..., attackData: { ..., attackSlot: 2 } }
```

**If `attackSlot` is missing or 0:**
- Unity not sending it correctly
- Check `BattleSubmitter.cs` sends `attackSlot` parameter
- Check `AttackSubmission` class has `public int attackSlot;`

### Check 4: MongoDB Connection Issues

**If logs show:**
```
[decrementAttackCountLogic] ❌ Failed to update attack count
```

**Check:**
- MongoDB Atlas is online
- Network access allows Vercel IPs
- Environment variables set correctly (`MONGODB_URI`, `MONGODB_DB`)

---

## 📊 Success Criteria

✅ Vercel logs show: `[executeBattle] ✅ Player1 attack slot X decremented`  
✅ Vercel logs show: `[decrementAttackCountLogic] Current count: X, new count: Y`  
✅ MongoDB `attackCounts.player.card.countX` decreases by 1  
✅ Next turn Unity UI shows decremented count (e.g., 44 instead of 45)  
✅ After 45 uses, count reaches 0 and button disables  

---

## 🎯 Why This Fix is Needed

**Current Flow (BROKEN):**
```
executeBattle → simulateBattle → saveSelectedCards → ❌ (nothing) → nextTurnReady reset
```

**Fixed Flow (CORRECT):**
```
executeBattle → simulateBattle → saveSelectedCards → ✅ decrementAttackCounts → nextTurnReady reset
```

**Impact:**
- Players could spam attacks infinitely (counts never decrease)
- Attack count system completely non-functional
- No resource management in battles

**Priority:** 🔴 **CRITICAL** - Core gameplay mechanic broken without this!

---

**Version:** V8 (Attack Count Decrement Fix)  
**File:** `mega-tresk-server/api/executeBattle.js`  
**Action:** Add decrement call after `saveSelectedCards()`  
**Deploy:** Git push → Vercel auto-deploy → Test immediately
