<template>
  <div class="eszkoz-lista-mobile">
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- HEADER -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="header">
      <h2 class="title">🔧 Eszközök</h2>
      <button class="btn-refresh" @click="fetchEszkozok" :disabled="loading">
        <span class="refresh-icon" :class="{ spinning: loading }">🔄</span>
      </button>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- FILTER CHIPS -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="filters">
      <button 
        v-for="filter in filters" 
        :key="filter.value"
        class="filter-chip"
        :class="{ active: activeFilter === filter.value }"
        @click="activeFilter = filter.value"
      >
        <span class="chip-icon">{{ filter.icon }}</span>
        <span class="chip-label">{{ filter.label }}</span>
        <span v-if="getCount(filter.value) > 0" class="chip-count">
          {{ getCount(filter.value) }}
        </span>
      </button>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- LOADING STATE -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Betöltés...</p>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- EMPTY STATE -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-else-if="filteredEszkozok.length === 0" class="empty-state">
      <span class="empty-icon">📦</span>
      <p class="empty-text">Nincs megjeleníthető eszköz</p>
      <p class="empty-hint">Próbálj más szűrőt választani vagy adj hozzá új eszközt</p>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- ESZKÖZ KÁRTYÁK -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-else class="cards-grid">
      <div 
        v-for="eszkoz in filteredEszkozok" 
        :key="eszkoz.eszkozID"
        class="eszkoz-card"
        :class="getCardClass(eszkoz.status)"
      >
        <!-- Kép -->
        <div class="card-image">
          <img 
            v-if="eszkoz.kepUrl" 
            :src="eszkoz.kepUrl" 
            :alt="eszkoz.nev"
            @error="handleImageError"
          />
          <div v-else class="image-placeholder">
            🔧
          </div>
          
          <!-- Státusz badge -->
          <span class="status-badge" :class="getStatusClass(eszkoz.status)">
            {{ getStatusLabel(eszkoz.status) }}
          </span>
        </div>

        <!-- Tartalom -->
        <div class="card-content">
          <div class="card-header">
            <h3 class="card-title">{{ eszkoz.nev }}</h3>
            <span class="card-id">#{{ eszkoz.eszkozID }}</span>
          </div>

          <p class="card-category">{{ eszkoz.kategoriaNev }}</p>

          <div v-if="eszkoz.leiras" class="card-description">
            {{ eszkoz.leiras }}
          </div>

          <div v-if="eszkoz.megjegyzes" class="card-note">
            <span class="note-icon">ℹ️</span>
            <span class="note-text">{{ eszkoz.megjegyzes }}</span>
          </div>

          <div class="card-price">
            <span class="price-label">Kiadás:</span>
            <span class="price-value">{{ formatAr(eszkoz.kiadasiAr) }} Ft/óra</span>
          </div>
        </div>

        <!-- Akciók -->
        <div class="card-actions">
          <button 
            class="action-btn edit"
            @click="openEditModal(eszkoz)"
          >
            <span class="btn-icon">✏️</span>
            <span class="btn-label">Szerkeszt</span>
          </button>

          <button 
            v-if="eszkoz.status === 'Elerheto'"
            class="action-btn delete"
            @click="deleteEszkoz(eszkoz)"
          >
            <span class="btn-icon">🗑️</span>
            <span class="btn-label">Törlés</span>
          </button>

          <button 
            v-if="eszkoz.status === 'Kivonva'"
            class="action-btn restore"
            @click="elerhetovaTetel(eszkoz.eszkozID)"
          >
            <span class="btn-icon">✅</span>
            <span class="btn-label">Visszaállít</span>
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- ✨ FAB GOMB - ÚJ ESZKÖZ HOZZÁADÁSA -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <button 
      class="fab"
      @click="$router.push('/admin/eszkoz/uj')"
      aria-label="Új eszköz hozzáadása"
    >
      <span class="fab-icon">➕</span>
      <span class="fab-text">Új eszköz</span>
    </button>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'

const router = useRouter()

// ═══════════════════════════════════════════════════════════════════════════
// STATE
// ═══════════════════════════════════════════════════════════════════════════

const eszkozok = ref([])
const loading = ref(false)
const activeFilter = ref('osszes')

const filters = [
  { value: 'osszes', label: 'Összes', icon: '📋' },
  { value: 'Elerheto', label: 'Elérhető', icon: '✅' },
  { value: 'Kiadva', label: 'Kiadva', icon: '🔄' },
  { value: 'Kivonva', label: 'Kivonva', icon: '⚠️' }
]

// ═══════════════════════════════════════════════════════════════════════════
// COMPUTED
// ═══════════════════════════════════════════════════════════════════════════

const filteredEszkozok = computed(() => {
  if (!Array.isArray(eszkozok.value)) return []
  if (activeFilter.value === 'osszes') return eszkozok.value
  return eszkozok.value.filter(e => e.status === activeFilter.value)
})

// ═══════════════════════════════════════════════════════════════════════════
// METHODS
// ═══════════════════════════════════════════════════════════════════════════

function getCount(filterValue) {
  if (!Array.isArray(eszkozok.value)) return 0
  if (filterValue === 'osszes') return eszkozok.value.length
  return eszkozok.value.filter(e => e.status === filterValue).length
}

function getCardClass(status) {
  return `status-${status?.toLowerCase() || 'elerheto'}`
}

function getStatusClass(status) {
  const classes = {
    'Elerheto': 'status-available',
    'Kiadva': 'status-rented',
    'Kivonva': 'status-withdrawn'
  }
  return classes[status] || 'status-available'
}

function getStatusLabel(status) {
  const labels = {
    'Elerheto': 'Elérhető',
    'Kiadva': 'Kiadva',
    'Kivonva': 'Kivonva'
  }
  return labels[status] || status
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function handleImageError(e) {
  e.target.style.display = 'none'
  e.target.parentElement.innerHTML = '<div class="image-placeholder">🔧</div>'
}

async function fetchEszkozok() {
  loading.value = true
  try {
    const response = await api.get('/Eszkozok/admin')
    const data = response.data
    // Biztonsági ellenőrzés: ha az API válasz nem tömb, megpróbáljuk kinyerni
    if (Array.isArray(data)) {
      eszkozok.value = data
    } else if (data && Array.isArray(data.data)) {
      eszkozok.value = data.data
    } else if (data && Array.isArray(data.eszkozok)) {
      eszkozok.value = data.eszkozok
    } else {
      console.warn('⚠️ Váratlan API válasz formátum:', typeof data, data)
      eszkozok.value = []
    }
    console.log('✅ Eszközök betöltve:', eszkozok.value.length)
  } catch (error) {
    console.error('❌ Eszközök betöltése sikertelen:', error)
    eszkozok.value = []
    alert('Hiba az eszközök betöltésekor')
  } finally {
    loading.value = false
  }
}

function openEditModal(eszkoz) {
  // TODO: Implementáld a szerkesztés modal-t
  console.log('Szerkesztés:', eszkoz)
  alert('Szerkesztés funkció hamarosan!')
}

async function deleteEszkoz(eszkoz) {
  if (!confirm(`Biztosan törölni szeretnéd: ${eszkoz.nev}?`)) {
    return
  }

  try {
    await api.delete(`/Eszkozok/${eszkoz.eszkozID}`)
    alert('✅ Eszköz törölve!')
    await fetchEszkozok()
  } catch (error) {
    console.error('❌ Törlés sikertelen:', error)
    const msg = error.response?.data?.message || 'Hiba történt a törlés során'
    alert('❌ ' + msg)
  }
}

async function elerhetovaTetel(eszkozID) {
  try {
    await api.post(`/Eszkozok/${eszkozID}/elerheto`)
    alert('✅ Eszköz visszaállítva!')
    await fetchEszkozok()
  } catch (error) {
    console.error('❌ Visszaállítás sikertelen:', error)
    alert('❌ Hiba történt')
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// LIFECYCLE
// ═══════════════════════════════════════════════════════════════════════════

onMounted(() => {
  fetchEszkozok()
})
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* CONTAINER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.eszkoz-lista-mobile {
  padding: 16px;
  padding-bottom: 100px; /* FAB miatt */
  min-height: 100vh;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.title {
  margin: 0;
  font-size: 24px;
  color: #3d2f1f;
  font-weight: 700;
}

.btn-refresh {
  width: 44px;
  height: 44px;
  border: 2px solid #d4c5b0;
  border-radius: 50%;
  background: white;
  font-size: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.btn-refresh:active {
  transform: scale(0.95);
}

.refresh-icon.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* FILTERS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.filters {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  margin-bottom: 20px;
  padding-bottom: 4px;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}

.filters::-webkit-scrollbar {
  display: none;
}

.filter-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 16px;
  background: white;
  border: 2px solid #d4c5b0;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
  transition: all 0.2s ease;
  min-height: 44px;
}

.filter-chip.active {
  background: #6b8e23;
  border-color: #6b8e23;
  color: white;
}

.filter-chip:active {
  transform: scale(0.95);
}

.chip-count {
  background: rgba(0, 0, 0, 0.1);
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
}

.filter-chip.active .chip-count {
  background: rgba(255, 255, 255, 0.3);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* STATES */
/* ═══════════════════════════════════════════════════════════════════════════ */

.loading-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e8dcc8;
  border-top-color: #6b8e23;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.empty-icon {
  font-size: 60px;
  margin-bottom: 16px;
}

.empty-text {
  font-size: 18px;
  font-weight: 600;
  color: #3d2f1f;
  margin: 0 0 8px 0;
}

.empty-hint {
  font-size: 14px;
  color: #6b5d4f;
  margin: 0;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CARDS GRID */
/* ═══════════════════════════════════════════════════════════════════════════ */

.cards-grid {
  display: grid;
  gap: 16px;
}

.eszkoz-card {
  background: white;
  border: 2px solid #e8dcc8;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.2s ease;
}

.eszkoz-card:active {
  transform: scale(0.98);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CARD IMAGE */
/* ═══════════════════════════════════════════════════════════════════════════ */

.card-image {
  position: relative;
  width: 100%;
  height: 200px;
  background: #faf8f3;
  display: flex;
  align-items: center;
  justify-content: center;
}

.card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-placeholder {
  font-size: 60px;
}

.status-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 6px 12px;
  border-radius: 16px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-available {
  background: #10b981;
  color: white;
}

.status-rented {
  background: #3b82f6;
  color: white;
}

.status-withdrawn {
  background: #f59e0b;
  color: white;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CARD CONTENT */
/* ═══════════════════════════════════════════════════════════════════════════ */

.card-content {
  padding: 16px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 8px;
}

.card-title {
  flex: 1;
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #3d2f1f;
  line-height: 1.3;
}

.card-id {
  flex-shrink: 0;
  font-size: 14px;
  color: #6b5d4f;
  font-weight: 600;
}

.card-category {
  font-size: 14px;
  color: #6b8e23;
  font-weight: 600;
  margin: 0 0 12px 0;
}

.card-description {
  font-size: 14px;
  color: #6b5d4f;
  margin-bottom: 12px;
  line-height: 1.5;
}

.card-note {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 10px;
  background: #fff3cd;
  border-left: 3px solid #f59e0b;
  border-radius: 6px;
  margin-bottom: 12px;
}

.note-icon {
  flex-shrink: 0;
  font-size: 16px;
}

.note-text {
  flex: 1;
  font-size: 13px;
  color: #856404;
  font-weight: 500;
}

.card-price {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: #f5f1e8;
  border-radius: 8px;
  margin-bottom: 12px;
}

.price-label {
  font-size: 14px;
  color: #6b5d4f;
  font-weight: 600;
}

.price-value {
  font-size: 18px;
  color: #6b8e23;
  font-weight: 700;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CARD ACTIONS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.card-actions {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
  padding: 0 16px 16px;
}

.action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 12px;
  border: none;
  border-radius: 8px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 48px;
}

.action-btn:active {
  transform: scale(0.95);
}

.action-btn.edit {
  background: #7a9b57;
  color: white;
}

.action-btn.delete {
  background: #ef4444;
  color: white;
}

.action-btn.restore {
  background: #10b981;
  color: white;
  grid-column: 1 / -1;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* ✨ FAB GOMB */
/* ═══════════════════════════════════════════════════════════════════════════ */

.fab {
  position: fixed;
  bottom: 24px;
  right: 24px;
  
  display: flex;
  align-items: center;
  gap: 12px;
  
  padding: 16px 24px;
  background: linear-gradient(135deg, #6b8e23, #8ba83e);
  color: white;
  
  border: none;
  border-radius: 28px;
  box-shadow: 0 6px 20px rgba(107, 142, 35, 0.4);
  
  font-size: 16px;
  font-weight: 700;
  
  cursor: pointer;
  z-index: 100;
  
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  animation: fabEnter 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275) 0.3s both;
}

.fab:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(107, 142, 35, 0.5);
}

.fab:active {
  transform: translateY(-2px) scale(0.95);
}

.fab-icon {
  font-size: 20px;
  line-height: 1;
}

.fab-text {
  font-size: 16px;
  line-height: 1;
}

@keyframes fabEnter {
  from {
    opacity: 0;
    transform: scale(0) rotate(-180deg);
  }
  to {
    opacity: 1;
    transform: scale(1) rotate(0);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* RESPONSIVE */
/* ═══════════════════════════════════════════════════════════════════════════ */

@media (max-width: 480px) {
  .eszkoz-lista-mobile {
    padding: 12px;
  }

  .title {
    font-size: 20px;
  }

  .fab {
    width: 56px;
    height: 56px;
    padding: 0;
    border-radius: 50%;
    justify-content: center;
  }

  .fab-text {
    display: none;
  }

  .fab-icon {
    font-size: 24px;
  }

  .card-actions {
    grid-template-columns: 1fr;
  }

  .action-btn.edit,
  .action-btn.delete {
    grid-column: 1 / -1;
  }
}
</style>