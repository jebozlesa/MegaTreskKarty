# 🚨 Kill Counter Debug Checklist

## Problém: Žiadne debug logy z kill counter systému!

**Missing logs:**
- ❌ `[KillCounterManager] 🔧 Awake() called - GameObject is ACTIVE!`
- ❌ `[KillCounterManager] Start() called - Initializing...`
- ❌ `[KillCounterUI] ... set to ALIVE (green)`

---

## ✅ CHECKLIST - Skontroluj V UNITY EDITOR:

### 1️⃣ Hierarchy - Nájdi GameObject "MultiplayerKillCounterManager"

**Kde má byť:**
```
Multiplayer (Scene)
  └─ Canvas
      └─ PlayerPanel
          └─ PlayerLive (1)
              └─ Image (1)   ← Tento má KillCounterUI script
```

**ALEBO:**
```
Multiplayer (Scene)
  └─ MultiplayerKillCounterManager   ← Samostatný GameObject
```

**AKO SKONTROLOVAŤ:**
1. Unity Editor → Hierarchy panel (ľavý stĺpec)
2. Ctrl+F → Search "MultiplayerKillCounterManager"
3. **Výsledok:**
   - ✅ NÁJDENÉ → Klikni naň, skontroluj Inspector
   - ❌ NENÁJDENÉ → **GameObject neexistuje! TOTO JE PROBLÉM!**

---

### 2️⃣ Inspector - Ak GameObject existuje, skontroluj:

**A) GameObject je ENABLED:**
- ✅ Checkbox vedľa názvu je **zaškrtnutý** (modrý)
- ❌ Ak je sivý/vypnutý → ZAPNI HO!

**B) Script je ENABLED:**
- Nájdi komponent "Multiplayer Kill Counter Manager (Script)"
- ✅ Checkbox vedľa scriptu je **zaškrtnutý**
- ❌ Ak nie → ZAPNI HO!

**C) Console Filter:**
- Unity Console → Clear All
- Unity Console → Filter: "KillCounter" (horný search bar)
- Play Mode → Spusti hru
- **Očakávané logy:**
  ```
  [KillCounterManager] 🔧 Awake() called - GameObject is ACTIVE!
  [KillCounterManager] Start() called - Initializing...
  [KillCounterUI] PlayerLive (1) set to ALIVE (green)
  [KillCounterUI] PlayerLive (2) set to ALIVE (green)
  [KillCounterUI] PlayerLive (3) set to ALIVE (green)
  [KillCounterUI] EnemyLive (1) set to ALIVE (green)
  ...
  ```

---

### 3️⃣ Ak GameObject NEEXISTUJE → VYTVOR HO!

**KROK 1 - Vytvor prázdny GameObject:**
1. Hierarchy → Right Click → Create Empty
2. Rename na: **"MultiplayerKillCounterManager"**

**KROK 2 - Pridaj script:**
1. Inspector → Add Component
2. Search: "MultiplayerKillCounterManager"
3. Vyberi script

**KROK 3 - Drag & Drop referencie:**
1. **Player Kill Indicators (3 elementy):**
   - Drag: `PlayerPanel → PlayerLive (1) → Image (1)` do Element 0
   - Drag: `PlayerPanel → PlayerLive (2) → Image (1)` do Element 1
   - Drag: `PlayerPanel → PlayerLive (3) → Image (1)` do Element 2

2. **Enemy Kill Indicators (3 elementy):**
   - Drag: `EnemyPanel → EnemyLive (1) → Image (1)` do Element 0
   - Drag: `EnemyPanel → EnemyLive (2) → Image (1)` do Element 1
   - Drag: `EnemyPanel → EnemyLive (3) → Image (1)` do Element 2

3. **Fight System:**
   - Drag: `FightSystemMultiplayer` GameObject do "Fight System" field

**KROK 4 - Overiť že každý Image má KillCounterUI script:**
1. Klikni na `PlayerPanel → PlayerLive (1) → Image (1)`
2. Inspector → Skontroluj že má komponent **"Kill Counter UI (Script)"**
3. Repeat pre všetkých 6 Images (3 player + 3 enemy)

---

## 🎮 TEST - Po oprave:

**Play Mode → Console filter "KillCounter" → Očakávané logy:**
```
[KillCounterManager] 🔧 Awake() called - GameObject is ACTIVE!
[KillCounterManager] Fight system not set, searching...
[KillCounterManager] ✅ Found FightSystemMultiplayer: FightSystemMultiplayer
[KillCounterManager] Start() called - Initializing...
[KillCounterUI] PlayerLive (1) set to ALIVE (green)
[KillCounterUI] PlayerLive (2) set to ALIVE (green)
[KillCounterUI] PlayerLive (3) set to ALIVE (green)
[KillCounterUI] EnemyLive (1) set to ALIVE (green)
[KillCounterUI] EnemyLive (2) set to ALIVE (green)
[KillCounterUI] EnemyLive (3) set to ALIVE (green)
[KillCounterManager] ✅ Initialized. Win condition: First to 3 kills wins!
```

---

## 📸 SCREENSHOT PRE DEBUG:

**Ak stále nič nevidíš, screenshootni:**
1. **Hierarchy panel** (ľavý stĺpec) → Search "MultiplayerKillCounterManager"
2. **Inspector panel** pre tento GameObject (pravý stĺpec)
3. **Console panel** s filtrom "KillCounter" (po spustení Play Mode)

---

**KRITICKÁ OTÁZKA:**  
Je GameObject `MultiplayerKillCounterManager` v Hierarchy paneli?  
✅ ÁNO → Skontroluj či je enabled  
❌ NIE → Vytvor ho podľa KROK 1-4 vyššie!
