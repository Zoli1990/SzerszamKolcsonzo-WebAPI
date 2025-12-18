<template>
  <div class="profilom-container">
    <div class="profilom-header">
      <h1>👤 Profilom</h1>
      <p class="user-email">{{ authStore.userEmail }}</p>
    </div>

    <div v-if="loading" class="loading">Betöltés...</div>

    <div v-else class="profil-card">
      <form @submit.prevent="handleSubmit" class="profil-form">
        <div class="form-group">
          <label>Email cím</label>
          <input
            type="email"
            :value="authStore.userEmail"
            disabled
            class="disabled-input"
          />
          <small class="hint">Az email cím nem módosítható</small>
        </div>

        <!-- ✅ NÉV MEZŐ -->
        <div class="form-group">
          <label>Név *</label>
          <input
            type="text"
            v-model="form.nev"
            placeholder="Kovács János"
            required
          />
        </div>

        <div class="section-title">Cím adatok</div>

        <div class="form-row">
          <div class="form-group small">
            <label>Irányítószám *</label>
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
            <label>Település</label>
            <input
              type="text"
              v-model="form.telepules"
              placeholder="Automatikusan kitöltődik..."
              :class="{ 'auto-filled': telepulesAutoFilled }"
              readonly
            />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group flex-grow">
            <label>Utca *</label>
            <input
              type="text"
              v-model="form.utca"
              placeholder="Kossuth Lajos utca"
              required
            />
          </div>

          <div class="form-group small">
            <label>Házszám *</label>
            <input
              type="text"
              v-model="form.hazszam"
              placeholder="12/A"
              required
            />
          </div>
        </div>

        <div class="form-group">
          <label>Telefonszám</label>
          <input
            type="tel"
            v-model="form.telefonszam"
            placeholder="+36301234567"
          />
        </div>

        <!-- Teljes cím előnézet -->
        <div v-if="teljesCim" class="cim-preview">
          <span class="preview-label">📍 Teljes cím:</span>
          <span class="preview-value">{{ teljesCim }}</span>
        </div>

        <!-- Hibaüzenet -->
        <div v-if="error" class="error-message">
          {{ error }}
        </div>

        <!-- Sikeres mentés -->
        <div v-if="successMessage" class="success-message">
          {{ successMessage }}
        </div>

        <!-- Gombok -->
        <div class="form-actions">
          <RouterLink to="/" class="btn-secondary">Vissza</RouterLink>
          <button type="submit" class="btn-primary" :disabled="submitting">
            {{ submitting ? 'Mentés...' : '💾 Mentés' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { iranyitoszamService } from '@/services/iranyitoszamService'

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

// Teljes cím előnézet
const teljesCim = computed(() => {
  if (!form.value.iranyitoszam || !form.value.utca || !form.value.hazszam) {
    return ''
  }
  const telepules = form.value.telepules ? form.value.telepules + ', ' : ''
  return `${form.value.iranyitoszam} ${telepules}${form.value.utca} ${form.value.hazszam}`.trim()
})

onMounted(async () => {
  loading.value = true
  try {
    // Ha van token, frissítsük a profilt a szerverről
    if (authStore.token) {
      await authStore.fetchProfile()
    }

    // Form feltöltése az aktuális adatokkal
    loadFormFromProfile()
  } catch (err) {
    console.error('Profil betöltése sikertelen:', err)
    error.value = 'Profil betöltése sikertelen'
  } finally {
    loading.value = false
  }
})

// Form adatok betöltése a profilból
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
    
    if (form.value.telepules) {
      telepulesAutoFilled.value = true
    }
  }
}

// Irányítószám alapján település keresése
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

    successMessage.value = 'Profil sikeresen mentve!'

    setTimeout(() => {
      successMessage.value = ''
    }, 3000)
  } catch (err) {
    console.error('Profil mentési hiba:', err)
    
    if (err.response?.data?.errors) {
      const errors = Object.values(err.response.data.errors).flat()
      error.value = errors.join(' ')
    } else {
      error.value = err.response?.data?.message || 'Hiba történt a mentés során.'
    }
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.profilom-container {
  max-width: 600px;
  margin: 0 auto;
  padding: 20px;
}

.profilom-header {
  margin-bottom: 32px;
}

.profilom-header h1 {
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

.profil-card {
  background: white;
  border-radius: 12px;
  padding: 32px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.profil-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
  margin: 8px 0 0 0;
  padding-bottom: 8px;
  border-bottom: 2px solid #e5e7eb;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  margin-bottom: 8px;
  font-weight: 600;
  color: #374151;
  font-size: 14px;
}

.form-group input {
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
  font-family: inherit;
}

.form-group input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-group input.disabled-input {
  background: #f3f4f6;
  color: #6b7280;
  cursor: not-allowed;
}

.form-group input.auto-filled {
  background: #f0fdf4;
  border-color: #86efac;
}

.form-group input[readonly] {
  background: #f9fafb;
  cursor: not-allowed;
}

.form-row {
  display: flex;
  gap: 12px;
}

.form-row .form-group.small {
  width: 120px;
  flex-shrink: 0;
}

.form-row .form-group.flex-grow {
  flex: 1;
}

.hint {
  margin-top: 6px;
  font-size: 12px;
  color: #9ca3af;
}

.cim-preview {
  padding: 12px 16px;
  background: #f0f9ff;
  border: 1px solid #bae6fd;
  border-radius: 6px;
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}

.preview-label {
  font-size: 14px;
  color: #0369a1;
}

.preview-value {
  font-size: 14px;
  font-weight: 600;
  color: #0c4a6e;
}

.error-message {
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  color: #dc2626;
  font-size: 14px;
}

.success-message {
  padding: 12px;
  background: #d1fae5;
  border: 1px solid #a7f3d0;
  border-radius: 6px;
  color: #065f46;
  font-size: 14px;
}

.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
}

.btn-secondary,
.btn-primary {
  padding: 12px 24px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  font-size: 14px;
  font-family: inherit;
  text-decoration: none;
}

.btn-secondary {
  background: white;
  border: 1px solid #d1d5db;
  color: #374151;
}

.btn-secondary:hover {
  background: #f9fafb;
}

.btn-primary {
  background: #3b82f6;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-primary:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

/* Responsive */
@media (max-width: 640px) {
  .profil-card {
    padding: 20px;
  }

  .form-row {
    flex-direction: column;
  }

  .form-row .form-group.small {
    width: 100%;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-secondary,
  .btn-primary {
    width: 100%;
    text-align: center;
  }
}
</style>