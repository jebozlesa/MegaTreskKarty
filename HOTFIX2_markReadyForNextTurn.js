// 🎯 HOTFIX #2 pre markReadyForNextTurn.js
// Problem: Reset nextTurnReady flags aj keď sú obaja ready
// Fix: Ak sú obaja ready, vráť success bez resetu flags

// V markReadyForNextTurn.js, NAJDI RIADKY 71-84:
/*
    // Ak sú obaja ready → clear battleData a reset nextTurnReady
    if (bothReady) {
      console.log('[markReadyForNextTurn] Both players ready, cleared battleData for next turn');
      
      // Reset nextTurnReady pre všetkých hráčov
      const resetNextTurnReady = {};
      room.players.forEach(pid => {
        resetNextTurnReady[pid] = false;
      });

      updateQuery = {
        $set: {
          nextTurnReady: resetNextTurnReady,
          lastActivity: new Date()
        },
        $unset: {
          battleData: ""
        }
      };
      
      message = "Both ready, cleared for next turn";
    }
*/

// NAHRAĎ S TÝMTO:
/*
    // Ak sú obaja ready → clear battleData, ale NEZRESETUJ nextTurnReady
    if (bothReady) {
      console.log('[markReadyForNextTurn] Both players ready, clearing battleData only');
      
      updateQuery = {
        $set: {
          nextTurnReady: nextTurnReady, // Ponechaj aktuálny stav!
          lastActivity: new Date()
        },
        $unset: {
          battleData: ""
        }
      };
      
      message = "Both ready, cleared for next turn";
    }
*/

console.log("HOTFIX #2 ready for deployment");