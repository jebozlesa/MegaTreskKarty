// ✂️ COPY-PASTE FIX #1: Reset nextTurnReady po battle
// Location: c:\Zlozka\mega-tresk-server\api\executeBattle.js
// Line: ~350-365 (po simulateBattle, pred return)

// ❌ REMOVE THIS:
/*
      // Vyčisti battleData pre ďalšie kolo
      await collection.updateOne(
        { roomCode },
        {
          $set: {
            'battleData.player1.submitted': false,
            'battleData.player2.submitted': false,
            'battleData.lastResult': battleResult,
            'battleData.lastBattleTime': new Date()
          }
        }
      );

      return res.status(200).json({
        success: true,
        bothPlayersReady: true,
        battleResult: battleResult
      });
*/

// ✅ REPLACE WITH THIS:
      // ✅ RESET nextTurnReady pred vyčistením battleData!
      const resetNextTurnReady = {};
      room.players.forEach(pid => {
        resetNextTurnReady[pid] = false;
      });

      // Vyčisti battleData pre ďalšie kolo + RESET nextTurnReady
      await collection.updateOne(
        { roomCode },
        {
          $set: {
            'battleData.player1.submitted': false,
            'battleData.player2.submitted': false,
            'battleData.lastResult': battleResult,
            'battleData.lastBattleTime': new Date(),
            nextTurnReady: resetNextTurnReady  // ✅ RESET!
          }
        }
      );

      console.log('[executeBattle] Battle complete, nextTurnReady reset to:', resetNextTurnReady);

      return res.status(200).json({
        success: true,
        bothPlayersReady: true,
        battleResult: battleResult
      });
