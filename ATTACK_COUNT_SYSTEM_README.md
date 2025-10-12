# Attack Count System - Kompletný Prehľad

## ✅ Implementované komponenty

### 🎮 Unity (Client-Side)

#### 1. **AttackCountLoader.cs** (Nový súbor)
**Umiestnenie:** `Assets/Scripts/Multiplayer/AttackCountLoader.cs`

**Zodpovednosti:**
- Posielanie požiadaviek na server pre výpočet attack counts
- Prijímanie a parsovanie výsledkov zo servera
- Zobrazovanie counts v UI (button1CountText, button2CountText, atď.)
- Error handling a validácia

**Verejné metódy:**
```csharp
void LoadAttackCounts(Kard card, Action<AttackCountsResult> onComplete = null)
void ClearAttackCounts()
```

**Referencie potrebné v Unity Inspectore:**
- `button1CountText` - TMP_Text pre prvý útok
- `button2CountText` - TMP_Text pre druhý útok
- `button3CountText` - TMP_Text pre tretí útok
- `button4CountText` - TMP_Text pre štvrtý útok
- `serverFunctionsManager` - ServerFunctionsManager komponent

---

#### 2. **ServerFunctionsManager.cs** (Aktualizovaný)
**Umiestnenie:** `Assets/Scripts/Networking/ServerFunctionsManager.cs`

**Nová metóda:**
```csharp
void CalculateAttackCounts(CardStatsForCalculation cardStats, Action<ExecuteFunctionResult> callback)
```

**Čo robí:**
- Pripravuje parametre pre server call
- Volá PlayFab CloudScript funkciu `calculateAttackCounts`
- Vracia výsledok cez callback

---

#### 3. **FightSystemMultiplayer.cs** (Aktualizovaný)
**Umiestnenie:** `Assets/Scripts/Multiplayer/FightSystemMultiplayer.cs`

**Pridané:**
```csharp
public AttackCountLoader attackCountLoader;

public void LoadAttackCounts(Kard card)
```

**Použitie:**
V `OnCardDropped()` sa teraz volá:
```csharp
LoadAttackNames(card);      // Zobrazí názvy
LoadAttackCounts(card);     // Vypočíta a zobrazí počty
```

---

#### 4. **Nové dátové triedy**

```csharp
// Vstupné dáta pre server
public class CardStatsForCalculation
{
    public int attack1, attack2, attack3, attack4;
    public int strength, defense, attack;
    public int knowledge, charisma, speed;
}

// Výstup zo servera
public class AttackCountsResult
{
    public int count1, count2, count3, count4;
}
```

---

## 🖥️ Server-Side (PlayFab CloudScript)

### Serverová funkcia: `calculateAttackCounts`

**Umiestnenie:** PlayFab Dashboard → Automation → CloudScript

**Vstup:**
```javascript
{
    attack1: 1,      // ID útoku (1-123)
    attack2: 3,
    attack3: 5,
    attack4: 10,
    strength: 50,    // Štatistiky karty
    defense: 30,
    attack: 40,
    knowledge: 60,
    charisma: 45,
    speed: 20
}
```

**Výstup:**
```javascript
{
    count1: 70,     // Vypočítaný damage/heal pre attack1
    count2: 23,     // Vypočítaný damage/heal pre attack2
    count3: 55,     // atď.
    count4: 70
}
```

**Implementácia:**
Pozri `SERVER_ATTACK_COUNT_IMPLEMENTATION.md` pre kompletný kód

---

## 📋 Setup Kroky v Unity

### 1. Pridaj komponenty na scénu
Na GameObject s `FightSystemMultiplayer`:
1. Pridaj komponent `AttackCountLoader`
2. Pridaj komponent `AttackNamesLoader` (ak ešte nemáš)

### 2. Priraď referencie v Inspectore

**AttackCountLoader:**
- `button1CountText` → UI Text pre count prvého útoku
- `button2CountText` → UI Text pre count druhého útoku
- `button3CountText` → UI Text pre count tretieho útoku
- `button4CountText` → UI Text pre count štvrtého útoku
- `serverFunctionsManager` → ServerFunctionsManager komponent

**FightSystemMultiplayer:**
- `attackCountLoader` → AttackCountLoader komponent
- `attackNamesLoader` → AttackNamesLoader komponent (už je)

### 3. UI Setup
Uisti sa, že máš v scéne:
- ✅ TMP_Text pre názvy útokov (button1Text, button2Text, ...)
- ✅ TMP_Text pre počty útokov (button1CountText, button2CountText, ...)
- ✅ Button komponenty (button1, button2, button3, button4)

---

## 🔄 Tok dát

```
1. Hráč pustí kartu na board
   ↓
2. OnCardDropped(card) sa zavolá
   ↓
3. LoadAttackNames(card) - zobrazí názvy útokov
   ↓
4. LoadAttackCounts(card)
   ↓
5. AttackCountLoader.LoadAttackCounts()
   ↓
6. ServerFunctionsManager.CalculateAttackCounts()
   ↓
7. PlayFab CloudScript API call
   ↓
8. Server: calculateAttackCounts handler
   ↓
9. calculateAttackCount() pre každý útok
   ↓
10. Návrat výsledku do Unity
   ↓
11. Parsovanie JSON do AttackCountsResult
   ↓
12. DisplayAttackCounts() - zobrazenie v UI
```

---

## 🧪 Testovanie

### V Unity Editor:
1. Spusti multiplayerový zápas
2. Vyber kartu a presuň ju na board
3. Mal by si vidieť:
   - Názvy útokov (napr. "Punch", "Heal")
   - Počty útokov (napr. "70", "23")

### V PlayFab API Explorer:
```json
POST /CloudScript/ExecuteFunction
{
  "FunctionName": "calculateAttackCounts",
  "FunctionParameter": {
    "attack1": 1,
    "attack2": 3,
    "attack3": 5,
    "attack4": 10,
    "strength": 50,
    "defense": 30,
    "attack": 40,
    "knowledge": 60,
    "charisma": 45,
    "speed": 20
  }
}
```

---

## 🐛 Debugging

### Unity Console logs:
```
[AttackCountLoader] Loading attack counts for card: CardName
[AttackCountLoader] Received attack counts: 70, 23, 55, 70
```

### PlayFab logs:
```javascript
log.info("Attack counts calculated", {
    attackIds: [1, 3, 5, 10],
    counts: [70, 23, 55, 70]
});
```

### Časté problémy:
- **Counts sa nezobrazujú:** Skontroluj referencie v Inspectore
- **Server error:** Uisti sa, že CloudScript je deployed
- **Nesprávne counts:** Skontroluj vzorce v `calculateAttackCount()`

---

## 🔐 Bezpečnosť

**Prečo server-side?**
- ✅ Anti-cheat - hráči nemôžu manipulovať výpočty
- ✅ Jednotnosť - všetci hráči vidia rovnaké hodnoty
- ✅ Centralizácia - vzorce na jednom mieste
- ✅ Validácia - server kontroluje vstupné hodnoty

---

## 🎯 Ďalšie kroky

1. **Deploy serverovú funkciu** - Pozri `SERVER_ATTACK_COUNT_IMPLEMENTATION.md`
2. **Testuj v Unity** - Vyskúšaj s rôznymi kartami
3. **Optimalizuj UI** - Pridaj animácie, farby
4. **Extend** - Môžeš pridať caching, ak je potrebné

---

## 📚 Súvisiace súbory

- `AttackCountLoader.cs` - Klientska logika
- `AttackNamesLoader.cs` - Názvy útokov
- `ServerFunctionsManager.cs` - Server komunikácia
- `FightSystemMultiplayer.cs` - Hlavný fight system
- `AttackDescriptions.cs` - Definície útokov
- `SERVER_ATTACK_COUNT_IMPLEMENTATION.md` - Server implementácia

---

Made with ❤️ by Copilot
