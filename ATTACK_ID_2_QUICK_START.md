# 🎮 Attack ID 2 (Kick) - Quick Reference

## ✅ Unity Client - HOTOVO ✅

### Zmeny v `BattleResultProcessor.cs`:
1. ✅ Parse `attackId` from server response
2. ✅ `ExecuteAttackAnimation()` metóda - dynamické animácie
3. ✅ `GetAttackName()` helper
4. ✅ Refaktorovaný `PlayBattleAnimations()` - eliminovaná duplicita
5. ✅ Switch statement pre Attack ID 1, 2

**Rozširenie na Attack ID 3+:**
Len pridaj case do switch:
```csharp
case 3: // Heal
    yield return StartCoroutine(animations.PlayHealAnimation(...));
    break;
```

---

## ⏳ Server - ČAKÁ NA IMPLEMENTÁCIU

### Súbory pre teba:
1. **`SERVER_INSTRUCTIONS_ATTACK_ID_2_KICK.md`** - Detailné inštrukcie
2. **`SERVER_COPILOT_ATTACK_GUIDE.md`** - Template pre Copilot

### Kroky:
```bash
# 1. Otvor server projekt
cd c:\Zlozka\mega-tresk-server

# 2. Skopíruj SERVER_COPILOT_ATTACK_GUIDE.md do .github/copilot-instructions.md
#    (alebo merge s existujúcimi inštrukciami)

# 3. Implementuj zmeny v api/executeBattle.js:
#    - Pridaj executeKick() funkciu
#    - Pridaj case 2 do simulateBattle()
#    - Pridaj attackId do response

# 4. Test lokálne
vercel dev

# 5. Deploy
git add .
git commit -m "feat: Add Attack ID 2 (Kick) support"
git push

# 6. Test v Unity
```

---

## 🧪 Testing

### Po server implementácii:

1. **Unity Console** - kontroluj logy:
```
[BattleResultProcessor] MyAttackId=2, MyDamage=5, EnemyAttackId=1, EnemyDamage=2
[PlayBattleAnimations] MyAttackId=2, EnemyAttackId=1
[ExecuteAttackAnimation] Henry Ford uses Kick!
```

2. **Animácie:**
- ✅ `PlayKickAnimation()` sa volá (nie Punch!)
- ✅ Správny damage aplikovaný
- ✅ Dialog text: "Henry Ford uses Kick!"

3. **Damage Check:**
- Base: `(speed/3) - (defense/3)`, min 1
- 20% šanca: +4 extra damage

---

## 📋 Checklist

### Unity (DONE):
- [x] AttackId parsing
- [x] ExecuteAttackAnimation() method
- [x] GetAttackName() helper
- [x] Switch case 1, 2
- [x] Refactored PlayBattleAnimations()

### Server (TODO):
- [ ] executeKick() function
- [ ] simulateBattle() case 2
- [ ] attackId in response
- [ ] Local test
- [ ] Deploy
- [ ] E2E test

---

## 🚀 Next Attack: ID 3 (Heal)

**Unity:** Už pripravené! Len pridaj `case 3` do switch.

**Server:** Použiť ten istý pattern ako Kick:
1. Create `executeHeal()` funkcia
2. Add `case 3` to switch
3. Include `attackId: 3` in response

**Heal špecifiká:**
- Self-heal: `Random(1,2) + (knowledge/4)`
- Remove effects: burn, bleed, poison, sleep

---

**Quick Start:** Začni s `SERVER_INSTRUCTIONS_ATTACK_ID_2_KICK.md` 🎯
