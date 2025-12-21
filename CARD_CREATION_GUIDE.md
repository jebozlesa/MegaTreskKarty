# 🎴 Príručka na pridávanie nových kariet do hry

## 📍 Kde je SQLite databáza?

### Zdrojový súbor (editovateľný):
```
Assets/StreamingAssets/MyDatabase.db
```
Tento súbor editujete pomocou SQLite nástroja (DB Browser for SQLite, DBeaver, atď.)

### Runtime kópia:
```
Application.persistentDataPath/MyDatabase.db
```
- **Windows:** `C:\Users\[User]\AppData\LocalLow\[Company]\[Game]\MyDatabase.db`
- **Android:** `/data/data/[package]/files/MyDatabase.db`

Pri prvom spustení hry sa `StreamingAssets/MyDatabase.db` skopíruje do `persistentDataPath`. Ak zmeníte databázu, musíte **odstrániť runtime kópiu** alebo reinstallnuť hru.

---

## ➕ Postup pridania novej karty

### 1️⃣ Pridať obrázok karty
Uložte obrázok do:
```
Assets/Resources/Cards/[meno_karty].png
```

**Požiadavky:**
- Formát: PNG (s alpha channel pre priehľadnosť)
- Veľkosť: odporúčané 512x512 alebo 1024x1024
- Názov súboru: bez medzier, najlepšie lowercase (`jesus.png`, `hitler.png`)

---

### 2️⃣ Pridať do SQLite databázy

Otvorte `Assets/StreamingAssets/MyDatabase.db` v SQLite editore.

#### A) Tabuľka `CardDatabase`
```sql
INSERT INTO CardDatabase (StyleID, PersonName, Health, Strength, Speed, Attack, Defense, Knowledge, Charisma, Color, Series)
VALUES (
    50,                    -- StyleID (unique ID karty)
    'Nikola Tesla',        -- PersonName (meno na karte)
    10,                    -- Health (max HP)
    8,                     -- Strength
    7,                     -- Speed
    9,                     -- Attack
    6,                     -- Defense
    10,                    -- Knowledge
    5,                     -- Charisma
    '138;43;226',          -- Color (RGB separated by ;)
    1                      -- Series (1 = AI only, 2+ = pre hráčov)
);
```

#### B) Tabuľka `CardVisuals`
```sql
INSERT INTO CardVisuals (CharacterID, Series, Color, Image)
VALUES (
    50,                    -- CharacterID (must match StyleID)
    2,                     -- Series (2 = normal hráči, 3 = rare, atď.)
    '138;43;226',          -- Color (RGB)
    'nikola_tesla'         -- Image (názov bez .png)
);
```

**Poznámka:** Ak chcete kartu dostupnú pre hráčov, použite **Series 2 alebo vyššiu**. Series 1 je rezervovaná pre AI.

#### C) Tabuľka `CharacterAttacks`
Priraďte 4-8 útokov, z ktorých sa náhodne vyberú 4:
```sql
INSERT INTO CharacterAttacks (CharacterID, AttackID)
VALUES 
    (50, 1),  -- MegaGul
    (50, 2),  -- MegaPalka
    (50, 5),  -- Lightning Attack (custom)
    (50, 7),  -- Electric Shock
    (50, 11), -- Thunderbolt
    (50, 14); -- Voltage Surge
```

---

### 3️⃣ Odstrániť starú runtime databázu

Po úprave `MyDatabase.db` musíte zabezpečiť, aby sa nová verzia skopírovala:

**Option A:** Vymazať runtime databázu manuálne
```
Windows: C:\Users\[User]\AppData\LocalLow\[Company]\[Game]\MyDatabase.db
```

**Option B:** Reinstall hry

**Option C:** Pridať debug kód do `Database.cs`:
```csharp
File.Delete(destinationFilePath); // Force overwrite
```

---

## 🎯 Ako fungujú Serie kariet

### **Series 1 (AI Only)**
- Používa sa **len pre AI protivníkov** v Royal Battle a Campaign
- Hráči **NEMÔŽU** získať tieto karty v balíčkoch ani ako odmenu

### **Series 2+ (Hráči)**
- **Series 2:** Normálne karty (dostupné v balíčkoch a ako Royal Battle odmeny)
- **Series 3+:** Pre budúce rozšírenia (rare karty, event karty, atď.)

**Aktuálne nastavenie v `CardGenerator.cs`:**
```csharp
private int GetRandomAvailableSeries()
{
    return 2; // Všetky karty sú Series 2
}
```

**Budúce rozšírenie (ak pridáte viac sérií):**
```csharp
private int GetRandomAvailableSeries()
{
    int[] availableSeries = new int[] { 2, 3, 4 }; // Series pre hráčov
    return availableSeries[UnityEngine.Random.Range(0, availableSeries.Length)];
}
```

---

## 📊 Themed Packs (Tematické balíčky)

V `CardGenerator.cs` sú definované balíčky s konkrétnymi kartami:

```csharp
public List<int>[] themedPacks = new List<int>[]
{
    new List<int> { 5, 8, 11, 12, 15, 16, 20, 23, 25, 27, 28, 29, 30, 33, 38, 39, 41, 42, 44, 45 }, // Balíček 0
    new List<int> { 2, 3, 4, 7, 13, 14, 18, 19, 21, 22, 24, 26, 32, 34, 37, 43 },                  // Balíček 1
    new List<int> { 4, 10, 25, 30, 35, 36, 41, 44, 45 },                                           // Balíček 2
};
```

**Pridať novú kartu do balíčka:**
```csharp
new List<int> { 5, 8, 11, 50 } // Pridali sme StyleID 50 (Nikola Tesla)
```

---

## 🖥️ Server vs Client generovanie

### **Aktuálny stav: Client-side generovanie**
✅ Výhody:
- Rýchle (žiadny network delay)
- Funguje offline
- Jednoduchšie

❌ Nevýhody:
- Hacknuteľné (upraviť SQLite → získať aké karty chceš)
- Ťažšie updatovať karty (musíš vydať nový build)
- APK obsahuje celú databázu

### **Možné riešenie: Server-side generovanie**
✅ Výhody:
- **Bezpečnosť** - nikto nemôže hacknuť karty
- **Dynamické updaty** - pridáš kartu na server → okamžite všade
- **Events** - sezónne karty, promo karty
- **Analytics** - sledovanie drop rates
- **Monetizácia kontrola**

❌ Nevýhody:
- Potrebuješ internet
- Server cost (PlayFab calls)
- Pomalšie (network latency)

**Odporúčanie:**
- Pre casual single-player hru → **ponechať client-side**
- Pre competitive multiplayer s monetizáciou → **presunúť na server**

---

## 🔧 V11 Zmeny v kóde

### Čo sa zmenilo:
1. **Marketplace balíčky:** Už negenerujú 5x Series 1 + 1x Series 2
2. **Nová logika:** Všetkých 6 kariet v balíčku môže byť **hocaká séria okrem Series 1**
3. **Royal Battle odmeny:** Už negenerujú Series 1, ale Series 2+

### Upravené súbory:
- `Assets/Scripts/CardGenerator.cs`
  - `GenerateCardPack()` - odstránené `series = 1; if (i == 5) series = 2;`
  - `AddRandomCardCoroutine()` - odstránené `WHERE Series = 1`
  - Pridaná funkcia `GetRandomAvailableSeries()` - vráti Series 2+

---

## 📝 Checklist pre pridanie karty

- [ ] Obrázok uložený do `Assets/Resources/Cards/[meno].png`
- [ ] Pridaný záznam do `CardDatabase` tabuľky (StyleID, stats)
- [ ] Pridaný záznam do `CardVisuals` tabuľky (Series 2+, color, image)
- [ ] Pridané útoky do `CharacterAttacks` tabuľky (4-8 útokov)
- [ ] Vymazaná runtime databáza alebo reinstall hry
- [ ] Pridaný StyleID do `themedPacks[]` v `CardGenerator.cs` (optional)
- [ ] Testované v hre (kúpiť balíček, skontrolovať že karta má správne stats)

---

## 🛠️ Užitočné nástroje

### SQLite editory:
- **DB Browser for SQLite** (free, GUI): https://sqlitebrowser.org/
- **DBeaver** (free, universal): https://dbeaver.io/
- **SQLite Expert** (paid, advanced): http://www.sqliteexpert.com/

### Export/Import:
```sql
-- Export karty do CSV
.mode csv
.output cards_backup.csv
SELECT * FROM CardDatabase;

-- Import karty z CSV
.mode csv
.import cards_backup.csv CardDatabase
```

---

## 🐛 Troubleshooting

### Karta sa nezobrazuje v hre
1. Skontrolujte že `Image` v `CardVisuals` **presne zodpovedá** názvu súboru bez `.png`
2. Skontrolujte že obrázok je v `Resources/Cards/` priečinku
3. Skontrolujte že `Series` je 2 alebo vyššia (nie 1)

### Karta má 0 HP alebo chýbajúce útoky
1. Skontrolujte že `StyleID` v `CardDatabase` je unikátne
2. Skontrolujte že `CharacterID` v `CardVisuals` a `CharacterAttacks` zodpovedá `StyleID`

### Nové karty sa neobjavili
1. Vymazať runtime databázu (`AppData/LocalLow/...`)
2. Alebo reinstall hry
3. Debug log: `Database.Instance.GetDatabasePath()` ukáže kde je runtime DB

---

## 📚 Ďalšie informácie

- Attack System: [.github/copilot-instructions.md](.github/copilot-instructions.md)
- Effect System: [EFFECT_SYSTEM_ARCHITECTURE.md](EFFECT_SYSTEM_ARCHITECTURE.md)
- Card Database Schema: Viď `CardDatabase` tabuľka štruktúra v SQLite

