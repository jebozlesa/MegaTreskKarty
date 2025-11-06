# ✅ Attack ID 2 (Kick) - Implementation Checklist

## Pre Server Developer (ty):

### 📁 Súbory na prenos do server projektu:
1. ✅ `SERVER_COPILOT_INSTRUCTIONS_ATTACK_ID_2.md` - Detailné inštrukcie
2. ✅ `COPILOT_CHAT_INSTRUCTIONS.txt` - Copy-paste do Copilot Chat

### 🎯 Použitie:

**Varianta A: Copilot Chat** (rýchlejšie)
```bash
cd c:\Zlozka\mega-tresk-server
# Otvor VS Code
# Stlač Ctrl+Shift+I (Copilot Chat)
# Skopíruj COPILOT_CHAT_INSTRUCTIONS.txt do chatu
# Copilot automaticky upraví executeBattle.js
```

**Varianta B: Copilot Instructions File**
```bash
cd c:\Zlozka\mega-tresk-server
# Skopíruj SERVER_COPILOT_INSTRUCTIONS_ATTACK_ID_2.md
# do .github/copilot-instructions.md
# Potom v Copilot Chat napíš: "Implement Attack ID 2 per instructions"
```

**Varianta C: Manuálne**
```bash
# Použi SERVER_COPILOT_INSTRUCTIONS_ATTACK_ID_2.md ako guide
# Edituj api/executeBattle.js
```

---

## 🔍 Po implementácii skontroluj:

### Kód Checklist:
- [ ] `executeKick()` funkcia existuje (hneď po `executePunch()`)
- [ ] Damage formula: `Math.floor(speed/3) - Math.floor(defense/3)`
- [ ] Extra damage: fixed 4 (nie random!)
- [ ] `simulateBattle()` má `else if (firstAttackId === 2)`
- [ ] `simulateBattle()` má `else if (secondAttackId === 2)`
- [ ] Response obsahuje `attackId: attackId1` pre card1
- [ ] Response obsahuje `attackId: attackId2` pre card2

### Local Test:
```bash
cd c:\Zlozka\mega-tresk-server
vercel dev

# V druhom terminali:
curl -X POST http://localhost:3000/api/executeBattle ^
  -H "Content-Type: application/json" ^
  -d "{\"roomCode\":\"TEST\",\"playerId\":\"p1\",\"attackData\":{\"cardId\":\"c1\",\"attackId\":2}}"
```

**Expected Output:**
```
[executeKick] Card1 -> Card2: 2 base damage
[simulateBattle] Result: { ... attackId: 2 ... }
```

### Deploy:
- [ ] `git add api/executeBattle.js`
- [ ] `git commit -m "feat(v9): Add Attack ID 2 (Kick) + attackId in response"`
- [ ] `git push`
- [ ] Vercel auto-deploy complete (~2 min)

---

## 🎮 Unity Testing (po deploy):

### Test Steps:
1. Spusti Unity Editor
2. Play multiplayer scene
3. Join/create room
4. Select card s Attack 2 (Kick)
5. Vyber Attack 2
6. Pozoruj animácie

### Expected Behavior:
- ✅ `PlayKickAnimation()` sa prehrá (NIE Punch!)
- ✅ Dialog: "[CardName] uses Kick!"
- ✅ Damage aplikovaný správne
- ✅ Unity Console: `[BattleResultProcessor] MyAttackId=2`

### Unity Console Logs:
```
[BattleResultProcessor] MyAttackId=2, MyDamage=5, EnemyAttackId=1, EnemyDamage=2
[PlayBattleAnimations] MyAttackId=2, EnemyAttackId=1
[ExecuteAttackAnimation] Henry Ford uses Kick!
Hit! 5 damage!
```

---

## 🐛 Troubleshooting:

### Problem: Unity zobrazuje "Unknown attackId=2"
**Príčina:** Server nevracia `attackId` v response  
**Fix:** Skontroluj Task 3 - `attackId: attackId2` musí byť v oboch attacks objektoch

### Problem: Kick používa Punch animáciu
**Príčina:** Rovnaký ako vyššie - chýba `attackId`  
**Fix:** Rovnaký

### Problem: Nesprávny damage
**Príčina:** Damage formula má `floor()` len raz  
**Fix:** Musí byť: `Math.floor(speed/3) - Math.floor(defense/3)` (floor DVAKRÁT!)

### Problem: Extra damage je random
**Príčina:** `Math.random() * X` namiesto fixed 4  
**Fix:** Extra damage = 4 (fixed value)

---

## 🚀 Po úspešnom teste:

**Gratulujeme!** 🎉 Attack ID 2 funguje!

**Next:**
- Attack ID 3 (Heal) - použiť rovnaký pattern
- Dokumentácia: `SERVER_COPILOT_INSTRUCTIONS_ATTACK_ID_2.md` je template

**Framework je hotový, ďalšie útoky budú ešte jednoduchšie!**

---

## 📞 Need Help?

- Detailná špecifikácia: `SERVER_COPILOT_INSTRUCTIONS_ATTACK_ID_2.md`
- Quick reference: `COPILOT_CHAT_INSTRUCTIONS.txt`
- Unity client code: `Assets/Scripts/Multiplayer/BattleResultProcessor.cs`
- Unity attack logic: `Assets/Scripts/Attack.cs` (line ~507)

---

**Status:** ⏳ Waiting for server implementation  
**Unity Client:** ✅ Ready (framework complete)  
**Estimated Time:** 10-15 min  
**Priority:** HIGH
