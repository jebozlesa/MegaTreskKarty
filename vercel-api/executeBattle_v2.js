// ================================================
// VERCEL FUNKCIA - executeBattle.js
// ================================================
// Tento súbor funguje ako server-side battle simulator
// Číta card stats z PlayFab TitleData (databáza)
// 
// Deployment:
// 1. Skopíruj tento súbor do /api/executeBattle.js vo Vercel projekte
// 2. Nastav environment variables v Vercel:
//    - PLAYFAB_TITLE_ID=tvoj_title_id
//    - PLAYFAB_SECRET_KEY=tvoj_secret_key
// 3. Vercel deploy --prod

const axios = require('axios');

// ================================================
// PLAYFAB API HELPERS
// ================================================

/**
 * Načíta card stats z PlayFab TitleData
 */
async function getCardStatsFromDB(cardId) {
    const titleId = process.env.PLAYFAB_TITLE_ID;
    const secretKey = process.env.PLAYFAB_SECRET_KEY;
    
    if (!titleId || !secretKey) {
        throw new Error('PLAYFAB_TITLE_ID or PLAYFAB_SECRET_KEY not configured');
    }
    
    try {
        // Načítaj card data z TitleData
        const response = await axios.post(
            `https://${titleId}.playfabapi.com/Server/GetTitleData`,
            {
                Keys: [`Card_${cardId}`]
            },
            {
                headers: {
                    'X-SecretKey': secretKey,
                    'Content-Type': 'application/json'
                }
            }
        );
        
        const cardDataKey = `Card_${cardId}`;
        if (!response.data.data.Data || !response.data.data.Data[cardDataKey]) {
            console.error(`Card ${cardId} not found in database`);
            return null;
        }
        
        const cardData = JSON.parse(response.data.data.Data[cardDataKey]);
        console.log(`[DB] Loaded card ${cardId}:`, cardData);
        
        return cardData;
        
    } catch (error) {
        console.error('[DB] Error loading card stats:', error.message);
        return null;
    }
}

/**
 * Uloží battle data do TitleData
 */
async function saveBattleData(roomCode, battleData) {
    const titleId = process.env.PLAYFAB_TITLE_ID;
    const secretKey = process.env.PLAYFAB_SECRET_KEY;
    
    try {
        await axios.post(
            `https://${titleId}.playfabapi.com/Server/SetTitleData`,
            {
                Key: `Battle_${roomCode}`,
                Value: JSON.stringify(battleData)
            },
            {
                headers: {
                    'X-SecretKey': secretKey,
                    'Content-Type': 'application/json'
                }
            }
        );
        
        console.log(`[DB] Saved battle data for room ${roomCode}`);
        
    } catch (error) {
        console.error('[DB] Error saving battle data:', error.message);
        throw error;
    }
}

/**
 * Načíta battle data z TitleData
 */
async function loadBattleData(roomCode) {
    const titleId = process.env.PLAYFAB_TITLE_ID;
    const secretKey = process.env.PLAYFAB_SECRET_KEY;
    
    try {
        const response = await axios.post(
            `https://${titleId}.playfabapi.com/Server/GetTitleData`,
            {
                Keys: [`Battle_${roomCode}`]
            },
            {
                headers: {
                    'X-SecretKey': secretKey,
                    'Content-Type': 'application/json'
                }
            }
        );
        
        const battleDataKey = `Battle_${roomCode}`;
        if (!response.data.data.Data || !response.data.data.Data[battleDataKey]) {
            return null;
        }
        
        return JSON.parse(response.data.data.Data[battleDataKey]);
        
    } catch (error) {
        console.error('[DB] Error loading battle data:', error.message);
        return null;
    }
}

// ================================================
// MAIN HANDLER
// ================================================

/**
 * Hlavná Vercel funkcia
 */
export default async function handler(req, res) {
    if (req.method !== 'POST') {
        return res.status(405).json({ error: 'Method not allowed' });
    }
    
    try {
        const { roomCode, playerId, attackData } = req.body;
        
        console.log('[executeBattle] Request:', { roomCode, playerId, attackId: attackData?.attackId });
        
        if (!roomCode || !playerId || !attackData) {
            return res.status(400).json({ error: 'Missing required fields' });
        }
        
        // Ak attackId je 0, je to len status check
        if (attackData.attackId === 0) {
            const battleData = await loadBattleData(roomCode);
            if (battleData && battleData.player1?.submitted && battleData.player2?.submitted) {
                // Battle už prebehol, vráť výsledok
                return res.status(200).json({
                    success: true,
                    bothPlayersReady: true,
                    battleResult: battleData.lastResult
                });
            } else {
                return res.status(200).json({
                    success: true,
                    bothPlayersReady: false,
                    playersReady: (battleData?.player1?.submitted ? 1 : 0) + (battleData?.player2?.submitted ? 1 : 0)
                });
            }
        }
        
        // Načítaj existujúce battle data
        let battleData = await loadBattleData(roomCode);
        
        if (!battleData) {
            battleData = {
                player1: null,
                player2: null
            };
        }
        
        // Urči ktorý hráč to je
        if (!battleData.player1) {
            battleData.player1 = {
                playerId: playerId,
                attackId: attackData.attackId,
                currentHealth: attackData.attackerHealth,
                submitted: true
            };
        } else if (battleData.player1.playerId !== playerId) {
            battleData.player2 = {
                playerId: playerId,
                attackId: attackData.attackId,
                currentHealth: attackData.attackerHealth,
                submitted: true
            };
        } else {
            // Update existujúceho player1
            battleData.player1.attackId = attackData.attackId;
            battleData.player1.currentHealth = attackData.attackerHealth;
            battleData.player1.submitted = true;
        }
        
        // Ulož späť
        await saveBattleData(roomCode, battleData);
        
        // Skontroluj či sú obaja hráči ready
        if (battleData.player1 && battleData.player1.submitted && 
            battleData.player2 && battleData.player2.submitted) {
            
            console.log('[executeBattle] Both players ready - executing battle');
            
            // NAČÍTAJ STATS Z DATABÁZY
            // TODO: Získaj cardId z battle data (musíš ich ukladať pri submission)
            // Zatiaľ použijeme mock data
            
            // VYKONAJ BATTLE SIMULÁCIU
            const result = await simulateBattle(battleData.player1, battleData.player2);
            
            // Vyčisti battle data (ready pre ďalšie kolo)
            await saveBattleData(roomCode, {
                player1: { playerId: battleData.player1.playerId, submitted: false },
                player2: { playerId: battleData.player2.playerId, submitted: false },
                lastResult: result
            });
            
            return res.status(200).json({
                success: true,
                bothPlayersReady: true,
                battleResult: result
            });
        } else {
            // Čakaj na druhého hráča
            return res.status(200).json({
                success: true,
                bothPlayersReady: false,
                playersReady: (battleData.player1?.submitted ? 1 : 0) + (battleData.player2?.submitted ? 1 : 0)
            });
        }
        
    } catch (error) {
        console.error('[executeBattle] Error:', error);
        return res.status(500).json({ error: error.message });
    }
}

// ================================================
// BATTLE SIMULATION
// ================================================

/**
 * Simuluje battle medzi dvomi hráčmi
 */
async function simulateBattle(player1Data, player2Data) {
    console.log('[simulateBattle] Starting battle simulation');
    
    // TODO: Načítaj stats z DB podľa cardId
    // Zatiaľ použijeme mock stats
    const p1Stats = {
        strength: 30,
        defense: 20,
        speed: 25,
        magic: 15,
        health: player1Data.currentHealth || 100
    };
    
    const p2Stats = {
        strength: 28,
        defense: 22,
        speed: 20,
        magic: 12,
        health: player2Data.currentHealth || 100
    };
    
    console.log('[simulateBattle] Player 1 stats:', p1Stats);
    console.log('[simulateBattle] Player 2 stats:', p2Stats);
    
    // Urč kto útočí prvý (podľa speed)
    let firstAttacker, secondAttacker;
    let firstStats, secondStats;
    
    if (p1Stats.speed > p2Stats.speed) {
        firstAttacker = 'player1';
        firstStats = p1Stats;
        secondStats = p2Stats;
    } else if (p2Stats.speed > p1Stats.speed) {
        firstAttacker = 'player2';
        firstStats = p2Stats;
        secondStats = p1Stats;
    } else {
        // Rovnaká speed - random
        if (Math.random() < 0.5) {
            firstAttacker = 'player1';
            firstStats = p1Stats;
            secondStats = p2Stats;
        } else {
            firstAttacker = 'player2';
            firstStats = p2Stats;
            secondStats = p1Stats;
        }
    }
    
    console.log(`[simulateBattle] First attacker: ${firstAttacker}`);
    
    // Simuluj prvý útok (Punch)
    const firstAttackResult = executePunch(firstStats, secondStats);
    secondStats.health = firstAttackResult.defenderHealth;
    
    let secondAttackResult = null;
    
    // Ak druhý hráč prežil, vykonaj jeho útok
    if (secondStats.health > 0) {
        secondAttackResult = executePunch(secondStats, firstStats);
        firstStats.health = secondAttackResult.defenderHealth;
    }
    
    // Vráť výsledok
    const result = {
        success: true,
        firstAttacker: firstAttacker,
        player1Health: firstAttacker === 'player1' ? firstStats.health : secondStats.health,
        player2Health: firstAttacker === 'player2' ? firstStats.health : secondStats.health,
        player1Damage: firstAttacker === 'player1' ? firstAttackResult.damageDealt : (secondAttackResult?.damageDealt || 0),
        player2Damage: firstAttacker === 'player2' ? firstAttackResult.damageDealt : (secondAttackResult?.damageDealt || 0),
        player1DidSleep: firstAttacker === 'player1' ? firstAttackResult.didSleep : (secondAttackResult?.didSleep || false),
        player2DidSleep: firstAttacker === 'player2' ? firstAttackResult.didSleep : (secondAttackResult?.didSleep || false),
        player1SleepDuration: firstAttacker === 'player1' ? firstAttackResult.sleepDuration : (secondAttackResult?.sleepDuration || 0),
        player2SleepDuration: firstAttacker === 'player2' ? firstAttackResult.sleepDuration : (secondAttackResult?.sleepDuration || 0)
    };
    
    console.log('[simulateBattle] Battle result:', result);
    
    return result;
}

/**
 * Vykonaj Punch útok (Attack ID 1)
 * Vzorec: damage = (attacker.strength / 3) - (receiver.defense / 3)
 * 20% šanca na sleep (1-2 turns)
 */
function executePunch(attackerStats, defenderStats) {
    // Vypočítaj damage
    let damage = Math.floor(attackerStats.strength / 3) - Math.floor(defenderStats.defense / 3);
    
    // Minimálny damage
    if (damage < 1) damage = 1;
    
    console.log(`[executePunch] Calculated damage: ${damage}`);
    
    // Aplikuj damage
    defenderStats.health -= damage;
    if (defenderStats.health < 0) defenderStats.health = 0;
    
    // 20% šanca na sleep
    let didSleep = false;
    let sleepDuration = 0;
    
    if (Math.random() <= 0.2 && defenderStats.health > 0) {
        didSleep = true;
        sleepDuration = Math.floor(Math.random() * 2) + 1; // 1 alebo 2
        console.log(`[executePunch] Sleep applied! Duration: ${sleepDuration}`);
    }
    
    return {
        damageDealt: damage,
        defenderHealth: defenderStats.health,
        didSleep: didSleep,
        sleepDuration: sleepDuration
    };
}

// ================================================
// LOCAL TESTING
// ================================================

if (require.main === module) {
    console.log('Testing executeBattle locally...');
    
    const mockRequest = {
        method: 'POST',
        body: {
            roomCode: 'TEST123',
            playerId: 'player1',
            attackData: {
                attackId: 1,
                attackerHealth: 100
            }
        }
    };
    
    const mockResponse = {
        status: (code) => ({
            json: (data) => {
                console.log(`Response ${code}:`, JSON.stringify(data, null, 2));
            }
        })
    };
    
    handler(mockRequest, mockResponse);
}
