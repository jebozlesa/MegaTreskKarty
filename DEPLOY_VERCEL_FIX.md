# 🚀 Deploy Guide - Aktualizácia Vercel Funkcie

## ❌ Problém
Server na Vercel má **starú verziu** `executeBattle.js` ktorá očakáva iné fieldy ako Unity posiela.

**Error:**
```
Missing required attack data fields: cardId, attackId, currentHealth
```

---

## ✅ Riešenie: Deploy novej verzie

### Krok 1: Skontroluj súbory

Máš 2 verzie v `vercel-api/`:
- `executeBattle.js` - **STARÁ** (client-sent stats)
- `executeBattle_v2.js` - **NOVÁ** (server reads from DB)

### Krok 2: Nahraď súbor

**Option A: Premenuj súbory lokálne**
```powershell
cd c:\Zlozka\MegaTresk\MegaTreskKarty\vercel-api

# Backup starej verzie
Rename-Item executeBattle.js executeBattle_old.js

# Použij novú verziu
Copy-Item executeBattle_v2.js executeBattle.js
```

**Option B: Priamo v Vercel Dashboard**
1. Otvor Vercel Dashboard → tvoj projekt
2. Source tab → `vercel-api/executeBattle.js`
3. Edit → skopíruj obsah z `executeBattle_v2.js`
4. Save & Deploy

### Krok 3: Deploy

```powershell
# Ak používaš Git deployment
git add vercel-api/executeBattle.js
git commit -m "Update executeBattle to v2 - server reads stats from DB"
git push

# Vercel automaticky redeploy-ne
```

**ALEBO** manuálny Vercel CLI:
```powershell
cd c:\Zlozka\MegaTresk\MegaTreskKarty
vercel --prod
```

### Krok 4: Skontroluj environment variables

V Vercel Dashboard → Settings → Environment Variables:

```
PLAYFAB_TITLE_ID = tvoj_title_id
PLAYFAB_SECRET_KEY = tvoj_secret_key
```

⚠️ **DÔLEŽITÉ:** Po pridaní env vars musíš **Redeploy**!

---

## 🧪 Test po deployi

```powershell
curl -X POST https://tvoja-app.vercel.app/api/executeBattle `
  -H "Content-Type: application/json" `
  -d '{
    "roomCode": "TEST123",
    "playerId": "test-player",
    "attackData": {
      "playerId": "test-player",
      "roomCode": "TEST123",
      "cardId": "test-card-id",
      "attackId": 1,
      "currentHealth": 100
    }
  }'
```

**Očakávaný výsledok:**
```json
{
  "success": true,
  "bothPlayersReady": false,
  "playersReady": 1
}
```

❌ **Zlý výsledok (stará verzia):**
```json
{
  "success": false,
  "error": "Missing required attack data fields: cardId, attackId, currentHealth"
}
```

---

## 📋 Checklist

- [ ] Backup `executeBattle.js` → `executeBattle_old.js`
- [ ] Copy `executeBattle_v2.js` → `executeBattle.js`  
- [ ] Git commit + push  
- [ ] Čakaj na Vercel auto-deploy (1-2 min)
- [ ] Skontroluj env vars (PLAYFAB_TITLE_ID, PLAYFAB_SECRET_KEY)
- [ ] Test cez curl alebo Unity
- [ ] Ak funguje → zmaž `executeBattle_old.js`

---

## 🐛 Ak stále nefunguje

### 1. Skontroluj deployment log
Vercel Dashboard → Deployments → Latest → Logs

Hľadaj:
```
✓ Built successfully
✓ Deployed to production
```

### 2. Skontroluj ktorú verziu používa PlayFab
PlayFab Dashboard → Automation → CloudScript → Functions

Nájdi `executeBattle` a skontroluj **Function URL**:
```
https://tvoja-app.vercel.app/api/executeBattle
```

### 3. Hard refresh cache
Vercel niekedy cachuje:
```powershell
# Redeploy force
vercel --prod --force
```

---

Hotovo! Teraz by Unity malo posielať správnu dáta štruktúru. 🎉
