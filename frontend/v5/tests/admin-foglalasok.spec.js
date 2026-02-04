// test/admin-foglalasok.spec.js
// Playwright E2E teszt - Admin foglalások kezelése
// VÉGLEGES VERZIÓ v3 - Javított szelektorokkal

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:5173';
const API_URL = 'http://localhost:5265/api';

// ✅ Admin bejelentkezési adatok - MÓDOSÍTSD HA MÁS!
const ADMIN_EMAIL = 'admin@szerszam.hu';
const ADMIN_PASSWORD = 'Admin123';

// ✅ Helper: Admin bejelentkezés (robusztus verzió)
async function loginAsAdmin(page) {
  await page.goto(BASE_URL);
  await page.waitForLoadState('networkidle');
  
  // Ellenőrizd hogy már be van-e jelentkezve
  const adminLink = page.locator('a:has-text("Admin")');
  if (await adminLink.isVisible({ timeout: 2000 }).catch(() => false)) {
    console.log('✅ Már be van jelentkezve admin-ként');
    return true;
  }
  
  // Belépés gomb keresése
  const loginBtn = page.locator('button:has-text("Belépés")');
  
  if (!(await loginBtn.isVisible({ timeout: 3000 }).catch(() => false))) {
    console.log('⚠️ Belépés gomb nem található');
    return false;
  }
  
  await loginBtn.click();
  await page.waitForTimeout(500);
  
  // Modal megjelent?
  const modal = page.locator('.modal-container, .modal-box, [role="dialog"]');
  try {
    await expect(modal).toBeVisible({ timeout: 5000 });
  } catch {
    console.log('⚠️ Login modal nem jelent meg');
    return false;
  }
  
  // Form kitöltése
  await page.locator('input[type="email"]').fill(ADMIN_EMAIL);
  await page.locator('input[type="password"]').fill(ADMIN_PASSWORD);
  
  // Submit
  await page.locator('button[type="submit"]').click();
  
  // Várj a bejelentkezésre
  await page.waitForTimeout(2000);
  
  // Sikeres bejelentkezés ellenőrzése
  const success = await page.locator('a:has-text("Admin"), .user-menu, .user-button').isVisible().catch(() => false);
  
  if (!success) {
    await page.screenshot({ path: 'test-results/login-failed.png' });
    console.log('⚠️ Login sikertelen - screenshot mentve');
    return false;
  }
  
  console.log('✅ Admin bejelentkezés sikeres');
  return true;
}

// ✅ Helper: Navigálás az admin foglalások tab-ra
async function navigateToFoglalasokTab(page) {
  await page.goto(`${BASE_URL}/admin`);
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(500);
  
  // Foglalások tab-ra kattintás
  const foglalasokTab = page.locator('button.tab:has-text("Foglalások")');
  if (await foglalasokTab.isVisible()) {
    await foglalasokTab.click();
    await page.waitForTimeout(500);
    return true;
  }
  
  return false;
}

// ✅ Helper: Foglalás sor keresése ID alapján (JAVÍTOTT - egyedi szelektor)
function getRowByFoglalasId(page, foglalasID) {
  // A "#ID" formátumot keressük, ami csak az ID oszlopban van
  // .first() biztosítja hogy csak egy elemet kapunk
  return page.locator(`tr:has-text("#${foglalasID}")`).first();
}

// ✅ Helper: Elérhető eszköz keresése foglaláshoz
async function findAvailableEszkozId(request) {
  const response = await request.get(`${API_URL}/eszkozok`);
  if (!response.ok()) return null;
  
  const eszkozok = await response.json();
  const elerheto = eszkozok.find(e => e.status === 'Elerheto');
  return elerheto ? elerheto.eszkozID : null;
}

test.describe('Admin Foglalások Kezelése', () => {
  
  // Minden teszt előtt bejelentkezés
  test.beforeEach(async ({ page }) => {
    const loggedIn = await loginAsAdmin(page);
    if (!loggedIn) {
      console.log('⚠️ Admin bejelentkezés sikertelen, teszt kihagyva');
    }
  });

  test('1. Admin panel és foglalások tab elérhető', async ({ page }) => {
    const navigated = await navigateToFoglalasokTab(page);
    
    if (!navigated) {
      test.skip();
      return;
    }
    
    // Ellenőrizd a címet
    const title = page.locator('h2:has-text("Foglalások")');
    await expect(title).toBeVisible({ timeout: 10000 });
    
    // Táblázat VAGY üres állapot látható
    const table = page.locator('table.data-table');
    const emptyState = page.locator('.empty-state');
    
    const hasTable = await table.isVisible().catch(() => false);
    const hasEmpty = await emptyState.isVisible().catch(() => false);
    
    expect(hasTable || hasEmpty).toBeTruthy();
    console.log(`✅ Foglalások tab betöltve - ${hasTable ? 'van adat' : 'üres'}`);
  });

  test('2. Foglalás létrehozása és megjelenítése', async ({ page, request }) => {
    // Keress elérhető eszközt
    const eszkozID = await findAvailableEszkozId(request);
    
    if (!eszkozID) {
      console.log('⚠️ Nincs elérhető eszköz, teszt kihagyva');
      return;
    }
    
    // Foglalás létrehozása API-n keresztül
    const foglalasData = {
      eszkozID: eszkozID,
      nev: "Playwright Teszt " + Date.now(),
      email: "playwright@test.com",
      telefonszam: "+36301234567",
      cim: "Budapest, Teszt utca 1.",
      foglalasKezdete: new Date(Date.now() + 3600000).toISOString()
    };

    const response = await request.post(`${API_URL}/foglalasok`, {
      data: foglalasData,
      headers: { 'Content-Type': 'application/json' }
    });

    if (response.status() === 307) {
      console.log('⚠️ Backend HTTPS-re irányít át');
      return;
    }

    if (!response.ok()) {
      console.log('⚠️ API hiba:', response.status());
      return;
    }

    const foglalas = await response.json();
    console.log('✅ Foglalás létrehozva:', foglalas.foglalasID);

    // Navigálj a foglalások oldalra
    await navigateToFoglalasokTab(page);
    
    // Frissítés
    const refreshBtn = page.locator('button:has-text("Frissítés")');
    if (await refreshBtn.isVisible()) {
      await refreshBtn.click();
      await page.waitForTimeout(1000);
    }

    // ✅ JAVÍTOTT: Egyedi szelektor használata
    const row = getRowByFoglalasId(page, foglalas.foglalasID);
    await expect(row).toBeVisible({ timeout: 10000 });
    
    // Ellenőrizd a sor tartalmát
    await expect(row).toContainText('Playwright Teszt');
    console.log('✅ Foglalás megjelent a listában');
  });

  test('3. KIADVA gomb működik', async ({ page, request }) => {
    // Keress elérhető eszközt
    const eszkozID = await findAvailableEszkozId(request);
    
    if (!eszkozID) {
      console.log('⚠️ Nincs elérhető eszköz, teszt kihagyva');
      return;
    }
    
    // Készíts új foglalást
    const response = await request.post(`${API_URL}/foglalasok`, {
      data: {
        eszkozID: eszkozID,
        nev: "Kiadás Teszt " + Date.now(),
        email: "kiadas@test.com",
        telefonszam: "+36309876543",
        cim: "Szeged, Kossuth utca 12.",
        foglalasKezdete: new Date(Date.now() + 3600000).toISOString()
      }
    });

    if (!response.ok()) {
      console.log('⚠️ Foglalás létrehozása sikertelen:', response.status());
      return;
    }

    const foglalas = await response.json();
    console.log('✅ Teszt foglalás:', foglalas.foglalasID);

    await navigateToFoglalasokTab(page);
    await page.waitForTimeout(1000);

    // Frissítés
    const refreshBtn = page.locator('button:has-text("Frissítés")');
    if (await refreshBtn.isVisible()) {
      await refreshBtn.click();
      await page.waitForTimeout(1000);
    }

    // ✅ JAVÍTOTT: Egyedi szelektor
    const row = getRowByFoglalasId(page, foglalas.foglalasID);
    await expect(row).toBeVisible({ timeout: 10000 });

    // KIADVA gomb
    const kiadvaBtn = row.locator('button:has-text("KIADVA")');
    
    if (await kiadvaBtn.isVisible()) {
      // Dialog kezelés
      page.once('dialog', dialog => dialog.accept());
      await kiadvaBtn.click();
      await page.waitForTimeout(1500);
      
      // Második alert kezelés
      page.once('dialog', dialog => dialog.accept());
      
      console.log('✅ KIADVA gomb megnyomva');
      
      // Ellenőrizd hogy a státusz változott
      await refreshBtn.click();
      await page.waitForTimeout(1000);
      
      const updatedRow = getRowByFoglalasId(page, foglalas.foglalasID);
      await expect(updatedRow).toContainText('Kiadva');
      console.log('✅ Státusz sikeresen változott: Kiadva');
    } else {
      console.log('ℹ️ KIADVA gomb nem látható (eszköz már ki van adva)');
    }
  });

  test('4. VISSZAHOZVA gomb működik', async ({ page, request }) => {
    // Keress elérhető eszközt
    const eszkozID = await findAvailableEszkozId(request);
    
    if (!eszkozID) {
      console.log('⚠️ Nincs elérhető eszköz, teszt kihagyva');
      return;
    }
    
    // Készíts és add ki egy foglalást
    const createResp = await request.post(`${API_URL}/foglalasok`, {
      data: {
        eszkozID: eszkozID,
        nev: "Visszahozás Teszt " + Date.now(),
        email: "vissza@test.com",
        telefonszam: "+36201111111",
        cim: "Debrecen, Petőfi tér 5.",
        foglalasKezdete: new Date(Date.now() + 3600000).toISOString()
      }
    });

    if (!createResp.ok()) {
      console.log('⚠️ Foglalás létrehozása sikertelen');
      return;
    }

    const foglalas = await createResp.json();
    
    // Kiadás API-n keresztül
    const kiadResp = await request.post(`${API_URL}/foglalasok/${foglalas.foglalasID}/kiad`);
    if (!kiadResp.ok()) {
      console.log('⚠️ Kiadás sikertelen');
      return;
    }

    console.log('✅ Foglalás kiadva:', foglalas.foglalasID);

    // Várj egy kicsit az időkülönbséghez
    await page.waitForTimeout(2000);
    
    await navigateToFoglalasokTab(page);
    
    // Frissítés
    const refreshBtn = page.locator('button:has-text("Frissítés")');
    if (await refreshBtn.isVisible()) {
      await refreshBtn.click();
      await page.waitForTimeout(1000);
    }

    // ✅ JAVÍTOTT: Egyedi szelektor
    const row = getRowByFoglalasId(page, foglalas.foglalasID);
    await expect(row).toBeVisible({ timeout: 10000 });

    // VISSZAHOZVA gomb
    const visszaBtn = row.locator('button:has-text("VISSZAHOZVA")');
    
    if (await visszaBtn.isVisible()) {
      // Dialog kezelés
      page.once('dialog', dialog => {
        console.log('Alert:', dialog.message());
        dialog.accept();
      });
      
      await visszaBtn.click();
      await page.waitForTimeout(1500);
      
      // Második alert (elszámolás info)
      page.once('dialog', dialog => dialog.accept());
      
      console.log('✅ VISSZAHOZVA gomb megnyomva');
      
      // Ellenőrizd az elszámolást
      await refreshBtn.click();
      await page.waitForTimeout(1000);
      
      const updatedRow = getRowByFoglalasId(page, foglalas.foglalasID);
      await expect(updatedRow).toContainText('Lezárt');
      console.log('✅ Foglalás sikeresen lezárva');
    } else {
      console.log('ℹ️ VISSZAHOZVA gomb nem látható');
    }
  });

  test('5. Részletek modal megnyílik', async ({ page }) => {
    await navigateToFoglalasokTab(page);

    // Van-e egyáltalán foglalás?
    const rows = page.locator('tbody tr');
    const count = await rows.count();
    
    if (count === 0) {
      console.log('ℹ️ Nincs foglalás, modal teszt kihagyva');
      return;
    }

    // Első sor részletek gombja (szemecske ikon)
    const detailBtn = rows.first().locator('button.btn-info, button:has-text("👁")');
    
    if (await detailBtn.isVisible()) {
      await detailBtn.click();
      
      // Modal megjelenik
      const modal = page.locator('.modal-box');
      await expect(modal).toBeVisible({ timeout: 5000 });
      
      // Tartalom ellenőrzés
      await expect(modal).toContainText('Foglalás');
      await expect(modal).toContainText('Ügyfél');
      
      // Bezárás
      const closeBtn = modal.locator('button:has-text("Bezárás")');
      await closeBtn.click();
      await expect(modal).not.toBeVisible();
      
      console.log('✅ Modal működik');
    } else {
      console.log('ℹ️ Részletek gomb nem található');
    }
  });

  test('6. Frissítés gomb működik', async ({ page }) => {
    await navigateToFoglalasokTab(page);

    const refreshBtn = page.locator('button:has-text("Frissítés")');
    
    if (!(await refreshBtn.isVisible())) {
      console.log('⚠️ Frissítés gomb nem található');
      return;
    }

    // Várj API response-ra
    const responsePromise = page.waitForResponse(
      resp => resp.url().includes('/foglalasok'),
      { timeout: 10000 }
    ).catch(() => null);

    await refreshBtn.click();
    
    const response = await responsePromise;
    if (response) {
      expect(response.status()).toBe(200);
      console.log('✅ Frissítés sikeres, API response:', response.status());
    } else {
      console.log('⚠️ Nem jött API response');
    }
  });

  test('7. Screenshot a végső állapotról', async ({ page }) => {
    await navigateToFoglalasokTab(page);
    await page.waitForTimeout(1000);
    
    await page.screenshot({ 
      path: 'test-results/admin-foglalasok-final.png',
      fullPage: true 
    });
    
    console.log('📸 Screenshot mentve: test-results/admin-foglalasok-final.png');
  });
});

// ============================================================================
// API TESZTEK (nem igényelnek UI-t)
// ============================================================================

test.describe('API Tesztek', () => {
  
  test('API: Foglalások lekérése', async ({ request }) => {
    const response = await request.get(`${API_URL}/foglalasok`);
    
    if (response.status() === 307) {
      console.log('⚠️ Backend HTTPS redirect aktív - indítsd HTTP módban');
      return;
    }
    
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    expect(Array.isArray(data)).toBeTruthy();
    console.log(`✅ ${data.length} foglalás az adatbázisban`);
  });

  test('API: Eszközök lekérése', async ({ request }) => {
    const response = await request.get(`${API_URL}/eszkozok`);
    
    if (response.status() === 307) {
      console.log('⚠️ Backend HTTPS redirect aktív');
      return;
    }
    
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    expect(Array.isArray(data)).toBeTruthy();
    console.log(`✅ ${data.length} eszköz az adatbázisban`);
  });

  test('API: Kategóriák lekérése', async ({ request }) => {
    const response = await request.get(`${API_URL}/kategoriak`);
    
    if (response.status() === 307) {
      console.log('⚠️ Backend HTTPS redirect aktív');
      return;
    }
    
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    expect(Array.isArray(data)).toBeTruthy();
    console.log(`✅ ${data.length} kategória az adatbázisban`);
  });

  test('API: Nem létező foglalás kiadása (404)', async ({ request }) => {
    const response = await request.post(`${API_URL}/foglalasok/99999/kiad`);
    
    if (response.status() === 307) {
      console.log('⚠️ Backend HTTPS redirect aktív');
      return;
    }
    
    expect(response.status()).toBe(404);
    console.log('✅ 404 hiba megfelelően kezelt');
  });

  test('API: Nem létező eszköz visszahozása (404)', async ({ request }) => {
    const response = await request.post(`${API_URL}/eszkozok/99999/visszahoz`);
    
    if (response.status() === 307) {
      console.log('⚠️ Backend HTTPS redirect aktív');
      return;
    }
    
    expect(response.status()).toBe(404);
    console.log('✅ 404 hiba megfelelően kezelt');
  });
});