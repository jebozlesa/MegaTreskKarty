import clientPromise from './mongodb';

/**
 * đźš€ V5 - CARDID-BASED BATTLE RESULTS
 * 
 * Endpoint: POST /api/executeBattle
 * 
 * âś… HP tracking v room.selectedCards (single source of truth)
 * âś… battleResult identifikuje karty cez cardId (nie player1/player2)
 * âś… battleResult obsahuje len damage/effects (nie HP!)
 * âś… Klient naÄŤĂ­ta finĂˇlne HP z selectedCards, damage aplikuje postupne v animĂˇciĂˇch
 * 
 * Request payload:
 * {
 *   roomCode: "ABC123",
 *   playerId: "player1_id",
 *   attackData: {
 *     cardId: "card_123",
 *     attackId: 1
 *   }
 * }
 * 
 * Response (battleResult):
 * {
 *   attacks: {
 *     "cardId1": { damage: 2, didSleep: false, ... },
 *     "cardId2": { damage: 1, didSleep: true, ... }
 *   },
 *   firstAttacker: "cardId1"
 * }
 */

function extractParam(req, key) {
  return req.body?.[key]
    || req.body?.FunctionArgument?.[key]
    || req.FunctionArgument?.[key]
    || req.query?.[key]
    || req.FunctionParameter?.[key];
}

/**
 * NaÄŤĂ­ta selectedCard pre danĂ©ho hrĂˇÄŤa (LIVE stats!)
 */
async function getSelectedCard(roomCode, playerId) {
  try {
    const client = await clientPromise;
    const db = client.db();
    const collection = db.collection('rooms');

    const room = await collection.findOne({ roomCode });

    if (!room || !room.selectedCards || !room.selectedCards[playerId]) {
      console.error(`[getSelectedCard] Card not found for player ${playerId}`);
      return null;
    }

    const card = room.selectedCards[playerId];
    console.log(`[getSelectedCard] ${playerId}: ${card.name} (HP:${card.health}/${card.maxHealth}, STR:${card.strength}, DEF:${card.defense}, SPD:${card.speed})`);
    
    return card;
  } catch (error) {
    console.error('[getSelectedCard] Error:', error.message);
    return null;
  }
}

/**
 * UloĹľĂ­ updatovanĂ© selectedCards spĂ¤ĹĄ do DB
 */
async function saveSelectedCards(roomCode, player1Id, player2Id, card1, card2) {
  try {
    const client = await clientPromise;
    const db = client.db();
    const collection = db.collection('rooms');

    await collection.updateOne(
      { roomCode },
      {
        $set: {
          [`selectedCards.${player1Id}`]: card1,
          [`selectedCards.${player2Id}`]: card2,
          lastActivity: new Date()
        }
      }
    );

    console.log(`[saveSelectedCards] Updated: ${card1.name}=${card1.health}HP, ${card2.name}=${card2.health}HP`);
    return true;
  } catch (error) {
    console.error('[saveSelectedCards] Error:', error.message);
    return false;
  }
}

/**
 * Simuluje Punch Ăştok (Attack ID 1)
 * Damage: (strength/3) - (defense/3), min 1
 * Effect: 20% Ĺˇanca na sleep (1-2 turns)
 */
function executePunch(attackerCard, defenderCard) {
  let damage = Math.floor(attackerCard.strength / 3) - Math.floor(defenderCard.defense / 3);
  if (damage < 1) damage = 1;

  console.log(`[executePunch] ${attackerCard.name} -> ${defenderCard.name}: ${damage} damage`);

  defenderCard.health -= damage;
  if (defenderCard.health < 0) defenderCard.health = 0;
      return res.status(200).json({
        success: true,
        bothPlayersReady: false,
        playersReady: playersReady,
        message: 'Waiting for opponent to submit attack'
      });
    }

  } catch (error) {
    console.error('[executeBattle] Error:', error);
    return res.status(500).json({
      success: false,
      error: error.message
    });
  }
}

