// ============================================================================
// 6. src/views/ProfilView.vue - User profil oldal (foglalások)
// ============================================================================

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
            <span class="label">📅 Kezdet:</span>
            <span>{{ formatDate(foglalas.foglalasKezdete) }}</span>
          </div>

          <div class="detail-row">
            <span class="label">📅 Vég:</span>
            <span>{{ formatDate(foglalas.foglalasVege) }}</span>
          </div>

          <div class="detail-row">
            <span class="label">⏱️ Órák:</span>
            <span>{{ foglalas.oraSzam }} óra</span>
          </div>

          <div class="detail-row">
            <span class="label">💰 Költség:</span>
            <span class="highlight">{{ formatAr(foglalas.bevetel) }} Ft</span>
          </div>

          <div class="detail-row">
            <span class="label">📍 Cím:</span>
            <span>{{ foglalas.cim }}</span>
          </div>
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
      f => f.email === authStore.userEmail
    )
  } catch (err) {
    console.error('Foglalások betöltése sikertelen:', err)
  } finally {
    loading.value = false
  }
}

function formatDate(dateString) {
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
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function getStatusText(status) {
  const map = {
    'Aktiv': 'Aktív',
    'Lezart': 'Lezárt',
    'Torolt': 'Törölt'
  }
  return map[status] || status
}

function getBadgeClass(status) {
  const map = {
    'Aktiv': 'badge-warning',
    'Lezart': 'badge-success',
    'Torolt': 'badge-danger'
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
  color: #1f2937;
}

.user-email {
  color: #6b7280;
  font-size: 16px;
}

.loading {
  text-align: center;
  padding: 60px;
  color: #6b7280;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.empty-state p {
  font-size: 18px;
  color: #6b7280;
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
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.foglalas-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 2px solid #e5e7eb;
}

.foglalas-header h3 {
  margin: 0;
  font-size: 20px;
  color: #1f2937;
}

.badge {
  padding: 6px 14px;
  border-radius: 14px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.badge-success {
  background: #d1fae5;
  color: #065f46;
}

.badge-warning {
  background: #fef3c7;
  color: #92400e;
}

.badge-danger {
  background: #fee2e2;
  color: #991b1b;
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
}

.detail-row .label {
  font-weight: 600;
  color: #6b7280;
}

.detail-row .highlight {
  font-weight: 700;
  color: #059669;
  font-size: 18px;
}

.btn-primary {
  display: inline-block;
  padding: 12px 24px;
  background: #3b82f6;
  color: white;
  text-decoration: none;
  border-radius: 6px;
  font-weight: 600;
  transition: background 0.2s;
}

.btn-primary:hover {
  background: #2563eb;
}
</style>
