# AI Agent Instructions: Next Turn Ready System Implementation

## 🎯 Project Context
You are working on a **multiplayer card battle game server** built with **Vercel serverless functions + MongoDB**. The project is located at `C:\Zlozka\mega-tresk-server\`.

## 📂 Project Structure
```
C:\Zlozka\mega-tresk-server\
├── api\                              ← All serverless functions here
│   ├── executeBattle.js              ← Main battle simulation (V3)  
│   ├── getBattleStatus.js            ← Battle status polling
│   ├── clearBattleData.js            ← Turn cleanup
│   ├── getSelectedCards.js           ← Load selected cards from room
│   ├── loadPlayerDecksIntoRoom.js    ← Initialize player decks
│   ├── mongodb.js                    ← MongoDB connection helper (REUSE THIS!)
│   └── ... (other endpoints)
│
├── package.json                      ← Dependencies: mongodb driver
├── vercel.json                       ← Vercel config
└── .env                              ← MONGODB_URI, MONGODB_DB
```

## 🗄️ MongoDB Schema
```javascript
// Collection: rooms
{
  _id: ObjectId("..."),
  roomCode: "ABC123",
  players: ["player1_id", "player2_id"],
  
  // Battle state (V3 - server authoritative HP)
  battleState: {
    playerHealths: {
      "player1_id": 27,  // Current HP
      "player2_id": 22
    },
    turnNumber: 1
  },
  
  // Battle submissions (gets cleared after each turn)
  battleData: {
    player1: { cardId: "05b120d1-...", attackId: 1, submitted: true },
    player2: { cardId: "dc8f40b2-...", attackId: 1, submitted: false },
    lastResult: { /* battle result */ }
  },
  
  // ✅ NEW: Next turn ready tracking
  nextTurnReady: {
    "player1_id": false,
    "player2_id": true
  },
  
  // Player decks with card stats
  playerDecks: { /* ... */ },
  status: "playing",
  lastActivity: ISODate("...")
}
```

## 🔧 Existing Helper Functions (REUSE THESE!)
```javascript
// From mongodb.js - ALWAYS use this pattern:
import clientPromise from './mongodb';
const client = await clientPromise;
const db = client.db();
const collection = db.collection('rooms');

// From executeBattle.js - REUSE this function:
function extractParam(req, key) {
  return req.body?.[key] 
    || req.body?.FunctionArgument?.[key]
    || req.query?.[key];
}

// Standard error response pattern:
return res.status(400).json({ 
  success: false, 
  error: "Room not found" 
});

// Standard success response pattern:
return res.status(200).json({ 
  success: true,
  bothPlayersReady: true,
  playersReady: { "player1_id": true, "player2_id": false }
});
```

## 🎯 Task: Implement Next Turn Ready System

### Function 1: markReadyForNextTurn.js
**Location**: `C:\Zlozka\mega-tresk-server\api\markReadyForNextTurn.js`

**Input** (via PlayFab CloudScript):
```javascript
{
  roomCode: "ABC123",
  playerId: "A36FA474C7B86D26"  // The player marking ready
}
```

**Logic**:
1. Extract roomCode, playerId using `extractParam()`
2. Load room from MongoDB using `collection.findOne({ roomCode })`
3. Validate room exists and player is in room
4. Initialize `room.nextTurnReady` if missing: `{ "player1_id": false, "player2_id": false }`
5. Set `room.nextTurnReady[playerId] = true`
6. Check if BOTH players are ready: `Object.values(room.nextTurnReady).every(ready => ready === true)`
7. If both ready:
   - Clear `battleData: null`
   - Reset `nextTurnReady: { [player1]: false, [player2]: false }`
   - Log: "Both players ready, cleared battleData for next turn"
8. Update room in MongoDB using `collection.updateOne()`

**Output**:
```javascript
{
  success: true,
  bothPlayersReady: true/false,
  playersReady: { "player1_id": true, "player2_id": false },
  message: "Marked ready for next turn" // or "Both ready, cleared for next turn"
}
```

### Function 2: checkNextTurnReady.js  
**Location**: `C:\Zlozka\mega-tresk-server\api\checkNextTurnReady.js`

**Input**:
```javascript
{
  roomCode: "ABC123"
}
```

**Logic**:
1. Extract roomCode using `extractParam()`
2. Load room from MongoDB
3. Validate room exists
4. Read `room.nextTurnReady` (default to all false if missing)
5. Check if both players ready
6. Return status (NO database updates)

**Output**:
```javascript
{
  success: true,
  bothPlayersReady: true/false,
  playersReady: { "player1_id": true, "player2_id": false }
}
```

## 🔄 Integration with Existing executeBattle.js
**IMPORTANT**: Update existing `executeBattle.js` to reset `nextTurnReady` when battle starts:

Add this logic at the beginning of executeBattle:
```javascript
// Reset next turn ready flags when new battle starts
if (room.nextTurnReady) {
  await collection.updateOne(
    { roomCode },
    { $unset: { nextTurnReady: "" } }
  );
  console.log('[executeBattle] Reset nextTurnReady flags');
}
```

## 📝 Naming Conventions
- **Functions**: `camelCase` - `markReadyForNextTurn`, `checkNextTurnReady`
- **Variables**: `camelCase` - `roomCode`, `playerId`, `bothPlayersReady`
- **Constants**: `UPPER_SNAKE_CASE` - `MAX_PLAYERS`
- **Database fields**: `camelCase` - `nextTurnReady`, `battleData`

## 🚨 Error Handling
```javascript
// Room not found
if (!room) {
  return res.status(404).json({ success: false, error: "Room not found" });
}

// Player not in room  
if (!room.players.includes(playerId)) {
  return res.status(400).json({ success: false, error: "Player not in room" });
}

// MongoDB errors
try {
  // database operations
} catch (error) {
  console.error('[functionName] Database error:', error);
  return res.status(500).json({ success: false, error: "Database error" });
}
```

## 🧪 Testing
After implementation, test with:
```bash
# Test markReadyForNextTurn
curl -X POST http://localhost:3000/api/markReadyForNextTurn \
  -H "Content-Type: application/json" \
  -d '{"roomCode":"TEST123","playerId":"player1"}'

# Test checkNextTurnReady  
curl -X POST http://localhost:3000/api/checkNextTurnReady \
  -H "Content-Type: application/json" \
  -d '{"roomCode":"TEST123"}'
```

## 📋 Checklist
- [ ] Create `markReadyForNextTurn.js` with proper error handling
- [ ] Create `checkNextTurnReady.js` for polling
- [ ] Update `executeBattle.js` to reset nextTurnReady
- [ ] Test both functions with curl
- [ ] Verify MongoDB schema is updated correctly
- [ ] Check console logs for proper debugging info

## 🎯 Expected Behavior
1. Player1 calls markReadyForNextTurn → `{ bothPlayersReady: false }`
2. Player2 calls markReadyForNextTurn → `{ bothPlayersReady: true }` + battleData cleared
3. Both players can now submit new attacks via executeBattle
4. Cycle repeats until someone dies

**IMPORTANT**: Follow the existing code style from `executeBattle.js` and `mongodb.js`. Use the same patterns for error handling, logging, and MongoDB operations.