// ÚPRAVY PRE EXISTUJÚCE SERVEROVÉ SÚBORY
// Toto sú zmeny ktoré musíte aplikovať na vašom serveri

// =====================================
// 1. ÚPRAVA joinOrCreateRoom.js
// =====================================

// PRIDAJTE toto do joinOrCreateRoom.js funkcie, okolo riadku kde sa hľadajú miestnosti:

// STARÝ KÓD (nájdite toto):
// const existingRoom = await collection.findOne({
//     $or: [
//         { players: { $size: 1 } }
//     ]
// });

// NOVÝ KÓD (nahraďte týmto):
const existingRoom = await collection.findOne({
    $and: [
        {
            $or: [
                { players: { $size: 1 } }
            ]
        },
        {
            status: { $ne: "completed" } // Nevyberie completed miestnosti
        }
    ]
});

// =====================================
// PRIDAJTE toto pri vytvorení novej miestnosti:

// STARÝ KÓD (nájdite toto):
// const newRoom = {
//     roomCode: roomCode,
//     players: [playerId],
//     playersInfo: [playerInfo],
//     createdAt: new Date(),
//     lastActivity: new Date()
// };

// NOVÝ KÓD (nahraďte týmto):
const newRoom = {
    roomCode: roomCode,
    players: [playerId],
    playersInfo: [playerInfo],
    status: "waiting", // Nové pole
    createdAt: new Date(),
    lastActivity: new Date()
};

// =====================================
// PRIDAJTE toto keď sa pripojí druhý hráč:

// NÁJDITE miesto kde sa aktualizuje miestnosť s druhým hráčom a PRIDAJTE:
const updateResult = await collection.updateOne(
    { roomCode: existingRoom.roomCode },
    {
        $push: {
            players: playerId,
            playersInfo: playerInfo
        },
        $set: {
            status: "active", // Označíme ako aktívnu
            lastActivity: new Date()
        }
    }
);

// =====================================
// 2. ÚPRAVA leaveRoom.js
// =====================================

// NÁJDITE miesto kde sa odstraňuje hráč a PRIDAJTE logiku:

// Po odstránení hráča, ak zostane len jeden:
if (updatedRoom.players.length === 1) {
    // Označíme miestnosť ako completed - už sa do nej nedá vrátiť
    await collection.updateOne(
        { roomCode: roomCode },
        {
            $set: {
                status: "completed",
                completedAt: new Date(),
                lastActivity: new Date()
            }
        }
    );
}

// =====================================
// 3. ÚPRAVA cleanupRooms.js
// =====================================

// PRIDAJTE cleanup pre staré completed miestnosti:

// Na koniec cleanupRooms funkcie PRIDAJTE:

// Odstráň staré completed miestnosti (staršie ako 1 hodina)
const oneHourAgo = new Date(Date.now() - 60 * 60 * 1000);
const cleanupCompleted = await collection.deleteMany({
    status: "completed",
    completedAt: { $lt: oneHourAgo }
});

// Aktualizujte return statement:
return {
    success: true,
    removedPlayers: removedPlayersCount,
    removedRooms: removedRoomsCount,
    removedCompletedRooms: cleanupCompleted.deletedCount, // Nové
    message: `Cleanup completed: ${removedPlayersCount} inactive players removed, ${removedRoomsCount} empty rooms removed, ${cleanupCompleted.deletedCount} old completed rooms removed`
};

// =====================================
// 4. NOVÁ FUNKCIA: markRoomAsCompleted.js
// =====================================

// Vytvorte nový súbor markRoomAsCompleted.js:

const { MongoClient } = require('mongodb');

module.exports = async (req, res) => {
    const { roomCode, playerId } = req.body;

    if (!roomCode || !playerId) {
        return res.status(400).json({
            success: false,
            message: 'roomCode and playerId are required'
        });
    }

    const client = new MongoClient(process.env.MONGODB_URI);

    try {
        await client.connect();
        const db = client.db('megatresk');
        const collection = db.collection('rooms');

        // Označ miestnosť ako completed
        const result = await collection.updateOne(
            { 
                roomCode: roomCode,
                players: playerId // Overiť že hráč je v miestnosti
            },
            {
                $set: {
                    status: "completed",
                    completedAt: new Date(),
                    completedBy: playerId,
                    lastActivity: new Date()
                }
            }
        );

        if (result.matchedCount === 0) {
            return res.status(404).json({
                success: false,
                message: 'Room not found or player not in room'
            });
        }

        return res.status(200).json({
            success: true,
            message: 'Room marked as completed',
            roomCode: roomCode
        });

    } catch (error) {
        console.error('Error marking room as completed:', error);
        return res.status(500).json({
            success: false,
            message: 'Internal server error'
        });
    } finally {
        await client.close();
    }
};