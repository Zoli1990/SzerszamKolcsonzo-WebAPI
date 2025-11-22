// ============================================================================ // 4.
src/components/admin/EszkozokAdmin.vue - Eszközök CRUD //
============================================================================

<template>
  <div class="eszkozok-admin">
    <div class="header">
      <h2>Eszközök kezelése</h2>
      <button class="btn-primary" @click="openCreateModal">+ Új eszköz</button>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

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
          <td>{{ eszkoz.eszkozID }}</td>
          <td>{{ eszkoz.nev }}</td>
          <td>{{ eszkoz.kategoriaNev }}</td>
          <td>{{ formatAr(eszkoz.vetelar) }} Ft</td>
          <td>{{ formatAr(eszkoz.kiadasiAr) }} Ft/óra</td>
          <td>
            <span
              :class="['badge', eszkoz.status === 'Elerheto' ? 'badge-success' : 'badge-danger']"
            >
              {{ eszkoz.status === 'Elerheto' ? 'Elérhető' : 'Kiadva' }}
            </span>
          </td>
          <td class="actions">
            <button class="btn-edit" @click="openEditModal(eszkoz)">✏️</button>
            <button class="btn-delete" @click="deleteEszkoz(eszkoz)">🗑️</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Modal -->
    <Teleport to="body">
      <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-box large" @click.stop>
          <h3>{{ isEditing ? 'Eszköz szerkesztése' : 'Új eszköz' }}</h3>

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
                  <option value="Kiadva">Kiadva</option>
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
import { ref, onMounted } from 'vue'
import { eszkozService } from '@/services/eszkozService'
import { kategoriaService } from '@/services/kategoriaService'

const eszkozok = ref([])
const kategoriak = ref([])
const loading = ref(false)
const modalOpen = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const error = ref(null)

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
})

async function fetchEszkozok() {
  loading.value = true
  try {
    const response = await eszkozService.getAllAdmin() // 🔥 részletes adatokat hoz
    eszkozok.value = response.data
  } catch (err) {
    console.error('Eszközök betöltése sikertelen:', err)
  } finally {
    loading.value = false
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
  modalOpen.value = true
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
      await eszkozService.update(form.value.eszkozID, payload)
    } else {
      await eszkozService.create(payload)
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
    await eszkozService.delete(eszkoz.eszkozID)
    await fetchEszkozok()
  } catch (err) {
    alert(err.response?.data?.message || 'Hiba történt a törlés során.')
  }
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
.eszkozok-admin .header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.eszkozok-admin h2 {
  margin: 0;
  font-size: 24px;
}

.badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.badge-success {
  background: #d1fae5;
  color: #065f46;
}

.badge-danger {
  background: #fee2e2;
  color: #991b1b;
}

.modal-box.large {
  max-width: 700px;
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
  color: #374151;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-family: inherit;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th,
.data-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.data-table th {
  background: #f9fafb;
  font-weight: 600;
  color: #374151;
}

.data-table tr:hover {
  background: #f9fafb;
}

.actions {
  display: flex;
  gap: 8px;
}

.btn-primary,
.btn-secondary,
.btn-edit,
.btn-delete {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-secondary {
  background: white;
  border: 1px solid #d1d5db;
  color: #374151;
}

.btn-edit {
  background: #10b981;
  color: white;
}

.btn-delete {
  background: #ef4444;
  color: white;
}

.link {
  color: #3b82f6;
  text-decoration: none;
}

.text-muted {
  color: #9ca3af;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-box {
  background: white;
  padding: 24px;
  border-radius: 12px;
  max-width: 500px;
  width: 100%;
}

.modal-box h3 {
  margin: 0 0 20px 0;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #374151;
}

.form-group input {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
}

.error-msg {
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  color: #dc2626;
  margin-bottom: 16px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #6b7280;
}
</style>
