# Unity Battle System Setup Guide

## 🎯 Čo musíš nastaviť v Unity Inspector

### Scéna: `Multiplayer.unity`

---

## 1️⃣ BattleSubmitter Component

### Vytvor GameObject:
1. V Hierarchy: klikni pravým → `Create Empty`
2. Pomenuj: `BattleSubmitter`
3. Add Component → `BattleSubmitter`

### Inspector nastavenia:
```
BattleSubmitter
├─ Server Functions Manager     → drag FightSystemMultiplayer → ServerFunctionsManager
├─ Fight System                  → drag FightSystemMultiplayer GameObject
└─ Result Processor              → drag BattleResultProcessor GameObject
```

**Reference Fields:**
- `serverFunctionsManager` - odkaz na ServerFunctionsManager z FightSystemMultiplayer
- `fightSystem` - odkaz na FightSystemMultiplayer
- `resultProcessor` - odkaz na BattleResultProcessor

---

## 2️⃣ BattleResultProcessor Component

### Vytvor GameObject:
1. V Hierarchy: klikni pravým → `Create Empty`
2. Pomenuj: `BattleResultProcessor`
3. Add Component → `BattleResultProcessor`

### Inspector nastavenia:
```
BattleResultProcessor
├─ Fight System        → drag FightSystemMultiplayer GameObject
├─ Attack Component    → drag Attack GameObject (má Attack.cs so singleplayer animáciami)
├─ Dialog Text         → drag UI Text element (TMP_Text)
├─ Player Life Bar     → drag Player HealthBar
└─ Enemy Life Bar      → drag Enemy HealthBar
```

**Reference Fields:**
- `fightSystem` - odkaz na FightSystemMultiplayer
- `attackComponent` - odkaz na **Attack.cs** komponent zo singleplayer scény (obsahuje `PlayPunchAnimation()` a iné animácie)
- `dialogText` - UI text element pre zobrazovanie správ (TMP_Text)
- `playerLifeBar` - zdravie hráča UI bar (HealthBar komponent)
- `enemyLifeBar` - zdravie nepriateľa UI bar (HealthBar komponent)

---

## 3️⃣ FightSystemMultiplayer Updates

Nájdi GameObject s **FightSystemMultiplayer** komponentom a pridaj referencie:

### Inspector nastavenia:
```
FightSystemMultiplayer (existujúce)
├─ ... (existujúce fieldy)
├─ Battle Submitter         → drag BattleSubmitter GameObject
└─ Battle Result Processor  → drag BattleResultProcessor GameObject
```

**Nové Fields:**
- `battleSubmitter` - odkaz na BattleSubmitter komponent
- `battleResultProcessor` - odkaz na BattleResultProcessor komponent

---

## 4️⃣ Hierarchia (odporúčaná štruktúra)

```
Multiplayer Scene
├─ Canvas
│   ├─ DialogText (TMP_Text)
│   ├─ PlayerLifeBar (HealthBar)
│   ├─ EnemyLifeBar (HealthBar)
│   └─ ...
├─ FightSystemMultiplayer
│   ├─ ServerFunctionsManager
│   ├─ MultiplayerService
│   ├─ AttackSelectionManager
│   └─ ...
├─ BattleSubmitter (NEW)
│   └─ Component: BattleSubmitter.cs
├─ BattleResultProcessor (NEW)
│   └─ Component: BattleResultProcessor.cs
└─ Attack
    └─ Component: Attack.cs (pre animácie)
```

---

## 5️⃣ Kontrola Referencií - Checklist

Skontroluj Inspector pred testovaním:

### ✅ FightSystemMultiplayer
- [ ] `battleSubmitter` - nie NULL
- [ ] `battleResultProcessor` - nie NULL
- [ ] `serverFunctionsManager` - nie NULL
- [ ] `player` - nie NULL (Player objekt)
- [ ] `enemy` - nie NULL (Enemy objekt)
- [ ] `dialogText` - nie NULL (TMP_Text)
- [ ] `playerLifeBar` - nie NULL (HealthBar)
- [ ] `enemyLifeBar` - nie NULL (HealthBar)

### ✅ BattleSubmitter
- [ ] `serverFunctionsManager` - nie NULL
- [ ] `fightSystem` - nie NULL
- [ ] `resultProcessor` - nie NULL

### ✅ BattleResultProcessor
- [ ] `fightSystem` - nie NULL
- [ ] `attackComponent` - nie NULL (**Attack.cs** z Game/Singleplayer scény - reuse animácií!)
- [ ] `dialogText` - nie NULL
- [ ] `playerLifeBar` - nie NULL
- [ ] `enemyLifeBar` - nie NULL

---

## 6️⃣ Alternatívne Riešenie (Child Objects)

Ak nechceš vytvárať samostatné GameObjecty, môžeš komponenty pridať ako **child objects**:

```
FightSystemMultiplayer GameObject
├─ Component: FightSystemMultiplayer.cs
├─ Component: BattleSubmitter.cs          ← pridaj sem
├─ Component: BattleResultProcessor.cs    ← pridaj sem
└─ Component: ServerFunctionsManager.cs
```

### Výhoda:
✅ Všetko na jednom GameObject  
✅ Jednoduchšie referencie  
✅ Menej objektov v hierarchii

### Nevýhoda:
❌ Menej prehľadné pri väčších projektoch  
❌ Ťažšie testovanie komponentov izolované

---

## 7️⃣ Script Execution Order (voliteľné)

Ak sa stretneme s race conditions:

1. Edit → Project Settings → Script Execution Order
2. Nastav poradie:
   ```
   ServerFunctionsManager     : -100
   FightSystemMultiplayer     : 0
   BattleSubmitter            : 100
   BattleResultProcessor      : 100
   ```

---

## 8️⃣ Testing v Editor

### Test 1: Skontroluj NULL referencie
```csharp
// Pridaj do FightSystemMultiplayer.Start()
void Start() {
    if (battleSubmitter == null) 
        Debug.LogError("BattleSubmitter je NULL!");
    if (battleResultProcessor == null)
        Debug.LogError("BattleResultProcessor je NULL!");
}
```

### Test 2: Spusti scénu
1. Play button v Unity
2. Choď do Multiplayer
3. Vyber kartu
4. Klikni na útok
5. Watch Console pre errory

---

## 9️⃣ Common Errors

### ❌ "NullReferenceException: battleSubmitter"
**Fix:** Drag BattleSubmitter GameObject do FightSystemMultiplayer → Battle Submitter field

### ❌ "NullReferenceException: serverFunctionsManager"
**Fix:** Drag ServerFunctionsManager do BattleSubmitter → Server Functions Manager field

### ❌ "Cannot find BattleSubmitter component"
**Fix:** Uisti sa že si pridal BattleSubmitter.cs komponent na GameObject

### ❌ "Dialog text not updating"
**Fix:** Drag správny TMP_Text do BattleResultProcessor → Dialog Text field

---

## 🔟 Ďalšie Kroky

Po nastavení Unity:
1. ✅ **Skontroluj všetky referencie** (žiadny NULL)
2. ⏳ **Deploy Vercel function** (`executeBattle.js`)
3. ⏳ **Register v PlayFab** (Dashboard → Functions)
4. ⏳ **Nahrať card stats** do PlayFab TitleData
5. ⏳ **Prvý test** s 2 zariadeniami

---

## 📸 Screenshot Reference

Takto by mal vyzerať Inspector:

```
╔═══════════════════════════════════════╗
║   FightSystemMultiplayer (Script)    ║
╠═══════════════════════════════════════╣
║ ... (existing fields)                 ║
║                                       ║
║ Battle Submitter                      ║
║   ┌─────────────────────────────┐    ║
║   │ BattleSubmitter (GameObject)│    ║
║   └─────────────────────────────┘    ║
║                                       ║
║ Battle Result Processor               ║
║   ┌─────────────────────────────┐    ║
║   │ BattleResultProcessor (GO)  │    ║
║   └─────────────────────────────┘    ║
╚═══════════════════════════════════════╝
```

---

Máš otázky k nastaveniu? 🤔

---

## ℹ️ Attack Component - Podrobnosti

### Prečo potrebujeme Attack.cs?

**Attack.cs** obsahuje všetky animácie zo singleplayer módu:
```csharp
// V Attack.cs (line 489-505)
public IEnumerator Punch(Kard attacker, Kard receiver, TMP_Text dialogText)
{
    yield return StartCoroutine(attackAnimations.PlayPunchAnimation(...));
    receiver.TakeDamage((attacker.strength / 3) - (receiver.defense / 3));
    // + sleep efekt logika
}
```

**V BattleResultProcessor.cs** používame len animačnú časť:
```csharp
// Line 95
yield return StartCoroutine(attackComponent.attackAnimations.PlayPunchAnimation(
    enemyCard.transform, 
    myCard.transform
));
```

### Kde nájsť Attack GameObject?

**Možnosti:**

1. **Ak máš Attack v Multiplayer scéne:**
   - Len drag GameObject s **Attack.cs** komponentom
   
2. **Ak Attack je len v Game.unity (singleplayer):**
   - Otvor `Game.unity`
   - Nájdi GameObject s **Attack** komponentom
   - Vytvor **Prefab**: drag GameObject z Hierarchy do Project folder
   - Otvor `Multiplayer.unity`
   - Drag prefab do scény ALEBO priamo do Inspector fieldu

3. **Ak Attack neexistuje v Multiplayer scéne:**
   ```
   Multiplayer.unity Hierarchy:
   └─ AttackManager (Create Empty)
      └─ Add Component: Attack.cs
   ```
   - Nastav referencie v Attack komponente:
     - `attackAnimations` → nájdi/vytvor AttackAnimations komponent

### Čo ak nemám AttackAnimations?

Ak vidíš error "AttackAnimations is null":
1. Nájdi GameObject s **AttackAnimations.cs**
2. Alebo vytvor nový:
   ```
   GameObject → Create Empty → "AttackAnimations"
   Add Component → AttackAnimations.cs
   ```
3. V **Attack.cs** Inspector nastav:
   - `attackAnimations` → drag AttackAnimations GameObject

### Alternatíva bez Attack.cs

Ak chceš vytvoriť úplne nový multiplayer animation system:

**TODO (budúcnosť):**
- Vytvor `MultiplayerAnimations.cs` 
- Implementuj `PlayPunchAnimation()` samostatne
- Zmeň `BattleResultProcessor.attackComponent` na `multiplayerAnimations`

Zatiaľ je jednoduchšie **reuse existing Attack.cs**! ♻️
