// ============================================================================
// 3. src/components/admin/KategoriakAdmin.vue - Kategóriák CRUD
// ============================================================================

<template>
  <div class="kategoriak-admin">
    <div class="header">
      <h2>Kategóriák kezelése</h2>
      <button class="btn-primary" @click="openCreateModal">+ Új kategória</button>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Név</th>
          <th>Kép URL</th>
          <th>Műveletek</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="kategoria in kategoriak" :key="kategoria.kategoriaID">
          <td>{{ kategoria.kategoriaID }}</td>
          <td>{{ kategoria.nev }}</td>
          <td>
            <a v-if="kategoria.kepUrl" :href="kategoria.kepUrl" target="_blank" class="link">
              Kép megtekintése
            </a>
            <span v-else class="text-muted">Nincs kép</span>
          </td>
          <td class="actions">
            <button class="btn-edit" @click="openEditModal(kategoria)">✏️ Szerkeszt</button>
            <button class="btn-delete" @click="deleteKategoria(kategoria)">🗑️ Törlés</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Modal -->
    <Teleport to="body">
      <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-box" @click.stop>
          <h3>{{ isEditing ? 'Kategória szerkesztése' : 'Új kategória' }}</h3>

          <form @submit.prevent="handleSubmit">
            <div class="form-group">
              <label>Név *</label>
              <input v-model="form.nev" type="text" required />
            </div>

            <div class="form-group">
              <label>Kép URL</label>
              <input v-model="form.kepUrl" type="url" placeholder="https://..." />
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
import { kategoriaService } from '@/services/kategoriaService'

const kategoriak = ref([])
const loading = ref(false)
const modalOpen = ref(false)
const isEditing = ref(false)
const submitting = ref(false)
const error = ref(null)

const form = ref({
  kategoriaID: null,
  nev: '',
  kepUrl: ''
})

onMounted(() => fetchKategoriak())

async function fetchKategoriak() {
  loading.value = true
  try {
    const response = await kategoriaService.getAll()
    kategoriak.value = response.data
  } catch (err) {
    console.error('Kategóriák betöltése sikertelen:', err)
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  isEditing.value = false
  form.value = { kategoriaID: null, nev: '', kepUrl: '' }
  modalOpen.value = true
}

function openEditModal(kategoria) {
  isEditing.value = true
  form.value = { ...kategoria }
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
  error.value = null
}

async function handleSubmit() {
  submitting.value = true
  error.value = null

  try {
    if (isEditing.value) {
      await kategoriaService.update(form.value.kategoriaID, {
        nev: form.value.nev,
        kepUrl: form.value.kepUrl
      })
    } else {
      await kategoriaService.create({
        nev: form.value.nev,
        kepUrl: form.value.kepUrl
      })
    }

    await fetchKategoriak()
    closeModal()
  } catch (err) {
    error.value = err.response?.data?.message || 'Hiba történt a mentés során.'
  } finally {
    submitting.value = false
  }
}

async function deleteKategoria(kategoria) {
  if (!confirm(`Biztosan törölni szeretnéd: ${kategoria.nev}?`)) return

  try {
    await kategoriaService.delete(kategoria.kategoriaID)
    await fetchKategoriak()
  } catch (err) {
    alert(err.response?.data?.message || 'Hiba történt a törlés során.')
  }
}
</script>

<style scoped>
.kategoriak-admin .header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.kategoriak-admin h2 {
  margin: 0;
  font-size: 24px;
  color: #3d2f1f;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
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

.actions {
  display: flex;
  gap: 8px;
}

.btn-primary, .btn-secondary, .btn-edit, .btn-delete {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-primary {
  background: #6b8e23;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #556b1a;
}

.btn-secondary {
  background: white;
  border: 1px solid #d4c5b0;
  color: #3d2f1f;
}

.btn-edit {
  background: #7a9b57;
  color: white;
}

.btn-delete {
  background: #b85c5c;
  color: white;
}

.link {
  color: #6b8e23;
  text-decoration: none;
}

.text-muted {
  color: #8a7f6f;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(61, 47, 31, 0.6);
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
  color: #3d2f1f;
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

.form-group input {
  width: 100%;
  padding: 10px;
  border: 1px solid #d4c5b0;
  border-radius: 6px;
  background: #fefdfb;
}

.error-msg {
  padding: 12px;
  background: #fef5f5;
  border: 1px solid #f5d7d7;
  border-radius: 6px;
  color: #b85c5c;
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
  color: #6b5d4f;
}
</style>