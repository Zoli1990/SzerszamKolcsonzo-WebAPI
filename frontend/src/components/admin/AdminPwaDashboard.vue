<!-- ============================================================================ -->
<!-- src/components/admin/AdminPwaDashboard.vue - Notification-focused PWA UI -->
<!-- ============================================================================ -->

<template>
  <div class="pwa-dashboard">
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- FŐKÉPERNYŐ - Értesítések & Új foglalás -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <section class="notifications-section">
      <h2 class="section-title">FŐKÉPERNYŐ</h2>
      <p class="section-subtitle">Értesítések & Aktív foglalások</p>

      <!-- Új foglalás értesítés -->
      <div v-if="latestPendingFoglalas" class="notification-card highlight">
        <div class="notification-header">
          <span class="notification-icon">🔔</span>
          <h3 class="notification-title">ÚJ FOGLALÁS ÉRKEZETT!</h3>
        </div>

        <div class="notification-body">
          <div class="info-row">
            <span class="label">Foglalás</span>
            <span class="value">#{{ latestPendingFoglalas.id }}</span>
          </div>
          <div class="info-row">
            <span class="label">Eszköz:</span>
            <span class="value">{{ latestPendingFoglalas.eszkoz?.nev }}</span>
          </div>
          <div class="info-row">
            <span class="label">Ügyfél:</span>
            <span class="value">{{ latestPendingFoglalas.felhasznalo?.nev }}</span>
          </div>
          <div class="info-row">
            <span class="label">Kezdés:</span>
            <span class="value">{{ formatDateTime(latestPendingFoglalas.kezdetDatum) }}</span>
          </div>
          <div class="info-row warning">
            <span class="label">Jóváhagyható:</span>
            <span class="value">{{ jovahagyhatoIg(latestPendingFoglalas) }}</span>
          </div>
        </div>

        <div class="notification-actions">
          <button
            class="btn-action btn-approve"
            @click="handleKiadva(latestPendingFoglalas)"
            :disabled="loading"
          >
            <span class="btn-icon">✅</span>
            <span class="btn-text">KIADVA</span>
            <span class="btn-subtext">(Megjelent)</span>
          </button>
          <button
            class="btn-action btn-reject"
            @click="handleNemJott(latestPendingFoglalas)"
            :disabled="loading"
          >
            <span class="btn-icon">❌</span>
            <span class="btn-text">NEM JÖTT</span>
            <span class="btn-subtext">(Törlés)</span>
          </button>
        </div>
      </div>

      <!-- Nincs új foglalás -->
      <div v-else class="notification-card empty">
        <div class="empty-icon">📭</div>
        <p class="empty-text">Nincs új foglalás</p>
      </div>
    </section>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- AKTÍV FOGLALÁSOK - Kártya lista -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <section class="active-section">
      <h2 class="section-title">AKTÍV FOGLALÁSOK ({{ aktivFoglalasok.length }})</h2>

      <!-- Foglalás kártyák -->
      <div class="foglalasok-list">
        <div
          v-for="foglalas in aktivFoglalasok"
          :key="foglalas.id"
          class="foglalas-card"
          :class="getStatusClass(foglalas.status)"
        >
          <!-- Státusz header -->
          <div class="card-header">
            <div class="status-badge" :class="getStatusClass(foglalas.status)">
              <span class="status-icon">{{ getStatusIcon(foglalas.status) }}</span>
              <span class="status-text">{{ getStatusText(foglalas.status) }}</span>
              <span class="status-id">(#{{ foglalas.id }})</span>
            </div>
          </div>

          <!-- Foglalás info -->
          <div class="card-body">
            <div class="info-row">
              <span class="label">Eszköz:</span>
              <span class="value bold">{{ foglalas.eszkoz?.nev }}</span>
            </div>

            <!-- VÁRAKOZIK státusz -->
            <template v-if="foglalas.status === 1">
              <div class="info-row">
                <span class="label">Ügyfél:</span>
                <span class="value">{{ foglalas.felhasznalo?.nev }}</span>
              </div>
              <div class="info-row">
                <span class="label">Kezdés:</span>
                <span class="value">{{ formatTime(foglalas.kezdetDatum) }}</span>
                <span class="label">Lejár:</span>
                <span class="value warning">{{ jovahagyhatoIg(foglalas) }}</span>
              </div>
            </template>

            <!-- KIADVA státusz -->
            <template v-else-if="foglalas.status === 2">
              <div class="info-row">
                <span class="label">Kiadva:</span>
                <span class="value">{{ formatTime(foglalas.kiadasDatum) }}</span>
                <span class="label">Eltelt idő:</span>
                <span class="value primary">{{ elteltIdo(foglalas.kiadasDatum) }}</span>
              </div>
              <div class="info-row price">
                <span class="label">Jelenlegi díj:</span>
                <span class="value price-value">~{{ jelenlegiDij(foglalas) }} Ft</span>
              </div>
            </template>
          </div>

          <!-- Akció gombok -->
          <div class="card-actions">
            <!-- VÁRAKOZIK → KIADVA / NEM JÖTT -->
            <template v-if="foglalas.status === 1">
              <button
                class="btn-action btn-approve small"
                @click="handleKiadva(foglalas)"
                :disabled="loading"
              >
                ✅ KIADVA
              </button>
              <button
                class="btn-action btn-reject small"
                @click="handleNemJott(foglalas)"
                :disabled="loading"
              >
                ❌ NEM JÖTT
              </button>
            </template>

            <!-- KIADVA → VISSZAHOZVA -->
            <template v-else-if="foglalas.status === 2">
              <button
                class="btn-action btn-return"
                @click="handleVisszahozva(foglalas)"
                :disabled="loading"
              >
                ↩️ VISSZAHOZVA
              </button>
            </template>
          </div>
        </div>
      </div>

      <!-- Nincs aktív foglalás -->
      <div v-if="aktivFoglalasok.length === 0" class="empty-state">
        <div class="empty-icon">📭</div>
        <p class="empty-text">Nincs aktív foglalás</p>
      </div>
    </section>

    <!-- Loading overlay -->
    <div v-if="loading" class="loading-overlay">
      <div class="loading-spinner"></div>
      <p>Művelet folyamatban...</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_URL || 'https://szerszamkolcsonzo.runasp.net/api'

// State
const foglalasok = ref([])
const loading = ref(false)

// Computed
const latestPendingFoglalas = computed(() => {
  return foglalasok.value
    .filter((f) => f.status === 1) // VÁRAKOZIK (nem előfoglalás!)
    .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))[0]
})

const aktivFoglalasok = computed(() => {
  return foglalasok.value
    .filter((f) => f.status === 1 || f.status === 2) // VÁRAKOZIK vagy KIADVA
    .sort((a, b) => {
      // VÁRAKOZIK first, then KIADVA
      if (a.status !== b.status) return a.status - b.status
      return new Date(b.createdAt) - new Date(a.createdAt)
    })
})

// Helpers
const formatDateTime = (date) => {
  if (!date) return ''
  const d = new Date(date)
  return d.toLocaleString('hu-HU', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const formatTime = (date) => {
  if (!date) return ''
  const d = new Date(date)
  return d.toLocaleTimeString('hu-HU', { hour: '2-digit', minute: '2-digit' })
}

const jovahagyhatoIg = (foglalas) => {
  if (!foglalas?.kezdetDatum) return ''
  const start = new Date(foglalas.kezdetDatum)
  const deadline = new Date(start.getTime() + 2.25 * 60 * 60 * 1000) // 2h 15m
  const now = new Date()

  if (now > deadline) return '⚠️ Lejárt!'

  const diff = deadline - now
  const hours = Math.floor(diff / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))

  return `${formatTime(deadline)} (${hours}:${minutes.toString().padStart(2, '0')}-ig)`
}

const elteltIdo = (kiadasDatum) => {
  if (!kiadasDatum) return ''
  const start = new Date(kiadasDatum)
  const now = new Date()
  const diff = now - start

  const hours = Math.floor(diff / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))

  return `${hours}h ${minutes}m`
}

const jelenlegiDij = (foglalas) => {
  if (!foglalas?.kiadasDatum || !foglalas?.eszkoz?.ar) return 0

  const start = new Date(foglalas.kiadasDatum)
  const now = new Date()
  const diffMs = now - start
  const diffHours = diffMs / (1000 * 60 * 60)

  const perHourPrice = foglalas.eszkoz.ar / 24
  const currentPrice = Math.ceil(diffHours * perHourPrice)

  return currentPrice.toLocaleString('hu-HU')
}

const getStatusIcon = (status) => {
  const icons = {
    0: '📅', // ELŐFOGLALÁS (nem jelenik meg PWA-ban)
    1: '🟠', // VÁRAKOZIK
    2: '🔵', // KIADVA
    3: '🟢', // LEZÁRT
    4: '🔴', // TÖRÖLT
  }
  return icons[status] || '⚪'
}

const getStatusText = (status) => {
  const texts = {
    0: 'ELŐFOGLALÁS',
    1: 'VÁRAKOZIK',
    2: 'KIADVA',
    3: 'LEZÁRT',
    4: 'TÖRÖLT',
  }
  return texts[status] || 'Ismeretlen'
}

const getStatusClass = (status) => {
  const classes = {
    0: 'status-prefoglalas',
    1: 'status-pending',
    2: 'status-active',
    3: 'status-closed',
    4: 'status-deleted',
  }
  return classes[status] || ''
}

// Actions
const handleKiadva = async (foglalas) => {
  if (!confirm(`Kiadod az eszközt: ${foglalas.eszkoz?.nev}?`)) return

  loading.value = true
  try {
    // Status 1 (VÁRAKOZIK) → 2 (KIADVA)
    await axios.put(`${API_BASE}/Foglalasok/${foglalas.id}/kiadas`)
    await fetchFoglalasok()
    alert('✅ Eszköz kiadva!')
  } catch (error) {
    console.error('Kiadás hiba:', error)
    alert('❌ Hiba történt a kiadás során!')
  } finally {
    loading.value = false
  }
}

const handleNemJott = async (foglalas) => {
  if (!confirm(`Törlöd a foglalást: #${foglalas.id}?`)) return

  loading.value = true
  try {
    // Status 1 (VÁRAKOZIK) → 4 (TÖRÖLT)
    await axios.put(`${API_BASE}/Foglalasok/${foglalas.id}/torles`)
    await fetchFoglalasok()
    alert('✅ Foglalás törölve!')
  } catch (error) {
    console.error('Törlés hiba:', error)
    alert('❌ Hiba történt a törlés során!')
  } finally {
    loading.value = false
  }
}

const handleVisszahozva = async (foglalas) => {
  if (!confirm(`Visszahozta az eszközt: ${foglalas.eszkoz?.nev}?`)) return

  loading.value = true
  try {
    // Status 2 (KIADVA) → 3 (LEZÁRT)
    await axios.put(`${API_BASE}/Foglalasok/${foglalas.id}/lezaras`)
    await fetchFoglalasok()
    alert('✅ Eszköz visszahozva, foglalás lezárva!')
  } catch (error) {
    console.error('Lezárás hiba:', error)
    alert('❌ Hiba történt a lezárás során!')
  } finally {
    loading.value = false
  }
}

const fetchFoglalasok = async () => {
  loading.value = true
  try {
    const token = localStorage.getItem('auth_token')
    const response = await axios.get(`${API_BASE}/Foglalasok`, {
      headers: { Authorization: `Bearer ${token}` },
    })
    foglalasok.value = response.data
    console.log('[AdminPwaDashboard] Foglalások betöltve:', foglalasok.value.length)
  } catch (error) {
    console.error('Foglalások lekérése hiba:', error)
    alert('❌ Hiba a foglalások betöltése során!')
  } finally {
    loading.value = false
  }
}

// Lifecycle
onMounted(() => {
  fetchFoglalasok()

  // Auto-refresh every 30 seconds
  const interval = setInterval(fetchFoglalasok, 30000)

  // Cleanup
  return () => clearInterval(interval)
})
</script>

<style scoped>
/* ============================================================================ */
/* PWA DASHBOARD STYLES */
/* ============================================================================ */

.pwa-dashboard {
  padding: 0;
  background: #f5f1e8;
  min-height: 100vh;
}

/* ============================================================================ */
/* SECTIONS */
/* ============================================================================ */

.notifications-section,
.active-section {
  padding: 20px 16px;
}

.notifications-section {
  background: white;
  border-bottom: 2px solid #e8dcc8;
}

.section-title {
  margin: 0 0 4px 0;
  font-size: 22px;
  font-weight: 700;
  color: #3d2f1f;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.section-subtitle {
  margin: 0 0 20px 0;
  font-size: 16px;
  color: #6b5d4f;
}

/* ============================================================================ */
/* NOTIFICATION CARD (Highlighted) */
/* ============================================================================ */

.notification-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.notification-card.highlight {
  border: 3px solid #ff9800;
  background: #fff9f0;
}

.notification-card.empty {
  text-align: center;
  padding: 40px 20px;
  background: #f9f9f9;
}

.notification-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.notification-icon {
  font-size: 32px;
  animation: ring 2s ease-in-out infinite;
}

@keyframes ring {
  0%,
  100% {
    transform: rotate(0deg);
  }
  10%,
  30% {
    transform: rotate(-10deg);
  }
  20%,
  40% {
    transform: rotate(10deg);
  }
}

.notification-title {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: #ff9800;
  text-transform: uppercase;
}

.notification-body {
  margin-bottom: 20px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #e8dcc8;
  font-size: 16px;
}

.info-row:last-child {
  border-bottom: none;
}

.info-row .label {
  font-weight: 600;
  color: #6b5d4f;
}

.info-row .value {
  font-weight: 700;
  color: #3d2f1f;
  text-align: right;
}

.info-row.warning .value {
  color: #ff9800;
}

.info-row.price {
  padding: 12px 0;
  margin-top: 8px;
  border-top: 2px solid #e8dcc8;
}

.info-row .price-value {
  font-size: 20px;
  color: #6b8e23;
}

/* ============================================================================ */
/* ACTION BUTTONS */
/* ============================================================================ */

.notification-actions,
.card-actions {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}

.btn-action {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 16px 12px;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  min-height: 80px;
}

.btn-action.small {
  flex-direction: row;
  gap: 8px;
  min-height: 52px;
  padding: 12px 16px;
}

.btn-action:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-icon {
  font-size: 24px;
}

.btn-text {
  font-size: 18px;
}

.btn-subtext {
  font-size: 12px;
  opacity: 0.8;
}

.btn-approve {
  background: #4caf50;
  color: white;
}

.btn-approve:hover:not(:disabled) {
  background: #45a049;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(76, 175, 80, 0.3);
}

.btn-reject {
  background: #f44336;
  color: white;
}

.btn-reject:hover:not(:disabled) {
  background: #da190b;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(244, 67, 54, 0.3);
}

.btn-return {
  background: #2196f3;
  color: white;
}

.btn-return:hover:not(:disabled) {
  background: #0b7dda;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(33, 150, 243, 0.3);
}

/* ============================================================================ */
/* FOGLALÁS CARDS */
/* ============================================================================ */

.foglalasok-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.foglalas-card {
  background: white;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
  border-left: 4px solid #ccc;
}

.foglalas-card.status-pending {
  border-left-color: #ff9800;
}

.foglalas-card.status-active {
  border-left-color: #2196f3;
}

.card-header {
  margin-bottom: 12px;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 700;
  background: #f0f0f0;
}

.status-badge.status-pending {
  background: #fff3e0;
  color: #f57c00;
}

.status-badge.status-active {
  background: #e3f2fd;
  color: #1976d2;
}

.status-icon {
  font-size: 16px;
}

.status-id {
  font-size: 12px;
  opacity: 0.8;
}

.card-body .value.bold {
  font-size: 18px;
  color: #3d2f1f;
}

.card-body .value.primary {
  color: #2196f3;
  font-weight: 700;
}

/* ============================================================================ */
/* EMPTY STATES */
/* ============================================================================ */

.empty-state,
.notification-card.empty {
  text-align: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 64px;
  margin-bottom: 16px;
}

.empty-text {
  margin: 0;
  font-size: 18px;
  color: #6b5d4f;
}

/* ============================================================================ */
/* LOADING OVERLAY */
/* ============================================================================ */

.loading-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 20px;
  z-index: 9999;
  color: white;
  font-size: 18px;
  font-weight: 600;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 4px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* ============================================================================ */
/* RESPONSIVE - TELJES MOBIL OPTIMALIZÁLÁS */
/* ============================================================================ */

/* ============================================================================
   📱 MOBIL (max-width: 768px) - FŐSODOR
   ============================================================================ */
@media (max-width: 768px) {
  /* ─────────────────────────────────────────────────────────────────────
     1. DASHBOARD LAYOUT
     ───────────────────────────────────────────────────────────────────── */
  .pwa-dashboard {
    /* Padding bottom a biztonság kedvéért (iOS safe area) */
    padding-bottom: env(safe-area-inset-bottom, 20px);
  }

  /* ─────────────────────────────────────────────────────────────────────
     2. SECTIONS - Kompaktabb padding
     ───────────────────────────────────────────────────────────────────── */
  .notifications-section,
  .active-section {
    padding: 16px 14px; /* 20px→16px top/bottom, 16px→14px oldalt */
  }

  /* ─────────────────────────────────────────────────────────────────────
     3. TYPOGRAPHY - Kisebb méret mobilra
     ───────────────────────────────────────────────────────────────────── */
  .section-title {
    font-size: 19px; /* 22px → 19px (3px csökkentés) */
    margin-bottom: 6px; /* 4px → 6px (jobb lélegzés) */
    letter-spacing: 0.3px; /* Kisebb tracking */
  }

  .section-subtitle {
    font-size: 14px; /* 16px → 14px */
    margin-bottom: 16px; /* 20px → 16px */
  }

  /* ─────────────────────────────────────────────────────────────────────
     4. NOTIFICATION CARD - Highlight kártya optimalizálás
     ───────────────────────────────────────────────────────────────────── */
  .notification-card {
    padding: 16px; /* 20px → 16px */
    border-radius: 10px; /* 12px → 10px (kevésbé kerek) */
  }

  .notification-card.highlight {
    border-width: 2px; /* 3px → 2px (vékonyabb) */
  }

  .notification-card.empty {
    padding: 36px 16px; /* 40px → 36px */
  }

  /* ─────────────────────────────────────────────────────────────────────
     5. NOTIFICATION HEADER
     ───────────────────────────────────────────────────────────────────── */
  .notification-header {
    gap: 10px; /* 12px → 10px */
    margin-bottom: 14px; /* 16px → 14px */
  }

  .notification-icon {
    font-size: 28px; /* 32px → 28px */
  }

  .notification-title {
    font-size: 17px; /* 20px → 17px */
    line-height: 1.3; /* Jobb olvashatóság */
  }

  /* ─────────────────────────────────────────────────────────────────────
     6. NOTIFICATION BODY
     ───────────────────────────────────────────────────────────────────── */
  .notification-body {
    margin-bottom: 16px; /* 20px → 16px */
  }

  /* ─────────────────────────────────────────────────────────────────────
     7. INFO ROWS - KRITIKUS JAVÍTÁS!
     ───────────────────────────────────────────────────────────────────── */
  .info-row {
    padding: 9px 0; /* 10px → 9px */
    font-size: 15px; /* 16px → 15px */
    gap: 8px; /* Space label és value között */
    flex-wrap: wrap; /* 🔑 FONTOS: engedélyezi a tördelést! */
  }

  .info-row .label {
    font-size: 15px;
    font-weight: 600;
    min-width: 90px; /* 🔑 Minimum szélesség konzisztencia miatt */
    flex-shrink: 0; /* Ne zsugorodjon */
  }

  .info-row .value {
    font-size: 15px;
    font-weight: 700;
    text-align: right;
    flex: 1; /* Foglalja el a maradék helyet */
    word-break: break-word; /* 🔑 FONTOS: hosszú szavak tördelése */
    hyphens: auto; /* Szóelválasztás ha kell */
  }

  .info-row.warning .value {
    font-size: 15px; /* Konzisztens méret */
  }

  .info-row.price {
    padding: 10px 0; /* 12px → 10px */
    margin-top: 8px;
  }

  .info-row .price-value {
    font-size: 19px; /* 20px → 19px */
  }

  /* ─────────────────────────────────────────────────────────────────────
     8. ACTION BUTTONS - TOUCH OPTIMALIZÁLÁS!
     ───────────────────────────────────────────────────────────────────── */
  .notification-actions,
  .card-actions {
    gap: 10px; /* 12px → 10px */
    margin-top: 14px; /* 16px → 14px */
  }

  .btn-action {
    /* 🎯 iOS guideline: minimum 44pt (≈44px) touch target
       🎯 Ajánlott: 48-56px a kényelmes használathoz */
    min-height: 60px; /* 80px → 60px (még mindig bőven touch-friendly!) */
    min-width: 44px; /* iOS minimum */
    padding: 14px 12px; /* 16px → 14px */
    font-size: 15px; /* 16px → 15px */
    border-radius: 10px; /* 12px → 10px */
    gap: 5px; /* 4px → 5px */

    /* Touch feedback */
    -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
    touch-action: manipulation; /* Gyorsabb tap response */
  }

  .btn-action.small {
    flex-direction: row; /* Megtartjuk a horizontal layoutot */
    min-height: 50px; /* 52px → 50px (még mindig 44px felett!) */
    padding: 12px 14px; /* Kissé több oldalsó padding */
    gap: 8px;
  }

  .btn-icon {
    font-size: 22px; /* 24px → 22px */
  }

  .btn-text {
    font-size: 16px; /* 18px → 16px */
    font-weight: 700; /* Megtartjuk a vastag betűt */
    line-height: 1.2; /* Szorosabb line-height */
  }

  .btn-subtext {
    font-size: 11px; /* 12px → 11px */
    opacity: 0.85; /* 0.8 → 0.85 (kicsit jobban látszik) */
  }

  /* ─────────────────────────────────────────────────────────────────────
     9. FOGLALÁS CARDS - Aktív foglalások lista
     ───────────────────────────────────────────────────────────────────── */
  .foglalasok-list {
    gap: 12px; /* 16px → 12px */
  }

  .foglalas-card {
    padding: 14px; /* 16px → 14px */
    border-radius: 10px; /* 12px → 10px */
    border-left-width: 3px; /* 4px → 3px */
  }

  .card-header {
    margin-bottom: 10px; /* 12px → 10px */
  }

  .status-badge {
    padding: 6px 10px; /* 6px 12px → 6px 10px */
    font-size: 13px; /* 14px → 13px */
    gap: 5px; /* 6px → 5px */
    border-radius: 18px; /* 20px → 18px */
  }

  .status-icon {
    font-size: 14px;
  }

  .status-text {
    font-size: 13px;
  }

  .status-id {
    font-size: 12px;
  }

  .card-body {
    margin-bottom: 10px; /* Kompaktabb */
  }

  /* ─────────────────────────────────────────────────────────────────────
     10. EMPTY STATES
     ───────────────────────────────────────────────────────────────────── */
  .empty-state {
    padding: 48px 20px; /* 60px → 48px */
  }

  .empty-icon {
    font-size: 56px; /* 64px → 56px */
  }

  .empty-text {
    font-size: 16px; /* 18px → 16px */
    margin-top: 12px;
  }

  /* ─────────────────────────────────────────────────────────────────────
     11. LOADING OVERLAY
     ───────────────────────────────────────────────────────────────────── */
  .loading-overlay {
    padding: 20px;
  }

  .loading-overlay p {
    font-size: 15px; /* 18px → 15px */
    margin-top: 12px;
  }

  .loading-spinner {
    width: 44px; /* Láthatóbb méret */
    height: 44px;
    border-width: 4px;
  }
}

/* ============================================================================
   📱 EXTRA KICSI MOBILOK (max-width: 375px)
   iPhone SE, iPhone 12 mini, kis Android készülékek
   ============================================================================ */
@media (max-width: 375px) {
  /* Typography még kisebb */
  .section-title {
    font-size: 17px; /* 19px → 17px */
  }

  .section-subtitle {
    font-size: 13px; /* 14px → 13px */
  }

  .notification-title {
    font-size: 16px; /* 17px → 16px */
  }

  /* Padding csökkentés */
  .notifications-section,
  .active-section {
    padding: 14px 12px; /* 16px 14px → 14px 12px */
  }

  .notification-card,
  .foglalas-card {
    padding: 14px 12px; /* 16px/14px → 14px 12px */
  }

  /* Info rows kompaktabb */
  .info-row {
    padding: 8px 0; /* 9px → 8px */
    font-size: 14px; /* 15px → 14px */
  }

  .info-row .label,
  .info-row .value {
    font-size: 14px;
    min-width: 80px; /* 90px → 80px */
  }

  .info-row .price-value {
    font-size: 18px; /* 19px → 18px */
  }

  /* Gombok - MÉG MINDIG touch-friendly! */
  .btn-action {
    min-height: 56px; /* 60px → 56px */
    padding: 12px 10px; /* Kisebb padding */
    font-size: 14px; /* 15px → 14px */
  }

  .btn-action.small {
    min-height: 48px; /* 50px → 48px (még 44px felett!) */
    padding: 11px 12px;
  }

  .btn-text {
    font-size: 15px; /* 16px → 15px */
  }

  .btn-subtext {
    font-size: 10px; /* 11px → 10px */
  }

  .btn-icon {
    font-size: 20px; /* 22px → 20px */
  }

  /* Status badge kisebb */
  .status-badge {
    padding: 5px 8px; /* 6px 10px → 5px 8px */
    font-size: 12px; /* 13px → 12px */
  }

  .status-text {
    font-size: 12px;
  }

  .status-id {
    font-size: 11px;
  }
}

/* ============================================================================
   📱 LANDSCAPE MODE - Fekvő mobil nézet
   ============================================================================ */
@media (max-width: 768px) and (orientation: landscape) {
  /* Kevesebb vertical padding landscape-ben */
  .notifications-section,
  .active-section {
    padding: 12px 16px; /* Csökkentett top/bottom */
  }

  /* Gombok alacsonyabbak landscape-ben */
  .btn-action {
    min-height: 52px; /* 60px → 52px landscape-ben */
  }

  .btn-action.small {
    min-height: 46px;
  }

  /* Foglalás kártyák kompaktabbak */
  .foglalasok-list {
    gap: 10px;
  }

  .foglalas-card {
    padding: 12px 14px;
  }

  /* Empty states kisebb */
  .empty-state {
    padding: 32px 20px;
  }

  .empty-icon {
    font-size: 48px;
  }
}

/* ============================================================================
   📱 TABLET (769px - 1024px)
   iPad, Android tabletek
   ============================================================================ */
@media (min-width: 769px) and (max-width: 1024px) {
  /* Sections több padding */
  .notifications-section,
  .active-section {
    padding: 24px 20px;
  }

  /* Notification card központosítva, max-width */
  .notification-card {
    max-width: 640px;
    margin-left: auto;
    margin-right: auto;
  }

  /* Gombok nagyobbak tableten */
  .btn-action {
    min-height: 68px;
    padding: 16px 20px;
    font-size: 17px;
  }

  .btn-text {
    font-size: 18px;
  }

  /* Foglalások GRID layout (2 oszlop) */
  .foglalasok-list {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
  }

  /* Typography */
  .section-title {
    font-size: 21px;
  }

  .section-subtitle {
    font-size: 15px;
  }
}

/* ============================================================================
   🎨 AKADÁLYMENTESÍTÉS & PREFERENCIÁK
   ============================================================================ */

/* High Contrast Mode */
@media (prefers-contrast: high) {
  .btn-action {
    border: 2px solid currentColor;
    font-weight: 800;
  }

  .status-badge {
    border: 1px solid currentColor;
  }

  .notification-card {
    border: 2px solid #333;
  }
}

/* Reduced Motion - Animációk kikapcsolása */
@media (prefers-reduced-motion: reduce) {
  .btn-action,
  .foglalas-card,
  .notification-card {
    transition: none !important;
  }

  .notification-icon {
    animation: none !important;
  }

  .loading-spinner {
    animation: none !important;
  }

  .btn-action:hover {
    transform: none !important;
  }
}

/* Dark Mode Felkészülés (később implementálható) */
@media (prefers-color-scheme: dark) {
  /* Később bekapcsolható dark mode támogatás
  .pwa-dashboard {
    background: #1a1a1a;
    color: #f0f0f0;
  }
  .notification-card {
    background: #2a2a2a;
    border-color: #3a3a3a;
  }
  */
}
</style>
