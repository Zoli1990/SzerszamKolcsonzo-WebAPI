<template>
  <div class="eszkozok-admin">
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- HEADER -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="page-header">
      <h2 class="page-title">
        <span class="title-icon">🔧</span>
        <span>Eszközök</span>
      </h2>
      <div class="header-actions">
        <button class="btn-refresh" @click="fetchEszkozok" :disabled="loading">
          <span class="refresh-icon" :class="{ spinning: loading }">🔄</span>
        </button>
        <button class="btn-add" @click="openCreateModal">
          <span>+</span>
          <span class="btn-add-text">Új eszköz</span>
        </button>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- SZŰRŐK -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="filters-section">
      <div class="filter-chips">
        <button 
          v-for="filter in statusFilters" 
          :key="filter.value"
          class="filter-chip"
          :class="{ active: activeFilter === filter.value }"
          @click="activeFilter = filter.value"
        >
          <span class="chip-icon">{{ filter.icon }}</span>
          <span class="chip-label">{{ filter.label }}</span>
          <span class="chip-count" v-if="getStatusCount(filter.value) > 0">
            {{ getStatusCount(filter.value) }}
          </span>
        </button>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- LOADING / EMPTY -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div v-if="loading && !eszkozok.length" class="state-container">
      <div class="loading-spinner"></div>
      <p>Eszközök betöltése...</p>
    </div>

    <div v-else-if="filteredEszkozok.length === 0" class="state-container empty">
      <span class="state-icon">📦</span>
      <p>Nincs megjeleníthető eszköz.</p>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- MOBIL: KÁRTYA NÉZET -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="cards-view mobile-only">
      <div 
        v-for="eszkoz in filteredEszkozok" 
        :key="eszkoz.eszkozID"
        class="eszkoz-card"
        :class="getCardClass(eszkoz.status)"
      >
        <!-- Kártya fejléc -->
        <div class="card-header">
          <div class="card-id">#{{ eszkoz.eszkozID }}</div>
          <span class="status-badge" :class="getStatusBadge(eszkoz.status)">
            {{ getStatusText(eszkoz.status) }}
          </span>
        </div>

        <!-- Eszköz info -->
        <h3 class="card-title">{{ eszkoz.nev }}</h3>
        <p class="card-category">{{ eszkoz.kategoriaNev }}</p>

        <!-- Megjegyzés (ha van) -->
        <div v-if="eszkoz.megjegyzes" class="card-note">
          <span class="note-icon">💬</span>
          <span class="note-text">{{ eszkoz.megjegyzes }}</span>
        </div>

        <!-- Árak -->
        <div class="card-prices">
          <div class="price-item">
            <span class="price-label">Vételár</span>
            <span class="price-value">{{ formatAr(eszkoz.vetelar) }} Ft</span>
          </div>
          <div class="price-item primary">
            <span class="price-label">Kiadási ár</span>
            <span class="price-value">{{ formatAr(eszkoz.kiadasiAr) }} Ft/ó</span>
          </div>
        </div>

        <!-- Akció gombok -->
        <div class="card-actions">
          <button
            v-if="eszkoz.status === 'Elerheto' || eszkoz.status === 'Foglalva'"
            class="btn-card btn-kivon"
            @click="openKivonModal(eszkoz)"
          >
            <span class="btn-icon">🔴</span>
            <span>Kivonás</span>
          </button>

          <button
            v-if="eszkoz.status === 'Kivonva'"
            class="btn-card btn-elerheto"
            @click="elerhetovaTetel(eszkoz)"
          >
            <span class="btn-icon">🟢</span>
            <span>Elérhető</span>
          </button>

          <button class="btn-card btn-edit" @click="openEditModal(eszkoz)">
            <span class="btn-icon">✏️</span>
            <span>Szerkesztés</span>
          </button>
          <button class="btn-card btn-delete" @click="deleteEszkoz(eszkoz)">
            <span class="btn-icon">🗑️</span>
            <span>Törlés</span>
          </button>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- DESKTOP: TÁBLÁZAT NÉZET -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="table-view desktop-only">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Név</th>
            <th>Kategória</th>
            <th>Vétel</th>
            <th>Kiadás</th>
            <th>Státusz</th>
            <th>Műveletek</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="eszkoz in filteredEszkozok" :key="eszkoz.eszkozID">
            <td><strong>#{{ eszkoz.eszkozID }}</strong></td>
            <td>
              <div class="eszkoz-name">{{ eszkoz.nev }}</div>
              <div v-if="eszkoz.megjegyzes" class="note">💬 {{ eszkoz.megjegyzes }}</div>
            </td>
            <td>{{ eszkoz.kategoriaNev }}</td>
            <td>{{ formatAr(eszkoz.vetelar) }} Ft</td>
            <td><strong class="price-primary">{{ formatAr(eszkoz.kiadasiAr) }} Ft/ó</strong></td>
            <td>
              <span class="status-badge" :class="getStatusBadge(eszkoz.status)">
                {{ getStatusText(eszkoz.status) }}
              </span>
            </td>
            <td>
              <div class="table-actions">
                <button
                  v-if="eszkoz.status === 'Elerheto' || eszkoz.status === 'Foglalva'"
                  class="btn-table btn-kivon"
                  @click="openKivonModal(eszkoz)"
                >🔴</button>

                <button
                  v-if="eszkoz.status === 'Kivonva'"
                  class="btn-table btn-elerheto"
                  @click="elerhetovaTetel(eszkoz)"
                >🟢</button>

                <button class="btn-table btn-edit" @click="openEditModal(eszkoz)">✏️</button>
                <button class="btn-table btn-delete" @click="deleteEszkoz(eszkoz)">🗑️</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- KIVONÁS MODAL -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="kivonModalOpen" class="modal-overlay" @click="closeKivonModal">
          <div class="modal-container small" @click.stop>
            <div class="modal-header">
              <h3 class="modal-title">🔴 Eszköz kivonása</h3>
              <button class="btn-close" @click="closeKivonModal">✕</button>
            </div>

            <div class="modal-content">
              <div class="alert alert-warning">
                ⚠️ Az eszköz ideiglenesen nem lesz elérhető.
              </div>

              <div class="eszkoz-preview" v-if="selectedEszkoz">
                <strong>{{ selectedEszkoz.nev }}</strong>
              </div>

              <div class="form-group">
                <label>Megjegyzés *</label>
                <textarea 
                  v-model="kivonMegjegyzes" 
                  rows="3" 
                  placeholder="Pl: Szervizben, Nem hozták vissza..."
                  required
                ></textarea>
              </div>
            </div>

            <div class="modal-footer">
              <button class="btn-secondary" @click="closeKivonModal">Mégse</button>
              <button 
                class="btn-danger" 
                @click="handleKivonas"
                :disabled="!kivonMegjegyzes.trim()"
              >
                🔴 Kivon
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- SZERKESZTÉS/LÉTREHOZÁS MODAL -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="editModalOpen" class="modal-overlay" @click="closeEditModal">
          <div class="modal-container" @click.stop>
            <div class="modal-header">
              <button class="btn-back mobile-only" @click="closeEditModal">← Vissza</button>
              <h3 class="modal-title">{{ isEditing ? '✏️ Szerkesztés' : '➕ Új eszköz' }}</h3>
              <button class="btn-close desktop-only" @click="closeEditModal">✕</button>
            </div>

            <form @submit.prevent="handleSubmit" class="modal-content">
              <div class="form-row">
                <div class="form-group">
                  <label>Név *</label>
                  <input v-model="form.nev" type="text" required />
                </div>

                <div class="form-group">
                  <label>Kategória *</label>
                  <select v-model.number="form.kategoriaID" required>
                    <option value="">Válassz...</option>
                    <option v-for="kat in kategoriak" :key="kat.kategoriaID" :value="kat.kategoriaID">
                      {{ kat.nev }}
                    </option>
                  </select>
                </div>
              </div>

              <div class="form-group">
                <label>Leírás</label>
                <textarea v-model="form.leiras" rows="2"></textarea>
              </div>

              <div class="form-group">
                <label>Kép URL</label>
                <input v-model="form.kepUrl" type="url" placeholder="https://..." />
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>Vétel (Ft) *</label>
                  <input v-model.number="form.vetelar" type="number" required min="0" />
                </div>

                <div class="form-group">
                  <label>Kiadás (Ft/óra) *</label>
                  <input v-model.number="form.kiadasiAr" type="number" required min="0" />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>Beszerzési dátum *</label>
                  <input v-model="form.beszerzesiDatum" type="date" required />
                </div>

                <div class="form-group" v-if="isEditing">
                  <label>Státusz</label>
                  <select v-model="form.status">
                    <option value="Elerheto">Elérhető</option>
                    <option value="Foglalva">Foglalva</option>
                    <option value="Kiadva">Kiadva</option>
                    <option value="Kivonva">Kivonva</option>
                  </select>
                </div>
              </div>

              <div v-if="error" class="alert alert-error">{{ error }}</div>

              <div class="modal-footer form-footer">
                <button type="button" class="btn-secondary" @click="closeEditModal">Mégse</button>
                <button type="submit" class="btn-primary" :disabled="submitting">
                  {{ submitting ? 'Mentés...' : '💾 Mentés' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- TOAST -->
    <Transition name="toast">
      <div v-if="toast.show" class="toast" :class="toast.type">
        <span>{{ toast.type === 'success' ? '✅' : '❌' }} {{ toast.message }}</span>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { eszkozService } from '@/services/eszkozService'
import { kategoriaService } from '@/services/kategoriaService'
import api from '@/services/api'

// State
const eszkozok = ref([])
const kategoriak = ref([])
const loading = ref(false)
const editModalOpen = ref(false)
const kivonModalOpen = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const error = ref(null)
const selectedEszkoz = ref(null)
const kivonMegjegyzes = ref('')
const activeFilter = ref('all')
const toast = ref({ show: false, message: '', type: 'success' })

let autoRefreshInterval = null

const form = ref({
  eszkozID: null,
  nev: '',
  kategoriaID: '',
  leiras: '',
  kepUrl: '',
  vetelar: 0,
  kiadasiAr: 0,
  beszerzesiDatum: '',
  status: 'Elerheto',
})

// Szűrő opciók
const statusFilters = [
  { value: 'all', label: 'Összes', icon: '📦' },
  { value: 'Elerheto', label: 'Elérhető', icon: '🟢' },
  { value: 'Foglalva', label: 'Foglalva', icon: '🟡' },
  { value: 'Kiadva', label: 'Kiadva', icon: '🔵' },
  { value: 'Kivonva', label: 'Kivonva', icon: '🔴' },
]

// Computed
const filteredEszkozok = computed(() => {
  if (activeFilter.value === 'all') return eszkozok.value
  return eszkozok.value.filter(e => e.status === activeFilter.value)
})

// Methods
function getStatusCount(status) {
  if (status === 'all') return eszkozok.value.length
  return eszkozok.value.filter(e => e.status === status).length
}

function getCardClass(status) {
  return `card-${status.toLowerCase()}`
}

function getStatusBadge(status) {
  const map = {
    Elerheto: 'badge-success',
    Foglalva: 'badge-warning',
    Kiadva: 'badge-info',
    Kivonva: 'badge-danger'
  }
  return map[status] || ''
}

function getStatusText(status) {
  const map = {
    Elerheto: 'Elérhető',
    Foglalva: 'Foglalva',
    Kiadva: 'Kiadva',
    Kivonva: 'Kivonva'
  }
  return map[status] || status
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function showToast(message, type = 'success') {
  toast.value = { show: true, message, type }
  setTimeout(() => { toast.value.show = false }, 3000)
}

async function fetchEszkozok(silent = false) {
  if (!silent) loading.value = true
  try {
    const response = await eszkozService.getAllAdmin()
    eszkozok.value = response.data
  } catch (err) {
    console.error('Eszközök betöltése sikertelen:', err)
  } finally {
    if (!silent) loading.value = false
  }
}

async function fetchKategoriak() {
  try {
    const response = await kategoriaService.getAll()
    kategoriak.value = response.data
  } catch (err) {
    console.error('Kategóriák betöltése sikertelen:', err)
  }
}

function openCreateModal() {
  isEditing.value = false
  form.value = {
    eszkozID: null,
    nev: '',
    kategoriaID: '',
    leiras: '',
    kepUrl: '',
    vetelar: 0,
    kiadasiAr: 0,
    beszerzesiDatum: new Date().toISOString().split('T')[0],
    status: 'Elerheto',
  }
  editModalOpen.value = true
  document.body.style.overflow = 'hidden'
}

async function openEditModal(eszkoz) {
  isEditing.value = true
  try {
    const response = await eszkozService.getById(eszkoz.eszkozID)
    const data = response.data
    form.value = {
      eszkozID: data.eszkozID,
      nev: data.nev,
      kategoriaID: data.kategoriaID,
      leiras: data.leiras || '',
      kepUrl: data.kepUrl || '',
      vetelar: data.vetelar,
      kiadasiAr: data.kiadasiAr,
      beszerzesiDatum: data.beszerzesiDatum.split('T')[0],
      status: data.status,
    }
    editModalOpen.value = true
    document.body.style.overflow = 'hidden'
  } catch (err) {
    showToast('Hiba az eszköz betöltésekor', 'error')
  }
}

function closeEditModal() {
  editModalOpen.value = false
  error.value = null
  document.body.style.overflow = ''
}

function openKivonModal(eszkoz) {
  selectedEszkoz.value = eszkoz
  kivonMegjegyzes.value = ''
  kivonModalOpen.value = true
  document.body.style.overflow = 'hidden'
}

function closeKivonModal() {
  kivonModalOpen.value = false
  selectedEszkoz.value = null
  document.body.style.overflow = ''
}

async function handleSubmit() {
  submitting.value = true
  error.value = null

  try {
    const payload = {
      kategoriaID: form.value.kategoriaID,
      nev: form.value.nev,
      leiras: form.value.leiras || null,
      kepUrl: form.value.kepUrl || null,
      vetelar: form.value.vetelar,
      kiadasiAr: form.value.kiadasiAr,
      beszerzesiDatum: form.value.beszerzesiDatum,
    }

    if (isEditing.value) {
      payload.status = form.value.status
      await eszkozService.update(form.value.eszkozID, payload)
      showToast('Eszköz frissítve!')
    } else {
      await eszkozService.create(payload)
      showToast('Új eszköz létrehozva!')
    }

    await fetchEszkozok(true)
    closeEditModal()
  } catch (err) {
    error.value = err.response?.data?.message || 'Hiba a mentés során.'
  } finally {
    submitting.value = false
  }
}

async function handleKivonas() {
  if (!kivonMegjegyzes.value.trim()) return

  try {
    await api.post(`/eszkozok/${selectedEszkoz.value.eszkozID}/kivon`, {
      megjegyzes: kivonMegjegyzes.value
    })
    showToast('Eszköz kivonva!')
    await fetchEszkozok(true)
    closeKivonModal()
  } catch (err) {
    showToast(err.response?.data?.message || 'Hiba történt', 'error')
  }
}

async function elerhetovaTetel(eszkoz) {
  if (!confirm(`Visszaállítod elérhetőre?\n\n${eszkoz.nev}`)) return

  try {
    await api.post(`/eszkozok/${eszkoz.eszkozID}/elerheto`)
    showToast('Eszköz elérhető!')
    await fetchEszkozok(true)
  } catch (err) {
    showToast(err.response?.data?.message || 'Hiba történt', 'error')
  }
}

async function deleteEszkoz(eszkoz) {
  if (!confirm(`Biztosan törlöd?\n\n${eszkoz.nev}`)) return

  try {
    await eszkozService.delete(eszkoz.eszkozID)
    showToast('Eszköz törölve!')
    await fetchEszkozok(true)
  } catch (err) {
    showToast(err.response?.data?.message || 'Hiba a törlés során', 'error')
  }
}

// Lifecycle
onMounted(async () => {
  await fetchEszkozok()
  await fetchKategoriak()
  autoRefreshInterval = setInterval(() => fetchEszkozok(true), 10000)
})

onUnmounted(() => {
  if (autoRefreshInterval) clearInterval(autoRefreshInterval)
})
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* LAYOUT */
/* ═══════════════════════════════════════════════════════════════════════════ */

.eszkozok-admin {
  padding: var(--spacing-md);
  max-width: 1400px;
  margin: 0 auto;
}

@media (min-width: 768px) {
  .eszkozok-admin { padding: var(--spacing-lg); }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
}

.page-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text);
  margin: 0;
}

@media (min-width: 768px) {
  .page-title { font-size: 24px; }
}

.title-icon { font-size: 1.2em; }

.header-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.btn-refresh,
.btn-add {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  border: none;
  border-radius: var(--radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  min-height: 44px;
}

.btn-refresh {
  background: var(--color-background);
  color: var(--color-text);
}

.btn-add {
  background: var(--color-primary);
  color: white;
}

.btn-add:hover { background: var(--color-primary-dark, #556b1a); }
.btn-add-text { display: none; }

@media (min-width: 768px) {
  .btn-add-text { display: inline; }
}

.refresh-icon.spinning { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* FILTERS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.filters-section {
  margin-bottom: var(--spacing-md);
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.filters-section::-webkit-scrollbar { display: none; }

.filter-chips {
  display: flex;
  gap: var(--spacing-sm);
  padding-bottom: var(--spacing-xs);
}

.filter-chip {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 16px;
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  border-radius: 20px;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  cursor: pointer;
  white-space: nowrap;
  transition: all var(--transition-fast);
  min-height: 44px;
}

.filter-chip:hover { border-color: var(--color-primary); }

.filter-chip.active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
}

.chip-count {
  background: rgba(0, 0, 0, 0.1);
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
}

.filter-chip.active .chip-count { background: rgba(255, 255, 255, 0.2); }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* STATE CONTAINERS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.state-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: var(--spacing-xl);
  text-align: center;
}

.state-icon { font-size: 48px; margin-bottom: var(--spacing-md); }
.state-container p { color: var(--color-text-muted); margin: 0; }

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: var(--spacing-md);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MOBILE: CARD VIEW */
/* ═══════════════════════════════════════════════════════════════════════════ */

.cards-view {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.eszkoz-card {
  background: var(--color-surface);
  border-radius: 14px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(61, 47, 31, 0.08);
  border-left: 5px solid var(--color-border);
  transition: all var(--transition-fast);
}

.eszkoz-card.card-elerheto { border-left-color: var(--color-success); }
.eszkoz-card.card-foglalva { border-left-color: var(--color-waiting); }
.eszkoz-card.card-kiadva { border-left-color: var(--color-active); }
.eszkoz-card.card-kivonva { border-left-color: var(--color-danger); opacity: 0.7; }

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.card-id {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-muted);
  letter-spacing: 0.5px;
}

.card-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-text);
  margin: 0 0 4px 0;
  line-height: 1.3;
}

.card-category {
  font-size: 14px;
  color: var(--color-text-muted);
  margin: 0;
}

.card-note {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  margin-top: 12px;
  padding: 12px;
  background: #fef2f2;
  border-radius: 10px;
  font-size: 13px;
  color: #991b1b;
  line-height: 1.4;
}

.note-icon { flex-shrink: 0; }

.card-prices {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--color-border);
}

.price-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.price-label {
  font-size: 12px;
  color: var(--color-text-muted);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.price-value {
  font-size: 16px;
  font-weight: 700;
  color: var(--color-text);
}

.price-item.primary .price-value {
  color: var(--color-primary);
  font-size: 18px;
}

/* ── CARD ACTION BUTTONS ─────────────────────────────────────────── */

.card-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--color-border);
}

.btn-card {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 14px 12px;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 48px;
  -webkit-tap-highlight-color: transparent;
}

.btn-card:active {
  transform: scale(0.97);
}

.btn-card.btn-kivon {
  background: #fee2e2;
  color: #991b1b;
}

.btn-card.btn-kivon:active { background: #fecaca; }

.btn-card.btn-elerheto {
  background: #d1fae5;
  color: #065f46;
}

.btn-card.btn-elerheto:active { background: #a7f3d0; }

.btn-card.btn-edit {
  background: var(--color-background, #f5f1e8);
  color: var(--color-text);
}

.btn-card.btn-edit:active { background: #e8dcc8; }

.btn-card.btn-delete {
  background: var(--color-background, #f5f1e8);
  color: #991b1b;
}

.btn-card.btn-delete:active { background: #fee2e2; }

.btn-icon { font-size: 16px; flex-shrink: 0; }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* STATUS BADGE */
/* ═══════════════════════════════════════════════════════════════════════════ */

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 5px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
}

.badge-success { background: #d1fae5; color: #065f46; }
.badge-warning { background: #fef3c7; color: #92400e; }
.badge-info { background: #dbeafe; color: #1e40af; }
.badge-danger { background: #fee2e2; color: #991b1b; }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* DESKTOP: TABLE VIEW */
/* ═══════════════════════════════════════════════════════════════════════════ */

.data-table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.data-table th,
.data-table td {
  padding: var(--spacing-md);
  text-align: left;
  border-bottom: 1px solid var(--color-border);
}

.data-table th {
  background: var(--color-background);
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
}

.data-table tbody tr {
  transition: background var(--transition-fast);
}

.data-table tbody tr:hover { background: var(--color-background); }

.eszkoz-name { font-weight: 600; }
.note { font-size: 12px; color: #991b1b; margin-top: 4px; }
.price-primary { color: var(--color-primary); }

.table-actions { display: flex; gap: var(--spacing-xs); }

.btn-table {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: var(--radius-sm);
  cursor: pointer;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all var(--transition-fast);
}

.btn-table:hover { transform: translateY(-1px); }
.btn-table.btn-kivon { background: #fee2e2; }
.btn-table.btn-elerheto { background: #d1fae5; }
.btn-table.btn-edit { background: var(--color-background); }
.btn-table.btn-delete { background: var(--color-background); }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODALS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
  display: flex;
  align-items: flex-end;
}

@media (min-width: 768px) {
  .modal-overlay {
    align-items: center;
    justify-content: center;
    padding: var(--spacing-lg);
    background: rgba(61, 47, 31, 0.7);
    backdrop-filter: blur(4px);
  }
}

.modal-container {
  background: var(--color-surface);
  width: 100%;
  max-height: 92vh;
  border-radius: 16px 16px 0 0;
  display: flex;
  flex-direction: column;
  animation: slideUp 0.3s ease-out;
  overflow: hidden;
}

@media (min-width: 768px) {
  .modal-container {
    max-width: 600px;
    border-radius: var(--radius-lg);
    animation: scaleIn 0.2s ease-out;
  }
  .modal-container.small { max-width: 450px; }
}

@keyframes slideUp {
  from { transform: translateY(100%); }
  to { transform: translateY(0); }
}

@keyframes scaleIn {
  from { transform: scale(0.95); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--color-border);
  flex-shrink: 0;
}

.modal-title { font-size: 18px; font-weight: 600; margin: 0; }

.btn-back {
  background: none;
  border: none;
  color: var(--color-primary);
  font-weight: 600;
  cursor: pointer;
  padding: 8px;
  margin: -8px;
  font-size: 16px;
  min-height: 44px;
  display: flex;
  align-items: center;
}

.btn-close {
  width: 40px;
  height: 40px;
  background: none;
  border: none;
  font-size: 24px;
  color: var(--color-text-muted);
  cursor: pointer;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-content {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  -webkit-overflow-scrolling: touch;
}

.modal-footer {
  display: flex;
  gap: 12px;
  padding: 16px 20px;
  border-top: 1px solid var(--color-border);
  flex-shrink: 0;
}

/* Ha a modal-footer a form-on belül van */
.form-footer {
  margin-top: 8px;
  padding: 0;
  border-top: none;
}

.btn-primary,
.btn-secondary,
.btn-danger {
  flex: 1;
  padding: 14px;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  border: none;
  min-height: 48px;
  transition: all 0.2s ease;
}

.btn-primary { background: var(--color-primary); color: white; }
.btn-primary:hover:not(:disabled) { background: var(--color-primary-dark, #556b1a); }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-secondary { background: var(--color-surface); border: 2px solid var(--color-border); color: var(--color-text); }
.btn-danger { background: var(--color-danger, #dc2626); color: white; }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* FORM */
/* ═══════════════════════════════════════════════════════════════════════════ */

.form-group { margin-bottom: 16px; }

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 6px;
  font-size: 14px;
  color: var(--color-text);
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 12px 14px;
  border: 2px solid var(--color-border);
  border-radius: 10px;
  font-size: 16px;
  color: var(--color-text);
  background: var(--color-surface);
  transition: border-color 0.2s ease;
  appearance: none;
  -webkit-appearance: none;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

@media (max-width: 500px) {
  .form-row { grid-template-columns: 1fr; }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* ALERTS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.alert {
  padding: 14px 16px;
  border-radius: 10px;
  margin-bottom: 16px;
  font-size: 14px;
  line-height: 1.5;
}

.alert-warning { background: #fef3c7; color: #92400e; }
.alert-error { background: #fee2e2; color: #991b1b; }

.eszkoz-preview {
  padding: 14px 16px;
  background: var(--color-background);
  border-radius: 10px;
  margin-bottom: 16px;
  font-size: 16px;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* TOAST */
/* ═══════════════════════════════════════════════════════════════════════════ */

.toast {
  position: fixed;
  bottom: calc(var(--bottom-nav-height, 0px) + 16px);
  left: 16px;
  right: 16px;
  padding: 16px;
  border-radius: 12px;
  z-index: 2000;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  font-weight: 600;
  font-size: 15px;
}

@media (min-width: 768px) {
  .toast { left: auto; right: 24px; bottom: 24px; max-width: 400px; }
}

.toast.success { background: var(--color-success); color: white; }
.toast.error { background: var(--color-danger); color: white; }

.toast-enter-active,
.toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from { transform: translateY(100px); opacity: 0; }
.toast-leave-to { transform: translateX(100%); opacity: 0; }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* RESPONSIVE HELPERS */
/* ═══════════════════════════════════════════════════════════════════════════ */

.mobile-only { display: block; }
.desktop-only { display: none; }

@media (min-width: 768px) {
  .mobile-only { display: none; }
  .desktop-only { display: block; }
}

/* Modal transitions */
.modal-enter-active,
.modal-leave-active { transition: opacity 0.3s ease; }
.modal-enter-from,
.modal-leave-to { opacity: 0; }

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MOBILE BREAKPOINT (max-width: 768px)  */
/* ═══════════════════════════════════════════════════════════════════════════ */

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  .header-actions {
    width: 100%;
    display: grid;
    grid-template-columns: auto 1fr;
    gap: 10px;
  }

  .btn-add {
    justify-content: center;
  }

  /* ── Modal full-screen mobilon ─────────────────────────────── */
  .modal-container {
    max-height: 100vh;
    height: 100vh;
    border-radius: 0;
  }

  .modal-header {
    position: sticky;
    top: 0;
    background: var(--color-surface);
    z-index: 10;
  }

  .modal-content {
    flex: 1;
    max-height: none;
  }

  .modal-footer,
  .form-footer {
    position: sticky;
    bottom: 0;
    background: var(--color-surface);
    padding: 16px 20px;
    border-top: 1px solid var(--color-border);
  }

  .form-footer {
    margin: 0 -20px -20px;
    padding: 16px 20px;
    border-top: 1px solid var(--color-border);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* EXTRA SMALL (max-width: 480px) */
/* ═══════════════════════════════════════════════════════════════════════════ */

@media (max-width: 480px) {
  .eszkozok-admin { padding: 12px; }

  .page-title { font-size: 18px; }
  .title-icon { font-size: 20px; }

  .cards-view { gap: 12px; }

  .eszkoz-card { padding: 16px; }
  .card-title { font-size: 17px; }

  .card-actions {
    grid-template-columns: 1fr 1fr;
    gap: 8px;
  }

  .btn-card {
    padding: 12px 10px;
    font-size: 13px;
    min-height: 44px;
  }

  .filter-chip {
    padding: 8px 12px;
    font-size: 13px;
    min-height: 40px;
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* LANDSCAPE (kis képernyő landscape) */
/* ═══════════════════════════════════════════════════════════════════════════ */

@media (max-width: 768px) and (orientation: landscape) {
  .modal-container {
    max-height: 100vh;
  }
}
</style>