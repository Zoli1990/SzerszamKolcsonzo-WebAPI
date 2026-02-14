<template>
  <div class="admin-dashboard-mobile">
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- HEADER -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <header class="dashboard-header">
      <div class="header-icon">🔔</div>
      <h1 class="header-title">Admin Dashboard</h1>
    </header>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- TARTALOM -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <main class="dashboard-content">
      <h2 class="page-title">FŐKÉPERNYŐ</h2>
      <p class="page-subtitle">Értesítések & Aktív foglalások</p>

      <!-- ═══════════════════════════════════════════════════════════════════ -->
      <!-- ÚJ FOGLALÁS ÉRTESÍTÉS -->
      <!-- ═══════════════════════════════════════════════════════════════════ -->
      <div v-if="newBookings.length > 0" class="notifications-section">
        <div 
          v-for="booking in newBookings" 
          :key="booking.foglalasID"
          class="notification-card"
        >
          <div class="notification-header">
            <span class="notification-icon">🔔</span>
            <div class="notification-title">ÚJ FOGLALÁS ÉRKEZETT!</div>
          </div>
          
          <div class="notification-info">
            <p><strong>Foglalás #{{ booking.foglalasID }}</strong></p>
            <p><strong>Ügyfél:</strong> {{ booking.nev }}</p>
            <p><strong>Eszköz:</strong> {{ booking.eszkozNev }}</p>
            <p><strong>Kezdés:</strong> {{ formatDateTime(booking.foglalasKezdete) }}</p>
            
            <div v-if="booking.timeRemaining" class="timer-text">
              ⏰ Lejár: {{ booking.timeRemaining }}
            </div>
          </div>

          <div class="button-group">
            <button 
              class="btn btn-success" 
              @click="handleKiadva(booking)"
              :disabled="actionLoading === booking.foglalasID"
            >
              ✅ KIADVA
              <span class="btn-subtitle">Megjelent</span>
            </button>
            <button 
              class="btn btn-danger" 
              @click="handleNemJott(booking)"
              :disabled="actionLoading === booking.foglalasID"
            >
              ❌ NEM JÖTT
              <span class="btn-subtitle">Törlés</span>
            </button>
          </div>
        </div>
      </div>

      <!-- ═══════════════════════════════════════════════════════════════════ -->
      <!-- AKTÍV FOGLALÁSOK -->
      <!-- ═══════════════════════════════════════════════════════════════════ -->
      <section class="active-bookings-section">
        <h3 class="section-header">
          AKTÍV FOGLALÁSOK ({{ activeBookings.length }})
        </h3>

        <!-- LOADING -->
        <div v-if="loading" class="loading-container">
          <div class="loading-spinner"></div>
          <p>Betöltés...</p>
        </div>

        <!-- ÜRES ÁLLAPOT -->
        <div v-else-if="activeBookings.length === 0" class="empty-state">
          <span class="empty-icon">📭</span>
          <p>Nincs aktív foglalás</p>
        </div>

        <!-- FOGLALÁSOK LISTÁJA -->
        <div v-else class="bookings-list">
          <!-- VÁRAKOZÓ FOGLALÁSOK -->
          <div 
            v-for="booking in waitingBookings" 
            :key="booking.foglalasID"
            class="booking-card booking-waiting"
          >
            <div class="booking-header">
              <span class="status-badge status-waiting"></span>
              <span class="booking-id">VÁRAKOZIK (#{{ booking.foglalasID }})</span>
            </div>

            <div class="booking-info">
              <p><strong>Ügyfél:</strong> {{ booking.nev }}</p>
              <p><strong>Eszköz:</strong> {{ booking.eszkozNev }}</p>
              
              <div class="booking-time">
                <span><strong>Kezdés:</strong> {{ formatTime(booking.foglalasKezdete) }}</span>
                <span v-if="booking.timeRemaining" class="time-remaining">
                  <strong>Lejár:</strong> ⏰ {{ booking.timeRemaining }}
                </span>
              </div>
            </div>

            <div class="button-group">
              <button 
                class="btn btn-success" 
                @click="handleKiadva(booking)"
                :disabled="actionLoading === booking.foglalasID"
              >
                ✅ KIADVA
              </button>
              <button 
                class="btn btn-danger" 
                @click="handleNemJott(booking)"
                :disabled="actionLoading === booking.foglalasID"
              >
                ❌ NEM JÖTT
              </button>
            </div>
          </div>

          <!-- KIADOTT FOGLALÁSOK -->
          <div 
            v-for="booking in issuedBookings" 
            :key="booking.foglalasID"
            class="booking-card booking-issued"
          >
            <div class="booking-header">
              <span class="status-badge status-issued"></span>
              <span class="booking-id">KIADVA (#{{ booking.foglalasID }})</span>
            </div>

            <div class="booking-info">
              <p><strong>Ügyfél:</strong> {{ booking.nev }}</p>
              <p><strong>Eszköz:</strong> {{ booking.eszkozNev }}</p>
              
              <div class="booking-time">
                <span><strong>Kiadva:</strong> {{ formatTime(booking.kiadasIdopontja) }}</span>
                <span v-if="booking.elapsedTime" class="time-elapsed">
                  <strong>Eltelt:</strong> {{ booking.elapsedTime }}
                </span>
              </div>

              <div v-if="booking.bevetel" class="price-info">
                💰 {{ formatAr(booking.bevetel) }} Ft
              </div>
            </div>

            <div class="single-button">
              <button 
                class="btn btn-primary" 
                @click="handleVisszahozva(booking)"
                :disabled="actionLoading === booking.foglalasID"
              >
                🔵 VISSZAHOZVA
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Error Toast -->
      <Transition name="toast">
        <div v-if="errorMessage" class="error-toast" @click="errorMessage = ''">
          ⚠️ {{ errorMessage }}
        </div>
      </Transition>

      <!-- Success Toast -->
      <Transition name="toast">
        <div v-if="successMessage" class="success-toast" @click="successMessage = ''">
          ✅ {{ successMessage }}
        </div>
      </Transition>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { foglalasService } from '@/services/foglalasService'

// State
const foglalasok = ref([])
const loading = ref(false)
const actionLoading = ref(null)
const errorMessage = ref('')
const successMessage = ref('')
const timers = ref({})

// Computed
const newBookings = computed(() => {
  // Új foglalások (VÁRAKOZIK státusz + kevesebb mint 5 perc óta létezik)
  const now = new Date()
  return foglalasok.value.filter(f => {
    if (f.status !== 0) return false // Csak VÁRAKOZIK
    const created = new Date(f.foglalasKezdete)
    const diffMinutes = (now - created) / 1000 / 60
    return diffMinutes < 5 // Kevesebb mint 5 perce készült
  })
})

const activeBookings = computed(() => {
  // Aktív foglalások (VÁRAKOZIK vagy KIADVA)
  return foglalasok.value.filter(f => f.status === 0 || f.status === 1)
})

const waitingBookings = computed(() => {
  return foglalasok.value.filter(f => f.status === 0) // VÁRAKOZIK
})

const issuedBookings = computed(() => {
  return foglalasok.value.filter(f => f.status === 1) // KIADVA
})

// Methods
async function fetchFoglalasok() {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await foglalasService.getAll()
    foglalasok.value = response.data || []
    
    // Számítsuk ki az időket
    updateTimers()
  } catch (error) {
    console.error('Foglalások betöltési hiba:', error)
    errorMessage.value = 'Hiba történt a foglalások betöltésekor'
  } finally {
    loading.value = false
  }
}

function updateTimers() {
  const now = new Date()
  
  foglalasok.value.forEach(booking => {
    if (booking.status === 0) {
      // VÁRAKOZIK - lejáratig hátralévő idő (15 perc)
      const start = new Date(booking.foglalasKezdete)
      const expiry = new Date(start.getTime() + 15 * 60 * 1000) // +15 perc
      const diff = expiry - now
      
      if (diff > 0) {
        const minutes = Math.floor(diff / 60000)
        const seconds = Math.floor((diff % 60000) / 1000)
        booking.timeRemaining = `${minutes} perc ${seconds} mp`
      } else {
        booking.timeRemaining = 'LEJÁRT'
      }
    } else if (booking.status === 1) {
      // KIADVA - eltelt idő
      const issued = new Date(booking.kiadasIdopontja)
      const diff = now - issued
      const hours = Math.floor(diff / 3600000)
      const minutes = Math.floor((diff % 3600000) / 60000)
      
      if (hours > 0) {
        booking.elapsedTime = `${hours}ó ${minutes}p`
      } else {
        booking.elapsedTime = `${minutes} perc`
      }
      
      // Számoljuk ki a bevételt (példa: 1500 Ft/óra)
      if (booking.eszkozAr) {
        const totalHours = diff / 3600000
        booking.bevetel = Math.round(booking.eszkozAr * totalHours)
      }
    }
  })
}

async function handleKiadva(booking) {
  if (!confirm(`Biztosan kiadod az eszközt (${booking.eszkozNev}) ${booking.nev} részére?`)) {
    return
  }

  actionLoading.value = booking.foglalasID
  errorMessage.value = ''

  try {
    await foglalasService.kiad(booking.foglalasID)
    successMessage.value = 'Eszköz sikeresen kiadva!'
    
    setTimeout(() => {
      successMessage.value = ''
    }, 3000)
    
    await fetchFoglalasok()
  } catch (error) {
    console.error('Kiadási hiba:', error)
    errorMessage.value = error.response?.data?.message || 'Hiba történt a kiadás során'
    
    setTimeout(() => {
      errorMessage.value = ''
    }, 5000)
  } finally {
    actionLoading.value = null
  }
}

async function handleNemJott(booking) {
  if (!confirm(`Biztosan törlöd a foglalást? (${booking.nev} - ${booking.eszkozNev})`)) {
    return
  }

  actionLoading.value = booking.foglalasID
  errorMessage.value = ''

  try {
    await foglalasService.nemJott(booking.foglalasID)
    successMessage.value = 'Foglalás törölve (nem jelent meg)'
    
    setTimeout(() => {
      successMessage.value = ''
    }, 3000)
    
    await fetchFoglalasok()
  } catch (error) {
    console.error('Törlési hiba:', error)
    errorMessage.value = error.response?.data?.message || 'Hiba történt a törlés során'
    
    setTimeout(() => {
      errorMessage.value = ''
    }, 5000)
  } finally {
    actionLoading.value = null
  }
}

async function handleVisszahozva(booking) {
  if (!confirm(`Eszköz visszahozva? (${booking.eszkozNev})`)) {
    return
  }

  actionLoading.value = booking.foglalasID
  errorMessage.value = ''

  try {
    await foglalasService.lezar(booking.foglalasID)
    successMessage.value = `Foglalás lezárva! Bevétel: ${formatAr(booking.bevetel || 0)} Ft`
    
    setTimeout(() => {
      successMessage.value = ''
    }, 3000)
    
    await fetchFoglalasok()
  } catch (error) {
    console.error('Lezárási hiba:', error)
    errorMessage.value = error.response?.data?.message || 'Hiba történt a lezárás során'
    
    setTimeout(() => {
      errorMessage.value = ''
    }, 5000)
  } finally {
    actionLoading.value = null
  }
}

// Formatters
function formatDateTime(dateStr) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleString('hu-HU', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatTime(dateStr) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleTimeString('hu-HU', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatAr(amount) {
  return new Intl.NumberFormat('hu-HU').format(amount)
}

// Lifecycle
let timerInterval = null

onMounted(async () => {
  await fetchFoglalasok()
  
  // Frissítsd az időzítőket minden másodpercben
  timerInterval = setInterval(() => {
    updateTimers()
  }, 1000)
  
  // Auto-refresh 30 másodpercenként
  const refreshInterval = setInterval(() => {
    fetchFoglalasok()
  }, 30000)
  
  // Cleanup
  onUnmounted(() => {
    if (timerInterval) clearInterval(timerInterval)
    if (refreshInterval) clearInterval(refreshInterval)
  })
})
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* VÁLTOZÓK */
/* ═══════════════════════════════════════════════════════════════════════════ */

:root {
  --primary-color: #1565c0;
  --success-color: #2e7d32;
  --danger-color: #c62828;
  --warning-color: #ff9800;
  --waiting-color: #fb8c00;
  --issued-color: #1976d2;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CONTAINER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.admin-dashboard-mobile {
  min-height: 100vh;
  background: #f5f5f5;
  padding-bottom: calc(var(--bottom-nav-height) + 20px);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.dashboard-header {
  background: linear-gradient(135deg, #434343 0%, #282828 100%);
  color: white;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 15px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
  position: sticky;
  top: var(--header-height);
  z-index: 50;
}

.header-icon {
  font-size: 28px;
}

.header-title {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CONTENT */
/* ═══════════════════════════════════════════════════════════════════════════ */

.dashboard-content {
  padding: 20px;
}

.page-title {
  font-size: 30px;
  font-weight: 800;
  margin-bottom: 4px;
  color: #333;
}

.page-subtitle {
  font-size: 15px;
  color: #666;
  margin-bottom: 22px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* NOTIFICATION CARD */
/* ═══════════════════════════════════════════════════════════════════════════ */

.notifications-section {
  margin-bottom: 30px;
}

.notification-card {
  background: white;
  border-radius: 14px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 4px 18px rgba(0, 0, 0, 0.12);
  border-left: 6px solid var(--warning-color);
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.notification-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 14px;
}

.notification-icon {
  font-size: 24px;
}

.notification-title {
  font-size: 20px;
  font-weight: 800;
  color: #333;
}

.notification-info p {
  margin: 7px 0;
  font-size: 15px;
  color: #555;
}

.timer-text {
  margin-top: 10px;
  background: #fff3e0;
  color: #e65100;
  padding: 8px 12px;
  border-radius: 8px;
  font-weight: 700;
  text-align: center;
  font-size: 16px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* BUTTONS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.button-group {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}

.single-button {
  display: flex;
  justify-content: center;
  margin-top: 14px;
}

.btn {
  flex: 1;
  padding: 14px;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.btn:active:not(:disabled) {
  transform: scale(0.97);
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-success {
  background: var(--success-color);
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #1b5e20;
}

.btn-danger {
  background: var(--danger-color);
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #b71c1c;
}

.btn-primary {
  background: var(--primary-color);
  color: white;
  min-width: 200px;
}

.btn-primary:hover:not(:disabled) {
  background: #0d47a1;
}

.btn-subtitle {
  display: block;
  font-size: 12px;
  font-weight: 400;
  margin-top: 4px;
  opacity: 0.9;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* SECTION HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.section-header {
  font-size: 23px;
  font-weight: 800;
  margin-bottom: 18px;
  color: #333;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* BOOKING CARD */
/* ═══════════════════════════════════════════════════════════════════════════ */

.booking-card {
  background: white;
  border-radius: 14px;
  padding: 18px;
  margin-bottom: 16px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s, box-shadow 0.2s;
}

.booking-card:active {
  transform: scale(0.98);
}

.booking-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 12px;
  border-bottom: 1px dashed #ddd;
  padding-bottom: 10px;
}

.status-badge {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
}

.status-waiting {
  background: var(--waiting-color);
  box-shadow: 0 0 8px var(--waiting-color);
}

.status-issued {
  background: var(--issued-color);
  box-shadow: 0 0 8px var(--issued-color);
}

.booking-id {
  font-size: 17px;
  font-weight: 800;
  color: #333;
}

.booking-info p {
  font-size: 14.5px;
  margin: 5px 0;
  color: #555;
}

.booking-time {
  display: flex;
  justify-content: space-between;
  margin: 8px 0;
  font-size: 14px;
  color: #666;
}

.time-remaining {
  color: var(--warning-color);
  font-weight: 700;
}

.time-elapsed {
  color: var(--issued-color);
  font-weight: 700;
}

.price-info {
  margin-top: 8px;
  padding: 6px 10px;
  background: #e8f5e9;
  color: #1b5e20;
  border-radius: 8px;
  font-weight: 800;
  text-align: center;
  font-size: 16px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* LOADING & EMPTY STATE */
/* ═══════════════════════════════════════════════════════════════════════════ */

.loading-container,
.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #999;
}

.loading-spinner {
  width: 48px;
  height: 48px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-icon {
  font-size: 64px;
  display: block;
  margin-bottom: 16px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* TOASTS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.error-toast,
.success-toast {
  position: fixed;
  bottom: calc(var(--bottom-nav-height) + 20px);
  left: 50%;
  transform: translateX(-50%);
  padding: 16px 24px;
  border-radius: 8px;
  font-weight: 600;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  z-index: 1000;
  max-width: 90%;
  text-align: center;
  cursor: pointer;
}

.error-toast {
  background: #f44336;
  color: white;
}

.success-toast {
  background: #4caf50;
  color: white;
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(20px);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* RESPONSIVE */
/* ═══════════════════════════════════════════════════════════════════════════ */

@media (min-width: 768px) {
  .dashboard-content {
    max-width: 800px;
    margin: 0 auto;
  }
}
</style>