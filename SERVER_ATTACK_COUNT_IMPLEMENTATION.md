# Server-Side Attack Count Calculation - Implementation Guide

## 📋 Prehľad
Táto funkcia vypočíta počty útokov karty na základe jej štatistík. Všetky výpočty sa vykonávajú na serveri, aby sa zabránilo podvádzaniu.

## 🎯 Serverová Funkcia: `calculateAttackCounts`

### Vstupné Parametre (args):
```javascript
{
    attack1: number,    // ID prvého útoku (1-123)
    attack2: number,    // ID druhého útoku (1-123)
    attack3: number,    // ID tretieho útoku (1-123)
    attack4: number,    // ID štvrtého útoku (1-123)
    strength: number,   // Sila karty
    defense: number,    // Obrana karty
    attack: number,     // Útok karty
    knowledge: number,  // Vedomosti karty
    charisma: number,   // Charisma karty
    speed: number       // Rýchlosť karty (zatiaľ nepoužívaná)
}
```

### Výstup:
```javascript
{
    count1: number,     // Vypočítaný count pre attack1
    count2: number,     // Vypočítaný count pre attack2
    count3: number,     // Vypočítaný count pre attack3
    count4: number      // Vypočítaný count pre attack4
}
```

---

## 📝 PlayFab CloudScript Implementácia

### Kompletná funkcia:

```javascript
handlers.calculateAttackCounts = function (args, context) {
    // Validácia vstupov
    if (!args) {
        return { 
            error: "Missing arguments",
            count1: 0, count2: 0, count3: 0, count4: 0 
        };
    }
    
    // Extrakcia parametrov
    var attack1 = args.attack1 || 0;
    var attack2 = args.attack2 || 0;
    var attack3 = args.attack3 || 0;
    var attack4 = args.attack4 || 0;
    
    var strength = args.strength || 0;
    var defense = args.defense || 0;
    var attack = args.attack || 0;
    var knowledge = args.knowledge || 0;
    var charisma = args.charisma || 0;
    var speed = args.speed || 0;
    
    // Vypočítaj count pre každý útok
    var count1 = calculateAttackCount(attack1, strength, defense, attack, knowledge, charisma, speed);
    var count2 = calculateAttackCount(attack2, strength, defense, attack, knowledge, charisma, speed);
    var count3 = calculateAttackCount(attack3, strength, defense, attack, knowledge, charisma, speed);
    var count4 = calculateAttackCount(attack4, strength, defense, attack, knowledge, charisma, speed);
    
    // Logovanie pre debugging
    log.info("Attack counts calculated", {
        attackIds: [attack1, attack2, attack3, attack4],
        counts: [count1, count2, count3, count4]
    });
    
    return {
        count1: count1,
        count2: count2,
        count3: count3,
        count4: count4
    };
};

// Pomocná funkcia pre výpočet jedného útoku
function calculateAttackCount(attackId, strength, defense, attack, knowledge, charisma, speed) {
    var count = 0;
    
    switch (attackId) {
        case 1: // Punch
            count = 20 + strength;
            break;
        case 2: // Kick
            count = 20 + strength;
            break;
        case 3: // Heal
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 4: // Forgiveness
            count = 3 + Math.floor(charisma / 3);
            break;
        case 5: // Crusade
            count = 10 + charisma;
            break;
        case 6: // Water To Wine
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 7: // Car Hit
            count = 3 + Math.floor(charisma / 3);
            break;
        case 8: // Monkey Wrench
            count = 10 + strength;
            break;
        case 9: // Radiation
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 10: // Scratch
            count = 20 + strength;
            break;
        case 11: // Scientific Lecture
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 12: // Chi Sau
            count = 5 + Math.floor(defense / 2);
            break;
        case 13: // One Inch Punch
            count = 5 + Math.floor(attack / 2);
            break;
        case 14: // Up In Smoke
            count = 3 + Math.floor(charisma / 3);
            break;
        case 15: // Sing
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 16: // Revolver
            count = 6;
            break;
        case 17: // Artillery Regiment
            count = 3 + Math.floor(charisma / 3);
            break;
        case 18: // Bloodthirst
            count = 5 + Math.floor(attack / 2);
            break;
        case 19: // Sword
            count = 10 + strength;
            break;
        case 20: // Pike
            count = 10 + strength;
            break;
        case 21: // Terrify
            count = 5 + Math.floor(defense / 2);
            break;
        case 22: // Drink Wine
            count = 3 + Math.floor(strength / 3);
            break;
        case 23: // Flaming Gun
            count = 5 + Math.floor(attack / 2);
            break;
        case 24: // Cleaver
            count = 10 + strength;
            break;
        case 25: // Pan
            count = 10 + strength;
            break;
        case 26: // Boost
            count = 3 + Math.floor(charisma / 3);
            break;
        case 27: // Temptation
            count = 5 + Math.floor(charisma / 2);
            break;
        case 28: // Shamshir
            count = 10 + strength;
            break;
        case 29: // Diplomacy
            count = 5 + Math.floor(charisma / 2);
            break;
        case 30: // Siege
            count = 3 + Math.floor(charisma / 3);
            break;
        case 31: // 36 Stratagems
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 32: // Tomahawk
            count = 10 + strength;
            break;
        case 33: // Peace Pipe
            count = 3 + Math.floor(charisma / 3);
            break;
        case 34: // Recurve Bow
            count = 3 + Math.floor(attack / 3);
            break;
        case 35: // Fury
            count = 5 + Math.floor(attack / 2);
            break;
        case 36: // Guerilla
            count = 5 + Math.floor(defense / 2);
            break;
        case 37: // Famine
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 38: // Marxism
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 39: // Tesla Coil
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 40: // Wireless Charger
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 41: // Experiment
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 42: // Tommy Gun
            count = 5 + Math.floor(attack / 2);
            break;
        case 43: // Tie Up
            count = 3 + Math.floor(defense / 3);
            break;
        case 44: // Corruption
            count = 3 + Math.floor(charisma / 3);
            break;
        case 45: // Colt 1911
            count = 7;
            break;
        case 46: // Mortar
            count = 3 + Math.floor(attack / 3);
            break;
        case 47: // Great Army
            count = 5 + Math.floor(charisma / 2);
            break;
        case 48: // Scorched Earth
            count = 3 + Math.floor(attack / 3);
            break;
        case 49: // Double Envelopment
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 50: // Continental Blockade
            count = 5 + Math.floor(charisma / 2);
            break;
        case 51: // Depression
            count = 3 + Math.floor(charisma / 3);
            break;
        case 52: // Self Isolation
            count = 3 + Math.floor(defense / 3);
            break;
        case 53: // Knife
            count = 10 + strength;
            break;
        case 54: // Autoportrait
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 55: // Gravity Pull
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 56: // Kamikaze
            count = 5 + Math.floor(attack / 2);
            break;
        case 57: // Take-Off
            count = 3 + Math.floor(speed / 3);
            break;
        case 58: // Air Strike
            count = 5 + Math.floor(attack / 2);
            break;
        case 59: // Justice Crusade
            count = 10 + charisma;
            break;
        case 60: // Rapier
            count = 10 + strength;
            break;
        case 61: // Expeditionary Assault
            count = 5 + Math.floor(attack / 2);
            break;
        case 62: // Culverin
            count = 3 + Math.floor(attack / 3);
            break;
        case 63: // Fire Ship
            count = 5 + Math.floor(attack / 2);
            break;
        case 64: // Handcuff Escape
            count = 5 + Math.floor(defense / 2);
            break;
        case 65: // Illusion
            count = 3 + Math.floor(charisma / 3);
            break;
        case 66: // Carcano M91
            count = 7;
            break;
        case 67: // Winchester
            count = 7;
            break;
        case 68: // Ambush
            count = 5 + Math.floor(defense / 2);
            break;
        case 69: // Jupiter-C
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 70: // V-2
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 71: // Battle Cry
            count = 5 + Math.floor(charisma / 2);
            break;
        case 72: // Revelation
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 73: // Standard
            count = 5 + Math.floor(charisma / 2);
            break;
        case 74: // Pen
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 75: // Iambic Pentameter
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 76: // Ghost
            count = 3 + Math.floor(charisma / 3);
            break;
        case 77: // Buffalo Horns
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 78: // Iklwa
            count = 10 + strength;
            break;
        case 79: // Iwisa
            count = 10 + strength;
            break;
        case 80: // Niten Ichi-ryū
            count = 5 + Math.floor(attack / 2);
            break;
        case 81: // Tessenjutsu
            count = 5 + Math.floor(defense / 2);
            break;
        case 82: // Iaijutsu
            count = 5 + Math.floor(attack / 2);
            break;
        case 83: // Katana
            count = 10 + strength;
            break;
        case 84: // Nodachi
            count = 10 + strength;
            break;
        case 85: // Yumi
            count = 3 + Math.floor(attack / 3);
            break;
        case 86: // Jujutsu
            count = 5 + Math.floor(defense / 2);
            break;
        case 87: // Espionage
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 88: // Sabre
            count = 10 + strength;
            break;
        case 89: // Gamble
            count = 3 + Math.floor(charisma / 3);
            break;
        case 90: // Philosophy
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 91: // Calm
            count = 3 + Math.floor(defense / 3);
            break;
        case 92: // Honesty
            count = 3 + Math.floor(charisma / 3);
            break;
        case 93: // Valaska
            count = 10 + strength;
            break;
        case 94: // Moonshine
            count = 3 + Math.floor(charisma / 3);
            break;
        case 95: // Outlaw Band
            count = 5 + Math.floor(charisma / 2);
            break;
        case 96: // Flintlock Pistol
            count = 6;
            break;
        case 97: // Passive Resistance
            count = 5 + Math.floor(defense / 2);
            break;
        case 98: // Hunger Strike
            count = 3 + Math.floor(defense / 3);
            break;
        case 99: // Gladius
            count = 10 + strength;
            break;
        case 100: // Shield Bash
            count = 5 + Math.floor(defense / 2);
            break;
        case 101: // Yperit
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 102: // Blitzkrieg
            count = 5 + Math.floor(attack / 2);
            break;
        case 103: // Propaganda
            count = 5 + Math.floor(charisma / 2);
            break;
        case 104: // Retiarius
            count = 5 + Math.floor(defense / 2);
            break;
        case 105: // Shuriken
            count = 3 + Math.floor(attack / 3);
            break;
        case 106: // Kusarigama
            count = 10 + strength;
            break;
        case 107: // Ninjutsu
            count = 5 + Math.floor(knowledge / 2);
            break;
        case 108: // Oriental Spice
            count = 3 + Math.floor(charisma / 3);
            break;
        case 109: // Arquebus
            count = 7;
            break;
        case 110: // Pirate Raid
            count = 5 + Math.floor(attack / 2);
            break;
        case 111: // Axe
            count = 10 + strength;
            break;
        case 112: // Jaguar Warriors
            count = 5 + Math.floor(attack / 2);
            break;
        case 113: // Atlatl
            count = 3 + Math.floor(attack / 3);
            break;
        case 114: // Macuahuitl
            count = 10 + strength;
            break;
        case 115: // Cubism
            count = 3 + Math.floor(knowledge / 3);
            break;
        case 116: // La Cosa Nostra
            count = 5 + Math.floor(charisma / 2);
            break;
        case 117: // Act a fool
            count = 3 + Math.floor(charisma / 3);
            break;
        case 118: // Football
            count = 10 + strength;
            break;
        case 119: // Bicycle Kick
            count = 5 + Math.floor(attack / 2);
            break;
        case 120: // World Champion
            count = 5 + Math.floor(charisma / 2);
            break;
        case 121: // Shaolin Soccer
            count = 5 + Math.floor(attack / 2);
            break;
        case 123: // Curse
            count = 3 + Math.floor(charisma / 3);
            break;
        default:
            count = 0;
            break;
    }
    
    return count;
}
```

---

## 🚀 Inštrukcie pre nasadenie

### 1. Otvorte PlayFab Dashboard
- Prihláste sa na [PlayFab](https://playfab.com/)
- Vyberte váš Title

### 2. Prejdite do CloudScript
- V ľavom menu: **Automation** → **CloudScript**
- Vyberte **Azure Functions** alebo **Legacy CloudScript** (podľa toho, čo používate)

### 3. Pridajte funkciu
- Skopírujte celý kód vyššie
- Vložte ho do vášho CloudScript súboru
- Kliknite **Save and Deploy**

### 4. Otestujte funkciu
V **API Explorer** v PlayFab:
```json
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

Očakávaný výsledok:
```json
{
  "count1": 70,  // 20 + 50 (Punch)
  "count2": 23,  // 3 + 60/3 (Heal)
  "count3": 55,  // 10 + 45 (Crusade)
  "count4": 70   // 20 + 50 (Scratch)
}
```

---

## 📊 Vzorce útokov - Quick Reference

| Štatistika | Použitie |
|-----------|----------|
| **strength** | Fyzické útoky (Punch, Kick, Sword, atď.) |
| **defense** | Defenzívne schopnosti (Chi Sau, Shield Bash) |
| **attack** | Ofenzívne útoky (One-inch Punch, Blitzkrieg) |
| **knowledge** | Intelektuálne útoky (Heal, Scientific Lecture) |
| **charisma** | Sociálne útoky (Forgiveness, Diplomacy) |
| **speed** | Rýchlostné útoky (Take-Off) |

---

## ⚠️ Dôležité poznámky

1. **JavaScript Math.floor()** - Použite `Math.floor()` namiesto `int()` (JavaScript nemá int)
2. **Validácia** - Funkcia kontroluje null/undefined hodnoty
3. **Logging** - Všetky výpočty sú logované pre debugging
4. **Performance** - Funkcia je optimalizovaná pre rýchle výpočty
5. **Security** - Všetky výpočty na serveri = anti-cheat

---

## 🔧 Alternatívna implementácia (Azure Functions)

Ak používate Azure Functions namiesto Legacy CloudScript:

```javascript
module.exports = async function (context, req) {
    const args = req.body.FunctionArgument;
    
    // ... rovnaký kód ako vyššie ...
    
    context.res = {
        body: {
            count1: count1,
            count2: count2,
            count3: count3,
            count4: count4
        }
    };
};
```

---

## ✅ Checklist

- [ ] Skopírovaný kód do PlayFab CloudScript
- [ ] Deployed funkcia
- [ ] Otestovaná cez API Explorer
- [ ] Funguje v Unity klientovi
- [ ] Logovanie funguje správne
