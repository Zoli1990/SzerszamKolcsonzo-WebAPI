<!-- ============================================================================ -->
<!-- src/components/admin/AdminPwaDashboard.vue - PWA Admin Dashboard           -->
<!-- ============================================================================ -->

<template>
  <div class="pwa-dashboard">
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- PWA TELEPÍTÉS BANNER -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <Transition name="banner">
      <div v-if="showInstallBanner" class="pwa-install-banner">
        <div class="install-content">
          <div class="install-icon">📲</div>
          <div class="install-text">
            <strong>Telepítsd az alkalmazást!</strong>
            <p>Valós idejű értesítések és gyorsabb elérés.</p>
          </div>
        </div>
        <div class="install-actions">
          <button class="btn-install" @click="installPwa">Telepítés</button>
          <button class="btn-dismiss" @click="dismissInstallBanner">Később</button>
        </div>
      </div>
    </Transition>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- FŐKÉPERNYŐ - Értesítések & Új foglalás -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <section class="notifications-section">
      <div class="section-header">
        <div>
          <h2 class="section-title">FŐKÉPERNYŐ</h2>
          <p class="section-subtitle">Értesítések & Aktív foglalások</p>
        </div>
        <div class="header-actions">
          <span class="refresh-indicator" :class="{ active: isRefreshing }">
            {{ isRefreshing ? '🔄' : '●' }}
          </span>
          <button class="btn-refresh-mini" @click="fetchFoglalasok" :disabled="loading">🔄</button>
        </div>
      </div>

      <!-- Új foglalás értesítés -->
      <div v-if="latestPendingFoglalas" class="notification-card highlight">
        <div class="notification-header">
          <span class="notification-icon">🔔</span>
          <h3 class="notification-title">ÚJ FOGLALÁS ÉRKEZETT!</h3>
        </div>

        <div class="notification-body">
          <div class="info-row">
            <span class="label">Foglalás</span>
            <span class="value">#{{ latestPendingFoglalas.foglalasID }}</span>
          </div>
          <div class="info-row">
            <span class="label">Eszköz:</span>
            <span class="value">{{ latestPendingFoglalas.eszkozNev }}</span>
          </div>
          <div class="info-row">
            <span class="label">Ügyfél:</span>
            <span class="value">{{ latestPendingFoglalas.nev }}</span>
          </div>
          <div class="info-row">
            <span class="label">Kezdés:</span>
            <span class="value">{{ formatDateTime(latestPendingFoglalas.foglalasKezdete) }}</span>
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
          :key="foglalas.foglalasID"
          class="foglalas-card"
          :class="getStatusClass(foglalas.status)"
        >
          <!-- Státusz header -->
          <div class="card-header">
            <div class="status-badge" :class="getStatusClass(foglalas.status)">
              <span class="status-icon">{{ getStatusIcon(foglalas.status) }}</span>
              <span class="status-text">{{ getStatusText(foglalas.status) }}</span>
              <span class="status-id">(#{{ foglalas.foglalasID }})</span>
            </div>
          </div>

          <!-- Foglalás info -->
          <div class="card-body">
            <div class="info-row">
              <span class="label">Eszköz:</span>
              <span class="value bold">{{ foglalas.eszkozNev }}</span>
            </div>

            <!-- FOGLALVA státusz -->
            <template v-if="foglalas.status === 'Foglalva'">
              <div class="info-row">
                <span class="label">Ügyfél:</span>
                <span class="value">{{ foglalas.nev }}</span>
              </div>
              <div class="info-row">
                <span class="label">Kezdés:</span>
                <span class="value">{{ formatTime(foglalas.foglalasKezdete) }}</span>
              </div>
              <div class="info-row warning">
                <span class="label">Lejár:</span>
                <span class="value">{{ jovahagyhatoIg(foglalas) }}</span>
              </div>
            </template>

            <!-- KIADVA státusz -->
            <template v-else-if="foglalas.status === 'Kiadva'">
              <div class="info-row">
                <span class="label">Kiadva:</span>
                <span class="value">{{ formatTime(foglalas.kiadasIdopontja) }}</span>
              </div>
              <div class="info-row">
                <span class="label">Eltelt idő:</span>
                <span class="value primary">{{ elteltIdo(foglalas.kiadasIdopontja) }}</span>
              </div>
              <div class="info-row price">
                <span class="label">Jelenlegi díj:</span>
                <span class="value price-value">~{{ jelenlegiDij(foglalas) }} Ft</span>
              </div>
            </template>
          </div>

          <!-- Akció gombok -->
          <div class="card-actions">
            <!-- FOGLALVA → KIADVA / TÖRLÉS -->
            <template v-if="foglalas.status === 'Foglalva'">
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
            <template v-else-if="foglalas.status === 'Kiadva'">
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
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import api from '@/services/api'

// ═══════════════════════════════════════════════════════════════════════════
// STATE
// ═══════════════════════════════════════════════════════════════════════════

const foglalasok = ref([])
const loading = ref(false)
const isRefreshing = ref(false)

// PWA Install
const showInstallBanner = ref(false)
let deferredPrompt = null

// Timers
let autoRefreshInterval = null
let elapsedTimeInterval = null

// ═══════════════════════════════════════════════════════════════════════════
// COMPUTED
// ═══════════════════════════════════════════════════════════════════════════

// Legújabb "Foglalva" státuszú foglalás (amit az admin még nem hagyott jóvá)
const latestPendingFoglalas = computed(() => {
  return foglalasok.value
    .filter((f) => f.status === 'Foglalva')
    .sort((a, b) => new Date(b.letrehozasDatum) - new Date(a.letrehozasDatum))[0]
})

// Aktív foglalások: Foglalva + Kiadva
const aktivFoglalasok = computed(() => {
  const statusOrder = { Foglalva: 0, Kiadva: 1 }
  return foglalasok.value
    .filter((f) => f.status === 'Foglalva' || f.status === 'Kiadva')
    .sort((a, b) => {
      const orderA = statusOrder[a.status] ?? 99
      const orderB = statusOrder[b.status] ?? 99
      if (orderA !== orderB) return orderA - orderB
      return new Date(b.letrehozasDatum) - new Date(a.letrehozasDatum)
    })
})

const pendingCount = computed(() => {
  return foglalasok.value.filter((f) => f.status === 'Foglalva').length
})

// ═══════════════════════════════════════════════════════════════════════════
// PWA INSTALL
// ═══════════════════════════════════════════════════════════════════════════

function onBeforeInstallPrompt(e) {
  e.preventDefault()
  deferredPrompt = e
  const dismissed = sessionStorage.getItem('pwa-install-dismissed')
  if (!dismissed) {
    showInstallBanner.value = true
  }
}

async function installPwa() {
  if (!deferredPrompt) return
  deferredPrompt.prompt()
  const { outcome } = await deferredPrompt.userChoice
  if (outcome === 'accepted') {
    showInstallBanner.value = false
  }
  deferredPrompt = null
}

function dismissInstallBanner() {
  showInstallBanner.value = false
  sessionStorage.setItem('pwa-install-dismissed', 'true')
}

// ═══════════════════════════════════════════════════════════════════════════
// NOTIFICATION SOUND
// ═══════════════════════════════════════════════════════════════════════════

function playNotificationSound() {
  try {
    const audioCtx = new (window.AudioContext || window.webkitAudioContext)()
    const oscillator = audioCtx.createOscillator()
    const gainNode = audioCtx.createGain()

    oscillator.connect(gainNode)
    gainNode.connect(audioCtx.destination)

    oscillator.frequency.setValueAtTime(880, audioCtx.currentTime)
    oscillator.frequency.setValueAtTime(1108, audioCtx.currentTime + 0.15)
    gainNode.gain.setValueAtTime(0.3, audioCtx.currentTime)
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + 0.4)

    oscillator.start(audioCtx.currentTime)
    oscillator.stop(audioCtx.currentTime + 0.4)
  } catch (err) {
    console.warn('Hangjelzés nem sikerült:', err)
  }
}

watch(pendingCount, (newCount, oldCount) => {
  if (newCount > oldCount && oldCount !== undefined) {
    playNotificationSound()
    flashTitle('🔔 ÚJ FOGLALÁS!')
  }
})

let titleFlashInterval = null

function flashTitle(message) {
  const originalTitle = document.title
  let isOriginal = false

  clearInterval(titleFlashInterval)
  titleFlashInterval = setInterval(() => {
    document.title = isOriginal ? originalTitle : message
    isOriginal = !isOriginal
  }, 1000)

  setTimeout(() => {
    clearInterval(titleFlashInterval)
    document.title = originalTitle
  }, 10000)
}

// ═══════════════════════════════════════════════════════════════════════════
// FORMATTERS
// ═══════════════════════════════════════════════════════════════════════════

function formatDateTime(date) {
  if (!date) return '-'
  return new Date(date).toLocaleString('hu-HU', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function formatTime(date) {
  if (!date) return '-'
  return new Date(date).toLocaleTimeString('hu-HU', { hour: '2-digit', minute: '2-digit' })
}

/**
 * Jóváhagyási határidő: létrehozástól 15 perc
 */
function jovahagyhatoIg(foglalas) {
  if (!foglalas?.letrehozasDatum) return ''
  const created = new Date(foglalas.letrehozasDatum)
  const deadline = new Date(created.getTime() + 15 * 60 * 1000) // 15 perc
  const now = new Date()

  if (now > deadline) return '⚠️ Lejárt!'

  const diff = deadline - now
  const minutes = Math.floor(diff / (1000 * 60))
  const seconds = Math.floor((diff % (1000 * 60)) / 1000)

  return `${minutes}:${seconds.toString().padStart(2, '0')} perc`
}

function elteltIdo(kiadasIdopontja) {
  if (!kiadasIdopontja) return '-'
  const diff = Date.now() - new Date(kiadasIdopontja)
  const hours = Math.floor(diff / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))
  return `${hours}h ${minutes}m`
}

function jelenlegiDij(foglalas) {
  if (!foglalas?.kiadasIdopontja || !foglalas?.eszkozAr) return '0'
  const diffMs = Date.now() - new Date(foglalas.kiadasIdopontja)
  const diffMinutes = diffMs / (1000 * 60)
  const perMinutePrice = foglalas.eszkozAr / 60
  return Math.ceil(diffMinutes * perMinutePrice).toLocaleString('hu-HU')
}

// ═══════════════════════════════════════════════════════════════════════════
// STATUS HELPERS
// ═══════════════════════════════════════════════════════════════════════════

function getStatusIcon(status) {
  const icons = {
    Foglalva: '📌',
    Kiadva: '🔧',
    Lezart: '✅',
    Torolt: '❌',
  }
  return icons[status] || '⚪'
}

function getStatusText(status) {
  const texts = {
    Foglalva: 'FOGLALVA',
    Kiadva: 'KIADVA',
    Lezart: 'LEZÁRT',
    Torolt: 'TÖRÖLT',
  }
  return texts[status] || 'Ismeretlen'
}

function getStatusClass(status) {
  const classes = {
    Foglalva: 'status-pending',
    Kiadva: 'status-active',
    Lezart: 'status-closed',
    Torolt: 'status-deleted',
  }
  return classes[status] || ''
}

// ═══════════════════════════════════════════════════════════════════════════
// API ACTIONS
// ═══════════════════════════════════════════════════════════════════════════

async function fetchFoglalasok(silent = false) {
  if (!silent) loading.value = true
  isRefreshing.value = true

  try {
    const response = await api.get('/Foglalasok')
    const data = response.data

    if (Array.isArray(data)) {
      foglalasok.value = data
    } else if (data && Array.isArray(data.data)) {
      foglalasok.value = data.data
    } else if (data && Array.isArray(data.$values)) {
      foglalasok.value = data.$values
    } else {
      console.warn('Váratlan API válasz formátum:', typeof data, data)
      foglalasok.value = []
    }
  } catch (error) {
    console.error('Foglalások lekérése hiba:', error)
    if (!silent) alert('❌ Hiba a foglalások betöltése során!')
  } finally {
    loading.value = false
    isRefreshing.value = false
  }
}

async function handleKiadva(foglalas) {
  if (!confirm(`Kiadod az eszközt: ${foglalas.eszkozNev}?`)) return

  loading.value = true
  try {
    await api.put(`/Foglalasok/${foglalas.foglalasID}/kiadas`)
    await fetchFoglalasok(true)
    alert('✅ Eszköz kiadva!')
  } catch (error) {
    console.error('Kiadás hiba:', error)
    alert(error.response?.data?.message || '❌ Hiba történt a kiadás során!')
  } finally {
    loading.value = false
  }
}

async function handleNemJott(foglalas) {
  if (!confirm(`Törlöd a foglalást: #${foglalas.foglalasID}?`)) return

  loading.value = true
  try {
    await api.put(`/Foglalasok/${foglalas.foglalasID}/torles`)
    await fetchFoglalasok(true)
    alert('✅ Foglalás törölve!')
  } catch (error) {
    console.error('Törlés hiba:', error)
    alert(error.response?.data?.message || '❌ Hiba történt a törlés során!')
  } finally {
    loading.value = false
  }
}

async function handleVisszahozva(foglalas) {
  if (!confirm(`Visszahozta az eszközt: ${foglalas.eszkozNev}?`)) return

  loading.value = true
  try {
    await api.put(`/Foglalasok/${foglalas.foglalasID}/lezaras`)
    await fetchFoglalasok(true)
    alert('✅ Eszköz visszahozva, foglalás lezárva!')
  } catch (error) {
    console.error('Lezárás hiba:', error)
    alert(error.response?.data?.message || '❌ Hiba történt a lezárás során!')
  } finally {
    loading.value = false
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// LIFECYCLE
// ═══════════════════════════════════════════════════════════════════════════

onMounted(() => {
  fetchFoglalasok()

  // Auto-refresh: 10 másodpercenként
  autoRefreshInterval = setInterval(() => fetchFoglalasok(true), 10000)

  // Eltelt idő frissítése: 1 percenként
  elapsedTimeInterval = setInterval(() => {
    foglalasok.value = [...foglalasok.value]
  }, 60000)

  // PWA install prompt
  window.addEventListener('beforeinstallprompt', onBeforeInstallPrompt)

  if (window.matchMedia('(display-mode: standalone)').matches) {
    showInstallBanner.value = false
  }
})

onUnmounted(() => {
  clearInterval(autoRefreshInterval)
  clearInterval(elapsedTimeInterval)
  clearInterval(titleFlashInterval)
  window.removeEventListener('beforeinstallprompt', onBeforeInstallPrompt)
})
</script>

<style scoped>
/* ============================================================================ */
/* PWA DASHBOARD STYLES                                                         */
/* ============================================================================ */

.pwa-dashboard {
  padding: 0;
  background: #f5f1e8;
  min-height: 100vh;
}

/* ============================================================================ */
/* PWA INSTALL BANNER                                                           */
/* ============================================================================ */

.pwa-install-banner {
  position: sticky;
  top: 0;
  z-index: 500;
  background: linear-gradient(135deg, #6b8e23, #8ba83e);
  color: white;
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  box-shadow: 0 4px 16px rgba(107, 142, 35, 0.3);
}

.install-content {
  display: flex;
  align-items: center;
  gap: 14px;
}

.install-icon {
  font-size: 36px;
  flex-shrink: 0;
}

.install-text strong {
  display: block;
  font-size: 17px;
  margin-bottom: 4px;
}

.install-text p {
  margin: 0;
  font-size: 14px;
  opacity: 0.9;
}

.install-actions {
  display: flex;
  gap: 10px;
}

.btn-install {
  flex: 1;
  padding: 14px;
  background: white;
  color: #6b8e23;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  min-height: 48px;
  transition: all 0.2s;
  -webkit-tap-highlight-color: transparent;
}

.btn-install:active {
  transform: scale(0.97);
}

.btn-dismiss {
  padding: 14px 20px;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-radius: 10px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  min-height: 48px;
  -webkit-tap-highlight-color: transparent;
}

.btn-dismiss:active {
  transform: scale(0.97);
}

/* Banner transition */
.banner-enter-active,
.banner-leave-active {
  transition: all 0.4s ease;
}

.banner-enter-from {
  transform: translateY(-100%);
  opacity: 0;
}

.banner-leave-to {
  transform: translateY(-100%);
  opacity: 0;
}

/* ============================================================================ */
/* SECTION HEADER                                                               */
/* ============================================================================ */

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.refresh-indicator {
  font-size: 12px;
  color: #10b981;
  transition: all 0.3s;
}

.refresh-indicator.active {
  font-size: 16px;
  animation: spin 1s linear infinite;
}

.btn-refresh-mini {
  width: 44px;
  height: 44px;
  border: 2px solid #e8dcc8;
  border-radius: 50%;
  background: white;
  font-size: 18px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  -webkit-tap-highlight-color: transparent;
}

.btn-refresh-mini:active {
  transform: scale(0.95);
}

/* ============================================================================ */
/* SECTIONS                                                                     */
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
  margin: 0;
  font-size: 16px;
  color: #6b5d4f;
}

/* ============================================================================ */
/* NOTIFICATION CARD (Highlighted)                                              */
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
/* ACTION BUTTONS                                                               */
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
  -webkit-tap-highlight-color: transparent;
  touch-action: manipulation;
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

.btn-action:active:not(:disabled) {
  transform: scale(0.97);
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
}
.btn-reject {
  background: #f44336;
  color: white;
}
.btn-reject:hover:not(:disabled) {
  background: #da190b;
}
.btn-return {
  background: #2196f3;
  color: white;
}
.btn-return:hover:not(:disabled) {
  background: #0b7dda;
}

/* ============================================================================ */
/* FOGLALÁS CARDS                                                               */
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
/* EMPTY STATES                                                                 */
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
/* LOADING OVERLAY                                                              */
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
/* RESPONSIVE                                                                   */
/* ============================================================================ */

@media (max-width: 768px) {
  .pwa-dashboard {
    padding-bottom: env(safe-area-inset-bottom, 20px);
  }

  .notifications-section,
  .active-section {
    padding: 16px 14px;
  }

  .section-title {
    font-size: 19px;
  }
  .section-subtitle {
    font-size: 14px;
  }

  .notification-card {
    padding: 16px;
    border-radius: 10px;
  }
  .notification-card.highlight {
    border-width: 2px;
  }
  .notification-card.empty {
    padding: 36px 16px;
  }

  .notification-header {
    gap: 10px;
    margin-bottom: 14px;
  }
  .notification-icon {
    font-size: 28px;
  }
  .notification-title {
    font-size: 17px;
    line-height: 1.3;
  }

  .info-row {
    padding: 9px 0;
    font-size: 15px;
    gap: 8px;
    flex-wrap: wrap;
  }

  .info-row .label {
    font-size: 15px;
    min-width: 90px;
    flex-shrink: 0;
  }
  .info-row .value {
    font-size: 15px;
    flex: 1;
    word-break: break-word;
  }
  .info-row .price-value {
    font-size: 19px;
  }

  .notification-actions,
  .card-actions {
    gap: 10px;
    margin-top: 14px;
  }

  .btn-action {
    min-height: 60px;
    min-width: 44px;
    padding: 14px 12px;
    font-size: 15px;
    border-radius: 10px;
  }

  .btn-action.small {
    min-height: 50px;
    padding: 12px 14px;
  }
  .btn-icon {
    font-size: 22px;
  }
  .btn-text {
    font-size: 16px;
  }
  .btn-subtext {
    font-size: 11px;
  }

  .foglalasok-list {
    gap: 12px;
  }
  .foglalas-card {
    padding: 14px;
    border-radius: 10px;
    border-left-width: 3px;
  }
  .card-header {
    margin-bottom: 10px;
  }
  .status-badge {
    padding: 6px 10px;
    font-size: 13px;
  }

  .empty-state {
    padding: 48px 20px;
  }
  .empty-icon {
    font-size: 56px;
  }
  .empty-text {
    font-size: 16px;
  }
}

@media (max-width: 375px) {
  .section-title {
    font-size: 17px;
  }
  .section-subtitle {
    font-size: 13px;
  }
  .notification-title {
    font-size: 16px;
  }

  .notifications-section,
  .active-section {
    padding: 14px 12px;
  }

  .notification-card,
  .foglalas-card {
    padding: 14px 12px;
  }

  .info-row {
    padding: 8px 0;
    font-size: 14px;
  }
  .info-row .label,
  .info-row .value {
    font-size: 14px;
    min-width: 80px;
  }
  .info-row .price-value {
    font-size: 18px;
  }

  .btn-action {
    min-height: 56px;
    padding: 12px 10px;
    font-size: 14px;
  }
  .btn-action.small {
    min-height: 48px;
  }
  .btn-text {
    font-size: 15px;
  }
  .btn-subtext {
    font-size: 10px;
  }
  .btn-icon {
    font-size: 20px;
  }

  .status-badge {
    padding: 5px 8px;
    font-size: 12px;
  }

  /* Install banner kompaktabb */
  .pwa-install-banner {
    padding: 14px 16px;
    gap: 12px;
  }
  .install-icon {
    font-size: 30px;
  }
  .install-text strong {
    font-size: 15px;
  }
  .install-text p {
    font-size: 13px;
  }
}

@media (max-width: 768px) and (orientation: landscape) {
  .notifications-section,
  .active-section {
    padding: 12px 16px;
  }

  .btn-action {
    min-height: 52px;
  }
  .btn-action.small {
    min-height: 46px;
  }
  .foglalasok-list {
    gap: 10px;
  }
  .foglalas-card {
    padding: 12px 14px;
  }
  .empty-state {
    padding: 32px 20px;
  }
  .empty-icon {
    font-size: 48px;
  }
}

@media (min-width: 769px) and (max-width: 1024px) {
  .notifications-section,
  .active-section {
    padding: 24px 20px;
  }

  .notification-card {
    max-width: 640px;
    margin-left: auto;
    margin-right: auto;
  }
  .btn-action {
    min-height: 68px;
    padding: 16px 20px;
  }
  .btn-text {
    font-size: 18px;
  }

  .foglalasok-list {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
  }

  /* Install banner horizontal */
  .pwa-install-banner {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }

  .install-actions {
    flex-shrink: 0;
  }
}

/* ============================================================================ */
/* ACCESSIBILITY                                                                */
/* ============================================================================ */

@media (prefers-contrast: high) {
  .btn-action {
    border: 2px solid currentColor;
  }
  .status-badge {
    border: 1px solid currentColor;
  }
  .notification-card {
    border: 2px solid #333;
  }
}

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
  .banner-enter-active,
  .banner-leave-active {
    transition: none !important;
  }
}
</style>
