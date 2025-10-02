# 🎮 Návod na pripojenie Exit tlačidla v Unity

## 📋 Kroky pre setup UI v Multiplayer scéne:

### **1. Pridanie Exit tlačidla do scény:**
1. **Otvorte Multiplayer scénu** v Unity
2. **Nájdite Canvas** v Hierarchy
3. **Kliknite pravým tlačidlom** na Canvas → UI → Button - TextMeshPro
4. **Premenujte button** na "ExitButton"
5. **Nastavte pozíciu** (napr. pravý horný roh)
6. **Zmeňte text** na "Exit" alebo "Leave Game"

### **2. Pripojenie tlačidla k MultiplayerGameUI:**
1. **Vyberte GameObject** s `MultiplayerGameUI` scriptom
2. **V Inspectore** nájdite pole "Exit Button"
3. **Pretiahnite ExitButton** z Hierarchy do tohto poľa

### **3. Testovanie:**
- **Spustite hru** v Play mode
- **Pripojte sa do multiplayer**
- **Kliknite Exit** - malo by vás vrátiť do lobby

---

## ⚙️ Alternatívne možnosti:

### **A) Ak chcete ísť do hlavného menu namiesto lobby:**
Zmeňte v `MultiplayerGameUI.cs` riadok 391:
```csharp
// Nahraď túto cestu:
SceneManager.LoadScene("MultiplayerLobby");

// Na túto:
SceneManager.LoadScene("MainMenu");
```

### **B) Ak chcete oba tlačidlá (Exit to Lobby + Exit to Menu):**
1. **Vytvorte 2 tlačidlá**: "BackToLobby" a "BackToMainMenu"
2. **Pridajte oba do MultiplayerGameUI**:
```csharp
public Button exitToLobbyButton;
public Button exitToMainMenuButton;
```
3. **V Start() metóde pripojte oba**:
```csharp
if (exitToLobbyButton != null)
    exitToLobbyButton.onClick.AddListener(OnExitToLobby);
    
if (exitToMainMenuButton != null)
    exitToMainMenuButton.onClick.AddListener(OnExitToMainMenu);
```

### **C) Názvy scén:**
Upravte názvy scén v kóde podľa vašich skutočných scén:
- `"MultiplayerLobby"` → váš názov lobby scény
- `"MainMenu"` → váš názov hlavného menu

---

## 🎯 Čo sa stane keď kliknete Exit:

1. **Heartbeat sa zastaví** (prestane posielať životné signály)
2. **Hráč opustí miestnosť** (volá sa `leaveRoom` API)
3. **Lokálne údaje sa vymažú** (RoomCode, IsWaitingForOpponent)
4. **Polling sa zastaví** (ak čakal na súpera)
5. **Vráti sa do lobby/menu** (podľa nastavenia)

---

## 🛠️ Troubleshooting:

**Problém:** Exit tlačidlo nefunguje
- **Riešenie:** Skontrolujte či je tlačidlo pripojené v Inspectore

**Problém:** Nesprávny názov scény
- **Riešenie:** Skontrolujte Build Settings → aké scény máte pridané

**Problém:** Hráč zostáva v miestnosti aj po exit
- **Riešenie:** Skontrolujte console - či sa volá leaveRoom API úspešne

**Problém:** Chyby kompilátora
- **Riešenie:** Skontrolujte či máte správne using direktívy v script súboroch