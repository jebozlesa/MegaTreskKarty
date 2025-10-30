// ✂️ COPY-PASTE FIX #2: Atomic battleData updates (race condition fix)
// Location: c:\Zlozka\mega-tresk-server\api\executeBattle.js
// Line: ~300-320 (po načítaní room, pred check bothPlayersReady)

// ❌ REMOVE THIS:
/*
    // Ulož attack submission
    if (isPlayer1) {
      battleData.player1 = {
        playerId: playerId,
        cardId: cardId,
        attackId: attackId,
        submitted: true
      };
    } else {
      battleData.player2 = {
        playerId: playerId,
        cardId: cardId,
        attackId: attackId,
        submitted: true
      };
    }

    // Ulož battleData
    await collection.updateOne(
      { roomCode },
      { $set: { battleData: battleData, lastActivity: new Date() } }
    );
*/

// ✅ REPLACE WITH THIS:
    // ✅ ATOMIC UPDATE - použiť nested path namiesto celého objektu
    const playerKey = isPlayer1 ? 'player1' : 'player2';
    
    await collection.updateOne(
      { roomCode },
      {
        $set: {
          [`battleData.${playerKey}.playerId`]: playerId,
          [`battleData.${playerKey}.cardId`]: cardId,
          [`battleData.${playerKey}.attackId`]: attackId,
          [`battleData.${playerKey}.submitted`]: true,
          lastActivity: new Date()
        }
      }
    );
    
    console.log(`[executeBattle] ${playerKey} submitted attack ${attackId}`);
    
    // ✅ RE-LOAD battleData po zápise (aby sme mali fresh data)
    const updatedRoom = await collection.findOne({ roomCode });
    battleData = updatedRoom.battleData || { player1: null, player2: null };
