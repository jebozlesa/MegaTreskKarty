# ✅ Attack ID 3 (Heal) - Implementation Checklist

## Pre Server Developer (ty):

### 📁 Súbory na prenos do server projektu:
1. ✅ `COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt` - Copy-paste do Copilot Chat

### 🎯 Použitie:

**Odporúčaná Varianta: Copilot Chat**
```bash
cd c:\Zlozka\mega-tresk-server
# Otvor VS Code
# Stlač Ctrl+Shift+I (Copilot Chat)
# Skopíruj COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt do chatu
# Copilot automaticky upraví executeBattle.js
```

---

## ⚠️ HEAL JE ŠPECIÁLNY - SELF-HEAL ÚTOK!

### Rozdiel oproti Punch/Kick:
- ❌ **Punch/Kick:** Damage na opponent
- ✅ **Heal:** Heal na self (attacker), 0 damage na opponent

### Mechanika:
- **Heal Amount:** Random(1,2) + floor(knowledge / 4)
- **Target:** Attacker (NIE defender!)
- **Clamp:** Nemôže ísť nad maxHealth
- **Effects Removed:** [1=bleed, 4=exposure, 13=?, 24=?]

---

## 🔍 Po implementácii skontroluj:

### Kód Checklist:
- [ ] `executeHeal()` funkcia existuje (hneď po `executeKick()`)
- [ ] Heal formula: `Random(1,2) + Math.floor(knowledge/4)`
- [ ] Heals **ATTACKER** (nie defender!)
- [ ] Clamps to `maxHealth`
- [ ] Removes effects: `[1, 4, 13, 24]`
- [ ] Returns `{ healAmount, effectsRemoved, damage: 0 }`
- [ ] `simulateBattle()` má `else if (firstAttackId === 3)`
- [ ] `simulateBattle()` má `else if (secondAttackId === 3)`
- [ ] Response obsahuje `healAmount` field
- [ ] Response obsahuje `effectsRemoved` field
- [ ] Response obsahuje `attackId: 3`

### Local Test:
```bash
cd c:\Zlozka\mega-tresk-server
vercel dev

# V druhom terminali:
curl -X POST http://localhost:3000/api/executeBattle ^
  -H "Content-Type: application/json" ^
  -d "{\"roomCode\":\"TEST\",\"playerId\":\"p1\",\"attackData\":{\"cardId\":\"c1\",\"attackId\":3}}"
```

**Expected Output:**
```
[executeHeal] Card1 heals: 4 HP
[executeHeal] Removed effects: 1, 4
[simulateBattle] Result: { ... attackId: 3, healAmount: 4 ... }
```

### Deploy:
- [ ] `git add api/executeBattle.js`
- [ ] `git commit -m "feat(v9): Add Attack ID 3 (Heal) - self-heal + effect removal"`
- [ ] `git push`
- [ ] Vercel auto-deploy complete (~2 min)

---

## 🎮 Unity Testing (po deploy):

### Test Steps:
1. Spusti Unity Editor
2. Play multiplayer scene
3. Join/create room
4. Select card s Attack 3 (Heal)
5. Vyber Attack 3
6. Pozoruj animácie

### Expected Behavior:
- ✅ `PlayHealAnimation()` sa prehrá (self-heal animation!)
- ✅ Dialog: "[CardName] uses Heal!"
- ✅ Dialog: "[CardName] healed!"
- ✅ **Vlastné HP sa zvýši** (NIE opponent HP!)
- ✅ HP bar updatnutý správne
- ✅ Unity Console: `[BattleResultProcessor] MyAttackId=3`

### Unity Console Logs:
```
[BattleResultProcessor] MyAttackId=3, MyDamage=0, EnemyAttackId=1, EnemyDamage=2
[PlayBattleAnimations] MyAttackId=3, EnemyAttackId=1
[ExecuteAttackAnimation] Henry Ford uses Heal!
Henry Ford healed!
```

---

## 🧪 Test Scenarios:

### Scenario 1: Basic Heal
```
Card: knowledge=8, health=15/30
Heal: 1-2 + floor(8/4) = 1-2 + 2 = 3-4 HP
Final health: 18-19 HP
```

### Scenario 2: Overheal Protection
```
Card: knowledge=12, health=28/30
Heal: 1-2 + floor(12/4) = 1-2 + 3 = 4-5 HP
Expected: Clamped to 30 HP (maxHealth)
```

### Scenario 3: Effect Removal
```
Card: effects = [{ type: 1 }, { type: 5 }, { type: 4 }]
Expected: Remove 1 and 4, keep 5
Result: effects = [{ type: 5 }]
```

### Expected Server Response:
```json
{
  "battleResult": {
    "attacks": {
      "card1_id": {
        "attackId": 3,
        "damage": 0,
        "healAmount": 4,
        "effectsRemoved": [1, 4],
        "effects": []
      },
      "card2_id": {
        "attackId": 1,
        "damage": 2,
        "healAmount": 0,
        "effects": []
      }
    }
  }
}
```

---

## 🐛 Troubleshooting:

### Problem: Unity updatuje nepriateľovo HP namiesto vlastného
**Príčina:** ExecuteAttackAnimation nesprávne určilo HP bar  
**Fix:** Skontroluj Unity kód - Heal updatuje `isMyAttack ? playerLifeBar : enemyLifeBar`  
**Status:** ✅ Už opravené v Unity klient kóde

### Problem: Heal neodstráni effects
**Príčina:** Effect structure je iná (effect.id vs effect.type)  
**Fix:** Skontroluj existujúci executePunch() ako ukladá effects, použiť rovnakú štruktúru

### Problem: Heal amount je vždy rovnaký
**Príčina:** Chýba random component  
**Fix:** Musí byť `Math.floor(Math.random() * 2) + 1` pre 1-2 random

### Problem: Overheal (zdravie nad maxHealth)
**Príčina:** Chýba clamp check  
**Fix:** `if (health > maxHealth) health = maxHealth`

---

## 📊 Heal vs Damage Attack Comparison:

### Punch/Kick (Damage):
```javascript
- Target: defender
- Action: defender.health -= damage
- Return: { damage, ... }
- HP Bar: Update enemy HP
```

### Heal (Self-Heal):
```javascript
- Target: attacker (self!)
- Action: attacker.health += healAmount
- Return: { healAmount, effectsRemoved, damage: 0 }
- HP Bar: Update own HP
```

---

## 🚀 Po úspešnom teste:

**Gratulujeme!** 🎉 Attack ID 3 funguje!

**Unity Client:** ✅ Ready (Heal support complete)
- ✅ Heal animation support
- ✅ Self-heal HP bar update
- ✅ Special handling pre attackId === 3

**Next:**
- Attack ID 4 (Forgiveness) - support attack
- Použiť rovnaký pattern

**Framework je stále silnejší!** 💪

---

## 📞 Need Help?

- Detailná špecifikácia: `COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt`
- Unity client code: `Assets/Scripts/Multiplayer/BattleResultProcessor.cs` (line ~276)
- Unity attack logic: `Assets/Scripts/Attack.cs` (line ~518)
- Effect IDs:
  - 1 = Bleed
  - 3 = Sleep
  - 4 = Exposure/Radiation
  - 13, 24 = Unknown (remove anyway)

---

**Status:** ⏳ Waiting for server implementation  
**Unity Client:** ✅ Ready (V9 - Heal support)  
**Estimated Time:** 15-20 min  
**Priority:** HIGH  
**Difficulty:** Medium (self-heal mechanic)
