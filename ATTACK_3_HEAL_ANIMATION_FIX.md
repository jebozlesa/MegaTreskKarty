# 🩹 Attack ID 3 (Heal) - Zelená HP Animácia Fix

**Problém:** Heal útok sa spustil, ale nezobrazila sa zelená HP animácia ani sa nepridalo HP.

**Príčina:** Server vrátil `damage: 0`, ale **chýbal `healAmount` field**. Unity potrebuje `healAmount` pre `attacker.Heal()` metódu, ktorá spúšťa:
- ✅ Zelenú HP animáciu (`EffectAnimations(amount, "HP", color_green)`)
- ✅ Heal sound effect
- ✅ HP bar update

---

## ✅ Unity Client - HOTFIX Applied

### Zmeny v `BattleResultProcessor.cs`:

1. **Parse healAmount zo server response:**
```csharp
int myHealAmount = myAttackData.ContainsKey("healAmount") ? int.Parse(myAttackData["healAmount"].ToString()) : 0;
int enemyHealAmount = enemyAttackData.ContainsKey("healAmount") ? int.Parse(enemyAttackData["healAmount"].ToString()) : 0;
```

2. **Prechod healAmount cez coroutine chain:**
```csharp
StartCoroutine(PlayBattleAnimationsAndRefresh(..., myHealAmount, enemyHealAmount));
  → PlayBattleAnimations(..., myHealAmount, enemyHealAmount)
    → ExecuteAttackAnimation(..., healAmount)
```

3. **Zavolanie `attacker.Heal(healAmount)` namiesto `SetHP()`:**
```csharp
if (attackId == 3 && healAmount > 0) {
    attacker.Heal(healAmount);  // ✅ Trigger zelená HP animácia + sound!
    yield return StartCoroutine(ShowDialog($"{attacker.cardName} healed {healAmount} HP!"));
}
```

4. **Warning ak server nevráti healAmount:**
```csharp
else {
    Debug.LogWarning($"[ExecuteAttackAnimation] ⚠️ healAmount=0! Server didn't return healAmount!");
}
```

### Status:
✅ **Unity client code compiled successfully** (No errors)  
⏳ **Waiting for server implementation** (see below)

---

## ⚠️ Server Implementation REQUIRED

### FILE: `c:\Zlozka\mega-tresk-server\api\executeBattle.js`

### 📋 Use Instructions:
Otvor **COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt** v server projekte a aplikuj zmeny pomocou Copilot.

**CRITICAL UPDATE in TASK 3:**
```javascript
attacks: {
  [card1.cardId]: {
    attackId: attackId1,
    damage: ...,
    healAmount: firstAttacker === 'player1'  // ⚠️ CRITICAL: Unity needs this!
      ? (firstResult.healAmount || 0) 
      : (secondResult.healAmount || 0),
    effectsRemoved: ...
  }
}
```

**WHY:**
- Unity volá: `attacker.Heal(healAmount)` → zelená HP animácia
- BEZ `healAmount`: Animácia sa nespustí, iba HP bar sa updatne bez efektu
- Server už má `executeHeal()` funkciu, ale MUSÍ vrátiť `healAmount` v response!

### Verification:
```bash
# Test locally
cd c:\Zlozka\mega-tresk-server
vercel dev

# Curl test
curl -X POST http://localhost:3000/api/executeBattle \
  -H "Content-Type: application/json" \
  -d '{
    "roomCode": "TEST",
    "playerId": "p1",
    "attackData": {"cardId": "card1", "attackId": 3}
  }'

# Expected response:
{
  "success": true,
  "battleResult": {
    "attacks": {
      "card1_uuid": {
        "attackId": 3,
        "damage": 0,
        "healAmount": 5,  // ✅ MUST BE PRESENT!
        "effectsRemoved": [1, 4]
      }
    }
  }
}
```

---

## 🔍 Related Changes

### Updated Documentation:

1. **COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt** (TASK 3):
   - Added ⚠️ CRITICAL warnings about `healAmount`
   - Added Unity dependency explanation
   - Added "WITHOUT healAmount" warning message

2. **SERVER_COPILOT_ATTACK_GUIDE.md**:
   - Added Attack ID 3 to "Implemented Attacks" section
   - Added **new mistake**: "Missing healAmount for self-heal attacks"
   - Documented response format differences (damage vs healAmount)

---

## 🎯 Attack Types Framework

Unity teraz podporuje **3 typy útokov**:

### 1️⃣ Damage Attacks (Punch, Kick, atď.):
```csharp
defender.TakeDamage(damage);  // Red HP drop animation
playerLifeBar.SetHP(defender.health);
```

### 2️⃣ Self-Heal Attacks (Heal, atď.):
```csharp
attacker.Heal(healAmount);  // ✅ Green HP rise animation
// HP bar sa updatne automaticky v Heal() metóde
```

### 3️⃣ Buff/Debuff Attacks (budúcnosť):
```csharp
// Example: Attack ID 4 (Forgiveness) môže meniť ATT/DEF/SPD
attacker.HandleStrength(+5);  // Green "STR +5" animation
defender.HandleDefense(-3);   // Red "DEF -3" animation
```

**Expandable Pattern:**
```csharp
switch (attackId) {
    case 1: PlayPunchAnimation(); ApplyDamage(); break;
    case 2: PlayKickAnimation(); ApplyDamage(); break;
    case 3: PlayHealAnimation(); ApplyHeal(); break;
    case 4: PlayForgivenessAnimation(); ApplyBuffs(); break;
    // ... 119 more cases
}
```

---

## 📊 Before vs After

### ❌ BEFORE (Broken):
```
Server Response:
{
  "attackId": 3,
  "damage": 0
}

Unity:
- ✅ Heal animation plays
- ❌ No green HP animation
- ❌ No HP increase shown
- ❌ HP bar doesn't update
```

### ✅ AFTER (Fixed):
```
Server Response:
{
  "attackId": 3,
  "damage": 0,
  "healAmount": 5  // ✅ NEW!
}

Unity:
- ✅ Heal animation plays
- ✅ attacker.Heal(5) called
- ✅ Green "+5 HP" animation floats up
- ✅ Heal sound effect plays
- ✅ HP bar updates from 20 → 25
```

---

## 🚀 Next Steps

1. **Implement Server Changes:**
   - Otvor `COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt`
   - Apply TASK 3 update (add `healAmount` to response)
   - Test locally with `vercel dev`

2. **Deploy:**
   ```bash
   cd c:\Zlozka\mega-tresk-server
   git add api/executeBattle.js
   git commit -m "feat: Add healAmount to Heal attack response (Unity animation fix)"
   git push
   ```

3. **Test in Unity:**
   - Play multiplayer match
   - Use Attack ID 3 (Heal)
   - Verify green HP animation + sound
   - Check Unity console: `[ExecuteAttackAnimation] {name} heals for {amount} HP!`

4. **Attack ID 4:**
   - Po teste Attack ID 3, môžeme pokračovať na Attack ID 4 (Forgiveness)
   - Forgiveness: `-1 damage to opponent` + `75% Asceticism effect` (debuff)
   - Potrebuje support pre **debuffs** (nový attack type!)

---

**Status:** Unity ✅ Ready | Server ⏳ Pending | Testing ⏸️ Waiting  
**Version:** V9 (Dynamic Attack System + Heal Support)  
**Date:** 2025-11-06
