# Kill Counter System - Unity Setup Guide

**Date:** 2025-11-01  
**Version:** V1  
**Feature:** Visual kill counter with win condition (first to 3 kills wins)

---

## 🎯 Overview

Systém sleduje koľko kariet hráč zabil (červené štvorčeky). Prvý kto zabije **3 karty súpera** vyhráva hru.

---

## 📋 Unity Inspector Setup (Krok za krokom)

### 1️⃣ Vytvor Kill Counter UI Image Objects

V Multiplayer scéne:

#### **Player Kill Indicators** (3 štvorčeky vľavo):
```
Hierarchy:
Canvas
└── BattleInfoGramsBG
    └── PlayerLives (nový Empty GameObject)
        ├── PlayerLive (1)  ← UI Image
        ├── PlayerLive (2)  ← UI Image
        └── PlayerLive (3)  ← UI Image
```

#### **Enemy Kill Indicators** (3 štvorčeky vpravo):
```
Hierarchy:
Canvas
└── BattleInfoGramsBG
    └── EnemyLives (nový Empty GameObject)
        ├── EnemyLive (1)  ← UI Image
        ├── EnemyLive (2)  ← UI Image
        └── EnemyLive (3)  ← UI Image
```

**Image Settings:**
- Color: **White** (zmení sa na zelená/červená cez script)
- Sprite: Biely štvorček alebo kruh
- RectTransform: Umiestnenie podľa GUI layoutu

---

### 2️⃣ Pridaj KillCounterUI Component

Pre **každý** z 6 Image objektov (PlayerLive 1-3, EnemyLive 1-3):

1. Vyber objekt v Hierarchy (napr. `PlayerLive (1)`)
2. Inspector → **Add Component** → `KillCounterUI`

**Komponenta automaticky nastaví:**
- ✅ Alive Color: Zelená `(0.2, 0.8, 0.2, 1)` 
- ✅ Dead Color: Červená `(0.8, 0.2, 0.2, 1)`

**POZNÁMKA:** Môžeš nastaviť vlastné farby v Inspectore ak chceš.

---

### 3️⃣ Vytvor MultiplayerKillCounterManager GameObject

1. Hierarchy → **Create Empty GameObject**
2. Názov: `MultiplayerKillCounterManager`
3. Add Component → `MultiplayerKillCounterManager`

**Inspector Setup:**

#### **Player Kill Indicators:**
- Size: `3`
- Element 0: Drag `PlayerLive (1)` z Hierarchy
- Element 1: Drag `PlayerLive (2)`
- Element 2: Drag `PlayerLive (3)`

#### **Enemy Kill Indicators:**
- Size: `3`
- Element 0: Drag `EnemyLive (1)` z Hierarchy
- Element 1: Drag `EnemyLive (2)`
- Element 2: Drag `EnemyLive (3)`

#### **Game Systems:**
- Fight System: Drag `FightSystemMultiplayer` GameObject

---

### 4️⃣ Pripoj Manager k BattleResultProcessor

1. Vyber `BattleResultProcessor` GameObject v Hierarchy
2. Inspector → **Battle Result Processor (Script)**
3. Dependencies → **Kill Counter Manager**: Drag `MultiplayerKillCounterManager` GameObject

---

## 🧪 Testing

### Test Flow:
```
1. Spusti Multiplayer scene (Play Mode)
2. Začni battle
3. Zabij enemy kartu (HP → 0)
4. ✅ Prvý štvorček v Player Lives sa zmení na ČERVENÚ
5. Zabij ďalšie 2 enemy karty
6. ✅ Druhý a tretí štvorček červené
7. Po 3. kill → "Victory! You killed 3 enemy cards!"
8. State: WON
```

### Expected Logs:
```
[KillCounterManager] Initialized. Win condition: First to 3 kills wins!
[KillCounterUI] PlayerLive (1) set to ALIVE (green)
...
[BattleResultProcessor] Enemy card died
[KillCounterManager] 💀 Player killed enemy card! Count: 1/3
[KillCounterUI] PlayerLive (1) set to DEAD (red)
...
[KillCounterManager] 💀 Player killed enemy card! Count: 3/3
[KillCounterManager] 🎉 PLAYER WINS! Killed 3 enemy cards!
```

---

## 🎨 UI Layout Example

```
┌────────────────────────────────────────┐
│  JESUS CHRISTUS      SIDDHARTHA BUDDHA │
│  [🟢][🟢][🟢]        [🟢][🟢][🟢]      │ ← Kill indicators
│                                        │
│        [Player Card] vs [Enemy Card]   │
│                                        │
└────────────────────────────────────────┘

Po 1. player kill:
│  [🔴][🟢][🟢]        [🟢][🟢][🟢]      │

Po 3. player kills → VICTORY!
│  [🔴][🔴][🔴]        [🟢][🟢][🟢]      │
```

---

## 🔧 Customization

### Zmeniť farby:
V `KillCounterUI` komponente na každom Image objekte:
- **Alive Color**: Zelená (default `0.2, 0.8, 0.2`)
- **Dead Color**: Červená (default `0.8, 0.2, 0.2`)

### Zmeniť počet kills na výhru:
V `MultiplayerKillCounterManager.cs`:
```csharp
private const int KILLS_TO_WIN = 3; // ← Zmeň toto číslo
```

---

## 🐛 Common Issues

### "KillCounterUI Missing Image component"
**Fix:** Pridaj `Image` komponent na GameObject (UI → Image)

### "Player kill indicator [0] is NULL"
**Fix:** Drag all 3 PlayerLive objekty do Inspector poľa v `MultiplayerKillCounterManager`

### "KillCounterManager not assigned!"
**Fix:** Drag `MultiplayerKillCounterManager` GameObject do `BattleResultProcessor` → Kill Counter Manager field

---

## 📝 Files Created

- `Assets/Scripts/Multiplayer/KillCounterUI.cs` - UI component (green/red indicator)
- `Assets/Scripts/Multiplayer/MultiplayerKillCounterManager.cs` - Kill tracking manager
- `Assets/Scripts/Multiplayer/BattleResultProcessor.cs` - Modified (kill counter integration)

---

**Status:** Ready for Unity setup! 🎮
