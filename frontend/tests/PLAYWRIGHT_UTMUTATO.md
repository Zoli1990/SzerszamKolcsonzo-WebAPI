# 🎭 PLAYWRIGHT E2E TESZT - TELEPÍTÉS ÉS FUTTATÁS

## 📦 Telepítés

```bash
# 1. Playwright telepítése
npm init playwright@latest

# Vagy ha már van package.json:
npm install -D @playwright/test
npx playwright install

# 2. Másold be a fájlokat:
# - playwright.config.js → projekt gyökér
# - admin-foglalasok.spec.js → test/ mappa
```

---

## 🚀 Futtatás

### **Előfeltételek:**
```bash
# 1. Backend futnia KELL!
cd SzerszamKolcsonzo
dotnet run
# http://localhost:5265

# 2. Frontend futnia KELL!
cd frontend
npm run dev
# http://localhost:5173
```

### **Teszt futtatása:**

```bash
# TELJES TESZT (12 teszt)
npx playwright test

# CSAK ADMIN FOGLALÁSOK TESZT
npx playwright test admin-foglalasok

# LÁTHATÓ BÖNGÉSZŐ (headed mode)
npx playwright test --headed

# DEBUG MÓD (lépésről lépésre)
npx playwright test --debug

# UI MÓD (interaktív)
npx playwright test --ui
```

---

## 📊 TESZT LISTA

### **Alapműveletek (1-10):**
1. ✅ Oldal betöltődik és táblázat látható
2. ✅ Foglalás létrehozása API-ból és megjelenik
3. ✅ KIADVA gomb működik (Aktiv → Kiadva)
4. ✅ VISSZAHOZVA gomb működik + Elszámolás
5. ✅ Részletek modal helyes adatokkal
6. ✅ Automatikus frissítés (10 mp)
7. ✅ Frissítés gomb működik
8. ✅ Üres állapot megjelenik
9. ✅ Bevétel formázás helyes
10. ✅ Screenshot készítése

### **Hibaesetek (11-12):**
11. ✅ Nem létező foglalás kiadása (404)
12. ✅ Dupla kiadás próbálkozás (400)

---

## 📸 EREDMÉNYEK

```bash
# HTML Report megnézése
npx playwright show-report

# Screenshot-ok helye:
test-results/foglalasok-admin-final.png

# Videók (hiba esetén):
test-results/*.webm
```

---

## 🔍 MIT TESZTEL?

### **Frontend:**
- ✅ Táblázat oszlopok helyesek (Eltelt idő, Bevétel)
- ✅ KIADVA / VISSZAHOZVA gombok láthatók
- ✅ Modal megnyílik és új mezőket mutat
- ✅ Elszámolás alert megjelenik
- ✅ Automatikus frissítés működik
- ✅ Formázás helyes (4,250 Ft, 2h 45m)

### **Backend API:**
- ✅ POST /foglalasok (VegIdopont nélkül)
- ✅ POST /foglalasok/{id}/kiad
- ✅ POST /eszkozok/{id}/visszahoz (elszámolás)
- ✅ GET /foglalasok (új mezők)

### **Integráció:**
- ✅ Frontend ← API kommunikáció
- ✅ Valós idejű frissítés
- ✅ User flow végig

---

## ⚠️ FONTOS TUDNIVALÓK

### **Adatbázis:**
A tesztek **valós adatokat** hoznak létre az adatbázisban!

```sql
-- Cleanup teszt után (opcionális):
DELETE FROM Foglalasok WHERE Nev LIKE '%Teszt%';
```

### **Port-ok:**
- Frontend: `http://localhost:5173`
- Backend: `http://localhost:5265`

Ha másik porton fut, módosítsd:
```javascript
// admin-foglalasok.spec.js
const BASE_URL = 'http://localhost:XXXX';
const API_URL = 'http://localhost:YYYY/api';
```

---

## 🐛 HIBAELHÁRÍTÁS

### **"Failed to fetch" hiba:**
```bash
# Backend nem fut
cd SzerszamKolcsonzo
dotnet run
```

### **"Navigation timeout" hiba:**
```bash
# Frontend nem fut
cd frontend
npm run dev
```

### **"Cannot find table" hiba:**
```javascript
// FoglalasokAdmin.vue nincs betöltve
// Ellenőrizd a router-t:
{
  path: '/admin/foglalasok',
  component: FoglalasokAdmin
}
```

---

## 📈 ELVÁRT EREDMÉNY

```
Running 12 tests using 1 worker

  ✓ 1. Oldal betöltődik és táblázat látható (2.5s)
  ✓ 2. Foglalás létrehozása API-ból és megjelenik (3.2s)
  ✓ 3. KIADVA gomb működik (4.1s)
  ✓ 4. VISSZAHOZVA gomb működik + Elszámolás (5.3s)
  ✓ 5. Részletek modal helyes adatokkal (3.8s)
  ✓ 6. Automatikus frissítés működik (24.5s)
  ✓ 7. Frissítés gomb működik (1.9s)
  ✓ 8. Üres állapot megjelenik (1.2s)
  ✓ 9. Bevétel formázás helyes (1.5s)
  ✓ 10. Screenshot készítése (2.1s)
  ✓ 11. Nem létező foglalás kiadása (1.3s)
  ✓ 12. Dupla kiadás próbálkozás (2.8s)

12 passed (54.2s)
```

---

## 🎥 SCREENSHOT PÉLDA

A teszt után készül egy screenshot:
```
test-results/foglalasok-admin-final.png
```

Ez megmutatja a **végleges állapotot**:
- Táblázat adatokkal
- KIADVA / VISSZAHOZVA gombok
- Státuszok színekkel
- Elszámolás megjelenítve

---

## 🚀 GYORS START

```bash
# 1. Telepítés
npm install -D @playwright/test
npx playwright install

# 2. Fájlok bemásolása
# playwright.config.js → gyökér
# admin-foglalasok.spec.js → test/

# 3. Backend indítás
cd SzerszamKolcsonzo && dotnet run

# 4. Frontend indítás (másik terminal)
cd frontend && npm run dev

# 5. TESZT FUTTATÁS
npx playwright test --headed

# 6. Report megtekintése
npx playwright show-report
```

---

**Kész! Most látni fogod élőben hogy működik minden! 🎉**
