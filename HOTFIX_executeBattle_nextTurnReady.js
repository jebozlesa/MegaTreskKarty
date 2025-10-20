// 🎯 HOTFIX pre executeBattle.js
// Problem: nextTurnReady sa resetuje aj pri attackId=0 (polling)
// Fix: Reset iba pri skutočných attack submissions (attackId > 0)

// HĽADAJ TENTO KÓD V executeBattle.js:
/*
    // ✅ Reset next turn ready flags when new battle starts
    if (room.nextTurnReady) {
      await collection.updateOne(
        { roomCode },
        { $unset: { nextTurnReady: "" } }
      );
      console.log('[executeBattle] Reset nextTurnReady flags');
    }
*/

// NAHRAĎ S TÝMTO:
/*
    // ✅ Reset next turn ready flags ONLY when new attack is submitted (not during polling)
    // Do NOT reset during status checks (attackId=0)
    if (room.nextTurnReady && attackId > 0) {
      await collection.updateOne(
        { roomCode },
        { $unset: { nextTurnReady: "" } }
      );
      console.log('[executeBattle] Reset nextTurnReady flags for new battle');
    }
*/

// PRESNÁ LOKÁCIA: Riadok cca 170-178 v executeBattle.js, pred:
// const isPlayer1 = room.players[0] === playerId;

console.log("HOTFIX ready for deployment");