# 🚨 HOTFIX SUMMARY - Next Turn Race Condition

## 🎯 **Čo opraviť:**

### **Súbor: `c:\Zlozka\mega-tresk-server\api\executeBattle.js`**

**FIX #1:** Reset `nextTurnReady` po battle (riadok ~350)
```javascript
// Pridaj PRED return po battle:
const resetNextTurnReady = {};
room.players.forEach(pid => { resetNextTurnReady[pid] = false; });

await collection.updateOne({ roomCode }, {
  $set: {
    'battleData.player1.submitted': false,
    'battleData.player2.submitted': false,
    'battleData.lastResult': battleResult,
    nextTurnReady: resetNextTurnReady  // ← NOVÝ RIADOK!
  }
});
```

**FIX #2:** Atomic updates pre battleData (riadok ~300)
```javascript
// NAMIESTO: battleData.player1 = {...}; collection.updateOne({$set: {battleData}})
// POUŽI:
const playerKey = isPlayer1 ? 'player1' : 'player2';
await collection.updateOne({ roomCode }, {
  $set: {
    [`battleData.${playerKey}.playerId`]: playerId,
    [`battleData.${playerKey}.cardId`]: cardId,
    [`battleData.${playerKey}.attackId`]: attackId,
    [`battleData.${playerKey}.submitted`]: true
  }
});

// Re-load battleData
const updatedRoom = await collection.findOne({ roomCode });
battleData = updatedRoom.battleData || { player1: null, player2: null };
```

---

## ✅ **Súbory ktoré SÚ OK (žiadne zmeny):**

- ✅ `markReadyForNextTurn.js` - UŽ OPRAVENÝ (ponecháva nextTurnReady=true)
- ✅ `checkNextTurnReady.js` - read-only, funguje správne

---

## 🧪 **Test po deploye:**

```bash
# 1. Deploy
cd c:\Zlozka\mega-tresk-server
git add api/executeBattle.js
git commit -m "HOTFIX: Race condition v nextTurnReady + atomic battleData"
git push

# 2. Test v Unity
# - 2 klienti (Unity Editor + Android)
# - Zahraj 5+ kôl bez timeoutu
# - Skús submitnúť súčasne (< 1s rozdiel)
# - Check logy: "[executeBattle] Battle complete, nextTurnReady reset to: {}"
```

---

## 📊 **Expected Results:**

| Problém | Pred fix | Po fixe |
|---------|----------|---------|
| Timeout po 1. kole | 100% fail | 0% fail |
| Súbežné submity | ~50% fail | 0% fail |
| Polling calls per turn | 30-60 | 1-2 |

---

**Files:**
- 📄 `HOTFIX_CRITICAL_NextTurnRace.md` - Detailný návod
- 📄 `SNIPPET_FIX1_ResetNextTurn.js` - Copy-paste kód #1
- 📄 `SNIPPET_FIX2_AtomicUpdates.js` - Copy-paste kód #2

**Time estimate:** 10 min coding + 2 min deploy = **12 minút total**
