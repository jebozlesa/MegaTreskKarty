# 🎭 Effect System Architecture (Multiplayer)

**Účel:** Návrh server-authoritative effect systému pre multiplayer na základe singleplayer implementácie.

**Preštudované súbory:**
- `Effects.cs` (763 lines) - 26 effect typov
- `Kard.cs` - effect management (AddEffect, RemoveEffect, CheckEffect)
- `Attack.cs` - attack → effect aplikácia

---

## 📊 Effect Inventory (26 Typov)

### 🔴 **Damage-Over-Time (DOT) Effects:**
```
1.  Bleed      - Decreasing damage each turn (5→4→3→2→1)
4.  Exposure   - Random radiation damage, 20% chance to clear
16. Burn       - 1 damage per turn (permanent until cleared)
24. Poison     - 1 damage + 33% STR debuff per turn (permanent)
```
**Stacking:** Bleed & Exposure stack (multiple instances), Burn & Poison don't  
**Server Logic:** Apply damage at turn start, decrement duration/intensity

---

### 😴 **Movement-Blocking Effects:**
```
3.  Sleep      - Cannot attack (1-3 turns)
                 Wake-up attacks: {26} (Alarm Clock)
9.  Tether     - Cannot move (90% chance to decrement)
                 Break-free attacks: {64}
12. Blockade   - 50% chance to block, 2 dmg + -1 STR per turn
                 Break-through attacks: {64}
```
**Special:** Some effects don't execute during Sleep (Confusion)  
**Server Logic:** Set `canMove: false`, check wake-up conditions

---

### 🧠 **Mental/Debuff Effects:**
```
2.  Asceticism - Self-damage 1 per turn, cannot attack
13. Depression - -3 STR/SPD/ATT (debuff active during effect)
17. Confusion  - 33% hurt self (1-3 dmg), 33% forget attack
21. Calm       - -3 STR/ATT (debuff active)
26. Curse      - 25% chance to prevent attack
```
**Server Logic:** Modify stats during effect, restore on removal

---

### ⚔️ **Multi-Turn Attack Effects (Delayed Damage):**
```
5.  Siege          - 2-turn setup → 8-15 damage (-10 DEF to self)
11. Envelop        - 2-turn setup → 12-16 damage (Double Envelopment)
14. ArtInspiration - 3-turn setup → 10 damage + enemy debuff
15. Autoportrait   - 3-turn painting → +KNO/STR/DEF per turn
20. Horns          - 2-turn charging → 4-10 damage (Buffalo Horns)
22. Reloading      - 2-turn reload → 18-22 damage (Flintlock)
23. Trident        - 2-turn aim → 8 damage (Retiarius)
```
**Poznámka:** Tieto NIE SÚ efekty, ale **multi-turn attacks**!  
**Oddeliť:** Samostatný systém (attack phases) vs status effects

---

### 💪 **Buff Effects:**
```
6.  Fury       - +5 STR/ATT, -3 DEF (3 turns)
7.  Famine     - Heal 2 HP per turn (harvest phase)
18. Satellite  - +1 KNO per turn (permanent)
```
**Server Logic:** Apply stat bonuses, reverse on removal

---

### 🎯 **Hybrid/Unique Effects:**
```
8.  Electricity - Decreasing paralysis chance (X/5 chance to prevent move)
10. Starving    - -1 HP, +1 DEF per turn
19. Fear        - -5 STR/ATT, +3 DEF (stat modifier)
```

---

## 🏗️ Architectural Decisions

### ❌ **Čo NEROBÍME v Multiplayer:**

**1. Multi-Turn Attacks ≠ Effects**
```csharp
// ❌ Singleplayer: Siege je "effect" (ID 5)
yield return StartCoroutine(attacker.AddEffect(5, 2));  // 2-turn setup

// ✅ Multiplayer: Siege je attack s fázami
{
  attackId: 30,  // Siege
  phase: 1,      // Setup phase
  phasesRemaining: 2
}
```
**Prečo:** Multi-turn attacks menia gameplay flow, nie sú pasívne efekty.  
**Riešenie:** Samostatný `attackPhases` systém (budúcnosť).

---

**2. Visual-Only Effects v Unity**
```csharp
// ❌ NEPOSIELAJME na server:
PlayBleedAnimation()
PlaySleepAnimation()
ShowEffectIcon()

// ✅ Server posiela len data:
{
  effects: [
    { type: 1, duration: 5, intensity: 5 },  // Bleed
    { type: 3, duration: 2 }                 // Sleep
  ]
}
// Unity zobrazí animácie & ikony lokálne
```

---

### ✅ **Multiplayer Effect Model:**

```javascript
// MongoDB: selectedCards.{playerId}.effects
effects: [
  {
    type: 1,        // Effect ID (1=Bleed, 3=Sleep, ...)
    duration: 5,    // Turns remaining
    intensity: 5,   // Damage amount (pre Bleed/Exposure)
    source: "card_uuid_attacker"  // Kto aplikoval effect
  },
  {
    type: 3,
    duration: 2,
    wakeUpAttacks: [26]  // Attacks that instantly remove this effect
  }
]
```

**Schema:**
- `type` (int) - Effect ID (1-26)
- `duration` (int) - Turns remaining (-1 = permanent for Burn/Poison)
- `intensity` (int, optional) - Damage amount (Bleed: 5→4→3..., Exposure: random)
- `source` (string, optional) - CardId of attacker (for multi-target effects)
- `metadata` (object, optional) - Effect-specific data (wakeUpAttacks, etc.)

---

## 🎯 Effect Categories (Implementation Priority)

### **Tier 1: Basic Effects (Immediate)**
```
✅ Implementovať hneď (Attack ID 1-10):
- Sleep (3)       - Blocking + wake-up logic
- Bleed (1)       - Decreasing DOT
- Burn (16)       - Permanent DOT
- Poison (24)     - DOT + stat debuff
- Exposure (4)    - Random damage + clear chance
```

### **Tier 2: Stat Modifiers (Mid-Priority)**
```
⏳ Potrebuje stat tracking system:
- Fury (6)        - Temp buff
- Fear (19)       - Debuff
- Depression (13) - Debuff
- Calm (21)       - Debuff
- Satellite (18)  - Permanent buff
```

### **Tier 3: Complex Logic (Low-Priority)**
```
⏸️ Vyžaduje špeciálnu logiku:
- Confusion (17)  - Random behavior (hurt self vs skip turn)
- Asceticism (2)  - Self-damage + movement block
- Electricity (8) - Probabilistic paralysis
- Tether (9)      - Movement block + break-free attacks
- Blockade (12)   - 50% block + damage
- Curse (26)      - 25% attack prevention
```

### **Tier 4: Multi-Turn Attacks (Oddelený Systém)**
```
🚫 NIE efekty, ale attack phases:
- Siege (5)
- Envelop (11)
- ArtInspiration (14)
- Autoportrait (15)
- Horns (20)
- Reloading (22)
- Trident (23)
- Famine (7) - hybrid (heal effect, ale má preparation phase)
```

---

## 🔄 Effect Lifecycle (Server-Authoritative)

### **1. Effect Application (executeBattle.js)**
```javascript
// Attack ID 1 (Punch) má 20% sleep chance
if (Math.random() <= 0.2) {
  const sleepDuration = Math.floor(Math.random() * 2) + 1; // 1-2 turns
  
  // Apply to defender card
  await applyEffect(defenderCard, {
    type: 3,        // Sleep
    duration: sleepDuration
  });
}

// Update MongoDB selectedCards
await collection.updateOne(
  { roomCode, [`selectedCards.${defenderId}.cardId`]: defenderCardId },
  { 
    $push: { [`selectedCards.${defenderId}.effects`]: newEffect },
    $set: { [`selectedCards.${defenderId}.health`]: newHealth }
  }
);
```

### **2. Effect Processing (processEffects.js - nový endpoint)**
```javascript
// Called at TURN START (before attack selection)
async function processEffects(roomCode, playerId) {
  const room = await collection.findOne({ roomCode });
  const card = room.selectedCards[playerId];
  
  let totalDamage = 0;
  let statChanges = {};
  let effectsToRemove = [];
  
  for (let i = 0; i < card.effects.length; i++) {
    const effect = card.effects[i];
    
    switch (effect.type) {
      case 1: // Bleed
        totalDamage += effect.intensity;
        effect.intensity -= 1;  // Decrease bleeding
        if (effect.intensity <= 0) effectsToRemove.push(i);
        break;
        
      case 3: // Sleep
        effect.duration -= 1;
        if (effect.duration <= 0) effectsToRemove.push(i);
        break;
        
      case 16: // Burn (permanent)
        totalDamage += 1;
        break;
        
      // ... ďalšie efekty
    }
  }
  
  // Apply damage
  card.health -= totalDamage;
  if (card.health < 0) card.health = 0;
  
  // Remove expired effects
  effectsToRemove.reverse().forEach(idx => card.effects.splice(idx, 1));
  
  // Update DB
  await collection.updateOne(
    { roomCode },
    { $set: { [`selectedCards.${playerId}`]: card } }
  );
  
  return {
    damage: totalDamage,
    effectsRemoved: effectsToRemove.length,
    canAttack: !hasBlockingEffect(card.effects)  // Sleep/Tether check
  };
}
```

### **3. Unity Client (BattleResultProcessor.cs)**
```csharp
// Server vráti:
{
  effectsApplied: [
    { type: 3, duration: 2, target: "myCardId" }  // Sleep applied to me
  ],
  effectsProcessed: {
    damage: 5,           // Total DOT damage
    effectsRemoved: 1,   // Bleed expired
    canAttack: false     // Sleep blocks attack
  }
}

// Unity zobrazí:
foreach (var effect in result.effectsApplied) {
    if (effect.target == myCardId) {
        yield return StartCoroutine(ShowEffectAnimation(effect.type));
        myCard.AddEffectIcon(GetEffectName(effect.type));
    }
}

// DOT damage animation
if (result.effectsProcessed.damage > 0) {
    myCard.TakeDamage(result.effectsProcessed.damage);  // Red HP drop
}
```

---

## 🎮 Special Effect Behaviors

### **Sleep Wake-Up Attacks:**
```javascript
// Attack ID 26 (Alarm Clock) instantly removes Sleep
const wakeUpAttacks = [26];

if (wakeUpAttacks.includes(attackId)) {
  // Remove all Sleep effects instantly
  card.effects = card.effects.filter(e => e.type !== 3);
}
```

### **Bleed Stacking:**
```javascript
// Multiple Bleeds stack (singleplayer: ID 1 a 4 môžu byť viac krát)
card.effects = [
  { type: 1, duration: 5, intensity: 5 },  // First bleed (5 dmg)
  { type: 1, duration: 3, intensity: 3 }   // Second bleed (3 dmg)
];

// Total damage per turn: 5 + 3 = 8 damage!
```

### **Effect Removal (Heal Attack):**
```javascript
// Attack ID 3 (Heal) removes negative effects
const removedEffects = [1, 4, 13, 24];  // Bleed, Exposure, Depression, Poison

card.effects = card.effects.filter(e => !removedEffects.includes(e.type));
```

### **Effects Blocked by Sleep:**
```javascript
// Confusion (17) doesn't execute if card is sleeping
function canExecuteEffect(card, effectType) {
  const isSleeping = card.effects.some(e => e.type === 3 && e.duration > 0);
  
  if (isSleeping && effectType === 17) {  // Confusion
    return false;  // Don't hurt yourself while sleeping
  }
  
  return true;
}
```

---

## 🛠️ Implementation Plan

### **Phase 1: Basic DOT Effects (Week 1)**
```
✅ Server:
- applyEffect() helper function
- processEffects() endpoint (turn start)
- Bleed (decreasing), Burn (permanent), Poison (permanent + debuff)

✅ Unity:
- Effect icon system (reuse Kard.AddEffectIcon)
- Effect animations (reuse AttackAnimations)
- DOT damage visualization

✅ Attacks:
- Attack ID 1 (Punch) → Sleep
- Attack ID 3 (Heal) → Remove [1,4,13,24]
```

### **Phase 2: Movement-Blocking Effects (Week 2)**
```
✅ Server:
- canAttack flag in processEffects()
- Wake-up attack detection
- Sleep/Tether/Blockade logic

✅ Unity:
- "Cannot attack" UI state
- Sleep ZZZ animation
- Turn skip handling
```

### **Phase 3: Stat Modifiers (Week 3)**
```
✅ Server:
- Stat tracking (strength, defense, speed, knowledge buffed/debuffed)
- Stat restoration on effect removal
- Fury/Fear/Depression/Calm

✅ Unity:
- Stat change animations (green +5 STR, red -3 DEF)
- Visual stat indicators
```

### **Phase 4: Complex Effects (Week 4+)**
```
⏳ Server:
- Confusion (random behavior)
- Electricity (probabilistic paralysis)
- Curse (attack prevention)

⏸️ Future:
- Multi-turn attack phases (Siege, Reloading, etc.)
```

---

## 📋 Server Endpoints Needed

### **1. applyEffect (helper function)**
```javascript
// executeBattle.js (internal)
function applyEffect(card, effect) {
  // Check if effect can stack
  if (effect.type === 1 || effect.type === 4) {  // Bleed/Exposure
    card.effects.push(effect);  // Always add
  } else {
    // Check duplicates
    const exists = card.effects.some(e => e.type === effect.type);
    if (!exists) card.effects.push(effect);
  }
}
```

### **2. processEffects (new endpoint)**
```javascript
// api/processEffects.js
// Called at turn start (before attack selection UI)
POST /api/processEffects
Body: { roomCode, playerId }
Response: {
  damage: 5,           // Total DOT damage
  effectsRemoved: 2,   // Expired effects
  canAttack: true,     // Movement-blocking check
  statChanges: { strength: -1 }  // Poison debuff
}
```

### **3. removeEffect (optional endpoint)**
```javascript
// api/removeEffect.js
// For instant effect removal (Heal attack, wake-up attacks)
POST /api/removeEffect
Body: { 
  roomCode, 
  playerId, 
  effectTypes: [1, 4, 13, 24]  // Remove these effect IDs
}
Response: { success: true, effectsRemoved: 2 }
```

---

## 🎨 Unity UI Enhancements

### **Effect Icons (Reuse Kard.cs):**
```csharp
// Already implemented!
card.AddEffectIcon("Sleep");  // Loads Resources/Game/EffectIcons/Sleep.png
card.RemoveEffectIcon("Sleep");
```

### **DOT Damage Animation:**
```csharp
// Reuse existing TakeDamage() + EffectAnimations()
card.TakeDamage(5);  // Red "-5 HP" floats up
yield return StartCoroutine(attackAnimations.PlayBleedContinueAnimation(card.transform));
```

### **Stat Change Animation:**
```csharp
// cardAnimator.AnimateStatChange() - already exists!
yield return StartCoroutine(cardAnimator.AnimateStatChange(card, -3, "STR"));  // Red "-3 STR"
```

---

## 🧪 Testing Strategy

### **Test Scenario 1: Bleed (Decreasing DOT)**
```
Turn 1: Punch lands → 20% sleep miss, but add Bleed(5)
Turn 2: processEffects() → 5 damage, Bleed(4)
Turn 3: processEffects() → 4 damage, Bleed(3)
Turn 4: processEffects() → 3 damage, Bleed(2)
Turn 5: processEffects() → 2 damage, Bleed(1)
Turn 6: processEffects() → 1 damage, Bleed removed
```

### **Test Scenario 2: Sleep + Wake-Up**
```
Turn 1: Sleep(2) applied
Turn 2: processEffects() → canAttack=false, Sleep(1)
        Unity shows "Opponent is sleeping, waiting..."
Turn 3: Player uses Attack 26 (Alarm Clock) → Sleep removed instantly
        processEffects() → canAttack=true
```

### **Test Scenario 3: Heal Removes Effects**
```
Card has: Bleed(5), Poison, Depression
Turn X: Use Heal attack
Server: removeEffect([1,4,13,24])
Result: Bleed removed, Poison removed, Depression removed
Unity: Effect icons disappear, green heal animation
```

---

## 📚 Resources

- **Singleplayer Reference:** `Effects.cs` (line 113-763)
- **Effect Names:** `Kard.GetEffectNameById()` (line 640-734)
- **Attack → Effect Mapping:** `Attack.cs` (grep "AddEffect")
- **Stacking Logic:** `Kard.AddEffect()` (line 443-484)

---

## ✅ Next Steps

1. **Diskusia:** Potvrdiť prioritu (Tier 1 effects first?)
2. **Server Schema:** Finalizovať `effects` array structure v MongoDB
3. **Attack ID 1 Update:** Pridať Sleep effect do Punch implementácie
4. **processEffects Endpoint:** Implementovať turn start effect processing
5. **Unity Effect Icons:** Integrovať existujúci icon system do multiplayer

---

**Version:** V9 Effect System Proposal  
**Date:** 2025-11-06  
**Status:** Architecture Design - Awaiting User Approval
