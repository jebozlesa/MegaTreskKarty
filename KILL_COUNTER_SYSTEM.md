# Kill Counter System (V7)

## 🎯 Overview

**Win Condition:** Prvý hráč ktorý zabije **3 enemy cards** vyhráva!

Implementované: 2025-11-04  
Visual System: Zelené → Červené štvorčeky  
Auto scene transition: 2 sekundy po výhre/prehre

---

## 🏗️ Architecture

### Components:

```
MultiplayerKillCounterManager.cs  → Central kill tracking & win detection
└─ Manages: playerKillCount, enemyKillCount
└─ Checks: Win condition (3 kills)
└─ Actions: Display message, disable buttons, load scene

KillCounterUI.cs  → Individual kill indicator square
└─ States: Alive (green), Dead (red)
└─ Visual: Image component color change
└─ Per square: One kill indicator
```

### UI Structure:

```
Canvas
├─ PlayerKillCounters (3 squares)
│  ├─ Image (1) - KillCounterUI.cs → Green = alive, Red = dead
│  ├─ Image (2) - KillCounterUI.cs
│  └─ Image (3) - KillCounterUI.cs
│
└─ EnemyKillCounters (3 squares)
   ├─ Image (1) - KillCounterUI.cs
   ├─ Image (2) - KillCounterUI.cs
   └─ Image (3) - KillCounterUI.cs
```

---

## 🎨 Visual System

### Color Scheme:

```csharp
// KillCounterUI.cs
public Color aliveColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Zelená
public Color deadColor = new Color(0.8f, 0.2f, 0.2f, 1f);  // Červená
```

**States:**
- **Alive (Green):** `SetAlive()` - Called on Start() and game reset
- **Dead (Red):** `SetDead()` - Called when kill counter increments

### Debug Logging:

```csharp
// ✅ SPRÁVNE - Debug.LogWarning (user má Info logs disabled!)
Debug.LogWarning($"[KillCounterUI] {gameObject.name} set to ALIVE (green)");
Debug.LogWarning($"[KillCounterUI] {gameObject.name} set to DEAD (red)");
```

---

## 🔄 Game Flow

### Kill Detection:

```csharp
// BattleResultProcessor.cs - HandleCardDeath()

if (isEnemyCard) {
    // ✅ Call kill counter BEFORE starting death coroutine
    killCounterManager.OnPlayerKilledEnemyCard();
    
    // Then handle death
    yield return StartCoroutine(HandleEnemyCardDeath(...));
}
else {
    killCounterManager.OnEnemyKilledPlayerCard();
    yield return StartCoroutine(HandlePlayerCardDeath(...));
}
```

**Why before death handling?**
- Kill counter update je instant
- Death handling má coroutines a čaká
- Win condition check musí byť ASAP

### Win Condition Check:

```csharp
// MultiplayerKillCounterManager.cs

private const int KILLS_TO_WIN = 3;

private void CheckWinCondition() {
    if (playerKillCount >= KILLS_TO_WIN) {
        // Player WON!
        fightSystem.state = FightStateMultiplayer.WON;
        ShowWinMessage("You won!");
        DisableAttackButtons();
        StartCoroutine(ReturnToMenuAfterDelay(2f));
    }
    else if (enemyKillCount >= KILLS_TO_WIN) {
        // Player LOST!
        fightSystem.state = FightStateMultiplayer.LOST;
        ShowLoseMessage("You lost!");
        DisableAttackButtons();
        StartCoroutine(ReturnToMenuAfterDelay(2f));
    }
}
```

### End Game Sequence:

```
Kill #3 detected
↓
CheckWinCondition()
↓
Set FightState (WON/LOST)
↓
Show message in dialog box ("You won!" / "You lost!")
↓
Disable all 4 attack buttons (prevent further attacks)
↓
Wait 2 seconds (ReturnToMenuAfterDelay coroutine)
↓
SceneManager.LoadScene("Main")
```

---

## 🎮 Integration Points

### 1. BattleResultProcessor Integration:

```csharp
// Location: Assets/Scripts/Multiplayer/BattleResultProcessor.cs

private IEnumerator CheckBattleOutcome(...)
{
    // Check for deaths
    bool myCardDied = myCurrentHealth <= 0;
    bool enemyCardDied = enemyCurrentHealth <= 0;
    
    if (enemyCardDied) {
        // ✅ Kill counter BEFORE death handling
        killCounterManager.OnPlayerKilledEnemyCard();
        yield return StartCoroutine(HandleEnemyCardDeath(...));
    }
    
    if (myCardDied) {
        killCounterManager.OnEnemyKilledPlayerCard();
        yield return StartCoroutine(HandlePlayerCardDeath(...));
    }
}
```

### 2. FightSystemMultiplayer Integration:

```csharp
// Reference to kill counter manager
public MultiplayerKillCounterManager killCounterManager;

// States used:
FightStateMultiplayer.TURN     // Normal gameplay
FightStateMultiplayer.WON      // Player won (3 kills)
FightStateMultiplayer.LOST     // Player lost (enemy 3 kills)
```

### 3. Unity Inspector Setup:

```
MultiplayerKillCounterManager GameObject
├─ Player Kill Counters (array of 3 KillCounterUI)
├─ Enemy Kill Counters (array of 3 KillCounterUI)
├─ Dialog (TMP_Text reference)
├─ Fight System (FightSystemMultiplayer reference)
└─ Attack Buttons (Button array of 4)
```

---

## 📊 State Management

### Kill Count Tracking:

```csharp
private int playerKillCount = 0;  // Player kills enemy cards
private int enemyKillCount = 0;   // Enemy kills player cards

public void OnPlayerKilledEnemyCard() {
    playerKillCount++;
    Debug.LogWarning($"[KillCounterManager] 💀 Player killed enemy card! Count: {playerKillCount}/3");
    
    // Update UI - turn square red
    if (playerKillCount <= playerKillCounters.Length) {
        playerKillCounters[playerKillCount - 1].SetDead();
    }
    
    CheckWinCondition();
}
```

### UI Array Management:

```csharp
// Inspector assigned arrays:
[Header("Kill Counter UI")]
public KillCounterUI[] playerKillCounters;  // 3 squares
public KillCounterUI[] enemyKillCounters;   // 3 squares

// Initialization (Start):
void Start() {
    foreach (var counter in playerKillCounters) {
        counter.SetAlive();  // All green at start
    }
    foreach (var counter in enemyKillCounters) {
        counter.SetAlive();
    }
}
```

---

## 🐛 Common Issues & Fixes

### Issue #1: Kill Counter Not Updating
**Symptom:** Card dies but square stays green
**Debug:**
```csharp
// Check logs:
[KillCounterManager] 💀 Player killed enemy card! Count: 1/3  ← Should appear
[KillCounterUI] Image (1) set to DEAD (red)                   ← Should appear
```
**Fix:**
- Verify `killCounterManager` reference in `BattleResultProcessor`
- Check `playerKillCounters` array in Inspector (all 3 assigned?)
- Ensure `OnPlayerKilledEnemyCard()` is called BEFORE death coroutine

### Issue #2: Game Doesn't End at 3 Kills
**Symptom:** 3 squares red, but game continues
**Debug:**
```csharp
// CheckWinCondition() should trigger:
[KillCounterManager] 🏆 PLAYER WON! (3 kills)
```
**Fix:**
- Verify `CheckWinCondition()` is called in `OnPlayerKilledEnemyCard()`
- Check `KILLS_TO_WIN` constant = 3
- Verify `fightSystem` reference is assigned in Inspector

### Issue #3: Scene Not Loading After Win
**Symptom:** "You won!" message shows, but scene doesn't change
**Debug:**
```csharp
// Should see:
[KillCounterManager] 🔄 Returning to Main scene in 2 seconds...
```
**Fix:**
- Verify scene name is exactly "Main" (case-sensitive!)
- Check Build Settings → Scenes In Build (Main scene included?)
- Ensure `ReturnToMenuAfterDelay()` coroutine starts

### Issue #4: Debug Logs Not Visible
**Symptom:** Can't see kill counter logs in Unity Console
**Fix:**
```csharp
// ❌ WRONG - user má Info logs disabled!
Debug.Log("Kill count: 1");

// ✅ CORRECT - always use Warning!
Debug.LogWarning("[KillCounterManager] Kill count: 1");
```

---

## 🎛️ Configuration

### Tunable Parameters:

```csharp
// MultiplayerKillCounterManager.cs
private const int KILLS_TO_WIN = 3;        // Win condition threshold
private float sceneTransitionDelay = 2f;    // Wait before loading scene

// KillCounterUI.cs
public Color aliveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
public Color deadColor = new Color(0.8f, 0.2f, 0.2f, 1f);
```

### Scene Configuration:

```csharp
private const string MAIN_SCENE_NAME = "Main";  // Target scene after game end
```

**Important:** Scene must be in Build Settings!

---

## 🚀 Future Improvements

### Potential Enhancements:

1. **Animations:**
   ```csharp
   // Smooth color transition instead of instant
   StartCoroutine(FadeToRed(1f)); // 1 second fade
   ```

2. **Sound Effects:**
   ```csharp
   AudioSource.PlayOneShot(killSound);     // On kill
   AudioSource.PlayOneShot(victorySound);  // On win
   ```

3. **Particle Effects:**
   ```csharp
   Instantiate(explosionPrefab, square.position, Quaternion.identity);
   ```

4. **Kill Streak Counter:**
   ```csharp
   if (consecutiveKills >= 2) {
       ShowMessage("Double kill!");
   }
   ```

5. **Statistics Tracking:**
   ```csharp
   // Before loading Main scene:
   SaveMatchStats(playerKills, enemyKills, totalTurns);
   // Then show stats screen instead of direct Main menu
   ```

---

## 📝 Code Examples

### Complete Kill Counter Manager (Simplified):

```csharp
public class MultiplayerKillCounterManager : MonoBehaviour
{
    private const int KILLS_TO_WIN = 3;
    
    [Header("Kill Counter UI")]
    public KillCounterUI[] playerKillCounters;
    public KillCounterUI[] enemyKillCounters;
    
    [Header("References")]
    public TMP_Text dialogText;
    public FightSystemMultiplayer fightSystem;
    public Button[] attackButtons;
    
    private int playerKillCount = 0;
    private int enemyKillCount = 0;
    
    void Start()
    {
        InitializeCounters();
    }
    
    private void InitializeCounters()
    {
        foreach (var counter in playerKillCounters) counter.SetAlive();
        foreach (var counter in enemyKillCounters) counter.SetAlive();
        Debug.LogWarning("[KillCounterManager] ✅ Initialized. Win condition: First to 3 kills wins!");
    }
    
    public void OnPlayerKilledEnemyCard()
    {
        playerKillCount++;
        Debug.LogWarning($"[KillCounterManager] 💀 Player killed enemy! Count: {playerKillCount}/3");
        
        if (playerKillCount <= playerKillCounters.Length)
            playerKillCounters[playerKillCount - 1].SetDead();
        
        CheckWinCondition();
    }
    
    public void OnEnemyKilledPlayerCard()
    {
        enemyKillCount++;
        Debug.LogWarning($"[KillCounterManager] ☠️ Enemy killed player card! Count: {enemyKillCount}/3");
        
        if (enemyKillCount <= enemyKillCounters.Length)
            enemyKillCounters[enemyKillCount - 1].SetDead();
        
        CheckWinCondition();
    }
    
    private void CheckWinCondition()
    {
        if (playerKillCount >= KILLS_TO_WIN)
        {
            Debug.LogWarning("[KillCounterManager] 🏆 PLAYER WON!");
            fightSystem.state = FightStateMultiplayer.WON;
            ShowMessage("You won!");
            DisableAttackButtons();
            StartCoroutine(ReturnToMenuAfterDelay(2f));
        }
        else if (enemyKillCount >= KILLS_TO_WIN)
        {
            Debug.LogWarning("[KillCounterManager] 💀 PLAYER LOST!");
            fightSystem.state = FightStateMultiplayer.LOST;
            ShowMessage("You lost!");
            DisableAttackButtons();
            StartCoroutine(ReturnToMenuAfterDelay(2f));
        }
    }
    
    private void ShowMessage(string message)
    {
        if (dialogText != null)
            dialogText.text = message;
    }
    
    private void DisableAttackButtons()
    {
        foreach (var button in attackButtons)
            button.interactable = false;
    }
    
    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        Debug.LogWarning($"[KillCounterManager] 🔄 Returning to Main scene in {delay} seconds...");
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Main");
    }
}
```

---

**Version:** V7  
**Last Updated:** 2025-11-04  
**Status:** ✅ Production Ready  
**User Feedback:** "normalne mi funguje pekne hra :)"
