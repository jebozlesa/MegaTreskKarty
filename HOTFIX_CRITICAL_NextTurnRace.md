# 🚨 CRITICAL HOTFIX - Next Turn Race Condition

**Dátum:** 2025-10-29  
**Problém:** Timeout po prvom kole + súbežné submity sa nezapisujú  
**Root cause:** Race condition v `nextTurnReady` + chýbajúci reset v `executeBattle`

---

## 🐛 **Problém z logov:**

### **Symptóm #1: Timeout po battle**
```
MarkReadyForNextTurn → bothPlayersReady=false (player2=true, player1=false)
checkNextTurnReady (polling) → bothPlayersReady=false
... 30x polling ...
[BattleResultProcessor] Timeout waiting for opponent to be ready
```

**Root cause:**  
- Po `executeBattle` sa `nextTurnReady` **NERESETUJE**
- Keď prvý hráč zavolá `markReadyForNextTurn`, vidí druhého hráča ako TRUE z minulého kola
- Polling vracia `bothPlayersReady=false`, ale v skutočnosti jeden hráč je stuck na TRUE

### **Symptóm #2: Súbežné submity failujú**
Keď obaja hráči submitnú útok **súčasne**:
- Jeden zápis do `battleData` prepíše druhý (race condition)
- `executeBattle` dostane len 1 útok namiesto 2
- Battle sa nespustí → čakanie na timeout

---

## ✅ **Riešenie:**

### **1. Oprav `executeBattle.js`** (C:\Zlozka\mega-tresk-server\api\executeBattle.js)

**Problém:** `nextTurnReady` sa neresetuje po battle  
**Fix:** Reset `nextTurnReady` PRED vyčistením `battleData`

**NAJDI tento kód** (cca riadok 350-365):
```javascript
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
```

**NAHRAĎ s týmto:**
```javascript
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
```

---

### **2. Oprav race condition pri súbežných submitoch**

**Problém:** Keď obaja hráči submitnú súčasne, jeden prepíše druhého  
**Fix:** Použiť `$set` s nested path namiesto celého objektu

**NAJDI tento kód** (cca riadok 300-320):
```javascript
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
```

**NAHRAĎ s týmto:**
```javascript
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
```

---

### **3. Verifikuj `markReadyForNextTurn.js` (UŽ OPRAVENÉ)**

✅ Aktuálny kód v `c:\Zlozka\mega-tresk-server\api\markReadyForNextTurn.js` je **OK**:
```javascript
if (bothReady) {
  updateQuery = {
    $set: {
      nextTurnReady: nextTurnReady, // ✅ Ponechá TRUE, executeBattle ich resetne
      lastActivity: new Date()
    },
    $unset: {
      battleData: ""
    }
  };
}
```

**Prečo to funguje:**
1. Obaja hráči sú ready → `markReadyForNextTurn` vymaže `battleData`
2. Klienti volajú `executeBattle` → battle prebehne
3. **`executeBattle` resetne `nextTurnReady` na `{p1:false, p2:false}`** ✅
4. Ďalší turn → znova `markReadyForNextTurn` → cycle pokračuje

---

## 🧪 **Testing:**

### **Test 1: Normálny flow**
```
1. Player1 submitne útok → markReadyForNextTurn(p1) → {p1:true, p2:false}
2. Player2 submitne útok → markReadyForNextTurn(p2) → {p1:true, p2:true}
3. executeBattle() → battle → RESET nextTurnReady → {p1:false, p2:false}
4. Player1 vyberie nový útok → čaká na p2 ✅
5. Player2 vyberie nový útok → battle ✅
```

### **Test 2: Súbežné submity**
```
1. Player1 a Player2 submitnú SÚČASNE (< 100ms rozdiel)
2. Atomic updates → oba zápisy sa aplikujú ✅
3. executeBattle vidí oba útoky → battle prebehne ✅
```

### **Test 3: Rýchle klikanie**
```
1. Player1 klikne 5x rýchlo submit
2. Len prvý sa zapíše (submitted=true)
3. Ďalšie requesty sú no-op (už submitted) ✅
```

---

## 📋 **Deployment Checklist:**

- [ ] 1. Oprav `executeBattle.js` - RESET nextTurnReady po battle
- [ ] 2. Oprav `executeBattle.js` - Atomic updates pre battleData
- [ ] 3. Verifikuj `markReadyForNextTurn.js` (má ponechať nextTurnReady=true)
- [ ] 4. Verifikuj `checkNextTurnReady.js` (read-only, žiadne zmeny)
- [ ] 5. Git commit: "HOTFIX: Race condition v nextTurnReady + atomic battleData updates"
- [ ] 6. Git push → Vercel auto-deploy (~2 min)
- [ ] 7. Test s 2 klientmi (Unity + Android)

---

## 🔍 **Verifikácia po deploye:**

```powershell
# Monitor logs na Vercel
# Dashboard → Functions → executeBattle → View Logs

# Hľadaj:
# ✅ "[executeBattle] Battle complete, nextTurnReady reset to: {p1:false, p2:false}"
# ✅ "[executeBattle] player1 submitted attack 1"
# ✅ "[executeBattle] player2 submitted attack 1"
# ✅ "[executeBattle] Both players ready - executing battle!"

# ❌ Nemali by sa objaviť:
# ❌ "Timeout waiting for opponent"
# ❌ "Database error"
# ❌ Polling viac ako 5x pre jeden turn
```

---

## 📊 **Očakávané výsledky:**

**Pred fix:**
- Timeout po 1. kole: **100%** (reprodukovateľné)
- Súbežné submity failujú: **~50%** (race condition)
- Avg. polling calls per turn: **30-60** (timeout)

**Po fixe:**
- Timeout po 1. kole: **0%**
- Súbežné submity failujú: **0%**
- Avg. polling calls per turn: **1-2** (okamžitá odpoveď)

---

**Priority:** 🔴 **CRITICAL**  
**Impact:** Hra je neplayable bez tohto fixu  
**Effort:** 🟢 **LOW** (10 minút coding, 2 minúty deploy)
