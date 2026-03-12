<template>
  <div class="profilom-container">
    <div class="profilom-header">
      <h1>👤 {{ t('profilom.title') }}</h1>
      <p class="user-email">{{ authStore.userEmail }}</p>
    </div>

    <div v-if="loading" class="loading">{{ t('profilom.loading') }}</div>

    <div v-else class="profil-card">
      <form @submit.prevent="handleSubmit" class="profil-form">
        <div class="form-group">
          <label>{{ t('profilom.email') }}</label>
          <input type="email" :value="authStore.userEmail" disabled class="disabled-input" />
          <small class="hint">{{ t('profilom.emailHint') }}</small>
        </div>

        <div class="form-group">
          <label>{{ t('profilom.name') }} *</label>
          <input type="text" v-model="form.nev" :placeholder="t('profilom.namePlaceholder')" required />
        </div>

        <div class="section-title">{{ t('profilom.addressSection') }}</div>

        <div class="form-row">
          <div class="form-group small">
            <label>{{ t('profilom.zip') }} *</label>
            <input
              type="text"
              v-model="form.iranyitoszam"
              placeholder="6720"
              required
              maxlength="4"
              pattern="[1-9][0-9]{3}"
              @input="onIranyitoszamChange"
            />
          </div>
          <div class="form-group flex-grow">
            <label>{{ t('profilom.city') }}</label>
            <input
              type="text"
              v-model="form.telepules"
              :placeholder="t('profilom.cityPlaceholder')"
              :class="{ 'auto-filled': telepulesAutoFilled }"
              readonly
            />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group flex-grow">
            <label>{{ t('profilom.street') }} *</label>
            <input type="text" v-model="form.utca" :placeholder="t('profilom.streetPlaceholder')" required />
          </div>
          <div class="form-group small">
            <label>{{ t('profilom.houseNumber') }} *</label>
            <input type="text" v-model="form.hazszam" placeholder="12/A" required />
          </div>
        </div>

        <div class="form-group">
          <label>{{ t('profilom.phone') }}</label>
          <input type="tel" v-model="form.telefonszam" placeholder="+36301234567" />
        </div>

        <div v-if="teljesCim" class="cim-preview">
          <span class="preview-label">📍 {{ t('profilom.fullAddress') }}:</span>
          <span class="preview-value">{{ teljesCim }}</span>
        </div>

        <div v-if="error" class="error-message">{{ error }}</div>
        <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

        <div class="form-actions">
          <RouterLink to="/" class="btn-secondary">{{ t('profilom.back') }}</RouterLink>
          <button type="submit" class="btn-primary" :disabled="submitting">
            {{ submitting ? t('profilom.saving') : t('profilom.save') }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'
import { iranyitoszamService } from '@/services/iranyitoszamService'

const { t } = useI18n()
const authStore = useAuthStore()
const loading = ref(false)
const submitting = ref(false)
const error = ref(null)
const successMessage = ref('')
const telepulesAutoFilled = ref(false)

const form = ref({
  nev: '',
  iranyitoszam: '',
  telepules: '',
  utca: '',
  hazszam: '',
  telefonszam: ''
})

const teljesCim = computed(() => {
  if (!form.value.iranyitoszam || !form.value.utca || !form.value.hazszam) return ''
  const telepules = form.value.telepules ? form.value.telepules + ', ' : ''
  return `${form.value.iranyitoszam} ${telepules}${form.value.utca} ${form.value.hazszam}`.trim()
})

onMounted(async () => {
  loading.value = true
  try {
    if (authStore.token) await authStore.fetchProfile()
    loadFormFromProfile()
  } catch (err) {
    console.error('Profil betöltése sikertelen:', err)
    error.value = t('profilom.loadError')
  } finally {
    loading.value = false
  }
})

function loadFormFromProfile() {
  const profile = authStore.userProfile
  if (profile) {
    form.value = {
      nev: profile.nev || '',
      iranyitoszam: profile.iranyitoszam || '',
      telepules: profile.telepules || '',
      utca: profile.utca || '',
      hazszam: profile.hazszam || '',
      telefonszam: profile.telefonszam || ''
    }
    if (form.value.telepules) telepulesAutoFilled.value = true
  }
}

function onIranyitoszamChange() {
  const irsz = form.value.iranyitoszam
  if (irsz && irsz.length === 4 && /^[1-9]\d{3}$/.test(irsz)) {
    const telepules = iranyitoszamService.getTelepules(irsz)
    if (telepules) {
      form.value.telepules = telepules
      telepulesAutoFilled.value = true
    } else {
      form.value.telepules = ''
      telepulesAutoFilled.value = false
    }
  } else {
    form.value.telepules = ''
    telepulesAutoFilled.value = false
  }
}

async function handleSubmit() {
  submitting.value = true
  error.value = null
  successMessage.value = ''
  try {
    await authStore.updateProfile({
      nev: form.value.nev,
      telefonszam: form.value.telefonszam,
      iranyitoszam: form.value.iranyitoszam,
      telepules: form.value.telepules,
      utca: form.value.utca,
      hazszam: form.value.hazszam,
      cim: teljesCim.value
    })
    successMessage.value = t('profilom.saveSuccess')
    setTimeout(() => { successMessage.value = '' }, 3000)
  } catch (err) {
    console.error('Profil mentési hiba:', err)
    if (err.response?.data?.errors) {
      const errors = Object.values(err.response.data.errors).flat()
      error.value = errors.join(' ')
    } else {
      error.value = err.response?.data?.message || t('profilom.saveError')
    }
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.profilom-container { max-width: 600px; margin: 0 auto; padding: 20px; }
.profilom-header { margin-bottom: 32px; }
.profilom-header h1 { margin: 0 0 8px 0; font-size: 32px; color: #1f2937; }
.user-email { color: #6b7280; font-size: 16px; }
.loading { text-align: center; padding: 60px; color: #6b7280; }

.profil-card { background: white; border-radius: 12px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
.profil-form { display: flex; flex-direction: column; gap: 20px; }

.section-title {
  font-size: 16px; font-weight: 600; color: #1f2937;
  margin: 8px 0 0 0; padding-bottom: 8px; border-bottom: 2px solid #e5e7eb;
}

.form-group { display: flex; flex-direction: column; }
.form-group label { margin-bottom: 8px; font-weight: 600; color: #374151; font-size: 14px; }
.form-group input {
  padding: 12px; border: 1px solid #d1d5db; border-radius: 6px;
  font-size: 14px; transition: border-color 0.2s; font-family: inherit;
}
.form-group input:focus { outline: none; border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
.form-group input.disabled-input { background: #f3f4f6; color: #6b7280; cursor: not-allowed; }
.form-group input.auto-filled { background: #f0fdf4; border-color: #86efac; }
.form-group input[readonly] { background: #f9fafb; cursor: not-allowed; }

.form-row { display: flex; gap: 12px; }
.form-row .form-group.small { width: 120px; flex-shrink: 0; }
.form-row .form-group.flex-grow { flex: 1; }

.hint { margin-top: 6px; font-size: 12px; color: #9ca3af; }

.cim-preview {
  padding: 12px 16px; background: #f0f9ff; border: 1px solid #bae6fd;
  border-radius: 6px; display: flex; gap: 8px; align-items: center; flex-wrap: wrap;
}
.preview-label { font-size: 14px; color: #0369a1; }
.preview-value { font-size: 14px; font-weight: 600; color: #0c4a6e; }

.error-message { padding: 12px; background: #fef2f2; border: 1px solid #fecaca; border-radius: 6px; color: #dc2626; font-size: 14px; }
.success-message { padding: 12px; background: #d1fae5; border: 1px solid #a7f3d0; border-radius: 6px; color: #065f46; font-size: 14px; }

.form-actions { display: flex; gap: 12px; justify-content: flex-end; padding-top: 20px; border-top: 1px solid #e5e7eb; }

.btn-secondary, .btn-primary {
  padding: 12px 24px; border-radius: 6px; font-weight: 600; cursor: pointer;
  transition: all 0.2s; border: none; font-size: 14px; font-family: inherit; text-decoration: none;
}
.btn-secondary { background: white; border: 1px solid #d1d5db; color: #374151; }
.btn-secondary:hover { background: #f9fafb; }
.btn-primary { background: #3b82f6; color: white; }
.btn-primary:hover:not(:disabled) { background: #2563eb; }
.btn-primary:disabled { background: #9ca3af; cursor: not-allowed; }

@media (max-width: 640px) {
  .profil-card { padding: 20px; }
  .form-row { flex-direction: column; }
  .form-row .form-group.small { width: 100%; }
  .form-actions { flex-direction: column; }
  .btn-secondary, .btn-primary { width: 100%; text-align: center; }
}
</style>