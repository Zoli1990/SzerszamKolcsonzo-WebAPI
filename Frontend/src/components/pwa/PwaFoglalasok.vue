<!-- ============================================================================ -->
<!-- src/components/pwa/PwaFoglalasok.vue — Foglalások kezelése mobilra        -->
<!-- ============================================================================ -->

<template>
  <div class="pwa-foglalasok">
    <!-- Header -->
    <div class="page-header">
      <h2>📋 Foglalások</h2>
      <button class="btn-refresh" @click="fetchFoglalasok" :disabled="loading">
        <span :class="{ spinning: loading }">🔄</span>
      </button>
    </div>

    <!-- Szűrő -->
    <div class="filter-bar">
      <button
        v-for="f in filters"
        :key="f.value"
        class="filter-chip"
        :class="{ active: activeFilter === f.value }"
        @click="activeFilter = f.value"
      >
        {{ f.label }}
        <span v-if="f.count !== undefined" class="chip-count">{{ f.count }}</span>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading && !foglalasok.length" class="loading-state">
      <div class="spinner"></div>
      <p>Betöltés...</p>
    </div>

    <!-- Üres -->
    <div v-else-if="filteredFoglalasok.length === 0" class="empty-state">
      <span class="empty-icon">📭</span>
      <p>Nincs {{ activeFilter === 'all' ? '' : activeFilter === 'aktiv' ? 'aktív' : 'lezárt' }} foglalás</p>
    </div>

    <!-- Foglalás kártyák -->
    <div v-else class="foglalas-list">
      <div
        v-for="foglalas in filteredFoglalasok"
        :key="foglalas.foglalasID"
        class="foglalas-card"
        :class="'card-' + foglalas.status.toLowerCase()"
      >
        <!-- Fejléc: eszköz + státusz -->
        <div class="card-top">
          <div>
            <strong class="card-eszkoz">{{ foglalas.eszkozNev }}</strong>
            <span class="card-id">#{{ foglalas.foglalasID }}</span>
          </div>
          <span class="status-badge" :class="'badge-' + foglalas.status.toLowerCase()">
            {{ getStatusLabel(foglalas.status) }}
          </span>
        </div>

        <!-- Ügyfél + idő infók -->
        <div class="card-body">
          <div class="info-row">
            <span class="label">👤</span>
            <span class="value">{{ foglalas.nev }}</span>
          </div>
          <div class="info-row">
            <span class="label">📅</span>
            <span class="value">{{ formatDate(foglalas.foglalasKezdete) }}</span>
          </div>
          <div v-if="foglalas.telefonszam" class="info-row">
            <span class="label">📞</span>
            <a :href="'tel:' + foglalas.telefonszam" class="value link">{{ foglalas.telefonszam }}</a>
          </div>

          <!-- Kiadva: eltelt idő + díj -->
          <template v-if="foglalas.status === 'Kiadva' && foglalas.kiadasIdopontja">
            <div class="info-row highlight">
              <span class="label">⏱️</span>
              <span class="value">{{ elteltIdo(foglalas.kiadasIdopontja) }}</span>
            </div>
          </template>

          <!-- Lezárt: elszámolás -->
          <template v-if="foglalas.status === 'Lezart' && foglalas.fizetendoOsszeg">
            <div class="info-row price">
              <span class="label">💰</span>
              <span class="value">{{ formatAr(foglalas.fizetendoOsszeg) }} Ft</span>
            </div>
          </template>
        </div>

        <!-- Akció gombok -->
        <div class="card-actions" v-if="foglalas.status === 'Foglalva' || foglalas.status === 'Kiadva'">
          <!-- Foglalva → Kiadás / Törlés -->
          <template v-if="foglalas.status === 'Foglalva'">
            <button class="btn-action btn-kiad" @click="kiadEszkoz(foglalas)" :disabled="loading">
              ✅ Kiadás
            </button>
            <button class="btn-action btn-torol" @click="torolFoglalas(foglalas)" :disabled="loading">
              ❌
            </button>
          </template>

          <!-- Kiadva → Visszahozás -->
          <template v-if="foglalas.status === 'Kiadva'">
            <button class="btn-action btn-vissza" @click="visszahozEszkoz(foglalas)" :disabled="loading">
              ↩️ Visszahozva
            </button>
          </template>
        </div>
      </div>
    </div>

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="toast.show" class="toast" :class="toast.type">
        {{ toast.message }}
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import api from '@/services/api'

const foglalasok = ref([])
const loading = ref(false)
const activeFilter = ref('aktiv')
const toast = ref({ show: false, message: '', type: 'success' })

let autoRefresh = null

// ═══════════════════════════════════════════════════════════════════════════
// COMPUTED
// ═══════════════════════════════════════════════════════════════════════════
const filters = computed(() => [
  { value: 'aktiv', label: 'Aktív', count: foglalasok.value.filter(f => f.status === 'Foglalva' || f.status === 'Kiadva').length },
  { value: 'lezart', label: 'Lezárt', count: foglalasok.value.filter(f => f.status === 'Lezart').length },
  { value: 'all', label: 'Összes' },
])

const filteredFoglalasok = computed(() => {
  let list = foglalasok.value
  if (activeFilter.value === 'aktiv') {
    list = list.filter(f => f.status === 'Foglalva' || f.status === 'Kiadva')
  } else if (activeFilter.value === 'lezart') {
    list = list.filter(f => f.status === 'Lezart')
  }
  // Foglalva először, aztán Kiadva, aztán dátum szerint
  const order = { Foglalva: 0, Kiadva: 1, Lezart: 2, Torolt: 3 }
  return list.sort((a, b) => (order[a.status] ?? 9) - (order[b.status] ?? 9) || new Date(b.foglalasKezdete) - new Date(a.foglalasKezdete))
})

// ═══════════════════════════════════════════════════════════════════════════
// API
// ═══════════════════════════════════════════════════════════════════════════
async function fetchFoglalasok(silent = false) {
  if (!silent) loading.value = true
  try {
    const res = await api.get('/foglalasok')
    foglalasok.value = Array.isArray(res.data) ? res.data : res.data?.$values || []
  } catch (err) {
    console.error('Foglalások betöltése sikertelen:', err)
    if (!silent) showToast('Betöltés sikertelen!', 'error')
  } finally {
    loading.value = false
  }
}

async function kiadEszkoz(foglalas) {
  if (!confirm(`Kiadod: ${foglalas.eszkozNev}?\nÜgyfél: ${foglalas.nev}`)) return
  try {
    await api.put(`/foglalasok/${foglalas.foglalasID}/kiadas`)
    showToast('✅ Eszköz kiadva!')
    await fetchFoglalasok(true)
  } catch (err) {
    showToast(err.response?.data?.message || 'Kiadás sikertelen!', 'error')
  }
}

async function visszahozEszkoz(foglalas) {
  if (!confirm(`Visszahozták: ${foglalas.eszkozNev}?`)) return
  try {
    const res = await api.put(`/foglalasok/${foglalas.foglalasID}/lezaras`)
    const d = res.data
    if (d.elszamolhatoIdo) {
      showToast(`Visszahozva! ${formatIdo(d.elszamolhatoIdo)} — ${formatAr(d.fizetendoOsszeg)} Ft`)
    } else {
      showToast('✅ Visszahozva!')
    }
    await fetchFoglalasok(true)
  } catch (err) {
    showToast(err.response?.data?.message || 'Visszahozás sikertelen!', 'error')
  }
}

async function torolFoglalas(foglalas) {
  if (!confirm(`Törlöd: ${foglalas.eszkozNev} — ${foglalas.nev}?`)) return
  try {
    await api.put(`/foglalasok/${foglalas.foglalasID}/torles`)
    showToast('✅ Foglalás törölve!')
    await fetchFoglalasok(true)
  } catch (err) {
    showToast(err.response?.data?.message || 'Törlés sikertelen!', 'error')
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// HELPERS
// ═══════════════════════════════════════════════════════════════════════════
function showToast(message, type = 'success') {
  toast.value = { show: true, message, type }
  setTimeout(() => { toast.value.show = false }, 3000)
}

function formatDate(d) {
  if (!d) return '-'
  return new Date(d).toLocaleString('hu-HU', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}

function formatAr(ar) {
  if (!ar) return '0'
  return new Intl.NumberFormat('hu-HU').format(Math.round(ar))
}

function formatIdo(percek) {
  if (!percek) return '-'
  const h = Math.floor(percek / 60)
  const m = percek % 60
  return h === 0 ? `${m} perc` : `${h}ó ${m}p`
}

function elteltIdo(kiadasIdopontja) {
  if (!kiadasIdopontja) return '-'
  const diff = Date.now() - new Date(kiadasIdopontja)
  const h = Math.floor(diff / 3600000)
  const m = Math.floor((diff % 3600000) / 60000)
  return `${h}ó ${m}p`
}

function getStatusLabel(s) {
  return { Foglalva: '📌 Foglalva', Kiadva: '🔧 Kiadva', Lezart: '✅ Lezárt', Torolt: '❌ Törölt' }[s] || s
}

// ═══════════════════════════════════════════════════════════════════════════
// LIFECYCLE
// ═══════════════════════════════════════════════════════════════════════════
onMounted(() => {
  fetchFoglalasok()
  autoRefresh = setInterval(() => fetchFoglalasok(true), 15000)
})

onUnmounted(() => {
  clearInterval(autoRefresh)
})
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.page-header h2 { margin: 0; font-size: 20px; color: #3d2f1f; }
.btn-refresh { width: 40px; height: 40px; border: none; border-radius: 10px; background: white; font-size: 18px; cursor: pointer; box-shadow: 0 1px 3px rgba(0,0,0,0.08); }
.spinning { display: inline-block; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Filter */
.filter-bar { display: flex; gap: 8px; margin-bottom: 16px; overflow-x: auto; }
.filter-chip { padding: 8px 14px; border: 2px solid #e8dcc8; border-radius: 20px; background: white; font-size: 13px; font-weight: 600; cursor: pointer; white-space: nowrap; color: #6b5d4f; transition: all 0.2s; }
.filter-chip.active { background: #6b8e23; color: white; border-color: #6b8e23; }
.chip-count { background: rgba(0,0,0,0.1); padding: 1px 6px; border-radius: 10px; font-size: 11px; margin-left: 4px; }
.filter-chip.active .chip-count { background: rgba(255,255,255,0.3); }

/* States */
.loading-state, .empty-state { text-align: center; padding: 48px 16px; color: #6b5d4f; }
.empty-icon { font-size: 48px; display: block; margin-bottom: 12px; }
.spinner { width: 32px; height: 32px; border: 3px solid #e8dcc8; border-top-color: #6b8e23; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 12px; }

/* Cards */
.foglalas-list { display: flex; flex-direction: column; gap: 12px; }
.foglalas-card { background: white; border-radius: 14px; padding: 16px; border-left: 4px solid #e8dcc8; box-shadow: 0 1px 4px rgba(0,0,0,0.06); }
.card-foglalva { border-left-color: #f59e0b; }
.card-kiadva { border-left-color: #3b82f6; }
.card-lezart { border-left-color: #10b981; opacity: 0.8; }
.card-torolt { border-left-color: #ef4444; opacity: 0.5; }

.card-top { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 10px; }
.card-eszkoz { display: block; font-size: 16px; color: #3d2f1f; }
.card-id { font-size: 12px; color: #9ca3af; }

.status-badge { padding: 4px 10px; border-radius: 8px; font-size: 11px; font-weight: 600; white-space: nowrap; }
.badge-foglalva { background: #fef3c7; color: #92400e; }
.badge-kiadva { background: #dbeafe; color: #1e40af; }
.badge-lezart { background: #d1fae5; color: #065f46; }
.badge-torolt { background: #fee2e2; color: #991b1b; }

.card-body { display: flex; flex-direction: column; gap: 4px; margin-bottom: 12px; }
.info-row { display: flex; gap: 8px; font-size: 14px; align-items: center; }
.info-row .label { font-size: 16px; flex-shrink: 0; width: 24px; text-align: center; }
.info-row .value { color: #3d2f1f; }
.info-row .link { color: #3b82f6; text-decoration: none; }
.info-row.highlight .value { color: #6b8e23; font-weight: 700; }
.info-row.price .value { color: #6b8e23; font-weight: 700; font-size: 16px; }

/* Actions */
.card-actions { display: flex; gap: 8px; }
.btn-action { flex: 1; padding: 12px; border: none; border-radius: 10px; font-size: 14px; font-weight: 600; cursor: pointer; min-height: 44px; transition: all 0.15s; }
.btn-action:active { transform: scale(0.97); }
.btn-kiad { background: #10b981; color: white; }
.btn-vissza { background: #3b82f6; color: white; }
.btn-torol { background: #fee2e2; color: #dc2626; flex: 0; min-width: 44px; }

/* Toast */
.toast { position: fixed; bottom: 80px; left: 16px; right: 16px; padding: 14px; border-radius: 12px; font-weight: 600; text-align: center; z-index: 2000; box-shadow: 0 8px 24px rgba(0,0,0,0.2); }
.toast.success { background: #10b981; color: white; }
.toast.error { background: #ef4444; color: white; }
.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from { transform: translateY(100px); opacity: 0; }
.toast-leave-to { transform: translateX(100%); opacity: 0; }
</style>
