import clientPromise from './mongodb';

/**
 * 🚀 V5 - CARDID-BASED BATTLE RESULTS
 * 
 * Endpoint: POST /api/executeBattle
 * 
 * ✅ HP tracking v room.selectedCards (single source of truth)
 * ✅ battleResult identifikuje karty cez cardId (nie player1/player2)
 * ✅ battleResult obsahuje len damage/effects (nie HP!)
 * ✅ Klient načíta finálne HP z selectedCards, damage aplikuje postupne v animáciách
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
 * Načíta selectedCard pre daného hráča (LIVE stats!)
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
 * Uloží updatované selectedCards späť do DB
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
 * Simuluje Punch útok (Attack ID 1)
 * Damage: (strength/3) - (defense/3), min 1
 * Effect: 20% šanca na sleep (1-2 turns)
 */
function executePunch(attackerCard, defenderCard) {
  let damage = Math.floor(attackerCard.strength / 3) - Math.floor(defenderCard.defense / 3);
  if (damage < 1) damage = 1;

  console.log(`[executePunch] ${attackerCard.name} -> ${defenderCard.name}: ${damage} damage`);

  defenderCard.health -= damage;
  if (defenderCard.health < 0) defenderCard.health = 0;

  // 20% sleep chance
  let didSleep = false;
  let sleepDuration = 0;

  if (Math.random() <= 0.2 && defenderCard.health > 0) {
    didSleep = true;
    sleepDuration = Math.floor(Math.random() * 2) + 1; // 1-2 turns
    
    // Pridaj effect
    if (!defenderCard.effects) defenderCard.effects = [];
    defenderCard.effects.push({
      type: 'sleep',
      duration: sleepDuration,
      appliedTurn: (defenderCard.turnNumber || 1)
    });
    
    console.log(`[executePunch] ${defenderCard.name} fell asleep for ${sleepDuration} turns!`);
  }

  return {
    damage: damage,
    didSleep: didSleep,
    sleepDuration: sleepDuration
  };
}

/**
 * Simuluje battle medzi dvomi kartami
 */
function simulateBattle(card1, card2, attackId1, attackId2) {
  console.log('[simulateBattle] Starting battle');
  console.log(`  Card1: ${card1.name} (ATK:${attackId1}, HP:${card1.health}, STR:${card1.strength}, SPD:${card1.speed})`);
  console.log(`  Card2: ${card2.name} (ATK:${attackId2}, HP:${card2.health}, STR:${card2.strength}, SPD:${card2.speed})`);

  // Urč kto útočí prvý podľa speed
  let firstAttacker, secondAttacker;
  let firstCard, secondCard;
  let firstAttackId, secondAttackId;

  if (card1.speed > card2.speed) {
    firstAttacker = 'player1';
    firstCard = card1;
    secondCard = card2;
    firstAttackId = attackId1;
    secondAttackId = attackId2;
  } else if (card2.speed > card1.speed) {
    firstAttacker = 'player2';
    firstCard = card2;
    secondCard = card1;
    firstAttackId = attackId2;
    secondAttackId = attackId1;
  } else {
    // Rovnaká rýchlosť - random
    if (Math.random() < 0.5) {
      firstAttacker = 'player1';
      firstCard = card1;
      secondCard = card2;
      firstAttackId = attackId1;
      secondAttackId = attackId2;
    } else {
      firstAttacker = 'player2';
      firstCard = card2;
      secondCard = card1;
      firstAttackId = attackId2;
      secondAttackId = attackId1;
    }
  }

  console.log(`[simulateBattle] First attacker: ${firstAttacker} (${firstCard.name})`);

  // Prvý útok
  let firstResult = { damage: 0, didSleep: false, sleepDuration: 0 };
  if (firstAttackId === 1) {
    firstResult = executePunch(firstCard, secondCard);
  }
  // TODO: Pridaj switch pre ostatné attackId (2, 3, 4, ...)

  // Druhý útok (len ak prežil)
  let secondResult = { damage: 0, didSleep: false, sleepDuration: 0 };
  if (secondCard.health > 0) {
    if (secondAttackId === 1) {
      secondResult = executePunch(secondCard, firstCard);
    }
    // TODO: Switch pre ostatné attackId
  }

  // Zostav result object
  // ✅ V5: Identifikácia pomocou cardId namiesto player1/player2
  const result = {
    firstAttacker: firstCard.cardId,
    
    attacks: {
      [card1.cardId]: {
        damage: firstAttacker === 'player1' ? firstResult.damage : secondResult.damage,
        didSleep: firstAttacker === 'player1' ? secondResult.didSleep : firstResult.didSleep,
        sleepDuration: firstAttacker === 'player1' ? secondResult.sleepDuration : firstResult.sleepDuration,
        effects: card1.effects || []
      },
      [card2.cardId]: {
        damage: firstAttacker === 'player2' ? firstResult.damage : secondResult.damage,
        didSleep: firstAttacker === 'player2' ? secondResult.didSleep : firstResult.didSleep,
        sleepDuration: firstAttacker === 'player2' ? secondResult.sleepDuration : firstResult.sleepDuration,
        effects: card2.effects || []
      }
    }
  };

  console.log('[simulateBattle] Result:', JSON.stringify(result, null, 2));
  return result;
}

/**
 * Main handler
 */
export default async function handler(req, res) {
  if (req.method !== 'POST') {
    return res.status(405).json({ success: false, error: 'Method not allowed' });
  }

  try {
    const roomCode = extractParam(req, 'roomCode');
    const playerId = extractParam(req, 'playerId');
    const attackDataParam = extractParam(req, 'attackData');

    console.log('[executeBattle] Request:', { roomCode, playerId, attackData: attackDataParam });

    if (!roomCode || !playerId || !attackDataParam) {
      return res.status(400).json({
        success: false,
        error: 'Missing required fields: roomCode, playerId, attackData'
      });
    }

    const { cardId, attackId } = attackDataParam;

    if (!cardId || attackId === undefined) {
      return res.status(400).json({
        success: false,
        error: 'Missing required attack data fields: cardId, attackId'
      });
    }

    // ✅ STATUS CHECK (polling s attackId=0)
    if (attackId === 0) {
      const client = await clientPromise;
      const db = client.db();
      const collection = db.collection('rooms');

      const room = await collection.findOne({ roomCode });
      if (!room || !room.battleData) {
        return res.status(200).json({
          success: true,
          bothPlayersReady: false,
          playersReady: 0
        });
      }

      const battleData = room.battleData;
      
      // Vráť lastResult ak existuje
      if (battleData.lastResult) {
        console.log('[executeBattle] Returning cached lastResult');
        return res.status(200).json({
          success: true,
          bothPlayersReady: true,
          battleResult: battleData.lastResult
        });
      }

      return res.status(200).json({
        success: true,
        bothPlayersReady: false,
        playersReady: (battleData.player1?.submitted ? 1 : 0) + (battleData.player2?.submitted ? 1 : 0)
      });
    }

    // ✅ ATTACK SUBMISSION
    const client = await clientPromise;
    const db = client.db();
    const collection = db.collection('rooms');

    const room = await collection.findOne({ roomCode });
    if (!room) {
      return res.status(404).json({ success: false, error: 'Room not found' });
    }

    // Urč player1 / player2
    const isPlayer1 = room.players[0] === playerId;
    const player1Id = room.players[0];
    const player2Id = room.players[1];

    // Načítaj/vytvor battleData
    let battleData = room.battleData || { player1: null, player2: null };

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

    // ✅ CHECK: Sú obaja hráči ready?
    if (battleData.player1?.submitted && battleData.player2?.submitted) {
      console.log('[executeBattle] Both players ready - executing battle!');

      // Načítaj selectedCards (LIVE stats!)
      const card1 = await getSelectedCard(roomCode, player1Id);
      const card2 = await getSelectedCard(roomCode, player2Id);

      if (!card1 || !card2) {
        return res.status(500).json({ success: false, error: 'Failed to load cards' });
      }

      // Simuluj battle (modifikuje card1 a card2 in-place!)
      const battleResult = simulateBattle(
        card1,
        card2,
        battleData.player1.attackId,
        battleData.player2.attackId
      );

      // ✅ ULOŽ UPDATOVANÉ CARDS (HP, effects, atď.)
      await saveSelectedCards(roomCode, player1Id, player2Id, card1, card2);

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
    } else {
      // Čakaj na druhého hráča
      const playersReady = (battleData.player1?.submitted ? 1 : 0) + (battleData.player2?.submitted ? 1 : 0);
      console.log(`[executeBattle] Waiting for opponent (${playersReady}/2)`);

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
