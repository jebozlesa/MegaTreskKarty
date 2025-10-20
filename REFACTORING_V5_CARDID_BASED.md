# V5 - CardID-Based Battle Results

## Čo sa zmenilo?

### Problém vo V4:
- `battleResult` obsahoval `player1Health/player2Health` (redundantné s `selectedCards`)
- Klient musel riešiť "som ja player1 alebo player2?" mapping
- Mätúce keď jeden hráč videl zlé HP (mapping issue)

### Riešenie vo V5:
- ✅ **HP iba v `selectedCards`** (single source of truth)
- ✅ **`battleResult` identifikuje karty cez `cardId`** (nie player1/player2!)
- ✅ **`battleResult` obsahuje len damage/effects** (nie HP!)
- ✅ **Žiadny player1/player2 mapping potrebný!**

---

## Server Changes (executeBattle.js)

### Stará V4 štruktúra:
```javascript
{
  firstAttacker: "player1",  // ❌ Mätúce
  player1Health: 21,          // ❌ Redundantné (máme v selectedCards)
  player2Health: 26,          // ❌ Redundantné
  player1Damage: 1,           // ✅ Potrebné
  player2Damage: 1,           // ✅ Potrebné
  player1DidSleep: false,
  player2DidSleep: false,
  player1Effects: [],
  player2Effects: []
}
```

### Nová V5 štruktúra:
```javascript
{
  firstAttacker: "05b120d1-...",  // ✅ cardId namiesto "player1"!
  
  attacks: {
    "05b120d1-...": {  // Henry Ford (cardId)
      damage: 1,
      didSleep: false,
      sleepDuration: 0,
      effects: []
    },
    "dc8f40b2-...": {  // Bruce Lee (cardId)
      damage: 1,
      didSleep: false,
      sleepDuration: 0,
      effects: []
    }
  }
}
```

### Server kód:
```javascript
// V5: Zostav result pomocou cardId
const result = {
  firstAttacker: firstCard.cardId,
  
  attacks: {
    [card1.cardId]: {
      damage: firstAttacker === 'player1' ? firstResult.damage : secondResult.damage,
      didSleep: firstAttacker === 'player1' ? secondResult.didSleep : firstResult.didSleep,
      sleepDuration: firstAttacker === 'player1' ? secondResult.sleepDuration : firstResult.sleepDuration,
      effects: card1.effects || []
    },
    [card2.cardId]: {
      damage: firstAttacker === 'player2' ? firstResult.damage : secondResult.damage,
      didSleep: firstAttacker === 'player2' ? secondResult.didSleep : firstResult.didSleep,
      sleepDuration: firstAttacker === 'player2' ? secondResult.sleepDuration : firstResult.sleepDuration,
      effects: card2.effects || []
    }
  }
};
```

---

## Unity Changes (BattleResultProcessor.cs)

### ProcessBattleResult:
```csharp
// ✅ V5: Parsuj attacks object (indexované podľa cardId)
var attacksJson = battleResult["attacks"].ToString();
var attacks = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
    .DeserializeObject<Dictionary<string, object>>(attacksJson);

string firstAttacker = battleResult["firstAttacker"].ToString();

// ✅ Nájdi damage pre moju kartu a nepriateľa pomocou cardId
string myCardId = myCard.GetComponent<GeneratedCard>()?.CardID ?? "";
string enemyCardId = enemyCard.GetComponent<GeneratedCard>()?.CardID ?? "";

var myAttackData = attacks[myCardId];
var enemyAttackData = attacks[enemyCardId];

int myDamage = int.Parse(myAttackData["damage"].ToString());
int enemyDamage = int.Parse(enemyAttackData["damage"].ToString());

// ✅ Spusti animácie
StartCoroutine(PlayBattleAnimationsAndRefresh(
    myCard, enemyCard, 
    firstAttacker,      // cardId kto útočil prvý
    myCardId, enemyCardId, 
    myDamage, enemyDamage
));
```

### PlayBattleAnimations:
```csharp
// V5: Používa cardId na určenie kto útočil prvý
bool iAttackedFirst = (firstAttacker == myCardId);

// ✅ Žiadny iAmPlayer1 mapping!
// ✅ Damage parametre už sú správne určené v ProcessBattleResult
```

---

## Výhody V5:

1. **Jednoduchosť**: Žiadny player1/player2 mapping
2. **Clarity**: cardId je jedinečné, nezávislé na pozícii hráča
3. **Single Source of Truth**: HP len v `selectedCards`
4. **Flexibilita**: Ľahko rozšíriteľné pre viac kariet (future: team battles)
5. **Debugging**: Jasné logy s cardId

---

## Deployment Checklist:

### Server:
- [x] Update `executeBattle.js` - V5 battleResult structure
- [ ] Deploy to Vercel (`git push` v `mega-tresk-server` repo)
- [ ] Test s `curl` na production

### Unity:
- [x] Update `BattleResultProcessor.cs` - V5 parsing
- [ ] Test v Unity Editor (Play Mode)
- [ ] Build Android APK
- [ ] Test na live server

---

## Testing:

### Expected Behavior:
1. Hráč1 (Ford) vs Hráč2 (Brude) submitli útoky
2. Server vráti:
   ```json
   {
     "firstAttacker": "dc8f40b2-...",  // Brude (vyššia speed: 9 vs 8)
     "attacks": {
       "05b120d1-...": { "damage": 1, ... },  // Ford dostal 1 damage
       "dc8f40b2-...": { "damage": 1, ... }   // Brude dostal 1 damage
     }
   }
   ```
3. Obaja hráči vidia správne animácie:
   - Brude útočí prvý (speed 9)
   - Ford útočí druhý (speed 8)
   - HP sa updatuje postupne počas animácií
4. Po animáciách:
   - `RefreshCardsFromServer()` načíta finálne HP z `selectedCards`
   - Ford: 26/27 HP ✅
   - Brude: 21/22 HP ✅
   - Obaja hráči vidia rovnaké hodnoty! ✅

---

**Version**: V5  
**Date**: 2025-10-14  
**Branch**: Multiplayer
