<!-- ============================================================================ -->
<!-- src/components/pwa/PwaEszkozok.vue — Eszközök lista + Új eszköz wizard    -->
<!-- ============================================================================ -->

<template>
  <div class="pwa-eszkozok">
    <!-- Header -->
    <div class="page-header">
      <h2>🔨 Eszközök</h2>
      <router-link to="/pwa/eszkozok/uj" class="btn-add">+ Új</router-link>
    </div>

    <!-- Szűrő: kategória + státusz -->
    <div class="filter-bar">
      <select v-model="filterKategoria" class="filter-select">
        <option value="">Összes kategória</option>
        <option v-for="k in kategoriak" :key="k.kategoriaID" :value="k.kategoriaID">
          {{ k.nev }}
        </option>
      </select>
      <select v-model="filterStatus" class="filter-select">
        <option value="">Összes státusz</option>
        <option value="Elerheto">Elérhető</option>
        <option value="Foglalva">Foglalva</option>
        <option value="Kiadva">Kiadva</option>
      </select>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Betöltés...</p>
    </div>

    <!-- Üres -->
    <div v-else-if="filteredEszkozok.length === 0" class="empty-state">
      <span class="empty-icon">🔨</span>
      <p>Nincs találat</p>
    </div>

    <!-- Eszköz kártyák -->
    <div v-else class="eszkoz-list">
      <div
        v-for="eszkoz in filteredEszkozok"
        :key="eszkoz.eszkozID"
        class="eszkoz-card"
      >
        <div class="card-left">
          <img
            v-if="eszkoz.kepUrl"
            :src="getImageUrl(eszkoz.kepUrl)"
            :alt="eszkoz.nev"
            class="eszkoz-img"
            @error="(e) => e.target.src = 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=100&q=60'"
          />
          <div v-else class="eszkoz-img-placeholder">🔧</div>
        </div>
        <div class="card-right">
          <strong class="eszkoz-nev">{{ eszkoz.nev }}</strong>
          <span class="eszkoz-kategoria">{{ eszkoz.kategoriaNev }}</span>
          <div class="eszkoz-meta">
            <span class="eszkoz-ar">{{ formatAr(eszkoz.kiadasiAr) }} Ft/óra</span>
            <span class="status-badge" :class="'badge-' + eszkoz.status.toLowerCase()">
              {{ getStatusLabel(eszkoz.status) }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'

const eszkozok = ref([])
const kategoriak = ref([])
const loading = ref(false)
const filterKategoria = ref('')
const filterStatus = ref('')

const filteredEszkozok = computed(() => {
  let list = eszkozok.value
  if (filterKategoria.value) {
    list = list.filter(e => e.kategoriaID === Number(filterKategoria.value))
  }
  if (filterStatus.value) {
    list = list.filter(e => e.status === filterStatus.value)
  }
  return list
})

async function fetchData() {
  loading.value = true
  try {
    const [eszRes, katRes] = await Promise.all([
      api.get('/eszkozok/admin'),
      api.get('/kategoriak'),
    ])
    eszkozok.value = eszRes.data
    kategoriak.value = katRes.data
  } catch (err) {
    console.error('Eszközök betöltése sikertelen:', err)
  } finally {
    loading.value = false
  }
}

function getImageUrl(kepUrl) {
  if (!kepUrl) return ''
  if (kepUrl.startsWith('http')) return kepUrl
  const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5265/api'
  return apiBase.replace(/\/api\/?$/, '') + kepUrl
}

function formatAr(ar) {
  if (!ar) return '0'
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function getStatusLabel(s) {
  return { Elerheto: '✅', Foglalva: '📌', Kiadva: '🔧' }[s] || s
}

onMounted(() => fetchData())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.page-header h2 { margin: 0; font-size: 20px; color: #3d2f1f; }
.btn-add { padding: 10px 18px; background: #6b8e23; color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 14px; text-decoration: none; }

/* Filter */
.filter-bar { display: flex; gap: 8px; margin-bottom: 16px; }
.filter-select { flex: 1; padding: 10px 12px; border: 2px solid #e8dcc8; border-radius: 10px; font-size: 14px; background: white; color: #3d2f1f; font-family: inherit; }
.filter-select:focus { outline: none; border-color: #6b8e23; }

/* States */
.loading-state, .empty-state { text-align: center; padding: 48px 16px; color: #6b5d4f; }
.empty-icon { font-size: 48px; display: block; margin-bottom: 12px; }
.spinner { width: 32px; height: 32px; border: 3px solid #e8dcc8; border-top-color: #6b8e23; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 12px; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Cards */
.eszkoz-list { display: flex; flex-direction: column; gap: 10px; }
.eszkoz-card { display: flex; gap: 14px; background: white; border-radius: 12px; padding: 12px; box-shadow: 0 1px 4px rgba(0,0,0,0.06); }

.eszkoz-img { width: 64px; height: 64px; border-radius: 10px; object-fit: cover; }
.eszkoz-img-placeholder { width: 64px; height: 64px; border-radius: 10px; background: #f5f1e8; display: flex; align-items: center; justify-content: center; font-size: 28px; }

.card-right { flex: 1; display: flex; flex-direction: column; gap: 2px; }
.eszkoz-nev { font-size: 15px; color: #3d2f1f; }
.eszkoz-kategoria { font-size: 13px; color: #6b5d4f; }
.eszkoz-meta { display: flex; justify-content: space-between; align-items: center; margin-top: 4px; }
.eszkoz-ar { font-size: 13px; font-weight: 700; color: #6b8e23; }

.status-badge { padding: 2px 8px; border-radius: 6px; font-size: 12px; font-weight: 600; }
.badge-elerheto { background: #d1fae5; color: #065f46; }
.badge-foglalva { background: #fef3c7; color: #92400e; }
.badge-kiadva { background: #dbeafe; color: #1e40af; }
</style>
