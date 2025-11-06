# 🎯 Server Implementation: Attack ID 2 (Kick)

## 📋 Súhrn Zmien

Aktivácia **Attack ID 2 (Kick)** v multiplayer battle systéme.

---

## 🔍 Attack ID 2 Špecifikácia (zo singleplayer Attack.cs)

```csharp
// Unity Singleplayer: Attack.cs -> Kick()
public IEnumerator Kick(Kard attacker, Kard receiver, TMP_Text dialogText)
{
    // Damage vzorec:
    receiver.TakeDamage((attacker.speed / 3) - (receiver.defense / 3));
    
    // 20% šanca na +4 extra damage:
    if (UnityEngine.Random.value <= 0.2f) 
        receiver.TakeDamage(4);
}
```

**Mechanika:**
- **Base Damage:** `(speed / 3) - (defense / 3)`, minimum 1
- **Extra Damage:** 20% šanca na +4 extra damage
- **Priorita:** Vyššia speed útočí prvý (rovnako ako Punch)
- **Animácia:** `PlayKickAnimation()` (už existuje v Unity)

---

## 🛠️ Server Changes (mega-tresk-server/api/executeBattle.js)

### 1️⃣ Pridať `executeKick()` funkciu

```javascript
/**
 * Simuluje Kick útok (Attack ID 2)
 * Damage: (speed/3) - (defense/3), min 1
 * Effect: 20% šanca na +4 extra damage
 */
function executeKick(attackerCard, defenderCard) {
  // Base damage
  let damage = Math.floor(attackerCard.speed / 3) - Math.floor(defenderCard.defense / 3);
  if (damage < 1) damage = 1;
  
  console.log(`[executeKick] ${attackerCard.name} -> ${defenderCard.name}: ${damage} base damage`);
  
  defenderCard.health -= damage;
  if (defenderCard.health < 0) defenderCard.health = 0;
  
  // 20% šanca na +4 extra damage
  let didExtraDamage = false;
  let extraDamage = 0;
  
  if (Math.random() <= 0.2 && defenderCard.health > 0) {
    extraDamage = 4;
    defenderCard.health -= extraDamage;
    if (defenderCard.health < 0) defenderCard.health = 0;
    didExtraDamage = true;
    
    console.log(`[executeKick] Extra damage! Total: ${damage + extraDamage}`);
  }
  
  return {
    damage: damage + extraDamage,  // Total damage
    didExtraDamage: didExtraDamage,
    extraDamage: extraDamage
  };
}
```

### 2️⃣ Pridať case 2 do `simulateBattle()`

Nájdi funkciu `simulateBattle()` a pridaj:

```javascript
function simulateBattle(card1, card2, attackId1, attackId2) {
  // ... existujúci kód ...
  
  // Prvý útok
  let firstResult = { damage: 0, didSleep: false, sleepDuration: 0 };
  if (firstAttackId === 1) {
    firstResult = executePunch(firstCard, secondCard);
  } 
  // ✅ PRIDAJ TENTO BLOCK:
  else if (firstAttackId === 2) {
    firstResult = executeKick(firstCard, secondCard);
  }
  
  // Druhý útok (len ak prežil)
  let secondResult = { damage: 0, didSleep: false, sleepDuration: 0 };
  if (secondCard.health > 0) {
    if (secondAttackId === 1) {
      secondResult = executePunch(secondCard, firstCard);
    } 
    // ✅ PRIDAJ TENTO BLOCK:
    else if (secondAttackId === 2) {
      secondResult = executeKick(secondCard, firstCard);
    }
  }
  
  // ... zvyšok kódu ...
}
```

### 3️⃣ **KRITICKÉ:** Pridať `attackId` do response

Nájdi časť kde sa konštruuje `result` object a **pridaj `attackId`**:

```javascript
// Zostav result object
const result = {
  firstAttacker: firstCard.cardId,
  
  attacks: {
    [card1.cardId]: {
      attackId: attackId1,  // ✅ PRIDAJ TOTO!
      damage: firstAttacker === 'player1' ? firstResult.damage : secondResult.damage,
      didSleep: firstAttacker === 'player1' ? secondResult.didSleep : firstResult.didSleep,
      sleepDuration: firstAttacker === 'player1' ? secondResult.sleepDuration : firstResult.sleepDuration,
      effects: card1.effects || []
    },
    [card2.cardId]: {
      attackId: attackId2,  // ✅ PRIDAJ TOTO!
      damage: firstAttacker === 'player2' ? firstResult.damage : secondResult.damage,
      didSleep: firstAttacker === 'player2' ? secondResult.didSleep : firstResult.didSleep,
      sleepDuration: firstAttacker === 'player2' ? secondResult.sleepDuration : firstResult.sleepDuration,
      effects: card2.effects || []
    }
  }
};
```

---

## ✅ Checklist

- [ ] Pridať `executeKick()` funkciu
- [ ] Pridať `case 2` do `simulateBattle()` pre firstAttackId
- [ ] Pridať `case 2` do `simulateBattle()` pre secondAttackId
- [ ] **Pridať `attackId: attackId1` do `attacks[card1.cardId]`**
- [ ] **Pridať `attackId: attackId2` do `attacks[card2.cardId]`**
- [ ] Deploy na Vercel (`git push`)
- [ ] Test v Unity (vyberte Attack 2 - Kick)

---

## 🧪 Testing

### Manual Test:

```bash
# Local dev server
cd c:\Zlozka\mega-tresk-server
vercel dev

# Test Kick (attack ID 2)
curl -X POST http://localhost:3000/api/executeBattle `
  -H "Content-Type: application/json" `
  -d '{
    "roomCode": "TEST123",
    "playerId": "player1",
    "attackData": {
      "cardId": "test-card-id",
      "attackId": 2
    }
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "bothPlayersReady": true,
  "battleResult": {
    "firstAttacker": "card1_uuid",
    "attacks": {
      "card1_uuid": {
        "attackId": 2,
        "damage": 5,
        "didSleep": false,
        "effects": []
      },
      "card2_uuid": {
        "attackId": 1,
        "damage": 2,
        "didSleep": false,
        "effects": []
      }
    }
  }
}
```

---

## 📚 Príklady

### Kick Damage Kalkulácia:

```javascript
// Card 1: Speed=12, Defense=3
// Card 2: Speed=9, Defense=6

// Card1 Kick na Card2:
baseDamage = floor(12/3) - floor(6/3) = 4 - 2 = 2
extraDamage = 20% šanca na +4
totalDamage = 2 (alebo 6 ak proc)

// Card2 Kick na Card1:
baseDamage = floor(9/3) - floor(3/3) = 3 - 1 = 2
extraDamage = 20% šanca na +4
totalDamage = 2 (alebo 6 ak proc)
```

---

## 🔄 Next Steps

Po úspešnom teste Attack ID 2:
1. Otestuj v Unity multiplayer battle
2. Skontroluj logy (`Debug.LogWarning` v Unity)
3. Overiť že `PlayKickAnimation()` sa volá správne
4. Pokračuj na Attack ID 3 (Heal) - rovnaký workflow

---

## 📞 Troubleshooting

**Problem:** Unity zobrazuje "Unknown attackId=2"
- **Fix:** Skontroluj či server vracia `attackId: 2` v response
- **Log:** `Debug.LogWarning` v `BattleResultProcessor.cs` ukáže `attackId`

**Problem:** Kick animácia nejde
- **Fix:** Overiť že `AttackAnimations.cs` má `PlayKickAnimation()` metódu
- **Fallback:** Kód použije Punch animáciu ak Kick chýba

**Problem:** Damage je nesprávny
- **Fix:** Skontroluj damage vzorec - musí byť `floor(speed/3) - floor(defense/3)`, min 1
- **Test:** Logovať `attackerCard.speed`, `defenderCard.defense` pred výpočtom

---

**Version:** V8  
**Created:** 2025-11-05  
**Attack ID:** 2 (Kick)  
**Status:** Ready for implementation
