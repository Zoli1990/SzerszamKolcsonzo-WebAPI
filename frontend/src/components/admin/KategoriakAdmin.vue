<template>
  <div class="kategoriak-admin">
    <div class="page-header">
      <h2 class="page-title">
        <span class="title-icon">📁</span>
        <span>Kategóriák</span>
      </h2>
      <button class="btn-add" @click="openCreateModal">
        <span>+</span>
        <span class="btn-add-text">Új kategória</span>
      </button>
    </div>

    <div v-if="loading" class="state-container">
      <div class="loading-spinner"></div>
      <p>Kategóriák betöltése...</p>
    </div>

    <div v-else-if="kategoriak.length === 0" class="state-container empty">
      <span class="state-icon">📂</span>
      <p>Nincs még kategória az adatbázisban.</p>
    </div>

    <!-- MOBIL: KÁRTYA NÉZET -->
    <div v-else class="mobile-only">
      <div class="cards-view">
        <div v-for="kategoria in kategoriak" :key="kategoria.kategoriaID" class="kategoria-card">
          <div class="card-header">
            <div class="card-id-badge">#{{ kategoria.kategoriaID }}</div>
            <span v-if="kategoria.kepUrl" class="img-badge has-img">📷 Van kép</span>
            <span v-else class="img-badge no-img">Nincs kép</span>
          </div>

          <h3 class="card-title">{{ kategoria.nev }}</h3>

          <a v-if="kategoria.kepUrl" :href="kategoria.kepUrl" target="_blank" class="card-img-link">
            🔗 Kép megtekintése
          </a>

          <div class="card-actions">
            <button class="btn-card btn-edit" @click="openEditModal(kategoria)">
              <span class="btn-icon">✏️</span>
              <span>Szerkesztés</span>
            </button>
            <button class="btn-card btn-delete" @click="deleteKategoria(kategoria)">
              <span class="btn-icon">🗑️</span>
              <span>Törlés</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- DESKTOP: TÁBLÁZAT NÉZET -->
    <div v-if="!loading && kategoriak.length > 0" class="desktop-only">
      <table class="data-table">
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
            <td>
              <strong>#{{ kategoria.kategoriaID }}</strong>
            </td>
            <td>
              <span class="table-name">{{ kategoria.nev }}</span>
            </td>
            <td>
              <a v-if="kategoria.kepUrl" :href="kategoria.kepUrl" target="_blank" class="link"
                >Kép megtekintése</a
              >
              <span v-else class="text-muted">Nincs kép</span>
            </td>
            <td>
              <div class="table-actions">
                <button class="btn-table btn-edit" @click="openEditModal(kategoria)">
                  ✏️ Szerkeszt
                </button>
                <button class="btn-table btn-delete" @click="deleteKategoria(kategoria)">
                  🗑️ Törlés
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- MODAL -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalOpen" class="modal-overlay" @click="closeModal">
          <div class="modal-container" @click.stop>
            <div class="modal-header">
              <button class="btn-back mobile-only" @click="closeModal">← Vissza</button>
              <h3 class="modal-title">{{ isEditing ? '✏️ Szerkesztés' : '➕ Új kategória' }}</h3>
              <button class="btn-close desktop-only" @click="closeModal">✕</button>
            </div>

            <form @submit.prevent="handleSubmit" class="modal-content">
              <div class="form-group">
                <label>Név *</label>
                <input v-model="form.nev" type="text" required placeholder="Kategória neve" />
              </div>

              <div class="form-group">
                <label>Kép URL</label>
                <input v-model="form.kepUrl" type="url" placeholder="https://..." />
              </div>

              <div v-if="error" class="alert alert-error">{{ error }}</div>

              <div class="form-footer">
                <button type="button" class="btn-secondary" @click="closeModal">Mégse</button>
                <button type="submit" class="btn-primary" :disabled="submitting">
                  {{ submitting ? 'Mentés...' : '💾 Mentés' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- TOAST -->
    <Transition name="toast">
      <div v-if="toast.show" class="toast" :class="toast.type">
        <span>{{ toast.type === 'success' ? '✅' : '❌' }} {{ toast.message }}</span>
      </div>
    </Transition>
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
const toast = ref({ show: false, message: '', type: 'success' })

const form = ref({ kategoriaID: null, nev: '', kepUrl: '' })

onMounted(() => fetchKategoriak())

function showToast(message, type = 'success') {
  toast.value = { show: true, message, type }
  setTimeout(() => {
    toast.value.show = false
  }, 3000)
}

async function fetchKategoriak() {
  loading.value = true
  try {
    const response = await kategoriaService.getAll()
    kategoriak.value = response.data
  } catch (err) {
    console.error('Kategóriák betöltése sikertelen:', err)
    showToast('Hiba a betöltés során', 'error')
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  isEditing.value = false
  form.value = { kategoriaID: null, nev: '', kepUrl: '' }
  modalOpen.value = true
  document.body.style.overflow = 'hidden'
}

function openEditModal(kategoria) {
  isEditing.value = true
  form.value = { ...kategoria }
  modalOpen.value = true
  document.body.style.overflow = 'hidden'
}

function closeModal() {
  modalOpen.value = false
  error.value = null
  document.body.style.overflow = ''
}

async function handleSubmit() {
  submitting.value = true
  error.value = null
  try {
    if (isEditing.value) {
      await kategoriaService.update(form.value.kategoriaID, {
        nev: form.value.nev,
        kepUrl: form.value.kepUrl,
      })
      showToast('Kategória frissítve!')
    } else {
      await kategoriaService.create({ nev: form.value.nev, kepUrl: form.value.kepUrl })
      showToast('Új kategória létrehozva!')
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
    showToast('Kategória törölve!')
    await fetchKategoriak()
  } catch (err) {
    showToast(err.response?.data?.message || 'Hiba történt a törlés során', 'error')
  }
}
</script>

<style scoped>
.kategoriak-admin {
  padding: var(--spacing-md, 16px);
  max-width: 1400px;
  margin: 0 auto;
}

@media (min-width: 768px) {
  .kategoriak-admin {
    padding: var(--spacing-lg, 24px);
  }
}

/* HEADER */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md, 16px);
}

.page-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 20px;
  font-weight: 700;
  color: var(--color-text, #3d2f1f);
  margin: 0;
}

@media (min-width: 768px) {
  .page-title {
    font-size: 24px;
  }
}

.title-icon {
  font-size: 1.2em;
}

.btn-add {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 16px;
  background: var(--color-primary, #6b8e23);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 44px;
}

.btn-add:hover {
  background: var(--color-primary-dark, #556b1a);
}
.btn-add-text {
  display: none;
}
@media (min-width: 768px) {
  .btn-add-text {
    display: inline;
  }
}

/* STATE */
.state-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 48px 16px;
  text-align: center;
}

.state-icon {
  font-size: 48px;
  margin-bottom: 16px;
}
.state-container p {
  color: var(--color-text-muted, #6b5d4f);
  margin: 0;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--color-border, #e8dcc8);
  border-top-color: var(--color-primary, #6b8e23);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* MOBILE CARDS */
.cards-view {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.kategoria-card {
  background: var(--color-surface, white);
  border-radius: 14px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(61, 47, 31, 0.08);
  border: 1px solid var(--color-border, #e8dcc8);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.card-id-badge {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-muted, #6b5d4f);
  letter-spacing: 0.5px;
}

.img-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.img-badge.has-img {
  background: #d1fae5;
  color: #065f46;
}
.img-badge.no-img {
  background: #e5e7eb;
  color: #6b7280;
}

.card-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--color-text, #3d2f1f);
  margin: 0 0 8px 0;
  line-height: 1.3;
}

.card-img-link {
  display: inline-block;
  color: var(--color-primary, #6b8e23);
  text-decoration: none;
  font-size: 14px;
  font-weight: 500;
  padding: 4px 0;
}

.card-img-link:active {
  opacity: 0.7;
}

.card-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--color-border, #e8dcc8);
}

.btn-card {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 14px 12px;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  min-height: 48px;
  -webkit-tap-highlight-color: transparent;
}

.btn-card:active {
  transform: scale(0.97);
}

.btn-card.btn-edit {
  background: var(--color-primary, #6b8e23);
  color: white;
}
.btn-card.btn-edit:active {
  background: #556b1a;
}

.btn-card.btn-delete {
  background: #fee2e2;
  color: #991b1b;
}
.btn-card.btn-delete:active {
  background: #fecaca;
}

.btn-icon {
  font-size: 16px;
  flex-shrink: 0;
}

/* DESKTOP TABLE */
.data-table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface, white);
  border-radius: 12px;
  overflow: hidden;
}

.data-table th,
.data-table td {
  padding: 14px 16px;
  text-align: left;
  border-bottom: 1px solid var(--color-border, #e8dcc8);
}

.data-table th {
  background: var(--color-background, #f5f1e8);
  font-weight: 600;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted, #6b5d4f);
}

.data-table tbody tr {
  transition: background 0.15s ease;
}
.data-table tbody tr:hover {
  background: var(--color-background, #faf8f3);
}

.table-name {
  font-weight: 600;
}
.link {
  color: var(--color-primary, #6b8e23);
  text-decoration: none;
  font-weight: 500;
}
.link:hover {
  text-decoration: underline;
}
.text-muted {
  color: #9ca3af;
}

.table-actions {
  display: flex;
  gap: 8px;
}

.btn-table {
  padding: 8px 14px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  font-size: 13px;
  transition: all 0.15s ease;
  min-height: 36px;
}

.btn-table:hover {
  transform: translateY(-1px);
}
.btn-table.btn-edit {
  background: var(--color-primary, #6b8e23);
  color: white;
}
.btn-table.btn-delete {
  background: #b85c5c;
  color: white;
}

/* MODAL */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
  display: flex;
  align-items: flex-end;
}

@media (min-width: 768px) {
  .modal-overlay {
    align-items: center;
    justify-content: center;
    padding: 24px;
    background: rgba(61, 47, 31, 0.7);
    backdrop-filter: blur(4px);
  }
}

.modal-container {
  background: var(--color-surface, white);
  width: 100%;
  max-height: 92vh;
  border-radius: 16px 16px 0 0;
  display: flex;
  flex-direction: column;
  animation: slideUp 0.3s ease-out;
  overflow: hidden;
}

@media (min-width: 768px) {
  .modal-container {
    max-width: 500px;
    border-radius: 12px;
    animation: scaleIn 0.2s ease-out;
  }
}

@keyframes slideUp {
  from {
    transform: translateY(100%);
  }
  to {
    transform: translateY(0);
  }
}

@keyframes scaleIn {
  from {
    transform: scale(0.95);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--color-border, #e8dcc8);
  flex-shrink: 0;
}

.modal-title {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
  color: var(--color-text, #3d2f1f);
}

.btn-back {
  background: none;
  border: none;
  color: var(--color-primary, #6b8e23);
  font-weight: 600;
  cursor: pointer;
  padding: 8px;
  margin: -8px;
  font-size: 16px;
  min-height: 44px;
  display: flex;
  align-items: center;
}

.btn-close {
  width: 40px;
  height: 40px;
  background: none;
  border: none;
  font-size: 24px;
  color: var(--color-text-muted, #6b5d4f);
  cursor: pointer;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-content {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  -webkit-overflow-scrolling: touch;
}

/* FORM */
.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 6px;
  font-size: 14px;
  color: var(--color-text, #3d2f1f);
}

.form-group input {
  width: 100%;
  padding: 12px 14px;
  border: 2px solid var(--color-border, #e8dcc8);
  border-radius: 10px;
  font-size: 16px;
  color: var(--color-text, #3d2f1f);
  background: var(--color-surface, #fefdfb);
  transition: border-color 0.2s ease;
  appearance: none;
  -webkit-appearance: none;
}

.form-group input:focus {
  outline: none;
  border-color: var(--color-primary, #6b8e23);
}

.alert {
  padding: 14px 16px;
  border-radius: 10px;
  margin-bottom: 16px;
  font-size: 14px;
}
.alert-error {
  background: #fee2e2;
  color: #991b1b;
}

.form-footer {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

.btn-primary,
.btn-secondary {
  flex: 1;
  padding: 14px;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  border: none;
  min-height: 48px;
  transition: all 0.2s ease;
}

.btn-primary {
  background: var(--color-primary, #6b8e23);
  color: white;
}
.btn-primary:hover:not(:disabled) {
  background: var(--color-primary-dark, #556b1a);
}
.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.btn-secondary {
  background: var(--color-surface, white);
  border: 2px solid var(--color-border, #e8dcc8);
  color: var(--color-text, #3d2f1f);
}

/* TOAST */
.toast {
  position: fixed;
  bottom: calc(var(--bottom-nav-height, 0px) + 16px);
  left: 16px;
  right: 16px;
  padding: 16px;
  border-radius: 12px;
  z-index: 2000;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  font-weight: 600;
  font-size: 15px;
}

@media (min-width: 768px) {
  .toast {
    left: auto;
    right: 24px;
    bottom: 24px;
    max-width: 400px;
  }
}

.toast.success {
  background: var(--color-success, #10b981);
  color: white;
}
.toast.error {
  background: var(--color-danger, #ef4444);
  color: white;
}

.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from {
  transform: translateY(100px);
  opacity: 0;
}
.toast-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

/* RESPONSIVE HELPERS */
.mobile-only {
  display: block;
}
.desktop-only {
  display: none;
}

@media (min-width: 768px) {
  .mobile-only {
    display: none;
  }
  .desktop-only {
    display: block;
  }
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

/* MOBILE */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  .btn-add {
    width: 100%;
    justify-content: center;
  }

  .btn-add-text {
    display: inline;
  }

  .modal-container {
    max-height: 100vh;
    height: auto;
    min-height: 50vh;
    border-radius: 16px 16px 0 0;
  }

  .modal-header {
    position: sticky;
    top: 0;
    background: var(--color-surface, white);
    z-index: 10;
  }

  .form-footer {
    position: sticky;
    bottom: 0;
    background: var(--color-surface, white);
    padding: 16px 0 0;
    margin-top: 16px;
  }
}

/* EXTRA SMALL */
@media (max-width: 480px) {
  .kategoriak-admin {
    padding: 12px;
  }
  .page-title {
    font-size: 18px;
  }
  .cards-view {
    gap: 12px;
  }
  .kategoria-card {
    padding: 16px;
  }
  .card-title {
    font-size: 17px;
  }

  .btn-card {
    padding: 12px 10px;
    font-size: 13px;
    min-height: 44px;
  }
}
</style>
