// SERVER FUNCTIONS SPEC: Next Turn Ready System
// Location: mega-tresk-server/api/

/*
=== 1. markReadyForNextTurn.js ===
Označí hráča ako ready pre ďalší turn + clearuje battleData ak sú obaja ready

INPUT:
{
  roomCode: "ABC123",
  playerId: "player1_id"
}

LOGIC:
1. Načítaj room z MongoDB
2. Inicializuj room.nextTurnReady ak neexistuje: { player1: false, player2: false }
3. Nastav room.nextTurnReady[playerId] = true
4. Skontroluj či sú obaja ready
5. Ak áno: vymař battleData, resetuj nextTurnReady na false
6. Ulož room do MongoDB

OUTPUT:
{
  success: true,
  bothPlayersReady: true/false,
  playersReady: { "player1_id": true, "player2_id": false }
}

=== 2. checkNextTurnReady.js ===
Polling endpoint - skontroluje ready stav

INPUT:
{
  roomCode: "ABC123"
}

OUTPUT:
{
  success: true,
  bothPlayersReady: true/false,
  playersReady: { "player1_id": true, "player2_id": false }
}

=== MongoDB Schema Update ===
rooms collection:
{
  _id: ObjectId("..."),
  roomCode: "ABC123",
  
  // ✅ NEW: Next turn ready tracking
  nextTurnReady: {
    "player1_id": false,
    "player2_id": true
  },
  
  // Existujúce battleData sa vymaže keď sú obaja ready
  battleData: null,  // Cleared when both ready
  
  // Ostatné polia zostávajú...
  battleState: { ... },
  playerDecks: { ... }
}

=== Integration s existujúcim executeBattle.js ===
- executeBattle by mal resetovať nextTurnReady na začiatku battle
- Ak niekto submitne útok, nextTurnReady sa vynuluje

=== Error Handling ===
- Room not found → return error
- Invalid playerId → return error  
- MongoDB connection issues → return error
*/