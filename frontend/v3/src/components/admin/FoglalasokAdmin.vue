// ============================================================================ // 5.
src/components/admin/FoglalasokAdmin.vue - Foglalások listázása //
============================================================================

<template>
  <div class="foglalasok-admin">
    <div class="header">
      <h2>Foglalások kezelése</h2>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Eszköz</th>
          <th>Ügyfél</th>
          <th>Kezdet</th>
          <th>Vég</th>
          <th>Órák</th>
          <th>Bevétel</th>
          <th>Státusz</th>
          <th>Műveletek</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="foglalas in foglalasok" :key="foglalas.foglalasID">
          <td>{{ foglalas.foglalasID }}</td>
          <td>{{ foglalas.eszkozNev }}</td>
          <td>
            <div>
              <strong>{{ foglalas.nev }}</strong>
            </div>
            <div class="small">{{ foglalas.email }}</div>
            <div class="small">{{ foglalas.telefonszam }}</div>
          </td>
          <td>{{ formatDate(foglalas.foglalasKezdete) }}</td>
          <td>{{ formatDate(foglalas.foglalasVege) }}</td>
          <td>{{ foglalas.oraSzam }} óra</td>
          <td>
            <strong>{{ formatAr(foglalas.bevetel) }} Ft</strong>
          </td>
          <td>
            <span :class="['badge', getBadgeClass(foglalas.status)]">
              {{ getStatusText(foglalas.status) }}
            </span>
          </td>
          <td class="actions">
            <button
              v-if="foglalas.status === 'Aktiv'"
              class="btn-success"
              @click="lezarasFoglalas(foglalas)"
            >
              ✓ Lezárás
            </button>
            <button class="btn-info" @click="openDetailModal(foglalas)">👁️ Részletek</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Részletek Modal -->
    <Teleport to="body">
      <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-box" @click.stop>
          <h3>Foglalás részletei</h3>

          <div class="detail-grid">
            <div class="detail-item">
              <div class="detail-label">Foglalás ID:</div>
              <div class="detail-value">{{ selectedFoglalas.foglalasID }}</div>
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

            <div class="detail-item">
              <div class="detail-label">Foglalás vége:</div>
              <div class="detail-value">{{ formatDate(selectedFoglalas.foglalasVege) }}</div>
            </div>

            <div class="detail-item">
              <div class="detail-label">Órák száma:</div>
              <div class="detail-value">{{ selectedFoglalas.oraSzam }} óra</div>
            </div>

            <div class="detail-item">
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
import { ref, onMounted } from 'vue'
import { foglalasService } from '@/services/foglalasService'

const foglalasok = ref([])
const loading = ref(false)
const modalOpen = ref(false)
const selectedFoglalas = ref({})

onMounted(() => fetchFoglalasok())

async function fetchFoglalasok() {
  loading.value = true
  try {
    const response = await foglalasService.getAll()
    foglalasok.value = response.data
  } catch (err) {
    console.error('Foglalások betöltése sikertelen:', err)
  } finally {
    loading.value = false
  }
}

function openDetailModal(foglalas) {
  selectedFoglalas.value = foglalas
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
}

async function lezarasFoglalas(foglalas) {
  if (!confirm(`Biztosan lezárod a foglalást? (${foglalas.eszkozNev})`)) return

  try {
    await foglalasService.lezaras(foglalas.foglalasID)
    await fetchFoglalasok()
  } catch (err) {
    alert(err.response?.data?.message || 'Hiba történt a lezárás során.')
  }
}

function formatDate(dateString) {
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
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function getStatusText(status) {
  const map = {
    Aktiv: 'Aktív',
    Lezart: 'Lezárt',
    Torolt: 'Törölt',
  }
  return map[status] || status
}

function getBadgeClass(status) {
  const map = {
    Aktiv: 'badge-warning',
    Lezart: 'badge-success',
    Torolt: 'badge-danger',
  }
  return map[status] || ''
}
</script>

<style scoped>
.foglalasok-admin .header {
  margin-bottom: 24px;
}

.foglalasok-admin h2 {
  margin: 0;
  font-size: 24px;
  color: #3d2f1f;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.data-table th,
.data-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e8dcc8;
}

.data-table th {
  background: #f5f1e8;
  font-weight: 600;
  color: #3d2f1f;
}

.data-table tr:hover {
  background: #faf8f3;
}

.small {
  font-size: 12px;
  color: #6b5d4f;
}

.badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 11px;
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

.badge-danger {
  background: #f5d7d7;
  color: #7a2828;
}

.actions {
  display: flex;
  gap: 8px;
}

.btn-success,
.btn-info,
.btn-secondary {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 12px;
  transition: all 0.2s;
}

.btn-success {
  background: #7a9b57;
  color: white;
}

.btn-info {
  background: #6b8e23;
  color: white;
}

.btn-secondary {
  background: white;
  border: 1px solid #d4c5b0;
  color: #3d2f1f;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(61, 47, 31, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
  overflow-y: auto;
}

.modal-box {
  background: white;
  padding: 24px;
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
}

.modal-box h3 {
  margin: 0 0 20px 0;
  color: #3d2f1f;
}

.detail-grid {
  display: grid;
  gap: 16px;
  margin-bottom: 20px;
}

.detail-item {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 12px;
  padding: 12px;
  background: #f5f1e8;
  border-radius: 6px;
}

.detail-label {
  font-weight: 600;
  color: #6b5d4f;
  font-size: 14px;
}

.detail-value {
  color: #3d2f1f;
  font-size: 14px;
}

.detail-value.highlight {
  font-weight: 700;
  color: #6b8e23;
  font-size: 16px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  border-top: 1px solid #e8dcc8;
  padding-top: 20px;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #6b5d4f;
}
</style>