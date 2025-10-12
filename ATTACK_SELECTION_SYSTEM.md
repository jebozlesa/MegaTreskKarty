# Attack Selection System - Multiplayer

## 📋 Prehľad

Systém pre výber útokov v multiplayerovom režime funguje podobne ako v singleplayeri, ale s podporou pre server-side validáciu a synchronizáciu.

---

## 🎯 Nový komponent: AttackSelectionManager

### Zodpovednosti:
1. **Správa UI** - Enable/disable tlačidiel útokov
2. **Výber útoku** - Spracovanie kliknutia na tlačidlá
3. **Zobrazenie popisu** - Zobrazenie textu útoku v dialogu
4. **Potvrdenie** - Príprava dát pre odoslanie na server

---

## 🔄 Workflow

```
1. Hráč pustí kartu na board
   ↓
2. OnCardDropped() v FightSystemMultiplayer
   ↓
3. LoadAttackNames(card) - zobrazí názvy
   ↓
4. LoadAttackCounts(card) - načíta counts zo servera
   ↓
5. Po úspešnom načítaní: PrepareAttackSelection()
   ↓
6. Hráč klikne na tlačidlo útoku (1-4)
   ↓
7. OnAttackButtonClicked() - uloží výber
   ↓
8. DisplayAttackMultiplayer() - zobrazí text v dialogu
   ↓
9. Hráč klikne "Confirm Attack"
   ↓
10. OnConfirmAttackClicked()
   ↓
11. ConfirmAttackSelection() - vytvorí SelectedAttackData
   ↓
12. OnAttackConfirmed() v FightSystemMultiplayer
   ↓
13. TODO: Odoslanie na server
```

---

## 📝 Kód flow

### 1. Príprava výberu útoku

```csharp
// Po načítaní attack counts
attackSelectionManager.PrepareAttackSelection(card, attackCountsResult);
```

**Čo sa stane:**
- Nastaví `currentCard` a `currentAttackCounts`
- Zobrazí "Choose an attack" v dialógu
- Enable/disable tlačidlá podľa toho, či majú count > 0
- Disable confirm button

---

### 2. Kliknutie na útok

```csharp
// Hráč klikne na button1 (útok 1)
OnAttackButtonClicked(1);
```

**Čo sa stane:**
- Skontroluje, či je útok dostupný (attackId > 0 && attackCount > 0)
- Uloží `selectedAttackType = 1`
- Zavolá `DisplayAttackMultiplayer()` → zobrazí text v dialogu
- Enable confirm button

**Príklad textu v dialógu:**
```
Punch chosen
Launch here !!! 70 remaining
```

---

### 3. Potvrdenie útoku

```csharp
// Hráč klikne "Confirm Attack"
OnConfirmAttackClicked();
```

**Čo sa stane:**
- Vytvorí `SelectedAttackData`:
  ```csharp
  {
      attackType: 1,        // Ktoré tlačidlo (1-4)
      attackId: 1,          // ID útoku (1-123) - Punch
      attackCount: 70,      // Damage/heal
      cardId: "card123"     // ID karty
  }
  ```
- Zavolá `fightSystem.OnAttackConfirmed(attackData)`
- Disable všetky tlačidlá
- Zobrazí "Waiting for opponent..."

---

## 🎨 UI Elementy

### Potrebné komponenty v scéne:

1. **Attack Buttons** (4x Button)
   - `button1` - prvý útok
   - `button2` - druhý útok
   - `button3` - tretí útok
   - `button4` - štvrtý útok

2. **Dialog Text** (TMP_Text)
   - Zobrazuje aktuálny stav/výber

3. **Confirm Button** (Button)
   - "Launch Attack" / "Confirm" tlačidlo

---

## ⚙️ Setup v Unity

### 1. Pridaj komponent

Na GameObject s `FightSystemMultiplayer`:
```
Add Component → AttackSelectionManager
```

### 2. Priraď referencie

**AttackSelectionManager:**
- `dialogText` → TMP_Text pre hlavný dialog
- `confirmButton` → Button pre potvrdenie
- `button1` → Button pre útok 1
- `button2` → Button pre útok 2
- `button3` → Button pre útok 3
- `button4` → Button pre útok 4
- `attackDescriptions` → AttackDescriptions komponent
- `fightSystem` → FightSystemMultiplayer komponent

**FightSystemMultiplayer:**
- `attackSelectionManager` → AttackSelectionManager komponent

### 3. Pripoj Buttons v UI

V Unity editore:
- Nezabudni **ODSTRÁNIŤ** staré onClick handlery z tlačidiel
- Nové handlery sa pridávajú automaticky v `Start()` metóde

---

## 📊 Dátové štruktúry

### SelectedAttackData

```csharp
public class SelectedAttackData
{
    public int attackType;      // 1-4 (ktoré tlačidlo)
    public int attackId;        // 1-123 (ID útoku)
    public int attackCount;     // Damage/heal hodnota
    public string cardId;       // ID karty
}
```

**Použitie:**
- Posielanie na server
- Validácia výberu
- Synchronizácia medzi hráčmi

---

## 🔍 Metódy AttackSelectionManager

### Verejné metódy:

```csharp
// Priprav UI pre výber útoku
void PrepareAttackSelection(Kard card, AttackCountsResult attackCounts)

// Resetuj výber
void ResetSelection()

// Získaj aktuálne vybraný útok
int GetSelectedAttackType()
```

### Privátne metódy:

```csharp
// Handler pre kliknutie na útok
void OnAttackButtonClicked(int attackType)

// Handler pre potvrdenie
void OnConfirmAttackClicked()

// Potvrdenie a príprava dát
void ConfirmAttackSelection()

// Pomocné metódy
int GetAttackId(int attackType)
int GetAttackCount(int attackType)
bool CanSelectAttack(int attackType)
void SetAttackButtonsInteractable(AttackCountsResult counts)
void SetConfirmButtonState(bool enabled)
void DisableAttackSelection()
```

---

## 🆕 Nová metóda v AttackDescriptions

### DisplayAttackMultiplayer()

```csharp
public void DisplayAttackMultiplayer(
    Kard attacker, 
    int attackType, 
    TMP_Text dialogText, 
    int attackCount
)
```

**Parametre:**
- `attacker` - Karta, ktorá útočí
- `attackType` - 1-4 (ktoré tlačidlo)
- `dialogText` - Text pole pre zobrazenie
- `attackCount` - Count zo servera (nie z karty)

**Výstup:**
```
"Punch chosen
Launch here !!! 70 remaining"
```

**Rozdiel od singleplayeru:**
- V singleplayeri: `attacker.attackCount[attackType]` (lokálne)
- V multiplayeri: `attackCount` parameter (zo servera)

---

## 🎮 Príklad použitia

```csharp
// V FightSystemMultiplayer po načítaní counts
attackCountLoader.LoadAttackCounts(card, result => 
{
    if (result != null)
    {
        // Priprav výber útoku
        attackSelectionManager.PrepareAttackSelection(card, result);
    }
});

// Po potvrdení útoku
public void OnAttackConfirmed(SelectedAttackData attackData)
{
    Debug.Log($"Attack: {attackData.attackId}, Count: {attackData.attackCount}");
    
    // TODO: Odošli na server
    serverFunctionsManager.SubmitAttack(roomCode, playerId, attackData, ...);
}
```

---

## ✅ Checklist

- [ ] Pridaný `AttackSelectionManager` komponent
- [ ] Priraďené všetky UI referencie
- [ ] Pridaná referencia do `FightSystemMultiplayer`
- [ ] Odskúšané kliknutie na útoky
- [ ] Funguje zobrazenie textu v dialógu
- [ ] Funguje enable/disable tlačidiel
- [ ] Confirm button sa aktivuje po výbere
- [ ] Po potvrdení sa disable UI

---

## 🚀 Ďalšie kroky

1. ✅ **Zobrazenie názvov** - Hotovo
2. ✅ **Výpočet counts** - Hotovo (server-side)
3. ✅ **Výber útoku** - Hotovo (tento krok)
4. ⏳ **Odoslanie na server** - Nasledujúci krok
5. ⏳ **Synchronizácia hráčov** - Ďalší krok
6. ⏳ **Vykonanie útoku** - Posledný krok

---

## 🐛 Debug tipy

### Console logs na sledovanie:

```
[AttackSelectionManager] Attack selection prepared for card: CardName
[AttackSelectionManager] Attack 1 selected
[AttackSelectionManager] Confirming attack 1
[FightSystemMultiplayer] Attack confirmed: Type=1, ID=1, Count=70
```

### Časté problémy:

1. **Tlačidlá nereagujú**
   - Skontroluj, či sú priraďené v Inspectore
   - Skontroluj, či nemajú staré onClick handlery

2. **Confirm button je vždy disabled**
   - Skontroluj referenciu v Inspectore
   - Skontroluj, či sa volá `SetConfirmButtonState(true)`

3. **Text sa nezobrazí**
   - Skontroluj `dialogText` referenciu
   - Skontroluj `attackDescriptions` referenciu

---

Made with 🎯 by Copilot
