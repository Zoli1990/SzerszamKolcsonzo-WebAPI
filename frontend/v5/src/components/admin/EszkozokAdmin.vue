<template>
  <div class="eszkozok-admin">
    <div class="header">
      <h2>🔧 Eszközök kezelése</h2>
      <div class="header-actions">
        <button class="btn-refresh" @click="fetchEszkozok" :disabled="loading">
          🔄 Frissítés
        </button>
        <button class="btn-primary" @click="openCreateModal">+ Új eszköz</button>
      </div>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

    <div v-else-if="eszkozok.length === 0" class="empty-state">
      <p>📭 Nincs még eszköz az adatbázisban.</p>
    </div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Név</th>
          <th>Kategória</th>
          <th>Vételár</th>
          <th>Kiadási ár</th>
          <th>Státusz</th>
          <th>Műveletek</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="eszkoz in eszkozok" :key="eszkoz.eszkozID">
          <td><strong>#{{ eszkoz.eszkozID }}</strong></td>
          <td>
            <div class="eszkoz-name">{{ eszkoz.nev }}</div>
            <div v-if="eszkoz.megjegyzes" class="small text-danger">
              💬 {{ eszkoz.megjegyzes }}
            </div>
          </td>
          <td>{{ eszkoz.kategoriaNev }}</td>
          <td>{{ formatAr(eszkoz.vetelar) }} Ft</td>
          <td><strong>{{ formatAr(eszkoz.kiadasiAr) }} Ft/óra</strong></td>
          <td>
            <span :class="['badge', getStatusBadge(eszkoz.status)]">
              {{ getStatusText(eszkoz.status) }}
            </span>
          </td>
          <td class="actions">
            <!-- 🔴 KIVONVA gomb - ha Elérhető vagy Foglalva -->
            <button
              v-if="eszkoz.status === 'Elerheto' || eszkoz.status === 'Foglalva'"
              class="btn-kivon"
              @click="openKivonModal(eszkoz)"
              title="Eszköz kivonása (szerviz, elveszett, stb.)"
            >
              🔴 KIVONVA
            </button>

            <!-- 🟢 ELÉRHETŐ gomb - ha Kivonva -->
            <button
              v-if="eszkoz.status === 'Kivonva'"
              class="btn-elerheto"
              @click="elerhetovaTetel(eszkoz)"
              title="Eszköz visszaállítása elérhetőre"
            >
              🟢 ELÉRHETŐ
            </button>

            <!-- ✏️ Szerkesztés -->
            <button 
              class="btn-edit" 
              @click="openEditModal(eszkoz)"
              title="Szerkesztés"
            >
              ✏️
            </button>

            <!-- 🗑️ Törlés -->
            <button 
              class="btn-delete" 
              @click="deleteEszkoz(eszkoz)"
              title="Törlés"
            >
              🗑️
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- KIVONÁS MODAL -->
    <Teleport to="body">
      <div v-if="kivonModalOpen" class="modal-overlay" @click="closeKivonModal">
        <div class="modal-box" @click.stop>
          <h3>🔴 Eszköz kivonása</h3>
          
          <div class="alert alert-warning">
            <strong>⚠️ Figyelem!</strong><br>
            Az eszköz ideiglenesen vagy véglegesen nem lesz elérhető.
          </div>

          <div v-if="selectedEszkoz" class="eszkoz-info-box">
            <strong>{{ selectedEszkoz.nev }}</strong>
            <div class="small">ID: #{{ selectedEszkoz.eszkozID }}</div>
          </div>

          <form @submit.prevent="handleKivonas">
            <div class="form-group">
              <label>Megjegyzés *</label>
              <textarea 
                v-model="kivonMegjegyzes" 
                rows="4" 
                placeholder="Pl: Szervizben van, Nem hozták vissza, Leégett, stb."
                required
              ></textarea>
              <div class="small text-muted">Kötelező mező - add meg az okot!</div>
            </div>

            <div class="modal-actions">
              <button type="button" class="btn-secondary" @click="closeKivonModal">
                Mégse
              </button>
              <button type="submit" class="btn-danger" :disabled="!kivonMegjegyzes.trim()">
                🔴 Kivon
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>

    <!-- SZERKESZTÉS MODAL -->
    <Teleport to="body">
      <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-box large" @click.stop>
          <h3>{{ isEditing ? '✏️ Eszköz szerkesztése' : '➕ Új eszköz' }}</h3>

          <form @submit.prevent="handleSubmit">
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
              <textarea v-model="form.leiras" rows="3"></textarea>
            </div>

            <div class="form-group">
              <label>Kép URL</label>
              <input v-model="form.kepUrl" type="url" placeholder="https://..." />
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Vételár (Ft) *</label>
                <input v-model.number="form.vetelar" type="number" required min="0" />
              </div>

              <div class="form-group">
                <label>Kiadási ár (Ft/óra) *</label>
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

            <div v-if="error" class="error-msg">{{ error }}</div>

            <div class="modal-actions">
              <button type="button" class="btn-secondary" @click="closeModal">Mégse</button>
              <button type="submit" class="btn-primary" :disabled="submitting">
                {{ submitting ? 'Mentés...' : 'Mentés' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const API_BASE = 'http://localhost:5265/api'

const eszkozok = ref([])
const kategoriak = ref([])
const loading = ref(false)
const modalOpen = ref(false)
const kivonModalOpen = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const error = ref(null)
const selectedEszkoz = ref(null)
const kivonMegjegyzes = ref('')

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

onMounted(async () => {
  await fetchEszkozok()
  await fetchKategoriak()

  // Automatikus frissítés 10 másodpercenként
  autoRefreshInterval = setInterval(() => {
    fetchEszkozok(true)
  }, 10000)
})

onUnmounted(() => {
  if (autoRefreshInterval) {
    clearInterval(autoRefreshInterval)
  }
})

async function fetchEszkozok(silent = false) {
  if (!silent) loading.value = true
  
  try {
    const response = await axios.get(`${API_BASE}/eszkozok/admin`)
    eszkozok.value = response.data
  } catch (err) {
    console.error('Eszközök betöltése sikertelen:', err)
  } finally {
    if (!silent) loading.value = false
  }
}

async function fetchKategoriak() {
  try {
    const response = await axios.get(`${API_BASE}/kategoriak`)
    kategoriak.value = response.data
  } catch (err) {
    console.error('Kategóriák betöltése sikertelen:', err)
  }
}

function openKivonModal(eszkoz) {
  selectedEszkoz.value = eszkoz
  kivonMegjegyzes.value = ''
  kivonModalOpen.value = true
}

function closeKivonModal() {
  kivonModalOpen.value = false
  selectedEszkoz.value = null
  kivonMegjegyzes.value = ''
}

async function handleKivonas() {
  if (!kivonMegjegyzes.value.trim()) {
    alert('⚠️ A megjegyzés megadása kötelező!')
    return
  }

  try {
    const response = await axios.post(
      `${API_BASE}/eszkozok/${selectedEszkoz.value.eszkozID}/kivon`,
      { megjegyzes: kivonMegjegyzes.value }
    )

    alert(`✅ ${response.data.message}`)
    await fetchEszkozok()
    closeKivonModal()
  } catch (err) {
    alert(`❌ Hiba: ${err.response?.data?.message || 'Nem sikerült kivonni az eszközt'}`)
    console.error('Kivonás hiba:', err)
  }
}

async function elerhetovaTetel(eszkoz) {
  if (!confirm(`Eszköz visszaállítása elérhetőre?\n\n${eszkoz.nev}\nMegjegyzés: ${eszkoz.megjegyzes || 'nincs'}`)) {
    return
  }

  try {
    const response = await axios.post(`${API_BASE}/eszkozok/${eszkoz.eszkozID}/elerheto`)
    
    alert(`✅ ${response.data.message}`)
    await fetchEszkozok()
  } catch (err) {
    alert(`❌ Hiba: ${err.response?.data?.message || 'Nem sikerült visszaállítani'}`)
    console.error('Visszaállítás hiba:', err)
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
  modalOpen.value = true
}

async function openEditModal(eszkoz) {
  isEditing.value = true

  try {
    const response = await axios.get(`${API_BASE}/eszkozok/${eszkoz.eszkozID}`)
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
    modalOpen.value = true
  } catch (err) {
    console.error('Eszköz betöltése sikertelen:', err)
  }
}

function closeModal() {
  modalOpen.value = false
  error.value = null
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
      await axios.put(`${API_BASE}/eszkozok/${form.value.eszkozID}`, payload)
    } else {
      await axios.post(`${API_BASE}/eszkozok`, payload)
    }

    await fetchEszkozok()
    closeModal()
  } catch (err) {
    error.value = err.response?.data?.message || 'Hiba történt a mentés során.'
  } finally {
    submitting.value = false
  }
}

async function deleteEszkoz(eszkoz) {
  if (!confirm(`Biztosan törölni szeretnéd: ${eszkoz.nev}?`)) return

  try {
    await axios.delete(`${API_BASE}/eszkozok/${eszkoz.eszkozID}`)
    await fetchEszkozok()
  } catch (err) {
    alert(err.response?.data?.message || 'Hiba történt a törlés során.')
  }
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function getStatusText(status) {
  const map = {
    Elerheto: '🟢 Elérhető',
    Foglalva: '🟡 Foglalva',
    Kiadva: '🔵 Kiadva',
    Kivonva: '🔴 Kivonva'
  }
  return map[status] || status
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
</script>

<style scoped>
.eszkozok-admin {
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

.btn-refresh,
.btn-primary {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-refresh {
  background: #6b8e23;
  color: white;
}

.btn-primary {
  background: #6b8e23;
  color: white;
}

.btn-refresh:hover:not(:disabled),
.btn-primary:hover:not(:disabled) {
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

.eszkoz-name {
  font-weight: 600;
  color: #3d2f1f;
}

.small {
  font-size: 12px;
  margin-top: 4px;
}

.text-danger {
  color: #dc2626;
  font-weight: 500;
}

.text-muted {
  color: #9b8b7a;
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

.badge-success {
  background: #d1fae5;
  color: #065f46;
}

.badge-warning {
  background: #fef3c7;
  color: #92400e;
}

.badge-info {
  background: #dbeafe;
  color: #1e40af;
}

.badge-danger {
  background: #fee2e2;
  color: #991b1b;
}

.actions {
  display: flex;
  gap: 6px;
  align-items: center;
  flex-wrap: wrap;
}

.btn-kivon,
.btn-elerheto,
.btn-edit,
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

.btn-kivon {
  background: #ef4444;
  color: white;
}

.btn-kivon:hover {
  background: #dc2626;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(239, 68, 68, 0.3);
}

.btn-elerheto {
  background: #10b981;
  color: white;
}

.btn-elerheto:hover {
  background: #059669;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(16, 185, 129, 0.3);
}

.btn-edit {
  background: #7a9b57;
  color: white;
  min-width: 36px;
}

.btn-edit:hover {
  background: #6b8e23;
}

.btn-delete {
  background: #b85c5c;
  color: white;
  min-width: 36px;
}

.btn-delete:hover {
  background: #a04545;
}

.btn-secondary {
  background: white;
  border: 2px solid #d4c5b0;
  color: #3d2f1f;
}

.btn-secondary:hover {
  background: #f5f1e8;
}

.btn-danger {
  background: #ef4444;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-danger:hover:not(:disabled) {
  background: #dc2626;
}

.btn-danger:disabled {
  opacity: 0.5;
  cursor: not-allowed;
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
  max-width: 600px;
  width: 100%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.3s ease-out;
}

.modal-box.large {
  max-width: 800px;
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

.alert {
  padding: 14px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.alert-warning {
  background: #fef3c7;
  border: 2px solid #fbbf24;
  color: #92400e;
}

.eszkoz-info-box {
  padding: 16px;
  background: #f5f1e8;
  border-radius: 8px;
  margin-bottom: 20px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #3d2f1f;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 12px;
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  font-family: inherit;
  background: #fefdfb;
  transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #6b8e23;
}

.error-msg {
  padding: 12px;
  background: #fee2e2;
  border: 2px solid #ef4444;
  border-radius: 8px;
  color: #991b1b;
  margin-bottom: 16px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  border-top: 2px solid #e8dcc8;
  padding-top: 24px;
  margin-top: 24px;
}
</style>