# 🎯 Attack System Implementation Guide

## CRITICAL: Attack ID must be returned in battleResult!

When implementing new attacks in `executeBattle.js`, **ALWAYS** include `attackId` in the response:

```javascript
const result = {
  firstAttacker: firstCard.cardId,
  attacks: {
    [card1.cardId]: {
      attackId: attackId1,  // ✅ REQUIRED! Unity needs this for animations
      damage: ...,
      didSleep: ...,
      effects: ...
    },
    [card2.cardId]: {
      attackId: attackId2,  // ✅ REQUIRED!
      damage: ...,
      effects: ...
    }
  }
};
```

---

## 📋 Attack Implementation Template

### Step 1: Create Execute Function

```javascript
/**
 * Simuluje [Attack Name] útok (Attack ID X)
 * Damage: [vzorec]
 * Effect: [efekty]
 */
function execute[AttackName](attackerCard, defenderCard) {
  // Damage calculation (MUST match Unity Attack.cs!)
  let damage = Math.floor(attackerCard.stat / 3) - Math.floor(defenderCard.defense / 3);
  if (damage < 1) damage = 1;
  
  console.log(`[execute[AttackName]] ${attackerCard.name} -> ${defenderCard.name}: ${damage} damage`);
  
  defenderCard.health -= damage;
  if (defenderCard.health < 0) defenderCard.health = 0;
  
  // Effects (sleep, burn, etc.)
  let didEffect = false;
  if (Math.random() <= 0.XX && defenderCard.health > 0) {
    didEffect = true;
    // Apply effect...
  }
  
  return {
    damage: damage,
    didEffect: didEffect,
    // ... other effect data
  };
}
```

### Step 2: Add to simulateBattle() Switch

```javascript
function simulateBattle(card1, card2, attackId1, attackId2) {
  // ...
  
  // First attack
  let firstResult = { damage: 0 };
  if (firstAttackId === 1) {
    firstResult = executePunch(firstCard, secondCard);
  } else if (firstAttackId === 2) {
    firstResult = executeKick(firstCard, secondCard);
  } 
  // ✅ ADD YOUR ATTACK HERE:
  else if (firstAttackId === X) {
    firstResult = execute[AttackName](firstCard, secondCard);
  }
  
  // Second attack (same pattern)
  // ...
}
```

### Step 3: Include attackId in Response

```javascript
attacks: {
  [card1.cardId]: {
    attackId: attackId1,  // ✅ ALWAYS INCLUDE!
    damage: ...,
    // ... effects
  }
}
```

---

## 🎮 Implemented Attacks

### ✅ Attack ID 1: Punch
- **Type:** Damage attack
- **Damage:** `(strength/3) - (defense/3)`, min 1
- **Effect:** 20% sleep (1-2 turns)
- **Function:** `executePunch()`
- **Response:** `{ damage, didSleep, sleepDuration }`

### ✅ Attack ID 2: Kick
- **Type:** Damage attack
- **Damage:** `(speed/3) - (defense/3)`, min 1
- **Effect:** 20% +4 extra damage
- **Function:** `executeKick()`
- **Response:** `{ damage, didExtraDamage }`

### ✅ Attack ID 3: Heal
- **Type:** Self-heal attack ⚠️ SPECIAL!
- **Heal:** `Random(1,2) + floor(knowledge/4)`
- **Effect:** Removes effects [1, 4, 13, 24]
- **Function:** `executeHeal()`
- **Response:** `{ healAmount, effectsRemoved }` ⚠️ NOT damage!

### ⏳ Pending Attacks

Reference: `c:\Zlozka\MegaTreskKarty\Assets\Scripts\Attack.cs`

- Attack ID 4: Forgiveness
- Attack ID 5: Crusade
- ... (120 remaining attacks)

---

## 🔍 Finding Attack Logic in Unity

```bash
# Search for attack implementation:
grep -n "case X:" c:\Zlozka\MegaTreskKarty\Assets\Scripts\Attack.cs

# Find attack method:
grep -n "public IEnumerator [AttackName]" c:\Zlozka\MegaTreskKarty\Assets\Scripts\Attack.cs
```

---

## ⚠️ Common Mistakes

### ❌ WRONG: Missing attackId
```javascript
attacks: {
  [card1.cardId]: {
    damage: 5,  // ❌ Unity can't determine which animation to play!
  }
}
```

### ✅ CORRECT: Include attackId
```javascript
attacks: {
  [card1.cardId]: {
    attackId: 2,  // ✅ Unity knows to play Kick animation
    damage: 5,
  }
}
```

### ❌ WRONG: Missing healAmount for self-heal attacks
```javascript
// Attack ID 3 (Heal) response:
attacks: {
  [card1.cardId]: {
    attackId: 3,
    damage: 0,  // ❌ Unity NEEDS healAmount for green HP animation!
  }
}
```

### ✅ CORRECT: Include healAmount
```javascript
// Attack ID 3 (Heal) response:
attacks: {
  [card1.cardId]: {
    attackId: 3,
    damage: 0,
    healAmount: 5,  // ✅ Unity calls attacker.Heal(5) → green HP animation!
    effectsRemoved: [1, 4]
  }
}
```

### ❌ WRONG: Different damage formula than Unity
```javascript
// Unity: (speed/3) - (defense/3)
// Server: speed - defense  // ❌ INCONSISTENT!
```

### ✅ CORRECT: Match Unity formula exactly
```javascript
// Unity: (speed/3) - (defense/3)
let damage = Math.floor(attackerCard.speed / 3) - Math.floor(defenderCard.defense / 3);
// ✅ SAME AS UNITY!
```

---

## 🧪 Testing New Attacks

### Local Test:
```bash
cd c:\Zlozka\mega-tresk-server
vercel dev

curl -X POST http://localhost:3000/api/executeBattle \
  -H "Content-Type: application/json" \
  -d '{
    "roomCode": "TEST",
    "playerId": "p1",
    "attackData": {
      "cardId": "card1",
      "attackId": X
    }
  }'
```

### Expected Response:
```json
{
  "success": true,
  "battleResult": {
    "firstAttacker": "card1_uuid",
    "attacks": {
      "card1_uuid": {
        "attackId": X,  // ✅ Must match request
        "damage": Y
      }
    }
  }
}
```

---

## 📚 Resources

- Unity Attack.cs: `c:\Zlozka\MegaTreskKarty\Assets\Scripts\Attack.cs`
- Server executeBattle.js: `c:\Zlozka\mega-tresk-server\api\executeBattle.js`
- Unity Client: `c:\Zlozka\MegaTreskKarty\Assets\Scripts\Multiplayer\BattleResultProcessor.cs`

---

**Last Updated:** 2025-11-05  
**Current Attacks:** 2/123 (Punch, Kick)  
**Next:** Attack ID 3 (Heal)
