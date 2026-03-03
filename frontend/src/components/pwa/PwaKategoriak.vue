<!-- ============================================================================ -->
<!-- src/components/pwa/PwaKategoriak.vue — Kategóriák CRUD mobilra            -->
<!-- ============================================================================ -->

<template>
  <div class="pwa-kategoriak">
    <!-- Header -->
    <div class="page-header">
      <h2>📁 Kategóriák</h2>
      <button class="btn-add" @click="openCreate">+ Új</button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Betöltés...</p>
    </div>

    <!-- Üres -->
    <div v-else-if="kategoriak.length === 0" class="empty-state">
      <span class="empty-icon">📁</span>
      <p>Nincs kategória</p>
    </div>

    <!-- Lista -->
    <div v-else class="kategoria-list">
      <div v-for="k in kategoriak" :key="k.kategoriaID" class="kategoria-card">
        <!-- Megjelenítés mód -->
        <template v-if="editingId !== k.kategoriaID">
          <div class="card-content">
            <strong class="kat-nev">{{ k.nev }}</strong>
            <span class="kat-id">#{{ k.kategoriaID }}</span>
          </div>
          <div class="card-actions">
            <button class="btn-sm btn-edit" @click="startEdit(k)">✏️</button>
            <button class="btn-sm btn-delete" @click="deleteKategoria(k)">🗑️</button>
          </div>
        </template>

        <!-- Szerkesztés mód -->
        <template v-else>
          <input
            v-model="editForm.nev"
            class="edit-input"
            placeholder="Kategória neve"
            @keyup.enter="saveEdit"
            @keyup.esc="cancelEdit"
          />
          <div class="card-actions">
            <button class="btn-sm btn-save" @click="saveEdit">💾</button>
            <button class="btn-sm btn-cancel" @click="cancelEdit">✖️</button>
          </div>
        </template>
      </div>
    </div>

    <!-- Új kategória form -->
    <Transition name="slide">
      <div v-if="showCreateForm" class="create-form">
        <input
          v-model="createForm.nev"
          class="create-input"
          placeholder="Új kategória neve"
          @keyup.enter="createKategoria"
          ref="createInput"
        />
        <div class="create-actions">
          <button class="btn-create" @click="createKategoria" :disabled="!createForm.nev.trim()">
            ✅ Létrehozás
          </button>
          <button class="btn-cancel-create" @click="showCreateForm = false">Mégse</button>
        </div>
      </div>
    </Transition>

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="toast.show" class="toast" :class="toast.type">
        {{ toast.message }}
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, nextTick, onMounted } from 'vue'
import api from '@/services/api'

const kategoriak = ref([])
const loading = ref(false)
const toast = ref({ show: false, message: '', type: 'success' })

// Create
const showCreateForm = ref(false)
const createForm = ref({ nev: '' })
const createInput = ref(null)

// Edit
const editingId = ref(null)
const editForm = ref({ nev: '' })

// ═══════════════════════════════════════════════════════════════════════════
// API
// ═══════════════════════════════════════════════════════════════════════════
async function fetchKategoriak() {
  loading.value = true
  try {
    const res = await api.get('/kategoriak')
    kategoriak.value = res.data
  } catch (err) {
    console.error('Kategóriák betöltése:', err)
    showToast('Betöltés sikertelen!', 'error')
  } finally {
    loading.value = false
  }
}

async function createKategoria() {
  if (!createForm.value.nev.trim()) return
  try {
    await api.post('/kategoriak', { nev: createForm.value.nev.trim() })
    showToast('✅ Kategória létrehozva!')
    createForm.value.nev = ''
    showCreateForm.value = false
    await fetchKategoriak()
  } catch (err) {
    showToast(err.response?.data?.message || 'Létrehozás sikertelen!', 'error')
  }
}

async function saveEdit() {
  if (!editForm.value.nev.trim()) return
  try {
    await api.put(`/kategoriak/${editingId.value}`, { nev: editForm.value.nev.trim() })
    showToast('✅ Kategória frissítve!')
    editingId.value = null
    await fetchKategoriak()
  } catch (err) {
    showToast(err.response?.data?.message || 'Mentés sikertelen!', 'error')
  }
}

async function deleteKategoria(k) {
  if (!confirm(`Törlöd: "${k.nev}"?`)) return
  try {
    await api.delete(`/kategoriak/${k.kategoriaID}`)
    showToast('✅ Kategória törölve!')
    await fetchKategoriak()
  } catch (err) {
    showToast(err.response?.data?.message || 'Törlés sikertelen!', 'error')
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// UI
// ═══════════════════════════════════════════════════════════════════════════
function openCreate() {
  showCreateForm.value = true
  createForm.value.nev = ''
  nextTick(() => createInput.value?.focus())
}

function startEdit(k) {
  editingId.value = k.kategoriaID
  editForm.value.nev = k.nev
}

function cancelEdit() {
  editingId.value = null
}

function showToast(message, type = 'success') {
  toast.value = { show: true, message, type }
  setTimeout(() => { toast.value.show = false }, 3000)
}

onMounted(() => fetchKategoriak())
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { margin: 0; font-size: 20px; color: #3d2f1f; }
.btn-add { padding: 10px 18px; background: #6b8e23; color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 14px; cursor: pointer; }

/* States */
.loading-state, .empty-state { text-align: center; padding: 48px 16px; color: #6b5d4f; }
.empty-icon { font-size: 48px; display: block; margin-bottom: 12px; }
.spinner { width: 32px; height: 32px; border: 3px solid #e8dcc8; border-top-color: #6b8e23; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 12px; }
@keyframes spin { to { transform: rotate(360deg); } }

/* Cards */
.kategoria-list { display: flex; flex-direction: column; gap: 8px; }
.kategoria-card { display: flex; align-items: center; justify-content: space-between; background: white; border-radius: 12px; padding: 14px 16px; box-shadow: 0 1px 4px rgba(0,0,0,0.06); min-height: 52px; }

.card-content { display: flex; align-items: center; gap: 12px; }
.kat-nev { font-size: 16px; color: #3d2f1f; }
.kat-id { font-size: 12px; color: #9ca3af; }

.card-actions { display: flex; gap: 6px; }
.btn-sm { width: 38px; height: 38px; border: none; border-radius: 8px; font-size: 16px; cursor: pointer; display: flex; align-items: center; justify-content: center; }
.btn-edit { background: #fef3c7; }
.btn-delete { background: #fee2e2; }
.btn-save { background: #d1fae5; }
.btn-cancel { background: #f3f4f6; }

/* Edit input */
.edit-input { flex: 1; padding: 10px 14px; border: 2px solid #6b8e23; border-radius: 10px; font-size: 15px; font-family: inherit; margin-right: 8px; }
.edit-input:focus { outline: none; }

/* Create form */
.create-form { background: white; border-radius: 14px; padding: 16px; margin-top: 16px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
.create-input { width: 100%; padding: 14px 16px; border: 2px solid #e8dcc8; border-radius: 10px; font-size: 16px; font-family: inherit; box-sizing: border-box; margin-bottom: 12px; }
.create-input:focus { outline: none; border-color: #6b8e23; }
.create-actions { display: flex; gap: 8px; }
.btn-create { flex: 1; padding: 12px; background: #6b8e23; color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 14px; cursor: pointer; min-height: 44px; }
.btn-create:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-cancel-create { padding: 12px 20px; background: white; border: 2px solid #e8dcc8; border-radius: 10px; color: #6b5d4f; cursor: pointer; font-weight: 600; }

/* Transitions */
.slide-enter-active, .slide-leave-active { transition: all 0.3s ease; }
.slide-enter-from { transform: translateY(20px); opacity: 0; }
.slide-leave-to { transform: translateY(20px); opacity: 0; }

/* Toast */
.toast { position: fixed; bottom: 80px; left: 16px; right: 16px; padding: 14px; border-radius: 12px; font-weight: 600; text-align: center; z-index: 2000; box-shadow: 0 8px 24px rgba(0,0,0,0.2); }
.toast.success { background: #10b981; color: white; }
.toast.error { background: #ef4444; color: white; }
.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from { transform: translateY(100px); opacity: 0; }
.toast-leave-to { transform: translateX(100%); opacity: 0; }
</style>
