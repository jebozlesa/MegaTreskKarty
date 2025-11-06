# 🎯 Server Task: Implement Attack ID 2 (Kick) Support

## 📋 Context

Unity multiplayer client má nový dynamický attack system ktorý vyžaduje aby server vracel `attackId` v battle response. Teraz funguje len Attack ID 1 (Punch), potrebujeme pridať Attack ID 2 (Kick).

**Súbor na úpravu:** `api/executeBattle.js`

**Reference logika:** Pozri Unity singleplayer `c:\Zlozka\MegaTreskKarty\Assets\Scripts\Attack.cs`, metóda `Kick()` (riadok ~507-517)

---

## ✅ Task 1: Vytvor `executeKick()` funkciu

### Špecifikácia:

**Umiestnenie:** Pridaj HNEĎ PO existujúcej funkcii `executePunch()` (približne riadok 123)

**Function Signature:**
```javascript
function executeKick(attackerCard, defenderCard)
```

**Input:**
- `attackerCard` - object s properties: `{ name, speed, defense, health, effects }`
- `defenderCard` - object s properties: `{ name, speed, defense, health, effects }`

**Behavior:**
1. **Vypočítaj base damage:**
   - Formula: `Math.floor(attackerCard.speed / 3) - Math.floor(defenderCard.defense / 3)`
   - Minimum: 1 (ak výsledok < 1, nastav na 1)

2. **Aplikuj base damage:**
   - `defenderCard.health -= damage`
   - Ak `defenderCard.health < 0`, nastav na 0

3. **20% šanca na extra damage:**
   - Použiť `Math.random() <= 0.2`
   - Podmienka: `defenderCard.health > 0` (len ak ešte žije)
   - Extra damage: 4
   - Aplikuj: `defenderCard.health -= 4` (znova check pre < 0)

4. **Logging:**
   - Log base damage: `console.log(\`[executeKick] ${attackerCard.name} -> ${defenderCard.name}: ${baseDamage} base damage\`)`
   - Ak proc extra damage: `console.log(\`[executeKick] Extra damage! Total: ${totalDamage}\`)`

**Output (return object):**
```javascript
{
  damage: number,        // total damage (base + extra)
  didExtraDamage: bool,  // či sa proc extra damage
  extraDamage: number    // koľko extra (0 alebo 4)
}
```

**KRITICKÉ:**
- Damage formula MUSÍ byť identická ako v Unity `Attack.cs` Kick() metóde
- Extra damage je **PRESNE 4** (nie random!)
- 20% šansa = `Math.random() <= 0.2` (NIE `< 0.2`)

---

## ✅ Task 2: Pridaj Attack ID 2 do `simulateBattle()` funkcie

### Špecifikácia:

**Umiestnenie:** Funkcia `simulateBattle()`, približne riadok 193-217

**Nájdi existujúci kód:**
```javascript
// Prvý útok
let firstResult = { damage: 0, didSleep: false, sleepDuration: 0 };
if (firstAttackId === 1) {
  firstResult = executePunch(firstCard, secondCard);
}
// TODO: Pridaj switch pre ostatné attackId (2, 3, 4, ...)
```

**Úprava 1 - Prvý útok:**
ZMEŇ na:
```javascript
// Prvý útok
let firstResult = { damage: 0, didSleep: false, sleepDuration: 0 };
if (firstAttackId === 1) {
  firstResult = executePunch(firstCard, secondCard);
} else if (firstAttackId === 2) {
  firstResult = executeKick(firstCard, secondCard);
}
// TODO: Pridaj else if pre ostatné attackId (3, 4, 5, ...)
```

**Nájdi druhý útok:**
```javascript
// Druhý útok (len ak prežil)
let secondResult = { damage: 0, didSleep: false, sleepDuration: 0 };
if (secondCard.health > 0) {
  if (secondAttackId === 1) {
    secondResult = executePunch(secondCard, firstCard);
  }
  // TODO: Switch pre ostatné attackId
}
```

**Úprava 2 - Druhý útok:**
ZMEŇ na:
```javascript
// Druhý útok (len ak prežil)
let secondResult = { damage: 0, didSleep: false, sleepDuration: 0 };
if (secondCard.health > 0) {
  if (secondAttackId === 1) {
    secondResult = executePunch(secondCard, firstCard);
  } else if (secondAttackId === 2) {
    secondResult = executeKick(secondCard, firstCard);
  }
  // TODO: Pridaj else if pre ostatné attackId
}
```

**DÔLEŽITÉ:**
- Použiť `else if` pattern (NIE switch statement)
- Zachovať existujúce TODO komentáre (len upraviť text)

---

## ✅ Task 3: **KRITICKÉ** - Pridaj `attackId` do response

### Špecifikácia:

**Umiestnenie:** Funkcia `simulateBattle()`, koniec funkcie pred `return result`

**Nájdi existujúci kód:**
```javascript
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
```

**Úprava:**
PRIDAJ property `attackId` do OBOCH `attacks` objektov (card1 a card2):

```javascript
const result = {
  firstAttacker: firstCard.cardId,
  
  attacks: {
    [card1.cardId]: {
      attackId: attackId1,  // ← PRIDAJ TENTO RIADOK HNEĎ NA ZAČIATOK
      damage: firstAttacker === 'player1' ? firstResult.damage : secondResult.damage,
      didSleep: firstAttacker === 'player1' ? secondResult.didSleep : firstResult.didSleep,
      sleepDuration: firstAttacker === 'player1' ? secondResult.sleepDuration : firstResult.sleepDuration,
      effects: card1.effects || []
    },
    [card2.cardId]: {
      attackId: attackId2,  // ← PRIDAJ TENTO RIADOK HNEĎ NA ZAČIATOK
      damage: firstAttacker === 'player2' ? firstResult.damage : secondResult.damage,
      didSleep: firstAttacker === 'player2' ? secondResult.didSleep : firstResult.didSleep,
      sleepDuration: firstAttacker === 'player2' ? secondResult.sleepDuration : firstResult.sleepDuration,
      effects: card2.effects || []
    }
  }
};
```

**Pridaj comment:**
Pred `const result =` pridaj komentár:
```javascript
// ✅ V9: Pridané attackId pre Unity animácie
```

**PREČO JE TOTO KRITICKÉ:**
- BEZ `attackId` Unity klient nevie ktorú animáciu prehrať
- Klient má dynamický switch statement: `case 1: PlayPunch, case 2: PlayKick, ...`
- Ak chýba `attackId`, všetky útoky budú používať default Punch animáciu

---

## 🧪 Verifikácia

### Po implementácii skontroluj:

1. **executeKick() funkcia existuje:**
   - [ ] Nachádza sa hneď po executePunch()
   - [ ] Má správnu damage formulu: `Math.floor(speed/3) - Math.floor(defense/3)`
   - [ ] Má 20% extra damage (+4)
   - [ ] Return object obsahuje: `{ damage, didExtraDamage, extraDamage }`

2. **simulateBattle() má case 2:**
   - [ ] `else if (firstAttackId === 2)` pre prvý útok
   - [ ] `else if (secondAttackId === 2)` pre druhý útok
   - [ ] Obe volajú `executeKick()`

3. **Response obsahuje attackId:**
   - [ ] `attacks[card1.cardId].attackId = attackId1`
   - [ ] `attacks[card2.cardId].attackId = attackId2`
   - [ ] Komentár V9 pridaný

### Test Data:

**Scenario 1: Base Kick damage**
```
Card1: speed=12, defense=3
Card2: speed=9, defense=6

Card1 Kick → Card2:
  baseDamage = floor(12/3) - floor(6/3) = 4 - 2 = 2
  Expected: damage = 2 (alebo 6 ak proc)

Card2 Kick → Card1:
  baseDamage = floor(9/3) - floor(3/3) = 3 - 1 = 2
  Expected: damage = 2 (alebo 6 ak proc)
```

**Scenario 2: Response format**
```json
{
  "success": true,
  "battleResult": {
    "firstAttacker": "card1_uuid",
    "attacks": {
      "card1_uuid": {
        "attackId": 2,           // ← MUST BE PRESENT!
        "damage": 5,
        "didSleep": false,
        "effects": []
      },
      "card2_uuid": {
        "attackId": 1,           // ← MUST BE PRESENT!
        "damage": 2,
        "didSleep": false,
        "effects": []
      }
    }
  }
}
```

---

## ⚠️ Common Mistakes - AVOID!

### ❌ WRONG: Iná damage formula
```javascript
// NEPOUŽÍVAJ floor len raz!
let damage = Math.floor((attackerCard.speed / 3) - (defenderCard.defense / 3));
// ❌ ZLÉ - Unity používa floor DVAKRÁT!
```

### ✅ CORRECT:
```javascript
let damage = Math.floor(attackerCard.speed / 3) - Math.floor(defenderCard.defense / 3);
// ✅ SPRÁVNE - floor na každú hodnotu zvlášť
```

### ❌ WRONG: Zabudnutý attackId
```javascript
attacks: {
  [card1.cardId]: {
    damage: 5,  // ❌ Chýba attackId!
  }
}
```

### ❌ WRONG: Random extra damage
```javascript
extraDamage = Math.floor(Math.random() * 5) + 1;  // ❌ ZLÉ!
// Unity má PRESNE +4, nie random!
```

### ✅ CORRECT:
```javascript
extraDamage = 4;  // ✅ SPRÁVNE - fixed value
```

---

## 📚 Reference Files

### Unity Singleplayer Implementation:
**Súbor:** `c:\Zlozka\MegaTreskKarty\Assets\Scripts\Attack.cs`
**Metóda:** `Kick()` (riadok ~507-517)
**Kód:**
```csharp
public IEnumerator Kick(Kard attacker, Kard receiver, TMP_Text dialogText)
{
    receiver.TakeDamage((attacker.speed / 3) - (receiver.defense / 3));
    if (UnityEngine.Random.value <= 0.2f) receiver.TakeDamage(4);
}
```

### Unity Multiplayer Client:
**Súbor:** `c:\Zlozka\MegaTreskKarty\Assets\Scripts\Multiplayer\BattleResultProcessor.cs`
**Metóda:** `ExecuteAttackAnimation()` (riadok ~248)
**Kód:**
```csharp
switch (attackId)
{
    case 1: // Punch
        yield return StartCoroutine(animations.PlayPunchAnimation(...));
        break;
    case 2: // Kick
        yield return StartCoroutine(animations.PlayKickAnimation(...));
        break;
}
```

**DÔLEŽITÉ:** Unity OČAKÁVA že server vráti `attackId: 2` aby vedel zavolať `PlayKickAnimation()`!

---

## 🎯 Success Criteria

Po implementácii:
1. ✅ `executeKick()` funkcia existuje s presnou formulou ako Unity
2. ✅ `simulateBattle()` volá `executeKick()` keď `attackId === 2`
3. ✅ Response obsahuje `attackId` pre obe karty
4. ✅ Damage výpočet je identický s Unity (floor dvakrát!)
5. ✅ Extra damage je fixed 4 (nie random)

---

## 📞 Testing Instructions

### Local Test (pre developera):
```bash
cd c:\Zlozka\mega-tresk-server
vercel dev

# Test Kick attack
curl -X POST http://localhost:3000/api/executeBattle \
  -H "Content-Type: application/json" \
  -d '{
    "roomCode": "TEST123",
    "playerId": "player1",
    "attackData": {
      "cardId": "test-card-id",
      "attackId": 2
    }
  }'
```

**Expected Console Output:**
```
[executeBattle] Request: { roomCode: 'TEST123', ... }
[executeKick] TestCard1 -> TestCard2: 2 base damage
[executeKick] Extra damage! Total: 6  // (ak proc)
[simulateBattle] Result: { ... attackId: 2 ... }
```

### Deploy:
```bash
git add api/executeBattle.js
git commit -m "feat(v9): Add Attack ID 2 (Kick) support + attackId in response"
git push
# Vercel auto-deploy
```

### End-to-End Test (s Unity):
1. Spusti Unity multiplayer battle
2. Vyber Attack 2 (Kick) na jednej karte
3. Skontroluj Unity Console:
   - `[BattleResultProcessor] MyAttackId=2`
   - `[ExecuteAttackAnimation] uses Kick!`
   - `PlayKickAnimation()` sa volá (NIE Punch!)

---

## 🔄 Next Steps

Po úspešnej implementácii Attack ID 2:
- Attack ID 3 (Heal) - ten istý pattern
- Attack ID 4 (Forgiveness) - rovnaký workflow
- ...až Attack ID 123

**Template:** Tento dokument slúži ako template pre všetky budúce útoky.

---

**Version:** V9  
**Task:** Implement Attack ID 2 (Kick)  
**Priority:** HIGH (Unity client čaká na túto funkciu)  
**Estimated Time:** 10-15 min  
**Difficulty:** Easy (copy pattern from Punch)
