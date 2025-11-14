// ============================================================================
// 1. src/components/foglalas/FoglalasModal.vue
// ============================================================================

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-container" @click.stop>
          <!-- Header -->
          <div class="modal-header">
            <h2>Eszköz foglalása</h2>
            <button class="btn-close" @click="closeModal">&times;</button>
          </div>

          <!-- Eszköz info -->
          <div class="eszkoz-info">
            <img :src="eszkoz.kepUrl || defaultImage" :alt="eszkoz.nev" />
            <div class="eszkoz-details">
              <h3>{{ eszkoz.nev }}</h3>
              <p class="kategoria">{{ eszkoz.kategoriaNev }}</p>
              <p class="ar">{{ formatAr(eszkoz.kiadasiAr) }} Ft/óra</p>
            </div>
          </div>

          <!-- Form -->
          <form @submit.prevent="handleSubmit" class="foglalas-form">
            <!-- Dátumok -->
            <div class="form-row">
              <div class="form-group">
                <label>Foglalás kezdete *</label>
                <input
                  type="datetime-local"
                  v-model="form.foglalasKezdete"
                  required
                  :min="minDate"
                />
              </div>

              <div class="form-group">
                <label>Foglalás vége *</label>
                <input
                  type="datetime-local"
                  v-model="form.foglalasVege"
                  required
                  :min="form.foglalasKezdete"
                />
              </div>
            </div>

            <!-- Óraszám (automatikusan számolt) -->
            <div class="form-group">
              <label>Órák száma</label>
              <input
                type="number"
                v-model="oraSzam"
                readonly
                class="readonly-input"
              />
              <small v-if="oraSzam > 0">
                Becsült költség: <strong>{{ formatAr(becsultKoltseg) }} Ft</strong>
              </small>
            </div>

            <!-- Ügyfél adatok -->
            <div class="section-title">Ügyfél adatok</div>

            <div class="form-group">
              <label>Név *</label>
              <input
                type="text"
                v-model="form.nev"
                placeholder="Kovács János"
                required
              />
            </div>

            <div class="form-group">
              <label>Email cím *</label>
              <input
                type="email"
                v-model="form.email"
                placeholder="kovacs.janos@example.com"
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
                placeholder="Budapest, Fő utca 1."
                required
              />
            </div>

            <!-- Hibaüzenet -->
            <div v-if="error" class="error-message">
              {{ error }}
            </div>

            <!-- Gombok -->
            <div class="modal-footer">
              <button type="button" class="btn-secondary" @click="closeModal">
                Mégse
              </button>
              <button type="submit" class="btn-primary" :disabled="loading || !isValid">
                {{ loading ? 'Foglalás...' : 'Foglalás leadása' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { foglalasService } from '@/services/foglalasService'
import { useEszkozStore } from '@/stores/eszkozStore'

const props = defineProps({
  isOpen: Boolean,
  eszkoz: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['close', 'success'])

const eszkozStore = useEszkozStore()
const loading = ref(false)
const error = ref(null)

// Form adatok
const form = ref({
  nev: '',
  email: '',
  telefonszam: '',
  cim: '',
  foglalasKezdete: '',
  foglalasVege: ''
})

// Minimális dátum (ma)
const minDate = computed(() => {
  const now = new Date()
  return now.toISOString().slice(0, 16)
})

// Óraszám számítás
const oraSzam = computed(() => {
  if (!form.value.foglalasKezdete || !form.value.foglalasVege) return 0
  
  const start = new Date(form.value.foglalasKezdete)
  const end = new Date(form.value.foglalasVege)
  const diff = end - start
  
  if (diff <= 0) return 0
  
  return Math.ceil(diff / (1000 * 60 * 60)) // millisec → óra
})

// Becsült költség
const becsultKoltseg = computed(() => {
  return oraSzam.value * props.eszkoz.kiadasiAr
})

// Validáció
const isValid = computed(() => {
  return (
    form.value.nev &&
    form.value.email &&
    form.value.telefonszam &&
    form.value.cim &&
    form.value.foglalasKezdete &&
    form.value.foglalasVege &&
    oraSzam.value > 0
  )
})

const defaultImage = 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80'

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function closeModal() {
  emit('close')
  resetForm()
}

function resetForm() {
  form.value = {
    nev: '',
    email: '',
    telefonszam: '',
    cim: '',
    foglalasKezdete: '',
    foglalasVege: ''
  }
  error.value = null
}

async function handleSubmit() {
  loading.value = true
  error.value = null

  try {
    const foglalasData = {
      eszkozID: props.eszkoz.eszkozID,
      nev: form.value.nev,
      email: form.value.email,
      telefonszam: form.value.telefonszam,
      cim: form.value.cim,
      foglalasKezdete: new Date(form.value.foglalasKezdete).toISOString(),
      foglalasVege: new Date(form.value.foglalasVege).toISOString(),
      oraSzam: oraSzam.value
    }

    await foglalasService.create(foglalasData)

    // Eszközök újratöltése (státusz frissült)
    await eszkozStore.fetchEszkozok()

    emit('success', {
      eszkoz: props.eszkoz.nev,
      oraSzam: oraSzam.value,
      koltseg: becsultKoltseg.value
    })

    closeModal()
  } catch (err) {
    console.error('Foglalás hiba:', err)
    error.value = err.response?.data?.message || 'Hiba történt a foglalás során.'
  } finally {
    loading.value = false
  }
}

// Reset form amikor modal megnyílik
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    resetForm()
  }
})
</script>

<style scoped>
/* Modal overlay */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 20px;
  overflow-y: auto;
}

.modal-container {
  background: white;
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

/* Header */
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h2 {
  margin: 0;
  font-size: 24px;
  color: #1f2937;
}

.btn-close {
  background: none;
  border: none;
  font-size: 32px;
  color: #6b7280;
  cursor: pointer;
  line-height: 1;
  transition: color 0.2s;
}

.btn-close:hover {
  color: #1f2937;
}

/* Eszköz info */
.eszkoz-info {
  display: flex;
  gap: 16px;
  padding: 20px 24px;
  background: #f9fafb;
}

.eszkoz-info img {
  width: 100px;
  height: 100px;
  object-fit: cover;
  border-radius: 8px;
}

.eszkoz-details h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
  color: #1f2937;
}

.eszkoz-details .kategoria {
  margin: 0 0 8px 0;
  font-size: 14px;
  color: #6b7280;
}

.eszkoz-details .ar {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: #059669;
}

/* Form */
.foglalas-form {
  padding: 24px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #374151;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.readonly-input {
  background: #f3f4f6;
  cursor: not-allowed;
}

.form-group small {
  display: block;
  margin-top: 8px;
  color: #6b7280;
  font-size: 13px;
}

.section-title {
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
  margin: 24px 0 16px 0;
  padding-bottom: 8px;
  border-bottom: 2px solid #e5e7eb;
}

.error-message {
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  color: #dc2626;
  font-size: 14px;
  margin-bottom: 20px;
}

/* Footer */
.modal-footer {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
}

.btn-secondary,
.btn-primary {
  padding: 10px 20px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  font-size: 14px;
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

/* Transitions */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-active .modal-container,
.modal-leave-active .modal-container {
  transition: transform 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.95);
}

/* Responsive */
@media (max-width: 640px) {
  .form-row {
    grid-template-columns: 1fr;
  }

  .eszkoz-info {
    flex-direction: column;
  }

  .eszkoz-info img {
    width: 100%;
    height: 200px;
  }
}
</style>
