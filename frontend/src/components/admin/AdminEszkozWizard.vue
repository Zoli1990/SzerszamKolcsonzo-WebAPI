<template>
  <div class="wizard-container">
    <!-- PROGRESS BAR -->
    <div class="progress-section">
      <div class="progress-bar">
        <div class="progress-fill" :style="{ width: progressPercent + '%' }"></div>
      </div>
      <p class="step-text">{{ step }}/5 {{ t('wizard.step') }}</p>
    </div>

    <!-- STEP 1 - FOTÓ -->
    <div v-if="step === 1" class="step-content">
      <h2 class="step-title">📸 {{ t('wizard.step1.title') }}</h2>
      <input ref="fileInput" type="file" accept="image/*" capture="environment" @change="onFileChange" style="display: none" />
      <div v-if="preview" class="preview-container">
        <img :src="preview" :alt="t('wizard.step1.preview')" class="preview-image" />
        <button class="btn-change-photo" @click="triggerFile">🔄 {{ t('wizard.step1.changePhoto') }}</button>
      </div>
      <div v-else class="upload-area" @click="triggerFile">
        <div class="upload-icon">📷</div>
        <p class="upload-text">{{ t('wizard.step1.clickToPhoto') }}</p>
        <p class="upload-hint">{{ t('wizard.step1.orBrowse') }}</p>
      </div>
    </div>

    <!-- STEP 2 - ALAPADATOK -->
    <div v-if="step === 2" class="step-content">
      <h2 class="step-title">📝 {{ t('wizard.step2.title') }}</h2>
      <div class="form-group">
        <label>{{ t('wizard.step2.name') }} *</label>
        <input v-model="form.nev" type="text" :placeholder="t('wizard.step2.namePlaceholder')" class="form-input" required />
      </div>
      <div class="form-group">
        <label>{{ t('wizard.step2.description') }}</label>
        <textarea v-model="form.leiras" :placeholder="t('wizard.step2.descriptionPlaceholder')" class="form-textarea" rows="4"></textarea>
      </div>
    </div>

    <!-- STEP 3 - KATEGÓRIA -->
    <div v-if="step === 3" class="step-content">
      <h2 class="step-title">📁 {{ t('wizard.step3.title') }}</h2>
      <div class="form-group">
        <label>{{ t('wizard.step3.selectCategory') }} *</label>
        <select v-model="form.kategoriaId" class="form-select" required>
          <option :value="null" disabled>{{ t('wizard.step3.selectPlaceholder') }}</option>
          <option v-for="k in kategoriak" :key="k.kategoriaID" :value="k.kategoriaID">{{ k.nev }}</option>
        </select>
      </div>
      <button class="btn-new-category" @click="showKategoriModal = true">
        ➕ {{ t('wizard.step3.newCategory') }}
      </button>
    </div>

    <!-- STEP 4 - ÁR -->
    <div v-if="step === 4" class="step-content">
      <h2 class="step-title">💰 {{ t('wizard.step4.title') }}</h2>
      <div class="form-group">
        <label>{{ t('wizard.step4.price') }} *</label>
        <div class="input-with-unit">
          <input v-model.number="form.kiadasiAr" type="number" min="0" step="100" placeholder="1500" class="form-input" required />
          <span class="input-unit">Ft/{{ t('wizard.step4.hour') }}</span>
        </div>
      </div>
      <div class="price-examples">
        <p class="examples-title">{{ t('wizard.step4.quickSelect') }}:</p>
        <div class="price-buttons">
          <button v-for="ar in arPeldak" :key="ar" class="price-btn" @click="form.kiadasiAr = ar">{{ ar }} Ft</button>
        </div>
      </div>
    </div>

    <!-- STEP 5 - ÖSSZEGZÉS -->
    <div v-if="step === 5" class="step-content">
      <h2 class="step-title">✅ {{ t('wizard.step5.title') }}</h2>
      <div class="summary-card">
        <div v-if="preview" class="summary-image">
          <img :src="preview" :alt="t('wizard.step1.preview')" />
        </div>
        <div class="summary-item">
          <span class="summary-label">{{ t('wizard.step5.name') }}:</span>
          <span class="summary-value">{{ form.nev }}</span>
        </div>
        <div class="summary-item">
          <span class="summary-label">{{ t('wizard.step5.category') }}:</span>
          <span class="summary-value">{{ getKategoriaNev(form.kategoriaId) }}</span>
        </div>
        <div class="summary-item">
          <span class="summary-label">{{ t('wizard.step5.price') }}:</span>
          <span class="summary-value">{{ form.kiadasiAr }} Ft/{{ t('wizard.step4.hour') }}</span>
        </div>
        <div v-if="form.leiras" class="summary-item">
          <span class="summary-label">{{ t('wizard.step5.description') }}:</span>
          <span class="summary-value">{{ form.leiras }}</span>
        </div>
      </div>
    </div>

    <!-- NAVIGATION BUTTONS -->
    <div class="nav-buttons">
      <button v-if="step > 1" class="btn-back" @click="step--" :disabled="loading">← {{ t('wizard.back') }}</button>
      <button v-if="step < 5" class="btn-next" @click="nextStep" :disabled="!canProceed || loading">
        {{ t('wizard.next') }} →
      </button>
      <button v-if="step === 5" class="btn-save" @click="saveEszkoz" :disabled="loading">
        <span v-if="loading">⏳ {{ t('wizard.saving') }}</span>
        <span v-else>✅ {{ t('wizard.create') }}</span>
      </button>
    </div>

    <!-- KATEGÓRIA MODAL -->
    <Teleport to="body">
      <div v-if="showKategoriModal" class="modal-overlay" @click="showKategoriModal = false">
        <div class="modal-box" @click.stop>
          <h3>{{ t('wizard.modal.title') }}</h3>
          <div class="form-group">
            <label>{{ t('wizard.modal.categoryName') }} *</label>
            <input
              v-model="ujKategoriaNev"
              type="text"
              :placeholder="t('wizard.modal.categoryPlaceholder')"
              class="form-input"
              @keyup.enter="addKategoria"
            />
          </div>
          <div class="modal-actions">
            <button class="btn-secondary" @click="showKategoriModal = false">{{ t('wizard.modal.cancel') }}</button>
            <button class="btn-primary" @click="addKategoria" :disabled="!ujKategoriaNev.trim() || kategoriaLoading">
              <span v-if="kategoriaLoading">⏳</span>
              <span v-else>{{ t('wizard.modal.create') }}</span>
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import api from '@/services/api'

const router = useRouter()
const { t } = useI18n()

const step = ref(1)
const loading = ref(false)
const kategoriaLoading = ref(false)

const form = ref({
  nev: '',
  leiras: '',
  kategoriaId: null,
  kiadasiAr: null,
  foto: null,
})

const preview = ref(null)
const fileInput = ref(null)
const kategoriak = ref([])
const showKategoriModal = ref(false)
const ujKategoriaNev = ref('')
const arPeldak = [500, 1000, 1500, 2000, 3000, 5000]

const progressPercent = computed(() => (step.value / 5) * 100)

const canProceed = computed(() => {
  switch (step.value) {
    case 1: return !!form.value.foto
    case 2: return !!form.value.nev.trim()
    case 3: return !!form.value.kategoriaId
    case 4: return form.value.kiadasiAr > 0
    default: return true
  }
})

function triggerFile() { fileInput.value.click() }

function onFileChange(e) {
  const file = e.target.files[0]
  if (!file) return
  if (!file.type.startsWith('image/')) { alert(t('wizard.errors.onlyImages')); return }
  if (file.size > 5 * 1024 * 1024) { alert(t('wizard.errors.maxSize')); return }
  form.value.foto = file
  preview.value = URL.createObjectURL(file)
}

function nextStep() {
  if (!canProceed.value) {
    const messages = {
      1: t('wizard.errors.noPhoto'),
      2: t('wizard.errors.noName'),
      3: t('wizard.errors.noCategory'),
      4: t('wizard.errors.noPrice'),
    }
    alert(messages[step.value])
    return
  }
  if (step.value < 5) step.value++
}

function getKategoriaNev(id) {
  const kategoria = kategoriak.value.find((k) => k.kategoriaID === id)
  return kategoria ? kategoria.nev : t('wizard.step5.noneSelected')
}

async function loadKategoriak() {
  try {
    const response = await api.get('/kategoriak')
    kategoriak.value = response.data
  } catch (error) {
    console.error('❌ Kategóriák betöltése sikertelen:', error)
    alert(t('wizard.errors.loadCategories'))
    kategoriak.value = []
  }
}

async function addKategoria() {
  if (!ujKategoriaNev.value.trim()) { alert(t('wizard.errors.emptyCategoryName')); return }
  kategoriaLoading.value = true
  try {
    const response = await api.post('/kategoriak', { nev: ujKategoriaNev.value.trim(), kepUrl: '' })
    kategoriak.value.push(response.data)
    form.value.kategoriaId = response.data.kategoriaID
    ujKategoriaNev.value = ''
    showKategoriModal.value = false
    alert(t('wizard.modal.createSuccess'))
  } catch (error) {
    console.error('❌ Kategória létrehozása sikertelen:', error)
    alert('❌ ' + (error.response?.data?.message || t('wizard.modal.createError')))
  } finally {
    kategoriaLoading.value = false
  }
}

async function saveEszkoz() {
  loading.value = true
  try {
    const fd = new FormData()
    fd.append('nev', form.value.nev)
    fd.append('kategoriaID', form.value.kategoriaId)
    fd.append('kiadasiAr', form.value.kiadasiAr)
    fd.append('leiras', form.value.leiras || '')
    fd.append('vetelar', 0)
    fd.append('beszerzesiDatum', new Date().toISOString())
    if (form.value.foto) fd.append('kep', form.value.foto)

    await api.post('/eszkozok', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
    alert(t('wizard.saveSuccess'))
    router.push('/admin')
  } catch (error) {
    console.error('❌ Eszköz mentése sikertelen:', error)
    let errorMsg = t('wizard.saveError')
    if (error.response?.data?.message) errorMsg = error.response.data.message
    else if (error.response?.data?.errors) errorMsg = Object.values(error.response.data.errors).flat().join('\n')
    alert('❌ ' + errorMsg)
  } finally {
    loading.value = false
  }
}

onMounted(async () => { await loadKategoriak() })
</script>

<style scoped>
.wizard-container { max-width: 600px; margin: 0 auto; padding: 20px; min-height: calc(100vh - 100px); display: flex; flex-direction: column; }

.progress-section { margin-bottom: 32px; }
.progress-bar { width: 100%; height: 8px; background: #e8dcc8; border-radius: 4px; overflow: hidden; margin-bottom: 8px; }
.progress-fill { height: 100%; background: linear-gradient(90deg, #6b8e23, #8ba83e); transition: width 0.3s ease; }
.step-text { text-align: center; color: #6b5d4f; font-size: 14px; font-weight: 600; margin: 0; }

.step-content { flex: 1; display: flex; flex-direction: column; }
.step-title { font-size: 24px; color: #3d2f1f; margin: 0 0 24px 0; text-align: center; }

.upload-area {
  flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 16px;
  border: 3px dashed #d4c5b0; border-radius: 16px; background: #faf8f3;
  padding: 60px 20px; cursor: pointer; transition: all 0.2s ease;
}
.upload-area:hover { border-color: #6b8e23; background: #f5f1e8; }
.upload-icon { font-size: 80px; }
.upload-text { font-size: 18px; font-weight: 600; color: #3d2f1f; margin: 0; }
.upload-hint { font-size: 14px; color: #6b5d4f; margin: 0; }

.preview-container { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 16px; }
.preview-image { width: 100%; max-height: 400px; object-fit: contain; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
.btn-change-photo { padding: 12px 24px; background: white; border: 2px solid #d4c5b0; border-radius: 8px; color: #3d2f1f; font-size: 16px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; }
.btn-change-photo:hover { border-color: #6b8e23; color: #6b8e23; }

.form-group { margin-bottom: 24px; }
.form-group label { display: block; margin-bottom: 8px; font-weight: 600; color: #3d2f1f; font-size: 16px; }
.form-input, .form-textarea, .form-select { width: 100%; padding: 14px 16px; border: 2px solid #d4c5b0; border-radius: 10px; font-size: 16px; color: #3d2f1f; background: white; font-family: inherit; transition: border-color 0.2s ease; }
.form-input:focus, .form-textarea:focus, .form-select:focus { outline: none; border-color: #6b8e23; }
.form-textarea { resize: vertical; min-height: 100px; }
.input-with-unit { position: relative; }
.input-unit { position: absolute; right: 16px; top: 50%; transform: translateY(-50%); color: #6b5d4f; font-weight: 600; pointer-events: none; }

.price-examples { margin-top: 24px; padding: 20px; background: #faf8f3; border-radius: 12px; }
.examples-title { margin: 0 0 12px 0; color: #6b5d4f; font-size: 14px; font-weight: 600; }
.price-buttons { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; }
.price-btn { padding: 12px; background: white; border: 2px solid #d4c5b0; border-radius: 8px; color: #3d2f1f; font-size: 14px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; }
.price-btn:hover { border-color: #6b8e23; color: #6b8e23; }
.price-btn:active { transform: scale(0.95); }

.btn-new-category { width: 100%; padding: 14px; background: white; border: 2px dashed #6b8e23; border-radius: 10px; color: #6b8e23; font-size: 16px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; }
.btn-new-category:hover { background: #f5f1e8; }

.summary-card { background: white; border: 2px solid #e8dcc8; border-radius: 12px; padding: 24px; }
.summary-image { margin-bottom: 24px; border-radius: 8px; overflow: hidden; }
.summary-image img { width: 100%; height: auto; display: block; }
.summary-item { display: flex; justify-content: space-between; align-items: flex-start; padding: 12px 0; border-bottom: 1px solid #f5f1e8; gap: 16px; }
.summary-item:last-child { border-bottom: none; }
.summary-label { font-weight: 600; color: #6b5d4f; flex-shrink: 0; }
.summary-value { text-align: right; color: #3d2f1f; font-weight: 500; }

.nav-buttons { display: flex; gap: 12px; margin-top: 32px; padding-top: 24px; border-top: 2px solid #f5f1e8; }
.btn-back, .btn-next, .btn-save { flex: 1; padding: 16px; border: none; border-radius: 10px; font-size: 16px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; min-height: 56px; }
.btn-back { background: white; border: 2px solid #d4c5b0; color: #3d2f1f; }
.btn-back:hover:not(:disabled) { border-color: #6b5d4f; }
.btn-next, .btn-save { background: #6b8e23; color: white; }
.btn-next:hover:not(:disabled), .btn-save:hover:not(:disabled) { background: #5a7a1e; transform: translateY(-2px); box-shadow: 0 4px 12px rgba(107,142,35,0.3); }
.btn-back:disabled, .btn-next:disabled, .btn-save:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-save { background: linear-gradient(135deg, #6b8e23, #8ba83e); }

.modal-overlay { position: fixed; inset: 0; background: rgba(61,47,31,0.6); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 20px; }
.modal-box { background: white; border-radius: 16px; padding: 24px; max-width: 400px; width: 100%; box-shadow: 0 10px 40px rgba(0,0,0,0.2); }
.modal-box h3 { margin: 0 0 20px 0; color: #3d2f1f; font-size: 20px; }
.modal-actions { display: flex; gap: 12px; margin-top: 24px; }
.btn-primary, .btn-secondary { flex: 1; padding: 12px; border: none; border-radius: 8px; font-size: 16px; font-weight: 600; cursor: pointer; transition: all 0.2s ease; min-height: 48px; }
.btn-primary { background: #6b8e23; color: white; }
.btn-primary:hover:not(:disabled) { background: #5a7a1e; }
.btn-secondary { background: white; border: 2px solid #d4c5b0; color: #3d2f1f; }
.btn-secondary:hover { border-color: #6b5d4f; }
.btn-primary:disabled, .btn-secondary:disabled { opacity: 0.5; cursor: not-allowed; }

@media (max-width: 768px) {
  .wizard-container { padding: 16px; }
  .step-title { font-size: 20px; }
  .upload-area { padding: 40px 20px; }
  .upload-icon { font-size: 60px; }
  .price-buttons { grid-template-columns: repeat(2, 1fr); }
  .nav-buttons { flex-direction: column-reverse; }
  .btn-back { order: 2; }
}
</style>