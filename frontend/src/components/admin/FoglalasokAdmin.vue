<template>
  <div class="foglalasok-admin">
    <div class="page-header">
      <h2 class="page-title">
        <span class="title-icon">📋</span>
        <span>{{ t('foglalasokAdmin.title') }}</span>
      </h2>
      <button class="btn-refresh" @click="fetchFoglalasok" :disabled="loading">
        <span class="refresh-icon" :class="{ spinning: loading }">🔄</span>
        <span class="refresh-text">{{ t('foglalasokAdmin.refresh') }}</span>
      </button>
    </div>

    <!-- SZŰRŐK: MOBIL DROPDOWN -->
    <div class="filters-section mobile-only">
      <button class="filter-dropdown-toggle" @click="toggleFilterMenu">
        <span class="filter-active-icon">{{ getActiveFilterIcon() }}</span>
        <span class="filter-active-label">{{ getActiveFilterLabel() }}</span>
        <span class="filter-active-count" v-if="getStatusCount(activeFilter) > 0">{{ getStatusCount(activeFilter) }}</span>
        <span class="filter-chevron" :class="{ open: filterMenuOpen }">▾</span>
      </button>
      <Transition name="dropdown">
        <div v-if="filterMenuOpen" class="filter-dropdown-menu">
          <button v-for="filter in statusFilters" :key="filter.value" class="filter-dropdown-item" :class="{ active: activeFilter === filter.value }" @click="selectFilter(filter.value)">
            <span class="dropdown-item-icon">{{ filter.icon }}</span>
            <span class="dropdown-item-label">{{ filter.label }}</span>
            <span class="dropdown-item-count" v-if="getStatusCount(filter.value) > 0">{{ getStatusCount(filter.value) }}</span>
            <span v-if="activeFilter === filter.value" class="dropdown-item-check">✓</span>
          </button>
        </div>
      </Transition>
    </div>

    <!-- SZŰRŐK: DESKTOP CHIPS -->
    <div class="filters-section desktop-only">
      <div class="filter-chips">
        <button v-for="filter in statusFilters" :key="filter.value" class="filter-chip" :class="{ active: activeFilter === filter.value }" @click="activeFilter = filter.value">
          <span class="chip-icon">{{ filter.icon }}</span>
          <span class="chip-label">{{ filter.label }}</span>
          <span class="chip-count" v-if="getStatusCount(filter.value) > 0">{{ getStatusCount(filter.value) }}</span>
        </button>
      </div>
    </div>

    <div v-if="loading && !foglalasok.length" class="state-container">
      <div class="loading-spinner"></div>
      <p>{{ t('foglalasokAdmin.loading') }}</p>
    </div>

    <div v-else-if="filteredFoglalasok.length === 0" class="state-container empty">
      <span class="state-icon">📭</span>
      <p v-if="activeFilter === 'all'">{{ t('foglalasokAdmin.emptyAll') }}</p>
      <p v-else>{{ t('foglalasokAdmin.emptyFilter', { status: getFilterLabel(activeFilter) }) }}</p>
      <button v-if="activeFilter !== 'all'" class="btn-clear" @click="activeFilter = 'all'">{{ t('foglalasokAdmin.showAll') }}</button>
    </div>

    <!-- MOBIL: KÁRTYA NÉZET -->
    <div v-else class="cards-view mobile-only">
      <div v-for="foglalas in filteredFoglalasok" :key="foglalas.foglalasID" class="foglalas-card" :class="getCardClass(foglalas.status)" @click="openDetailModal(foglalas)">
        <div class="card-header">
          <div class="card-id">#{{ foglalas.foglalasID }}</div>
          <span class="status-badge" :class="getBadgeClass(foglalas.status)">{{ getStatusText(foglalas.status) }}</span>
        </div>
        <h3 class="card-title">{{ foglalas.eszkozNev }}</h3>
        <div class="card-info-rows">
          <div class="card-info-row"><span class="info-icon">👤</span><span class="info-value">{{ foglalas.nev }}</span></div>
          <div class="card-info-row"><span class="info-icon">📅</span><span class="info-value">{{ formatShortDate(foglalas.foglalasKezdete) }}</span></div>
          <div v-if="foglalas.elszamolhatoIdo" class="card-info-row"><span class="info-icon">⏱️</span><span class="info-value">{{ formatIdo(foglalas.elszamolhatoIdo) }}</span></div>
        </div>
        <div v-if="foglalas.bevetel" class="card-revenue">
          <span class="revenue-label">{{ t('foglalasokAdmin.revenue') }}</span>
          <span class="revenue-value">{{ formatAr(foglalas.bevetel) }} Ft</span>
        </div>
        <div class="card-actions" @click.stop>
          <button v-if="foglalas.status === 'Foglalva'" class="btn-card btn-kiad" @click="kiadEszkoz(foglalas)"><span class="btn-icon">✅</span><span>{{ t('foglalasokAdmin.issue') }}</span></button>
          <button v-if="foglalas.status === 'Kiadva'" class="btn-card btn-visszahoz" @click="visszahozEszkoz(foglalas)"><span class="btn-icon">🔄</span><span>{{ t('foglalasokAdmin.returned') }}</span></button>
          <button v-if="foglalas.status === 'Foglalva' || foglalas.status === 'Kiadva'" class="btn-card btn-torol" @click="openDeleteConfirm(foglalas)"><span class="btn-icon">❌</span><span>{{ t('foglalasokAdmin.delete') }}</span></button>
          <button class="btn-card btn-details" @click="openDetailModal(foglalas)"><span class="btn-icon">👁️</span><span>{{ t('foglalasokAdmin.details') }}</span></button>
        </div>
      </div>
    </div>

    <!-- DESKTOP: TÁBLÁZAT -->
    <div v-if="filteredFoglalasok.length > 0" class="table-view desktop-only">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>{{ t('foglalasokAdmin.tool') }}</th>
            <th>{{ t('foglalasokAdmin.client') }}</th>
            <th>{{ t('foglalasokAdmin.start') }}</th>
            <th>{{ t('foglalasokAdmin.elapsed') }}</th>
            <th>{{ t('foglalasokAdmin.revenue') }}</th>
            <th>{{ t('foglalasokAdmin.status') }}</th>
            <th>{{ t('foglalasokAdmin.actions') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="foglalas in filteredFoglalasok" :key="foglalas.foglalasID" :class="getRowClass(foglalas.status)">
            <td><strong>#{{ foglalas.foglalasID }}</strong></td>
            <td><div class="eszkoz-info">{{ foglalas.eszkozNev }}</div></td>
            <td>
              <div class="customer-cell">
                <strong>{{ foglalas.nev }}</strong>
                <span class="small">{{ foglalas.email }}</span>
                <span class="small">{{ foglalas.telefonszam }}</span>
              </div>
            </td>
            <td><span class="date-cell">{{ formatDate(foglalas.foglalasKezdete) }}</span></td>
            <td>
              <span v-if="foglalas.elszamolhatoIdo">{{ formatIdo(foglalas.elszamolhatoIdo) }}</span>
              <span v-else class="text-muted">-</span>
            </td>
            <td>
              <strong v-if="foglalas.bevetel" class="revenue">{{ formatAr(foglalas.bevetel) }} Ft</strong>
              <span v-else class="text-muted">-</span>
            </td>
            <td>
              <span class="status-badge" :class="getBadgeClass(foglalas.status)">{{ getStatusText(foglalas.status) }}</span>
              <div v-if="foglalas.kiadasIdopontja" class="small text-muted">{{ t('foglalasokAdmin.issuedAt') }}: {{ formatShortDate(foglalas.kiadasIdopontja) }}</div>
            </td>
            <td>
              <div class="table-actions">
                <button v-if="foglalas.status === 'Foglalva'" class="btn-table btn-kiad" @click="kiadEszkoz(foglalas)">✅ {{ t('foglalasokAdmin.issue') }}</button>
                <button v-if="foglalas.status === 'Kiadva'" class="btn-table btn-visszahoz" @click="visszahozEszkoz(foglalas)">🔄 {{ t('foglalasokAdmin.back') }}</button>
                <button class="btn-table btn-info" @click="openDetailModal(foglalas)">👁️</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- RÉSZLETEK MODAL -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
          <div class="modal-container" @click.stop>
            <div class="modal-header">
              <button class="btn-back mobile-only" @click="closeModal">← {{ t('foglalasokAdmin.back') }}</button>
              <h3 class="modal-title">{{ t('foglalasokAdmin.reservationTitle') }} #{{ selectedFoglalas.foglalasID }}</h3>
              <button class="btn-close desktop-only" @click="closeModal">✕</button>
            </div>
            <div class="modal-content">
              <div class="status-card" :class="getBadgeClass(selectedFoglalas.status)">
                <span class="status-label">{{ t('foglalasokAdmin.status') }}</span>
                <span class="status-value">{{ getStatusText(selectedFoglalas.status) }}</span>
              </div>
              <div class="details-grid">
                <div class="detail-group"><h4 class="group-title">🔧 {{ t('foglalasokAdmin.tool') }}</h4><p class="group-value">{{ selectedFoglalas.eszkozNev }}</p></div>
                <div class="detail-group"><h4 class="group-title">👤 {{ t('foglalasokAdmin.client') }}</h4><p class="group-value">{{ selectedFoglalas.nev }}</p><p class="group-sub">{{ selectedFoglalas.email }}</p><p class="group-sub">{{ selectedFoglalas.telefonszam }}</p></div>
                <div class="detail-group"><h4 class="group-title">📍 {{ t('foglalasokAdmin.address') }}</h4><p class="group-value">{{ selectedFoglalas.cim }}</p></div>
                <div class="detail-group"><h4 class="group-title">📅 {{ t('foglalasokAdmin.reservationStart') }}</h4><p class="group-value">{{ formatDate(selectedFoglalas.foglalasKezdete) }}</p></div>
                <div v-if="selectedFoglalas.kiadasIdopontja" class="detail-group"><h4 class="group-title">✅ {{ t('foglalasokAdmin.issued') }}</h4><p class="group-value">{{ formatDate(selectedFoglalas.kiadasIdopontja) }}</p></div>
                <div v-if="selectedFoglalas.visszahozasIdopontja" class="detail-group"><h4 class="group-title">🔄 {{ t('foglalasokAdmin.returnedAt') }}</h4><p class="group-value">{{ formatDate(selectedFoglalas.visszahozasIdopontja) }}</p></div>
                <div v-if="selectedFoglalas.elszamolhatoIdo" class="detail-group"><h4 class="group-title">⏱️ {{ t('foglalasokAdmin.elapsed') }}</h4><p class="group-value">{{ formatIdo(selectedFoglalas.elszamolhatoIdo) }}</p></div>
                <div v-if="selectedFoglalas.bevetel" class="detail-group highlight"><h4 class="group-title">💰 {{ t('foglalasokAdmin.revenue') }}</h4><p class="group-value large">{{ formatAr(selectedFoglalas.bevetel) }} Ft</p></div>
                <div v-if="selectedFoglalas.fizetendoOsszeg" class="detail-group highlight"><h4 class="group-title">💰 {{ t('foglalasokAdmin.amountDue') }}</h4><p class="group-value large">{{ formatAr(selectedFoglalas.fizetendoOsszeg) }} Ft</p></div>
              </div>
            </div>
            <div class="modal-footer">
              <button v-if="selectedFoglalas.status === 'Foglalva'" class="btn-primary" @click="handleModalAction(kiadEszkoz)">✅ {{ t('foglalasokAdmin.issueBtn') }}</button>
              <button v-if="selectedFoglalas.status === 'Kiadva'" class="btn-primary" @click="handleModalAction(visszahozEszkoz)">🔄 {{ t('foglalasokAdmin.returned') }}</button>
              <button v-if="selectedFoglalas.status === 'Foglalva' || selectedFoglalas.status === 'Kiadva'" class="btn-danger" @click="handleModalAction(openDeleteConfirm)">❌ {{ t('foglalasokAdmin.delete') }}</button>
              <button class="btn-secondary" @click="closeModal">{{ t('foglalasokAdmin.close') }}</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <Transition name="toast">
      <div v-if="toast.show" class="toast" :class="toast.type">
        <span class="toast-icon">{{ toast.type === 'success' ? '✅' : '❌' }}</span>
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'
import api from '@/services/api'

const { t } = useI18n()

const foglalasok = ref([])
const loading = ref(false)
const modalOpen = ref(false)
const selectedFoglalas = ref({})
const filterMenuOpen = ref(false)
const activeFilter = ref('all')
const toast = ref({ show: false, message: '', type: 'success' })

const statusFilters = computed(() => [
  { value: 'all', label: t('foglalasokAdmin.filterAll'), icon: '📋' },
  { value: 'Foglalva', label: t('foglalasokAdmin.statusMap.reserved'), icon: '📌' },
  { value: 'Kiadva', label: t('foglalasokAdmin.statusMap.issued'), icon: '🔧' },
  { value: 'Lezart', label: t('foglalasokAdmin.statusMap.closed'), icon: '✅' },
  { value: 'Torolt', label: t('foglalasokAdmin.statusMap.cancelled'), icon: '❌' },
])

const filteredFoglalasok = computed(() => {
  if (activeFilter.value === 'all') return foglalasok.value
  return foglalasok.value.filter((f) => f.status === activeFilter.value)
})

function getStatusCount(filterValue) {
  if (filterValue === 'all') return foglalasok.value.length
  return foglalasok.value.filter((f) => f.status === filterValue).length
}

function getFilterLabel(filterValue) {
  const filter = statusFilters.value.find((f) => f.value === filterValue)
  return filter ? filter.label : filterValue
}

function getActiveFilterIcon() {
  const filter = statusFilters.value.find((f) => f.value === activeFilter.value)
  return filter ? filter.icon : '📋'
}

function getActiveFilterLabel() { return getFilterLabel(activeFilter.value) }
function toggleFilterMenu() { filterMenuOpen.value = !filterMenuOpen.value }
function selectFilter(value) { activeFilter.value = value; filterMenuOpen.value = false }

let autoRefreshInterval = null

onMounted(() => {
  fetchFoglalasok()
  autoRefreshInterval = setInterval(() => fetchFoglalasok(true), 10000)
})

onUnmounted(() => { if (autoRefreshInterval) clearInterval(autoRefreshInterval) })

async function fetchFoglalasok(silent = false) {
  if (!silent) loading.value = true
  try {
    const response = await api.get('/foglalasok')
    foglalasok.value = response.data
  } catch (err) {
    console.error('Foglalások betöltése sikertelen:', err)
    if (!silent) showToast(t('foglalasokAdmin.errors.load'), 'error')
  } finally {
    if (!silent) loading.value = false
  }
}

async function kiadEszkoz(foglalas) {
  if (!confirm(`${t('foglalasokAdmin.confirms.issue')}\n\n${t('foglalasokAdmin.tool')}: ${foglalas.eszkozNev}\n${t('foglalasokAdmin.client')}: ${foglalas.nev}`)) return
  try {
    await api.put(`/foglalasok/${foglalas.foglalasID}/kiadas`)
    showToast(t('foglalasokAdmin.success.issued'))
    await fetchFoglalasok()
  } catch (err) {
    showToast(err.response?.data?.message || t('foglalasokAdmin.errors.issue'), 'error')
  }
}

async function visszahozEszkoz(foglalas) {
  if (!confirm(`${t('foglalasokAdmin.confirms.return')}\n\n${t('foglalasokAdmin.tool')}: ${foglalas.eszkozNev}`)) return
  try {
    const response = await api.put(`/foglalasok/${foglalas.foglalasID}/lezaras`)
    const data = response.data
    if (data.elszamolhatoIdo) {
      showToast(`${t('foglalasokAdmin.success.returned')} ${formatIdo(data.elszamolhatoIdo)} - ${formatAr(data.fizetendoOsszeg)} Ft`)
    } else {
      showToast(t('foglalasokAdmin.success.returnedSimple'))
    }
    await fetchFoglalasok()
  } catch (err) {
    showToast(err.response?.data?.message || t('foglalasokAdmin.errors.return'), 'error')
  }
}

async function openDeleteConfirm(foglalas) {
  if (!confirm(`${t('foglalasokAdmin.confirms.delete')}\n\n${t('foglalasokAdmin.tool')}: ${foglalas.eszkozNev}\n${t('foglalasokAdmin.client')}: ${foglalas.nev}`)) return
  try {
    await api.put(`/foglalasok/${foglalas.foglalasID}/torles`)
    showToast(t('foglalasokAdmin.success.deleted'))
    await fetchFoglalasok()
  } catch (err) {
    showToast(err.response?.data?.message || t('foglalasokAdmin.errors.delete'), 'error')
  }
}

function openDetailModal(foglalas) { selectedFoglalas.value = foglalas; modalOpen.value = true }
function closeModal() { modalOpen.value = false }
async function handleModalAction(actionFn) { await actionFn(selectedFoglalas.value); closeModal() }

function showToast(message, type = 'success') {
  toast.value = { show: true, message, type }
  setTimeout(() => { toast.value.show = false }, 3000)
}

function formatDate(dateString) {
  if (!dateString) return '-'
  return new Intl.DateTimeFormat('hu-HU', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(dateString))
}

function formatShortDate(dateString) {
  if (!dateString) return '-'
  return new Intl.DateTimeFormat('hu-HU', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(dateString))
}

function formatAr(ar) {
  if (!ar) return '0'
  return new Intl.NumberFormat('hu-HU').format(Math.round(ar))
}

function formatIdo(percek) {
  if (!percek) return '-'
  const orak = Math.floor(percek / 60)
  const maradekPercek = percek % 60
  if (orak === 0) return `${maradekPercek} ${t('foglalasokAdmin.minutes')}`
  return `${orak}h ${maradekPercek}m`
}

function getStatusText(status) {
  const map = {
    Foglalva: `📌 ${t('foglalasokAdmin.statusMap.reserved')}`,
    Kiadva: `🔧 ${t('foglalasokAdmin.statusMap.issued')}`,
    Lezart: `✅ ${t('foglalasokAdmin.statusMap.closed')}`,
    Torolt: `❌ ${t('foglalasokAdmin.statusMap.cancelled')}`,
  }
  return map[status] || status
}

function getBadgeClass(status) {
  return { Foglalva: 'badge-warning', Kiadva: 'badge-info', Lezart: 'badge-success', Torolt: 'badge-danger' }[status] || ''
}

function getCardClass(status) {
  return { Foglalva: 'card-foglalva', Kiadva: 'card-kiadva', Lezart: 'card-lezart', Torolt: 'card-torolt' }[status] || ''
}

function getRowClass(status) {
  return { Foglalva: 'row-Foglalva', Kiadva: 'row-kiadva', Lezart: 'row-lezart', Torolt: 'row-torolt' }[status] || ''
}
</script>

<style scoped>
.foglalasok-admin { padding: var(--spacing-md, 16px); max-width: 1400px; margin: 0 auto; }
@media (min-width: 768px) { .foglalasok-admin { padding: var(--spacing-lg, 24px); } }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--spacing-md, 16px); }
.page-title { display: flex; align-items: center; gap: var(--spacing-sm, 8px); font-size: 20px; font-weight: 700; color: var(--color-text, #1f2937); margin: 0; }
@media (min-width: 768px) { .page-title { font-size: 28px; } }
.title-icon { font-size: 1.2em; }
.btn-refresh { display: flex; align-items: center; gap: var(--spacing-xs, 6px); padding: 10px 16px; background: var(--color-primary, #6b8e23); color: white; border: none; border-radius: var(--radius-md, 8px); font-weight: 600; font-size: 14px; cursor: pointer; transition: all 0.2s ease; min-height: 44px; }
.btn-refresh:hover:not(:disabled) { background: var(--color-primary-dark, #5a7a1e); }
.btn-refresh:disabled { opacity: 0.6; cursor: not-allowed; }
.refresh-icon { font-size: 16px; display: inline-block; }
.refresh-icon.spinning { animation: spin 1s linear infinite; }
.refresh-text { display: none; }
@media (min-width: 768px) { .refresh-text { display: inline; } }
@keyframes spin { to { transform: rotate(360deg); } }

.filters-section { margin-bottom: var(--spacing-md, 16px); position: relative; }
.filter-dropdown-toggle { display: flex; align-items: center; gap: 10px; width: 100%; padding: 14px 16px; background: var(--color-surface, white); border: 2px solid var(--color-border, #e5e7eb); border-radius: 12px; font-size: 16px; font-weight: 600; color: var(--color-text, #1f2937); cursor: pointer; transition: all 0.2s ease; min-height: 52px; -webkit-tap-highlight-color: transparent; }
.filter-dropdown-toggle:active { transform: scale(0.99); }
.filter-active-icon { font-size: 20px; }
.filter-active-label { flex: 1; text-align: left; }
.filter-active-count { background: var(--color-primary, #6b8e23); color: white; padding: 2px 10px; border-radius: 10px; font-size: 13px; font-weight: 700; }
.filter-chevron { font-size: 18px; color: var(--color-text-muted, #6b7280); transition: transform 0.2s ease; }
.filter-chevron.open { transform: rotate(180deg); }
.filter-dropdown-menu { position: absolute; top: calc(100% + 6px); left: 0; right: 0; background: var(--color-surface, white); border-radius: 14px; box-shadow: 0 8px 30px rgba(0,0,0,0.15); z-index: 100; overflow: hidden; border: 1px solid var(--color-border, #e5e7eb); }
.filter-dropdown-item { display: flex; align-items: center; gap: 12px; width: 100%; padding: 14px 18px; background: none; border: none; border-bottom: 1px solid var(--color-border, #f3f4f6); font-size: 16px; font-weight: 500; color: var(--color-text, #1f2937); cursor: pointer; transition: background 0.15s ease; min-height: 52px; -webkit-tap-highlight-color: transparent; text-align: left; }
.filter-dropdown-item:last-child { border-bottom: none; }
.filter-dropdown-item:active { background: var(--color-background, #f5f1e8); }
.filter-dropdown-item.active { background: rgba(107,142,35,0.08); color: var(--color-primary, #6b8e23); font-weight: 600; }
.dropdown-item-icon { font-size: 20px; width: 28px; text-align: center; flex-shrink: 0; }
.dropdown-item-label { flex: 1; }
.dropdown-item-count { background: var(--color-border, #e5e7eb); padding: 2px 10px; border-radius: 10px; font-size: 13px; font-weight: 600; color: var(--color-text-muted, #6b7280); }
.filter-dropdown-item.active .dropdown-item-count { background: rgba(107,142,35,0.15); color: var(--color-primary, #6b8e23); }
.dropdown-item-check { color: var(--color-primary, #6b8e23); font-weight: 700; font-size: 18px; }
.dropdown-enter-active, .dropdown-leave-active { transition: all 0.2s ease; transform-origin: top center; }
.dropdown-enter-from, .dropdown-leave-to { opacity: 0; transform: scaleY(0.9) translateY(-4px); }

.filter-chips { display: flex; gap: var(--spacing-sm, 8px); padding-bottom: var(--spacing-xs, 4px); }
.filter-chip { display: flex; align-items: center; gap: 6px; padding: 10px 16px; background: var(--color-surface, white); border: 2px solid var(--color-border, #e5e7eb); border-radius: 20px; font-size: 14px; font-weight: 500; color: var(--color-text, #1f2937); cursor: pointer; white-space: nowrap; transition: all 0.2s ease; min-height: 44px; }
.filter-chip:hover { border-color: var(--color-primary, #6b8e23); }
.filter-chip.active { background: var(--color-primary, #6b8e23); border-color: var(--color-primary, #6b8e23); color: white; }
.chip-count { background: rgba(0,0,0,0.1); padding: 2px 8px; border-radius: 10px; font-size: 12px; font-weight: 600; }
.filter-chip.active .chip-count { background: rgba(255,255,255,0.2); }

.state-container { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: var(--spacing-xl, 60px) var(--spacing-md, 16px); text-align: center; }
.state-icon { font-size: 48px; margin-bottom: var(--spacing-md, 16px); }
.state-container p { color: var(--color-text-muted, #6b7280); margin: 0 0 var(--spacing-md, 16px) 0; font-size: 16px; }
.loading-spinner { width: 40px; height: 40px; border: 3px solid var(--color-border, #e5e7eb); border-top-color: var(--color-primary, #6b8e23); border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: var(--spacing-md, 16px); }
.btn-clear { padding: 12px 20px; background: var(--color-primary, #6b8e23); color: white; border: none; border-radius: var(--radius-md, 8px); font-weight: 600; font-size: 15px; cursor: pointer; min-height: 44px; }

.cards-view { display: flex; flex-direction: column; gap: 16px; }
.foglalas-card { background: var(--color-surface, white); border-radius: 14px; padding: 20px; box-shadow: 0 2px 8px rgba(61,47,31,0.08); border-left: 5px solid var(--color-border, #e5e7eb); cursor: pointer; transition: all 0.2s ease; -webkit-tap-highlight-color: transparent; }
.foglalas-card:active { transform: scale(0.98); }
.foglalas-card.card-foglalva { border-left-color: var(--color-waiting, #f59e0b); }
.foglalas-card.card-kiadva { border-left-color: var(--color-active, #3b82f6); }
.foglalas-card.card-lezart { border-left-color: var(--color-success, #10b981); }
.foglalas-card.card-torolt { border-left-color: var(--color-danger, #ef4444); opacity: 0.7; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
.card-id { font-size: 13px; font-weight: 700; color: var(--color-text-muted, #6b7280); letter-spacing: 0.5px; }
.card-title { font-size: 18px; font-weight: 700; color: var(--color-text, #1f2937); margin: 0 0 12px 0; line-height: 1.3; }
.card-info-rows { display: flex; flex-direction: column; gap: 8px; }
.card-info-row { display: flex; align-items: center; gap: 10px; font-size: 15px; color: var(--color-text-muted, #6b7280); }
.info-icon { font-size: 16px; flex-shrink: 0; width: 24px; text-align: center; }
.info-value { line-height: 1.3; }
.card-revenue { display: flex; justify-content: space-between; align-items: center; margin-top: 16px; padding-top: 16px; border-top: 1px solid var(--color-border, #e5e7eb); }
.revenue-label { font-size: 13px; color: var(--color-text-muted, #6b7280); font-weight: 500; text-transform: uppercase; letter-spacing: 0.3px; }
.revenue-value { font-size: 20px; font-weight: 800; color: var(--color-primary, #6b8e23); }
.card-actions { display: flex; gap: 10px; margin-top: 16px; padding-top: 16px; border-top: 1px solid var(--color-border, #e5e7eb); }
.btn-card { flex: 1; display: flex; align-items: center; justify-content: center; gap: 8px; padding: 14px 12px; border: none; border-radius: 10px; font-size: 14px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; min-height: 48px; -webkit-tap-highlight-color: transparent; }
.btn-card:active { transform: scale(0.97); }
.btn-card.btn-kiad { background: var(--color-success, #10b981); color: white; }
.btn-card.btn-visszahoz { background: var(--color-active, #3b82f6); color: white; }
.btn-card.btn-torol { background: var(--color-danger, #ef4444); color: white; }
.btn-card.btn-details { background: var(--color-background, #f5f1e8); color: var(--color-text, #1f2937); }
.btn-icon { font-size: 16px; flex-shrink: 0; }

.status-badge { display: inline-flex; align-items: center; padding: 5px 12px; border-radius: 12px; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.3px; white-space: nowrap; }
.badge-warning { background: #fef3c7; color: #92400e; }
.badge-info { background: #dbeafe; color: #1e40af; }
.badge-success { background: #d1fae5; color: #065f46; }
.badge-danger { background: #fee2e2; color: #991b1b; }

.data-table { width: 100%; border-collapse: collapse; background: var(--color-surface, white); border-radius: var(--radius-lg, 12px); overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
.data-table th, .data-table td { padding: var(--spacing-md, 16px); text-align: left; border-bottom: 1px solid var(--color-border, #e5e7eb); }
.data-table th { background: var(--color-background, #f9fafb); font-weight: 600; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; color: var(--color-text-muted, #6b7280); }
.data-table tbody tr { transition: background 0.2s ease; }
.data-table tbody tr:hover { background: var(--color-background, #f9fafb); }
.data-table tbody tr.row-Foglalva { border-left: 3px solid var(--color-waiting, #f59e0b); }
.data-table tbody tr.row-kiadva { border-left: 3px solid var(--color-active, #3b82f6); }
.data-table tbody tr.row-lezart { border-left: 3px solid var(--color-success, #10b981); }
.data-table tbody tr.row-torolt { border-left: 3px solid var(--color-danger, #ef4444); opacity: 0.6; }
.customer-cell { display: flex; flex-direction: column; gap: 2px; }
.small { font-size: 12px; color: var(--color-text-muted, #6b7280); }
.text-muted { color: #9ca3af; }
.revenue { color: var(--color-primary, #6b8e23); font-weight: 700; }
.table-actions { display: flex; gap: var(--spacing-xs, 6px); }
.btn-table { padding: 8px 14px; border: none; border-radius: var(--radius-sm, 6px); font-size: 13px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; min-height: 36px; }
.btn-table.btn-kiad { background: var(--color-success, #10b981); color: white; }
.btn-table.btn-visszahoz { background: var(--color-active, #3b82f6); color: white; }
.btn-table.btn-info { background: var(--color-background, #f3f4f6); color: var(--color-text, #374151); }
.btn-table:hover { transform: translateY(-1px); }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 1000; display: flex; align-items: flex-end; }
@media (min-width: 768px) { .modal-overlay { align-items: center; justify-content: center; padding: var(--spacing-lg, 24px); background: rgba(61,47,31,0.7); backdrop-filter: blur(4px); } }
.modal-container { background: var(--color-surface, white); width: 100%; max-height: 92vh; border-radius: 16px 16px 0 0; display: flex; flex-direction: column; animation: slideUp 0.3s ease-out; overflow: hidden; }
@media (min-width: 768px) { .modal-container { max-width: 600px; border-radius: var(--radius-lg, 16px); animation: scaleIn 0.2s ease-out; } }
@keyframes slideUp { from { transform: translateY(100%); } to { transform: translateY(0); } }
@keyframes scaleIn { from { transform: scale(0.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
.modal-header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; border-bottom: 1px solid var(--color-border, #e5e7eb); flex-shrink: 0; }
.modal-title { font-size: 18px; font-weight: 600; margin: 0; }
.btn-back { background: none; border: none; color: var(--color-primary, #6b8e23); font-weight: 600; cursor: pointer; padding: 8px; margin: -8px; font-size: 16px; min-height: 44px; display: flex; align-items: center; }
.btn-close { width: 40px; height: 40px; background: none; border: none; font-size: 24px; color: var(--color-text-muted, #6b7280); cursor: pointer; border-radius: var(--radius-md, 8px); display: flex; align-items: center; justify-content: center; }
.modal-content { flex: 1; overflow-y: auto; padding: 20px; -webkit-overflow-scrolling: touch; }
.status-card { display: flex; justify-content: space-between; align-items: center; padding: 14px 16px; border-radius: 10px; margin-bottom: 16px; }
.status-label { font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 500; }
.status-value { font-size: 16px; font-weight: 700; }
.details-grid { display: flex; flex-direction: column; gap: 12px; }
.detail-group { padding: 14px 16px; background: var(--color-background, #f9fafb); border-radius: 10px; }
.detail-group.highlight { background: #d1fae5; }
.group-title { font-size: 12px; font-weight: 600; color: var(--color-text-muted, #6b7280); margin: 0 0 6px 0; text-transform: uppercase; letter-spacing: 0.3px; }
.group-value { font-size: 16px; font-weight: 500; color: var(--color-text, #1f2937); margin: 0; line-height: 1.4; }
.group-value.large { font-size: 24px; font-weight: 700; color: var(--color-primary, #6b8e23); }
.group-sub { font-size: 14px; color: var(--color-text-muted, #6b7280); margin: 4px 0 0 0; }
.modal-footer { display: flex; gap: 12px; padding: 16px 20px; border-top: 1px solid var(--color-border, #e5e7eb); flex-shrink: 0; }
.btn-primary, .btn-secondary, .btn-danger { flex: 1; padding: 14px; border-radius: 10px; font-size: 16px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; border: none; min-height: 48px; }
.btn-primary { background: var(--color-primary, #6b8e23); color: white; }
.btn-primary:hover { background: var(--color-primary-dark, #5a7a1e); }
.btn-danger { background: var(--color-danger, #ef4444); color: white; }
.btn-danger:hover { background: #dc2626; }
.btn-secondary { background: var(--color-surface, white); border: 2px solid var(--color-border, #e5e7eb); color: var(--color-text, #1f2937); }

.toast { position: fixed; bottom: calc(var(--bottom-nav-height, 0px) + 16px); left: 16px; right: 16px; padding: 16px; border-radius: 12px; display: flex; align-items: center; gap: 10px; z-index: 2000; box-shadow: 0 8px 24px rgba(0,0,0,0.2); font-weight: 600; font-size: 15px; }
@media (min-width: 768px) { .toast { left: auto; right: 24px; bottom: 24px; max-width: 400px; } }
.toast.success { background: var(--color-success, #10b981); color: white; }
.toast.error { background: var(--color-danger, #ef4444); color: white; }
.modal-enter-active, .modal-leave-active { transition: opacity 0.3s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from { transform: translateY(100px); opacity: 0; }
.toast-leave-to { transform: translateX(100%); opacity: 0; }

.mobile-only { display: block; }
.desktop-only { display: none; }
@media (min-width: 768px) { .mobile-only { display: none; } .desktop-only { display: block; } }

@media (max-width: 768px) {
  .page-header { flex-direction: row; align-items: center; }
  .modal-container { max-height: 100vh; height: 100vh; border-radius: 0; }
  .modal-header { position: sticky; top: 0; background: var(--color-surface, white); z-index: 10; }
  .modal-content { flex: 1; max-height: none; }
  .modal-footer { position: sticky; bottom: 0; background: var(--color-surface, white); flex-direction: column; }
}

@media (max-width: 480px) {
  .foglalasok-admin { padding: 12px; }
  .page-title { font-size: 18px; }
  .cards-view { gap: 12px; }
  .foglalas-card { padding: 16px; }
  .card-title { font-size: 17px; }
  .card-info-row { font-size: 14px; }
  .revenue-value { font-size: 18px; }
  .card-actions { flex-wrap: wrap; }
  .btn-card { min-height: 48px; font-size: 15px; }
  .filter-chip { padding: 8px 12px; font-size: 13px; min-height: 40px; }
  .status-badge { font-size: 11px; padding: 4px 10px; }
}
</style>