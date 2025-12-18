<template>
  <div class="profil-container">
    <div class="profil-header">
      <h1>📋 Foglalásaim</h1>
      <p class="user-email">{{ authStore.userEmail }}</p>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

    <div v-else-if="foglalasok.length === 0" class="empty-state">
      <p>Még nem adtál le foglalást.</p>
      <RouterLink to="/" class="btn-primary">Böngészés az eszközök között</RouterLink>
    </div>

    <div v-else class="foglalasok-lista">
      <div v-for="foglalas in foglalasok" :key="foglalas.foglalasID" class="foglalas-card">
        <div class="foglalas-header">
          <h3>{{ foglalas.eszkozNev }}</h3>
          <span :class="['badge', getBadgeClass(foglalas.status)]">
            {{ getStatusText(foglalas.status) }}
          </span>
        </div>

        <div class="foglalas-details">
          <div class="detail-row">
            <span class="label">📅 Foglalás kezdete:</span>
            <span>{{ formatDate(foglalas.foglalasKezdete) }}</span>
          </div>

          <!-- Kiadás időpontja - ha már kiadták -->
          <div v-if="foglalas.kiadasIdopontja" class="detail-row">
            <span class="label">✅ Kiadva:</span>
            <span>{{ formatDate(foglalas.kiadasIdopontja) }}</span>
          </div>

          <!-- Visszahozás időpontja - ha már visszahozták -->
          <div v-if="foglalas.visszahozasIdopontja" class="detail-row">
            <span class="label">🔵 Visszahozva:</span>
            <span>{{ formatDate(foglalas.visszahozasIdopontja) }}</span>
          </div>

          <!-- Eltelt idő - ha van -->
          <div v-if="foglalas.elszamolhatoIdo" class="detail-row">
            <span class="label">⏱️ Használati idő:</span>
            <span>{{ formatIdo(foglalas.elszamolhatoIdo) }}</span>
          </div>

          <!-- Fizetendő összeg - ha van -->
          <div v-if="foglalas.fizetendoOsszeg" class="detail-row">
            <span class="label">💰 Fizetendő:</span>
            <span class="highlight">{{ formatAr(foglalas.fizetendoOsszeg) }} Ft</span>
          </div>

          <!-- Bevétel (legacy) - ha van és nincs fizetendőÖsszeg -->
          <div v-else-if="foglalas.bevetel" class="detail-row">
            <span class="label">💰 Költség:</span>
            <span class="highlight">{{ formatAr(foglalas.bevetel) }} Ft</span>
          </div>

          <div class="detail-row">
            <span class="label">📍 Cím:</span>
            <span>{{ foglalas.cim }}</span>
          </div>

          <div class="detail-row">
            <span class="label">📝 Létrehozva:</span>
            <span class="text-muted">{{ formatDate(foglalas.letrehozasDatum) }}</span>
          </div>
        </div>

        <!-- Státusz információ -->
        <div v-if="foglalas.status === 'Aktiv'" class="status-info warning">
          ⏰ A foglalás kezdetétől 15 percen belül meg kell jelenni, különben automatikusan törlődik.
        </div>
        <div v-if="foglalas.status === 'Kiadva'" class="status-info info">
          🔧 Az eszköz nálad van. Visszahozáskor számolódik a fizetendő összeg.
        </div>
        <div v-if="foglalas.status === 'Torolt'" class="status-info danger">
          ❌ A foglalás törölve lett (nem jelent meg időben vagy admin törölte).
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { foglalasService } from '@/services/foglalasService'

const authStore = useAuthStore()
const foglalasok = ref([])
const loading = ref(false)

onMounted(() => fetchFoglalasok())

async function fetchFoglalasok() {
  loading.value = true
  try {
    const response = await foglalasService.getAll()
    
    // Szűrés: csak a saját email címre szóló foglalások
    foglalasok.value = response.data.filter(
      f => f.email.toLowerCase() === authStore.userEmail?.toLowerCase()
    )
  } catch (err) {
    console.error('Foglalások betöltése sikertelen:', err)
  } finally {
    loading.value = false
  }
}

function formatDate(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('hu-HU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
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
  return `${orak} óra ${maradekPercek} perc`
}

function getStatusText(status) {
  const map = {
    'Aktiv': '🟡 Aktív',
    'Kiadva': '🔵 Kiadva',
    'Lezart': '🟢 Lezárt',
    'Torolt': '🔴 Törölt',
    'Lejart': '⏰ Lejárt'
  }
  return map[status] || status
}

function getBadgeClass(status) {
  const map = {
    'Aktiv': 'badge-warning',
    'Kiadva': 'badge-info',
    'Lezart': 'badge-success',
    'Torolt': 'badge-danger',
    'Lejart': 'badge-secondary'
  }
  return map[status] || ''
}
</script>

<style scoped>
.profil-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 20px;
}

.profil-header {
  margin-bottom: 32px;
}

.profil-header h1 {
  margin: 0 0 8px 0;
  font-size: 32px;
  color: #3d2f1f;
}

.user-email {
  color: #6b5d4f;
  font-size: 16px;
}

.loading {
  text-align: center;
  padding: 60px;
  color: #6b5d4f;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.empty-state p {
  font-size: 18px;
  color: #6b5d4f;
  margin-bottom: 24px;
}

.foglalasok-lista {
  display: grid;
  gap: 20px;
}

.foglalas-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(61, 47, 31, 0.1);
  border: 1px solid #e8dcc8;
}

.foglalas-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 2px solid #e8dcc8;
}

.foglalas-header h3 {
  margin: 0;
  font-size: 20px;
  color: #3d2f1f;
}

.badge {
  padding: 6px 14px;
  border-radius: 14px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.badge-success {
  background: #d4e7c5;
  color: #2d5016;
}

.badge-warning {
  background: #f5e6c8;
  color: #7a5a1a;
}

.badge-info {
  background: #dbeafe;
  color: #1e40af;
}

.badge-danger {
  background: #f5d7d7;
  color: #7a2828;
}

.badge-secondary {
  background: #e5e7eb;
  color: #374151;
}

.foglalas-details {
  display: grid;
  gap: 12px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0ebe0;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row .label {
  font-weight: 600;
  color: #6b5d4f;
}

.detail-row .highlight {
  font-weight: 700;
  color: #6b8e23;
  font-size: 18px;
}

.detail-row .text-muted {
  color: #9b8b7a;
  font-size: 14px;
}

.status-info {
  margin-top: 16px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
}

.status-info.warning {
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fbbf24;
}

.status-info.info {
  background: #dbeafe;
  color: #1e40af;
  border: 1px solid #60a5fa;
}

.status-info.danger {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #f87171;
}

.btn-primary {
  display: inline-block;
  padding: 12px 24px;
  background: #6b8e23;
  color: white;
  text-decoration: none;
  border-radius: 6px;
  font-weight: 600;
  transition: background 0.2s;
}

.btn-primary:hover {
  background: #556b1a;
}

/* Responsive */
@media (max-width: 640px) {
  .foglalas-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .detail-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }
}
</style>