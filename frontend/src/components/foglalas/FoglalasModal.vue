<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-container" @click.stop>
          <!-- Header -->
          <div class="modal-header">
            <h2>📋 Foglalás</h2>
            <button class="btn-close" @click="closeModal">&times;</button>
          </div>

          <!-- Eszköz info -->
          <div class="eszkoz-info">
            <div class="eszkoz-image" v-if="eszkoz?.kepUrl">
              <img :src="eszkoz.kepUrl" :alt="eszkoz.nev" />
            </div>
            <div class="eszkoz-details">
              <h3>{{ eszkoz?.nev }}</h3>
              <p class="kategoria">{{ eszkoz?.kategoriaNev }}</p>
              <p class="ar">💰 {{ formatAr(eszkoz?.kiadasiAr) }} Ft/óra</p>
            </div>
          </div>

          <!-- Form -->
          <form @submit.prevent="handleSubmit" class="foglalas-form">
            <div class="form-group">
              <label>Név *</label>
              <input 
                type="text" 
                v-model="form.nev" 
                placeholder="Teljes név" 
                required 
              />
            </div>

            <div class="form-group">
              <label>Email *</label>
              <input 
                type="email" 
                v-model="form.email" 
                placeholder="email@example.com" 
                required 
              />
            </div>

            <div class="form-group">
              <label>Telefonszám *</label>
              <input 
                type="tel" 
                v-model="form.telefonszam" 
                placeholder="+36301234567" 
                required 
              />
            </div>

            <div class="form-group">
              <label>Cím *</label>
              <input 
                type="text" 
                v-model="form.cim" 
                placeholder="Város, utca, házszám" 
                required 
              />
            </div>

            <div class="form-group">
              <label>Foglalás kezdete *</label>
              <input 
                type="datetime-local" 
                v-model="form.foglalasKezdete" 
                :min="minDateTime"
                required 
              />
              <small class="hint">
                ⏰ A foglalás kezdetétől 15 percen belül meg kell jelenni!
              </small>
            </div>

            <!-- Info box -->
            <div class="info-box">
              <p><strong>ℹ️ Fontos tudnivalók:</strong></p>
              <ul>
                <li>A foglalás kezdetétől <strong>15 percen belül</strong> meg kell jelenni</li>
                <li>Ha nem jelensz meg, a foglalás automatikusan törlődik</li>
                <li>A fizetendő összeg a tényleges használati idő alapján számolódik</li>
                <li>Óradíj: <strong>{{ formatAr(eszkoz?.kiadasiAr) }} Ft/óra</strong></li>
              </ul>
            </div>

            <!-- Hibaüzenet -->
            <div v-if="error" class="error-message">
              {{ error }}
            </div>

            <!-- Sikeres foglalás -->
            <div v-if="successMessage" class="success-message">
              {{ successMessage }}
            </div>

            <!-- Gombok -->
            <div class="form-actions">
              <button type="button" class="btn-secondary" @click="closeModal">
                Mégse
              </button>
              <button type="submit" class="btn-primary" :disabled="loading">
                {{ loading ? 'Foglalás...' : '✅ Foglalás leadása' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { foglalasService } from '@/services/foglalasService'

const props = defineProps({
  isOpen: Boolean,
  eszkoz: Object
})

const emit = defineEmits(['close', 'success'])

const authStore = useAuthStore()
const loading = ref(false)
const error = ref(null)
const successMessage = ref('')

const form = ref({
  nev: '',
  email: '',
  telefonszam: '',
  cim: '',
  foglalasKezdete: ''
})

// Minimum dátum = most + 1 óra
const minDateTime = computed(() => {
  const now = new Date()
  now.setHours(now.getHours() + 1)
  now.setMinutes(0, 0, 0)
  return now.toISOString().slice(0, 16)
})

// Ha be van jelentkezve, előtöltjük az adatokat
onMounted(() => {
  if (authStore.isAuthenticated && authStore.userProfile) {
    form.value.email = authStore.userEmail || ''
    form.value.nev = authStore.userProfile.nev || ''
    form.value.telefonszam = authStore.userProfile.telefonszam || ''
    form.value.cim = authStore.userProfile.cim || ''
  }
})

// Reset form amikor modal megnyílik
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      error.value = null
      successMessage.value = ''
      
      // Alapértelmezett kezdő időpont: holnap 10:00
      const tomorrow = new Date()
      tomorrow.setDate(tomorrow.getDate() + 1)
      tomorrow.setHours(10, 0, 0, 0)
      form.value.foglalasKezdete = tomorrow.toISOString().slice(0, 16)
      
      // User adatok előtöltése ha be van jelentkezve
      if (authStore.isAuthenticated && authStore.userProfile) {
        form.value.email = authStore.userEmail || ''
        form.value.nev = authStore.userProfile.nev || ''
        form.value.telefonszam = authStore.userProfile.telefonszam || ''
        form.value.cim = authStore.userProfile.cim || ''
      }
    }
  }
)

function closeModal() {
  emit('close')
}

async function handleSubmit() {
  loading.value = true
  error.value = null
  successMessage.value = ''

  try {
    // Validáció
    if (!form.value.nev || !form.value.email || !form.value.telefonszam || !form.value.cim) {
      error.value = 'Kérjük, töltsd ki az összes mezőt!'
      return
    }

    // API hívás - új séma szerint (nincs vége időpont, nincs óraszám)
    const payload = {
      eszkozID: props.eszkoz.eszkozID,
      nev: form.value.nev.trim(),
      email: form.value.email.trim().toLowerCase(),
      telefonszam: form.value.telefonszam.trim(),
      cim: form.value.cim.trim(),
      foglalasKezdete: new Date(form.value.foglalasKezdete).toISOString()
    }

    const response = await foglalasService.create(payload)

    successMessage.value = '✅ Foglalás sikeresen leadva!'

    // Emit success esemény
    emit('success', {
      eszkoz: props.eszkoz.nev,
      kezdete: form.value.foglalasKezdete,
      foglalasId: response.data.foglalasID
    })

    // 2 másodperc múlva bezárjuk
    setTimeout(() => {
      closeModal()
    }, 2000)

  } catch (err) {
    console.error('Foglalás hiba:', err)
    
    if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else if (err.response?.data?.errors) {
      const errors = Object.values(err.response.data.errors).flat()
      error.value = errors.join(' ')
    } else {
      error.value = 'Hiba történt a foglalás során. Kérjük, próbáld újra!'
    }
  } finally {
    loading.value = false
  }
}

function formatAr(ar) {
  if (!ar) return '0'
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
  padding: 20px;
  overflow-y: auto;
}

.modal-container {
  background: white;
  border-radius: 16px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 2px solid #e8dcc8;
  position: sticky;
  top: 0;
  background: white;
  z-index: 10;
}

.modal-header h2 {
  margin: 0;
  font-size: 24px;
  color: #3d2f1f;
}

.btn-close {
  background: none;
  border: none;
  font-size: 32px;
  color: #6b7280;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  transition: background 0.2s;
}

.btn-close:hover {
  background: #f3f4f6;
}

.eszkoz-info {
  display: flex;
  gap: 16px;
  padding: 20px 24px;
  background: #f5f1e8;
  border-bottom: 1px solid #e8dcc8;
}

.eszkoz-image {
  width: 100px;
  height: 100px;
  border-radius: 12px;
  overflow: hidden;
  flex-shrink: 0;
}

.eszkoz-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.eszkoz-details {
  flex: 1;
}

.eszkoz-details h3 {
  margin: 0 0 8px 0;
  font-size: 20px;
  color: #3d2f1f;
}

.eszkoz-details .kategoria {
  margin: 0 0 8px 0;
  font-size: 14px;
  color: #6b5d4f;
  text-transform: capitalize;
}

.eszkoz-details .ar {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #6b8e23;
}

.foglalas-form {
  padding: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #3d2f1f;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 12px;
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  font-size: 14px;
  font-family: inherit;
  background: #fefdfb;
  transition: border-color 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #6b8e23;
}

.hint {
  display: block;
  margin-top: 6px;
  font-size: 12px;
  color: #6b5d4f;
}

.info-box {
  padding: 16px;
  background: #f0f9ff;
  border: 2px solid #bae6fd;
  border-radius: 12px;
  margin-bottom: 20px;
}

.info-box p {
  margin: 0 0 12px 0;
  color: #0369a1;
}

.info-box ul {
  margin: 0;
  padding-left: 20px;
}

.info-box li {
  margin: 6px 0;
  font-size: 14px;
  color: #0c4a6e;
}

.error-message {
  padding: 12px 16px;
  background: #fef2f2;
  border: 2px solid #fecaca;
  border-radius: 8px;
  color: #dc2626;
  font-size: 14px;
  margin-bottom: 20px;
}

.success-message {
  padding: 12px 16px;
  background: #d1fae5;
  border: 2px solid #a7f3d0;
  border-radius: 8px;
  color: #065f46;
  font-size: 14px;
  margin-bottom: 20px;
  font-weight: 600;
}

.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 2px solid #e8dcc8;
}

.btn-secondary,
.btn-primary {
  padding: 12px 24px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  font-size: 14px;
  font-family: inherit;
}

.btn-secondary {
  background: white;
  border: 2px solid #d4c5b0;
  color: #3d2f1f;
}

.btn-secondary:hover {
  background: #f5f1e8;
}

.btn-primary {
  background: #6b8e23;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #556b1a;
  transform: translateY(-1px);
}

.btn-primary:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

/* Modal animáció */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .modal-container,
.modal-leave-active .modal-container {
  transition: transform 0.3s ease;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: translateY(20px);
}

/* Responsive */
@media (max-width: 640px) {
  .modal-container {
    margin: 10px;
    max-height: calc(100vh - 20px);
  }

  .eszkoz-info {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-secondary,
  .btn-primary {
    width: 100%;
  }
}
</style>