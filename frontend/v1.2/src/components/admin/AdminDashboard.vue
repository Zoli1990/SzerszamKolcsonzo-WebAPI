// ============================================================================ // 2.
src/components/admin/AdminDashboard.vue - Statisztikák //
============================================================================

<template>
  <div class="dashboard">
    <h2>Áttekintés</h2>

    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon">📁</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.kategoriak }}</div>
          <div class="stat-label">Kategória</div>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">🔨</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.eszkozok }}</div>
          <div class="stat-label">Eszköz</div>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">✅</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.elerheto }}</div>
          <div class="stat-label">Elérhető</div>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">🚫</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.kiadva }}</div>
          <div class="stat-label">Kiadva</div>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon">📋</div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.foglalasok }}</div>
          <div class="stat-label">Összes foglalás</div>
        </div>
      </div>

      <div class="stat-card highlight">
        <div class="stat-icon">💰</div>
        <div class="stat-info">
          <div class="stat-value">{{ formatAr(stats.osszesBevetel) }} Ft</div>
          <div class="stat-label">Összes bevétel</div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { kategoriaService } from '@/services/kategoriaService'
import { eszkozService } from '@/services/eszkozService'
import { foglalasService } from '@/services/foglalasService'

const loading = ref(false)
const stats = ref({
  kategoriak: 0,
  eszkozok: 0,
  elerheto: 0,
  kiadva: 0,
  foglalasok: 0,
  osszesBevetel: 0,
})

onMounted(async () => {
  loading.value = true
  try {
    const [kategoriakRes, eszkozokRes, foglalasokRes] = await Promise.all([
      kategoriaService.getAll(),
      eszkozService.getAll(),
      foglalasService.getAll(),
    ])

    stats.value.kategoriak = kategoriakRes.data.length
    stats.value.eszkozok = eszkozokRes.data.length
    stats.value.elerheto = eszkozokRes.data.filter((e) => e.status === 'Elerheto').length
    stats.value.kiadva = eszkozokRes.data.filter((e) => e.status === 'Kiadva').length
    stats.value.foglalasok = foglalasokRes.data.length
    stats.value.osszesBevetel = foglalasokRes.data.reduce((sum, f) => sum + f.bevetel, 0)
  } catch (error) {
    console.error('Statisztikák betöltése sikertelen:', error)
  } finally {
    loading.value = false
  }
})

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
.dashboard h2 {
  margin: 0 0 24px 0;
  font-size: 24px;
  color: #3d2f1f;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.stat-card {
  background: linear-gradient(135deg, #8b7355 0%, #6b5d4f 100%);
  color: white;
  padding: 24px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 4px 12px rgba(61, 47, 31, 0.2);
}

.stat-card.highlight {
  background: linear-gradient(135deg, #6b8e23 0%, #556b1a 100%);
  grid-column: span 2;
}

.stat-icon {
  font-size: 48px;
}

.stat-info {
  flex: 1;
}

.stat-value {
  font-size: 32px;
  font-weight: 700;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  opacity: 0.9;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #6b5d4f;
}
</style>