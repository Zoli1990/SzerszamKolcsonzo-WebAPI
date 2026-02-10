<template>
  <div class="foglalasok-admin">
    <div class="header">
      <h2>📋 Foglalások kezelése</h2>
      <div class="header-actions">
        <button class="btn-refresh" @click="fetchFoglalasok" :disabled="loading">
          🔄 Frissítés
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

    <div v-else-if="osszesFoglalas.length === 0" class="empty-state">
      <p>📭 Nincs még foglalás az adatbázisban.</p>
    </div>

    <template v-else>
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Eszköz</th>
            <th>Ügyfél</th>
            <th>Kezdés</th>
            <th>Eltelt idő</th>
            <th>Bevétel</th>
            <th>Státusz</th>
            <th>Műveletek</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="foglalas in aktualisOldalFoglalasai" :key="foglalas.foglalasID">
            <td><strong>#{{ foglalas.foglalasID }}</strong></td>
            <td>
              <div class="eszkoz-info">
                {{ foglalas.eszkozNev }}
              </div>
            </td>
            <td>
              <div>
                <strong>{{ foglalas.nev }}</strong>
              </div>
              <div class="small">{{ foglalas.email }}</div>
              <div class="small">{{ foglalas.telefonszam }}</div>
            </td>
            <td>
              <div class="small">{{ formatDate(foglalas.foglalasKezdete) }}</div>
              <div v-if="foglalas.kiadasIdopontja" class="small text-success">
                ✅ Kiadva: {{ formatDate(foglalas.kiadasIdopontja) }}
              </div>
              <div v-if="foglalas.visszahozasIdopontja" class="small text-info">
                🔵 Visszahozva: {{ formatDate(foglalas.visszahozasIdopontja) }}
              </div>
            </td>
            <td>
              <span v-if="foglalas.elszamolhatoIdo">{{ formatIdo(foglalas.elszamolhatoIdo) }}</span>
              <span v-else-if="foglalas.status === 'Kiadva'" class="text-muted">
                ⏱️ {{ getElapsedTime(foglalas.kiadasIdopontja) }}
              </span>
              <span v-else class="text-muted">-</span>
            </td>
            <td>
              <strong v-if="foglalas.fizetendoOsszeg" class="bevetel">
                {{ formatAr(foglalas.fizetendoOsszeg) }} Ft
              </strong>
              <strong v-else-if="foglalas.bevetel" class="bevetel">
                {{ formatAr(foglalas.bevetel) }} Ft
              </strong>
              <span v-else class="text-muted">-</span>
            </td>
            <td>
              <span :class="['badge', getBadgeClass(foglalas.status)]">
                {{ getStatusText(foglalas.status) }}
              </span>
            </td>
            <td class="actions">
              <!-- 🟢 KIADVA gomb - csak Aktiv foglaláshoz -->
              <button
                v-if="foglalas.status === 'Aktiv'"
                class="btn-kiad"
                @click="kiadEszkoz(foglalas)"
                title="Admin jóváhagyja - eszköz kiadása"
              >
                🟢 KIADVA
              </button>

              <!-- 🔵 VISSZAHOZVA gomb - csak Kiadva foglaláshoz -->
              <button
                v-if="foglalas.status === 'Kiadva'"
                class="btn-visszahoz"
                @click="visszahozEszkoz(foglalas)"
                title="Eszköz visszahozva"
              >
                🔵 VISSZAHOZVA
              </button>

              <!-- 🗑️ TÖRLÉS gomb - Aktiv vagy Kiadva státuszhoz -->
              <button
                v-if="foglalas.status === 'Aktiv' || foglalas.status === 'Kiadva'"
                class="btn-delete"
                @click="torolFoglalas(foglalas)"
                title="Foglalás törlése"
              >
                🗑️
              </button>

              <!-- 👁️ Részletek gomb - mindenkinek -->
              <button class="btn-info" @click="openDetailModal(foglalas)" title="Részletek">
                👁️
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- ============================================================================ -->
      <!-- ✅ LAPOZÓ GOMBOK -->
      <!-- ============================================================================ -->
      <div class="pagination">
        <button 
          class="pagination-btn" 
          @click="goToPage(1)" 
          :disabled="currentPage === 1"
          title="Első oldal"
        >
          ⏮️
        </button>
        
        <button 
          class="pagination-btn" 
          @click="goToPage(currentPage - 1)" 
          :disabled="currentPage === 1"
          title="Előző oldal"
        >
          ◀️ Előző
        </button>

        <div class="pagination-info">
          <span class="page-numbers">
            <strong>{{ currentPage }}</strong> / {{ totalPages }}
          </span>
          <span class="total-count">
            (összesen {{ osszesElemSzam }} foglalás)
          </span>
        </div>

        <button 
          class="pagination-btn" 
          @click="goToPage(currentPage + 1)" 
          :disabled="currentPage === totalPages"
          title="Következő oldal"
        >
          Következő ▶️
        </button>

        <button 
          class="pagination-btn" 
          @click="goToPage(totalPages)" 
          :disabled="currentPage === totalPages"
          title="Utolsó oldal"
        >
          ⏭️
        </button>
      </div>
    </template>

    <!-- Részletek Modal -->
    <Teleport to="body">
      <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-box" @click.stop>
          <h3>📋 Foglalás részletei</h3>

          <div class="detail-grid">
            <div class="detail-item">
              <div class="detail-label">Foglalás ID:</div>
              <div class="detail-value">#{{ selectedFoglalas.foglalasID }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Eszköz:</div>
              <div class="detail-value">{{ selectedFoglalas.eszkozNev }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Ügyfél neve:</div>
              <div class="detail-value">{{ selectedFoglalas.nev }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Email:</div>
              <div class="detail-value">{{ selectedFoglalas.email }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Telefonszám:</div>
              <div class="detail-value">{{ selectedFoglalas.telefonszam }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Cím:</div>
              <div class="detail-value">{{ selectedFoglalas.cim }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Foglalás kezdete:</div>
              <div class="detail-value">{{ formatDate(selectedFoglalas.foglalasKezdete) }}</div>
            </div>

            <div v-if="selectedFoglalas.kiadasIdopontja" class="detail-item">
              <div class="detail-label">Kiadva:</div>
              <div class="detail-value">{{ formatDate(selectedFoglalas.kiadasIdopontja) }}</div>
            </div>

            <div v-if="selectedFoglalas.visszahozasIdopontja" class="detail-item">
              <div class="detail-label">Visszahozva:</div>
              <div class="detail-value">{{ formatDate(selectedFoglalas.visszahozasIdopontja) }}</div>
            </div>

            <div v-if="selectedFoglalas.elszamolhatoIdo" class="detail-item">
              <div class="detail-label">Eltelt idő:</div>
              <div class="detail-value">{{ formatIdo(selectedFoglalas.elszamolhatoIdo) }}</div>
            </div>

            <div v-if="selectedFoglalas.fizetendoOsszeg" class="detail-item">
              <div class="detail-label">Fizetendő:</div>
              <div class="detail-value highlight">{{ formatAr(selectedFoglalas.fizetendoOsszeg) }} Ft</div>
            </div>

            <div v-if="selectedFoglalas.bevetel" class="detail-item">
              <div class="detail-label">Bevétel:</div>
              <div class="detail-value highlight">{{ formatAr(selectedFoglalas.bevetel) }} Ft</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Státusz:</div>
              <div class="detail-value">
                <span :class="['badge', getBadgeClass(selectedFoglalas.status)]">
                  {{ getStatusText(selectedFoglalas.status) }}
                </span>
              </div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Létrehozva:</div>
              <div class="detail-value">{{ formatDate(selectedFoglalas.letrehozasDatum) }}</div>
            </div>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-secondary" @click="closeModal">Bezárás</button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5265/api'

// ============================================================================
// ✅ FRONTEND LAPOZÁS - ÖSSZES ADT LETÖLTJÜK, ITT LAPOZUNK
// ============================================================================
const osszesFoglalas = ref([]) // Minden foglalás a backend-től
const loading = ref(false)
const modalOpen = ref(false)
const selectedFoglalas = ref({})

const currentPage = ref(1)
const pageSize = ref(5) // 5 elem oldalanként

// Computed property: aktuális oldal elemei
const aktualisOldalFoglalasai = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return osszesFoglalas.value.slice(start, end)
})

// Computed property: összes elemszám
const osszesElemSzam = computed(() => osszesFoglalas.value.length)

// Computed property: összes oldal száma
const totalPages = computed(() => {
  return Math.ceil(osszesElemSzam.value / pageSize.value) || 1
})

let autoRefreshInterval = null

onMounted(() => {
  fetchFoglalasok()
  
  // Automatikus frissítés 10 másodpercenként
  autoRefreshInterval = setInterval(() => {
    fetchFoglalasok(true) // true = csendes frissítés
  }, 10000)
})

onUnmounted(() => {
  if (autoRefreshInterval) {
    clearInterval(autoRefreshInterval)
  }
})

// ============================================================================
// ✅ EREDETI BACKEND HÍVÁS - VÁLTOZATLAN
// ============================================================================
async function fetchFoglalasok(silent = false) {
  if (!silent) loading.value = true
  
  try {
    const response = await axios.get(`${API_BASE}/foglalasok`)
    osszesFoglalas.value = response.data
    
    // Ha túl sok oldalt lapozunk előre és közben törölnek elemeket,
    // visszalépünk az utolsó létező oldalra
    if (currentPage.value > totalPages.value) {
      currentPage.value = totalPages.value
    }
  } catch (err) {
    console.error('Foglalások betöltése sikertelen:', err)
  } finally {
    if (!silent) loading.value = false
  }
}

// ============================================================================
// ✅ LAPOZÓ FUNKCIÓ
// ============================================================================
function goToPage(page) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
}

async function kiadEszkoz(foglalas) {
  if (!confirm(`Biztosan kiadod az eszközt?\n\nEszköz: ${foglalas.eszkozNev}\nÜgyfél: ${foglalas.nev}`)) {
    return
  }

  try {
    const response = await axios.post(`${API_BASE}/foglalasok/${foglalas.foglalasID}/kiad`)
    
    alert(`✅ ${response.data.message}`)
    await fetchFoglalasok()
  } catch (err) {
    alert(`❌ Hiba: ${err.response?.data?.message || 'Nem sikerült kiadni az eszközt'}`)
    console.error('Kiadás hiba:', err)
  }
}

async function visszahozEszkoz(foglalas) {
  if (!confirm(`Ügyfél visszahozta az eszközt?\n\nEszköz: ${foglalas.eszkozNev}`)) {
    return
  }

  try {
    const response = await axios.post(`${API_BASE}/eszkozok/${foglalas.eszkozID}/visszahoz`)
    
    // Elszámolás megjelenítése
    if (response.data.elszamolas) {
      const e = response.data.elszamolas
      const ido = formatIdo(e.elszamolhatoIdo)
      const osszeg = formatAr(e.fizetendoOsszeg)
      
      alert(
        `✅ Eszköz sikeresen visszahozva!\n\n` +
        `⏱️ Eltelt idő: ${ido}\n` +
        `💰 Fizetendő: ${osszeg} Ft`
      )
    } else {
      alert(`✅ ${response.data.message}`)
    }
    
    await fetchFoglalasok()
  } catch (err) {
    alert(`❌ Hiba: ${err.response?.data?.message || 'Nem sikerült visszahozni az eszközt'}`)
    console.error('Visszahozás hiba:', err)
  }
}

async function torolFoglalas(foglalas) {
  if (!confirm(`Biztosan törölni szeretnéd a foglalást?\n\nEszköz: ${foglalas.eszkozNev}\nÜgyfél: ${foglalas.nev}\n\nEz a művelet visszavonhatatlan!`)) {
    return
  }

  try {
    await axios.delete(`${API_BASE}/foglalasok/${foglalas.foglalasID}`)
    
    alert('✅ Foglalás sikeresen törölve!')
    await fetchFoglalasok()
  } catch (err) {
    alert(`❌ Hiba: ${err.response?.data?.message || 'Nem sikerült törölni a foglalást'}`)
    console.error('Törlés hiba:', err)
  }
}

function openDetailModal(foglalas) {
  selectedFoglalas.value = foglalas
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
}

function formatDate(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('hu-HU', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}

function formatAr(ar) {
  if (!ar) return '0'
  return new Intl.NumberFormat('hu-HU').format(Math.round(ar))
}

function formatIdo(percek) {
  if (!percek) return '-'
  const orak = Math.floor(percek / 60)
  const maradekPercek = percek % 60
  if (orak === 0) return `${maradekPercek} perc`
  return `${orak}h ${maradekPercek}m`
}

function getElapsedTime(kiadasIdopontja) {
  if (!kiadasIdopontja) return '-'
  const kiadas = new Date(kiadasIdopontja)
  const most = new Date()
  const percek = Math.floor((most - kiadas) / (1000 * 60))
  return formatIdo(percek)
}

function getStatusText(status) {
  const map = {
    Aktiv: '🟡 Aktív',
    Kiadva: '🔵 Kiadva',
    Lezart: '🟢 Lezárt',
    Torolt: '🔴 Törölt',
    Lejart: '⏰ Lejárt'
  }
  return map[status] || status
}

function getBadgeClass(status) {
  const map = {
    Aktiv: 'badge-warning',
    Kiadva: 'badge-info',
    Lezart: 'badge-success',
    Torolt: 'badge-danger',
    Lejart: 'badge-secondary'
  }
  return map[status] || ''
}
</script>

<style scoped>
.foglalasok-admin {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 2px solid #e8dcc8;
}

.header h2 {
  margin: 0;
  font-size: 28px;
  color: #3d2f1f;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.btn-refresh {
  padding: 10px 20px;
  background: #6b8e23;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-refresh:hover:not(:disabled) {
  background: #556b1a;
  transform: translateY(-1px);
}

.btn-refresh:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading {
  text-align: center;
  padding: 60px;
  color: #6b5d4f;
  font-size: 18px;
}

.empty-state {
  text-align: center;
  padding: 60px;
  background: #f5f1e8;
  border-radius: 12px;
  color: #6b5d4f;
}

.empty-state p {
  font-size: 18px;
  margin: 0;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(61, 47, 31, 0.08);
}

.data-table th,
.data-table td {
  padding: 14px 12px;
  text-align: left;
  border-bottom: 1px solid #e8dcc8;
}

.data-table th {
  background: #f5f1e8;
  font-weight: 600;
  color: #3d2f1f;
  text-transform: uppercase;
  font-size: 12px;
  letter-spacing: 0.5px;
}

.data-table tbody tr {
  transition: background 0.2s;
}

.data-table tbody tr:hover {
  background: #faf8f3;
}

.eszkoz-info {
  font-weight: 600;
  color: #3d2f1f;
}

.small {
  font-size: 12px;
  color: #6b5d4f;
  margin-top: 2px;
}

.text-muted {
  color: #9b8b7a;
}

.text-success {
  color: #059669;
}

.text-info {
  color: #0284c7;
}

.mt-1 {
  margin-top: 4px;
}

.bevetel {
  color: #6b8e23;
  font-weight: 700;
}

.badge {
  display: inline-block;
  padding: 5px 12px;
  border-radius: 14px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.badge-warning {
  background: #fef3c7;
  color: #92400e;
}

.badge-info {
  background: #dbeafe;
  color: #1e40af;
}

.badge-success {
  background: #d1fae5;
  color: #065f46;
}

.badge-danger {
  background: #fee2e2;
  color: #991b1b;
}

.badge-secondary {
  background: #e5e7eb;
  color: #374151;
}

.actions {
  display: flex;
  gap: 6px;
  align-items: center;
  flex-wrap: wrap;
}

.btn-kiad,
.btn-visszahoz,
.btn-info,
.btn-delete,
.btn-secondary {
  padding: 8px 14px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 12px;
  transition: all 0.2s;
  white-space: nowrap;
}

.btn-kiad {
  background: #10b981;
  color: white;
}

.btn-kiad:hover {
  background: #059669;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(16, 185, 129, 0.3);
}

.btn-visszahoz {
  background: #3b82f6;
  color: white;
}

.btn-visszahoz:hover {
  background: #2563eb;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(59, 130, 246, 0.3);
}

.btn-delete {
  background: #ef4444;
  color: white;
  min-width: 36px;
}

.btn-delete:hover {
  background: #dc2626;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(239, 68, 68, 0.3);
}

.btn-info {
  background: #6b8e23;
  color: white;
  min-width: 36px;
}

.btn-info:hover {
  background: #556b1a;
}

.btn-secondary {
  background: white;
  border: 2px solid #d4c5b0;
  color: #3d2f1f;
}

.btn-secondary:hover {
  background: #f5f1e8;
}

/* ============================================================================ */
/* ✅ LAPOZÓ STÍLUSOK */
/* ============================================================================ */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 12px;
  margin-top: 24px;
  padding: 20px;
  background: #f5f1e8;
  border-radius: 12px;
}

.pagination-btn {
  padding: 10px 18px;
  background: white;
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  color: #3d2f1f;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.pagination-btn:hover:not(:disabled) {
  background: #6b8e23;
  color: white;
  border-color: #6b8e23;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(107, 142, 35, 0.3);
}

.pagination-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  background: #e8dcc8;
  border-color: #d4c5b0;
  color: #9b8b7a;
}

.pagination-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 0 20px;
}

.page-numbers {
  font-size: 18px;
  color: #3d2f1f;
}

.page-numbers strong {
  font-size: 24px;
  color: #6b8e23;
}

.total-count {
  font-size: 12px;
  color: #6b5d4f;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(61, 47, 31, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
  padding: 20px;
  overflow-y: auto;
  backdrop-filter: blur(4px);
}

.modal-box {
  background: white;
  padding: 32px;
  border-radius: 16px;
  max-width: 700px;
  width: 100%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.3s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-box h3 {
  margin: 0 0 24px 0;
  color: #3d2f1f;
  font-size: 24px;
}

.detail-grid {
  display: grid;
  gap: 12px;
  margin-bottom: 24px;
}

.detail-item {
  display: grid;
  grid-template-columns: 160px 1fr;
  gap: 16px;
  padding: 14px;
  background: #f5f1e8;
  border-radius: 8px;
  align-items: center;
}

.detail-label {
  font-weight: 600;
  color: #6b5d4f;
  font-size: 13px;
}

.detail-value {
  color: #3d2f1f;
  font-size: 14px;
}

.detail-value.highlight {
  font-weight: 700;
  color: #6b8e23;
  font-size: 18px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  border-top: 2px solid #e8dcc8;
  padding-top: 24px;
}

/* Reszponzív lapozó kisebb képernyőkön */
@media (max-width: 768px) {
  .pagination {
    flex-wrap: wrap;
  }

  .pagination-btn {
    padding: 8px 12px;
    font-size: 12px;
  }

  .page-numbers {
    font-size: 16px;
  }

  .page-numbers strong {
    font-size: 20px;
  }
}
</style>