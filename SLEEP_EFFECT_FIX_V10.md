# Sleep Effect Fix V10 - Oprava effectApplied interpretácie

## 🐛 Problém
Sleep effect sa zobrazoval na **nesprávnej karte** - keď Bruce Lee útočil Supera, Sleep ikonka a KO animácia sa zobrazila na **Brucovi** namiesto **Supera**.

## 🔍 Root Cause
**Nesprávne pochopenie server response štruktúry**

Unity mal opačnú interpretáciu `effectApplied` fieldu:
- **Server vracia**: `attacks[cardId].effectApplied` = effect ktorý **TÁTO KARTA aplikovala** na oponenta
- **Unity interpretoval**: effectApplied = effect ktorý karta **DOSTALA**

Výsledok:
```csharp
// CHYBA: Keď JA útočím, použil som enemyEffectApplied
if (enemyEffectApplied != null) {  // ❌ Enemy neútočí!
    DisplayEffectIcon(enemyCard, enemyEffectApplied, false);
}

// SPRÁVNE: Keď JA útočím, musím použiť myEffectApplied
if (myEffectApplied != null) {  // ✅ JA aplikujem effect NA enemy
    DisplayEffectIcon(enemyCard, myEffectApplied, false);
}
```

## ✅ Riešenie

### 1. Unity - BattleResultProcessor.cs

**Debug logy (riadky 126, 133)**:
```csharp
// BOLO:
Debug.LogWarning($"🎭 [EFFECT] MY card received effect: ...");

// TERAZ:
Debug.LogWarning($"🎭 [EFFECT] MY card APPLIED effect to enemy: ...");
```

**Attack Site 1 - Ja útočím prvý (riadok 352)**:
```csharp
// BOLO:
if (enemyEffectApplied != null) {  // ❌
    DisplayEffectIcon(enemyCard, enemyEffectApplied, false);
}

// TERAZ:
if (myEffectApplied != null) {  // ✅
    DisplayEffectIcon(enemyCard, myEffectApplied, false);
}
```

**Attack Site 4** už bol správny, len aktualizovaný komentár.

**PlayWakeUpAnimation (riadok 641)** - pridané odstránenie Sleep ikonky:
```csharp
string sleepEffectName = GetEffectName(3); // 3 = Sleep
if (!string.IsNullOrEmpty(sleepEffectName))
{
    yield return StartCoroutine(card.RemoveEffectIcon(sleepEffectName));
    Debug.LogWarning($"⏰ [WAKE_UP] Removed {sleepEffectName} icon from {card.cardName}");
}
```

### 2. Server - executeBattle.js

Pridané `wokeUp` flag do **VŠETKÝCH** scenárov:

**Case 1: Both players blocked (riadok 168, 177)**:
```javascript
wokeUp: card1SleepCheck.wokeUp,  // ✅ Pridané
wokeUp: card2SleepCheck.wokeUp,  // ✅ Pridané
```

**Case 2: First attacker blocked (riadok 204)**:
```javascript
wokeUp: card1SleepCheck.wokeUp,  // ✅ Pridané
```

**Case 3: Second attacker blocked (riadok 239)**:
```javascript
wokeUp: card2SleepCheck.wokeUp,  // ✅ Pridané
```

## 📊 Správne mapovanie (4 Attack Sites)

| Site | Scenár | Damage | Effect Icon |
|------|--------|--------|-------------|
| 1 | JA útočím prvý | `enemyDamage` | `myEffectApplied` → enemy |
| 2 | Enemy kontratuje | `myDamage` | `enemyEffectApplied` → ja |
| 3 | Enemy útočí prvý | `myDamage` | `enemyEffectApplied` → ja |
| 4 | JA kontratujem | `enemyDamage` | `myEffectApplied` → enemy |

## 🎯 Výsledok

✅ Sleep sa zobrazí na **správnej karte** (na defenderovi, nie útočníkovi)  
✅ KO animácia + Sleep ikonka sa zobrazia správne  
✅ HP drop je okamžitý (počas animácie)  
✅ Sleep ikonka **zmizne po prebudení**  
✅ Všetky 4 attack scenáre fungujú správne

## 📝 Zmeny v súboroch

### Unity (Client)
- `Assets/Scripts/Multiplayer/BattleResultProcessor.cs`
  - Oprava debug logov (2x)
  - Oprava Attack Site 1 (myEffectApplied → enemy)
  - Pridanie sleep icon cleanup v PlayWakeUpAnimation

### Server
- `api/executeBattle.js`
  - Pridanie wokeUp flag do Case 1, 2, 3

### Dokumentácia
- `COPILOT_MULTIPLAYER_BATTLE_INSTRUCTIONS.txt` - nová komplexná dokumentácia
- `SLEEP_EFFECT_FIX_V10.md` - tento súbor

## 🧹 Čistenie
Nasledujúce dočasné súbory môžu byť odstránené (už zastarané):
- `COPILOT_CHAT_INSTRUCTIONS.txt` - zahrnuté v novej dokumentácii
- `COPILOT_CHAT_INSTRUCTIONS_ATTACK_3.txt` - zahrnuté v novej dokumentácii
- `SERVER_INSTRUCTIONS_*.md` - všetky implementované
- `SERVER_COPILOT_*.md` - zahrnuté v novej dokumentácii
- `SLEEP_*_IMPLEMENTATION.md` - nahradené týmto súborom
