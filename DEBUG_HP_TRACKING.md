# 🔍 HP Tracking Debug Guide

**Problém:** HP sa v health bare nepripočítavajú správne po Heal útoku. Hodnoty v DB a Unity editori sú rôzne.

**Pridané debug logy:** Teraz každý HP update loguje detaily, aby sme identifikovali kde sa HP desynchronizuje.

---

## 🎯 Debug Log Legend

### 📦 **[BATTLE_RESULT]** - Server Response
```
📦 [BATTLE_RESULT] ===== RAW SERVER RESPONSE =====
📦 [BATTLE_RESULT] firstAttacker: card_uuid_123
📦 [BATTLE_RESULT] attacks: {...}
```
**Čo sledovať:**
- ✅ Obsahuje `healAmount` field? (pre Attack ID 3)
- ✅ Obsahuje správne `attackId`? (1=Punch, 2=Kick, 3=Heal)

---

### 🎴 **[CARDS]** - Initial Card State
```
🎴 [CARDS] MY: Jesus Cristus (cardId=abc123, HP=25/25)
🎴 [CARDS] ENEMY: Bruce Lee (cardId=xyz789, HP=22/22)
```
**Čo sledovať:**
- ✅ Sú HP správne pred battle?
- ✅ Kard.health vs maxHealth (25/25 = full HP)

---

### 🩹 **[HEAL]** - Heal Attack Execution
```
🩹 [HEAL] Jesus Cristus heals for 5 HP! (Before: 20/25)
🩹 [HEAL] Jesus Cristus after Heal(): HP=25/25
🩹 [HEAL] Updating HP bar for MY card
🩹 [HEAL] playerLifeBar.SetHP(25) called
```
**Čo sledovať:**
- ✅ `healAmount > 0`? (ak 0, server nevráti healAmount!)
- ✅ HP pred vs po Heal() metóde
- ✅ HP bar update volanie (MY vs ENEMY)

---

### 💥 **[DAMAGE]** - Damage Attack Execution
```
💥 [DAMAGE] Bruce Lee attacks Jesus Cristus for 2 damage! (Defender HP before: 25/25)
💥 [DAMAGE] Jesus Cristus after damage: HP=23/25
💥 [DAMAGE] Updating HP bar for MY card
💥 [DAMAGE] playerLifeBar.SetHP(23) called
```
**Čo sledovať:**
- ✅ Damage amount správny?
- ✅ HP before vs after (25 → 23 = -2 damage)
- ✅ HP bar update správneho hráča (MY vs ENEMY)

---

### 🔄 **[REFRESH]** - Server Refresh (CRITICAL!)
```
🔄 [REFRESH] Updating Jesus Cristus from server:
🔄 [REFRESH]   - Current Kard.health: 25/25
🔄 [REFRESH]   - Server cardData.health: 23/25
🔄 [REFRESH]   - After update Kard.health: 23/25 (change: -2)
🔄 [REFRESH] playerLifeBar.SetHP(23) - MY card synced
```
**⚠️ CRITICAL - Tu môže byť problém!**

**Ak server má staré HP:**
- Scenario: Unity heal 20→25, ale server ešte nevidel heal → server vráti 20
- RefreshCardsFromServer() prepíše Kard.health = 20 (stratíme heal!)
- HP bar sa vráti na 20 namiesto 25

**Čo sledovať:**
- ❌ `change: -5` po heal útoku = SERVER PREPÍSAL HP!
- ✅ `change: 0` po heal útoku = server má správne HP
- ✅ STR/DEF/SPD changes (ak attack menil stats)

---

### ❤️ **[HP_BAR]** - Health Bar Updates
```
❤️ [HP_BAR] PlayerHealthBar.SetHP(25) → Clamped: 25/25 (fillAmount: 1.00) [Change: +5]
❤️ [HP_BAR] EnemyHealthBar.SetHP(20) → Clamped: 20/22 (fillAmount: 0.91) [Change: -2]
```
**Čo sledovať:**
- ✅ `fillAmount` = actual bar fill (1.00 = 100%, 0.50 = 50%)
- ✅ `Change:` = HP delta od posledného SetHP()
- ✅ Clamped HP (nemôže byť > maxHp alebo < 0)

---

## 🧪 Test Scenario: Heal Attack

### Expected Log Flow:
```
1. 📦 [BATTLE_RESULT] Server vráti: attackId=3, healAmount=5, damage=0
2. 🎴 [CARDS] MY: Jesus (HP=20/25), ENEMY: Bruce (HP=22/22)
3. 🩹 [HEAL] Jesus heals for 5 HP! (Before: 20/25)
4. 🩹 [HEAL] After Heal(): HP=25/25
5. 🩹 [HEAL] playerLifeBar.SetHP(25) called
6. ❤️ [HP_BAR] PlayerHealthBar.SetHP(25) → fillAmount: 1.00 [Change: +5]
7. 🔄 [REFRESH] Server cardData.health: 25/25 ✅ (ak 20 = BUG!)
8. ❤️ [HP_BAR] PlayerHealthBar.SetHP(25) → fillAmount: 1.00 [Change: 0]
```

### ❌ BUG Indicators:
```
❌ 🩹 [HEAL] healAmount=0! Server didn't return healAmount!
   → Server nerealizoval Task 3 (add healAmount to response)

❌ 🔄 [REFRESH] Server cardData.health: 20/25 (change: -5)
   → Server prepísal HP na staré hodnoty!
   → executeBattle.js neaplikoval heal na server-side selectedCards

❌ ❤️ [HP_BAR] PlayerHealthBar.SetHP(20) → fillAmount: 0.80 [Change: -5]
   → HP bar sa vrátil späť po refresh!
```

---

## 🧪 Test Scenario: Damage Attack

### Expected Log Flow:
```
1. 📦 [BATTLE_RESULT] attackId=1, damage=2
2. 🎴 [CARDS] MY: Jesus (HP=25/25), ENEMY: Bruce (HP=22/22)
3. 💥 [DAMAGE] Bruce attacks Jesus for 2 damage! (Before: 25/25)
4. 💥 [DAMAGE] Jesus after damage: HP=23/25
5. 💥 [DAMAGE] playerLifeBar.SetHP(23) called
6. ❤️ [HP_BAR] PlayerHealthBar.SetHP(23) → fillAmount: 0.92 [Change: -2]
7. 🔄 [REFRESH] Server cardData.health: 23/25 ✅
8. ❤️ [HP_BAR] PlayerHealthBar.SetHP(23) → fillAmount: 0.92 [Change: 0]
```

---

## 🔍 Common Issues

### Issue 1: "HP sa nezmení po heal"
**Príčina:** `healAmount=0` v server response  
**Fix:** Implementuj Task 3 v `executeBattle.js` (add healAmount field)

### Issue 2: "HP sa heal-ne, ale potom sa vráti späť"
**Príčina:** `RefreshCardsFromServer()` prepíše HP zo servera (server má staré hodnoty)  
**Fix:** Server musí aplikovať heal na `selectedCards` collection, nie len vrátiť v battleResult

### Issue 3: "HP bar ukazuje inú hodnotu než Kard.health"
**Príčina:** Desync medzi `card.health` a `healthBar.hp`  
**Check Logs:**
```
🩹 [HEAL] After Heal(): HP=25  ← Kard.health
❤️ [HP_BAR] SetHP(25) → Clamped: 25  ← HP bar value
```

### Issue 4: "DB má iné HP než Unity"
**Príčina 1:** `selectedCards` v MongoDB sa neupdatuje po battle  
**Príčina 2:** `RefreshCardsFromServer()` sa nevolá  
**Check Server:** MongoDB Compass → rooms collection → selectedCards.{playerId}.health

---

## 🛠️ Debug Commands

### Unity Console Filter:
```
Search: [HEAL] | [DAMAGE] | [REFRESH] | [HP_BAR]
```

### MongoDB Check:
```javascript
use megatresk
db.rooms.findOne(
  { roomCode: "N25ZGW" },
  { "selectedCards": 1, "battleData.lastResult": 1 }
)

// Expected:
selectedCards: {
  "player1_id": {
    name: "Jesus Cristus",
    health: 25,  // ✅ Should match Unity after refresh
    maxHealth: 25
  }
}
```

### Server Logs (Vercel):
```
Search for: [executeHeal] | selectedCards update | healAmount
```

---

## 📊 HP Lifecycle

```
1. INITIAL STATE (DB)
   MongoDB selectedCards.health = 25

2. UNITY LOAD (GetSelectedCards)
   Kard.health = 25
   playerLifeBar.SetHP(25) → fillAmount: 1.00

3. BATTLE EXECUTED
   Server: executeHeal() → health += 5 → 30 (clamped to 25)
   Server: Updates selectedCards.health = 25 in DB ✅
   Server: Returns { healAmount: 5 }

4. UNITY ANIMATION
   attacker.Heal(5) → health: 20→25
   playerLifeBar.SetHP(25) → fillAmount: 1.00

5. UNITY REFRESH (RefreshCardsFromServer)
   GetSelectedCards → { health: 25 }
   card.health = 25 (no change)
   playerLifeBar.SetHP(25) → fillAmount: 1.00 (no change)

✅ HP SYNCED: DB=25, Unity=25, HP Bar=100%
```

---

## 🚨 Ak vidíš toto v logoch:

### 🔴 **CRITICAL BUG:**
```
🔄 [REFRESH]   - Current Kard.health: 25/25
🔄 [REFRESH]   - Server cardData.health: 20/25  ← ❌ SERVER MÁ STARÉ HP!
🔄 [REFRESH]   - After update: 20/25 (change: -5)  ← ❌ HP PREPÍSANÉ!
```
**Action:** Server neaplikoval heal! Check `executeBattle.js` → executeHeal() → update selectedCards

### 🔴 **MISSING DATA:**
```
🩹 [HEAL] ⚠️ healAmount=0! Server didn't return healAmount!
```
**Action:** Server nevráti `healAmount` field! Check `executeBattle.js` Task 3

### 🟢 **ALL OK:**
```
🩹 [HEAL] Jesus heals for 5 HP! (Before: 20/25)
🩹 [HEAL] After Heal(): HP=25/25
❤️ [HP_BAR] SetHP(25) → fillAmount: 1.00 [Change: +5]
🔄 [REFRESH] Server cardData.health: 25/25 (change: 0)  ← ✅ SYNCED!
❤️ [HP_BAR] SetHP(25) → fillAmount: 1.00 [Change: 0]
```

---

**Version:** V9 Debug Build  
**Date:** 2025-11-06  
**Status:** Waiting for test run
