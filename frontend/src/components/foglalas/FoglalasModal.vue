<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" @click="handleOverlayClick">
        <div class="modal-container" :class="{ 'is-submitting': submitting }" @click.stop>
          <!-- ═══════════════════════════════════════════════════════════════ -->
          <!-- HEADER -->
          <!-- ═══════════════════════════════════════════════════════════════ -->
          <div class="modal-header">
            <button class="btn-back" @click="handleClose" aria-label="Bezárás">
              <span class="back-icon">←</span>
              <span class="back-text">Vissza</span>
            </button>
            <h2 class="modal-title">Foglalás</h2>
            <button class="btn-close" @click="handleClose" aria-label="Bezárás">✕</button>
          </div>

          <!-- ═══════════════════════════════════════════════════════════════ -->
          <!-- ESZKÖZ INFO -->
          <!-- ═══════════════════════════════════════════════════════════════ -->
          <div class="eszkoz-info" v-if="eszkoz">
            <div class="eszkoz-image">
              <img :src="eszkozImageUrl" :alt="eszkoz.nev" @error="handleImageError" />
            </div>
            <div class="eszkoz-details">
              <h3 class="eszkoz-nev">{{ eszkoz.nev }}</h3>
              <p class="eszkoz-kategoria">{{ eszkoz.kategoriaNev }}</p>
              <div class="eszkoz-ar">
                <span class="ar-value">{{ formatAr(eszkoz.kiadasiAr) }}</span>
                <span class="ar-unit">Ft/óra</span>
              </div>
            </div>
          </div>

          <!-- ═══════════════════════════════════════════════════════════════ -->
          <!-- FORM -->
          <!-- ═══════════════════════════════════════════════════════════════ -->
          <form @submit.prevent="handleSubmit" class="foglalas-form">
            <!-- Kapcsolattartó adatok -->
            <div class="form-section">
              <h4 class="section-title">
                <span class="section-icon">👤</span>
                Kapcsolattartó adatok
              </h4>

              <div class="form-group">
                <label for="nev">Teljes név *</label>
                <input
                  id="nev"
                  v-model="form.nev"
                  type="text"
                  placeholder="Kovács János"
                  required
                  :disabled="submitting"
                />
              </div>

              <div class="form-group">
                <label for="email">Email cím *</label>
                <input
                  id="email"
                  v-model="form.email"
                  type="email"
                  placeholder="pelda@email.com"
                  required
                  :disabled="submitting"
                />
              </div>

              <div class="form-group">
                <label for="telefonszam">Telefonszám *</label>
                <input
                  id="telefonszam"
                  v-model="form.telefonszam"
                  type="tel"
                  placeholder="+36 30 123 4567"
                  required
                  :disabled="submitting"
                />
              </div>
            </div>

            <!-- Cím -->
            <div class="form-section">
              <h4 class="section-title">
                <span class="section-icon">📍</span>
                Cím
              </h4>

              <div class="form-group">
                <label for="cim">Teljes cím *</label>
                <input
                  id="cim"
                  v-model="form.cim"
                  type="text"
                  placeholder="1234 Budapest, Példa utca 12."
                  required
                  :disabled="submitting"
                />
              </div>
            </div>

            <!-- Időpont - MOBILBARÁT VERZIÓ -->
            <div class="form-section">
              <h4 class="section-title">
                <span class="section-icon">📅</span>
                Átvétel időpontja
              </h4>

              <!-- Dátum választó (nagy gombok) -->
              <div class="date-selector">
                <button
                  v-for="day in availableDays"
                  :key="day.value"
                  type="button"
                  :class="['date-btn', { active: selectedDate === day.value }]"
                  @click="selectDate(day.value)"
                  :disabled="submitting"
                >
                  <div class="date-label">{{ day.label }}</div>
                  <div class="date-value">{{ day.display }}</div>
                </button>
              </div>

              <!-- Idő választó -->
              <div class="time-selector">
                <label class="time-label">Időpont *</label>

                <div class="time-row">
                  <!-- Óra -->
                  <select
                    v-model="selectedHour"
                    class="time-select"
                    :disabled="submitting"
                    @change="updateFormTime"
                  >
                    <option v-for="h in availableHours" :key="h" :value="h">
                      {{ String(h).padStart(2, '0') }}
                    </option>
                  </select>

                  <!-- Perc gombok -->
                  <div class="minute-buttons">
                    <button
                      v-for="m in [0, 15, 30, 45]"
                      :key="m"
                      type="button"
                      :class="['minute-btn', { active: selectedMinute === m }]"
                      @click="selectMinute(m)"
                      :disabled="submitting"
                    >
                      {{ String(m).padStart(2, '0') }}
                    </button>
                  </div>
                </div>
              </div>

              <!-- Időpont előnézet -->
              <div class="time-preview">
                <span class="preview-icon">✅</span>
                <span class="preview-text">{{ timePreview }}</span>
              </div>

              <p class="form-hint">
                💡 Az eszközt a megadott időpontban kell átvenni. 15 percen belül meg kell jelenni,
                különben a foglalás törlődik.
              </p>
            </div>

            <!-- Hibaüzenet -->
            <div v-if="error" class="error-message">
              <span class="error-icon">⚠️</span>
              <span>{{ error }}</span>
            </div>

            <!-- ═══════════════════════════════════════════════════════════════ -->
            <!-- FOOTER / GOMBOK -->
            <!-- ═══════════════════════════════════════════════════════════════ -->
            <div class="modal-footer">
              <button
                type="button"
                class="btn-secondary"
                @click="handleClose"
                :disabled="submitting"
              >
                Mégse
              </button>
              <button type="submit" class="btn-primary" :disabled="submitting || !isFormValid">
                <span v-if="submitting" class="btn-loading">
                  <span class="spinner"></span>
                  Feldolgozás...
                </span>
                <span v-else> 📅 Foglalás véglegesítése </span>
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
import { BASE_URL } from '@/services/api'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false,
  },
  eszkoz: {
    type: Object,
    default: null,
  },
})

const emit = defineEmits(['close', 'success'])

const authStore = useAuthStore()
const submitting = ref(false)
const error = ref(null)

// Új state változók az időpont választóhoz
const selectedDate = ref('')
const selectedHour = ref(9)
const selectedMinute = ref(0)

// Form adatok
const form = ref({
  nev: '',
  email: '',
  telefonszam: '',
  cim: '',
  datum: '',
  ido: '',
})

// Computed
const minDate = computed(() => {
  const today = new Date()
  return today.toISOString().split('T')[0]
})

const maxDate = computed(() => {
  const today = new Date()
  const maxDay = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000)
  return maxDay.toISOString().split('T')[0]
})

// Elérhető napok (ma + 7 nap)
const availableDays = computed(() => {
  const days = []
  const today = new Date()

  for (let i = 0; i < 8; i++) {
    const date = new Date(today.getTime() + i * 24 * 60 * 60 * 1000)
    const dateStr = date.toISOString().split('T')[0]

    let label = ''
    if (i === 0) label = 'Ma'
    else if (i === 1) label = 'Holnap'
    else {
      const dayNames = ['Vasárnap', 'Hétfő', 'Kedd', 'Szerda', 'Csütörtök', 'Péntek', 'Szombat']
      label = dayNames[date.getDay()]
    }

    const display = `${date.getMonth() + 1}/${date.getDate()}`

    days.push({ value: dateStr, label, display })
  }

  return days
})

// Elérhető órák (8-20)
const availableHours = computed(() => {
  const hours = []
  for (let h = 8; h <= 20; h++) {
    hours.push(h)
  }
  return hours
})

// Időpont előnézet
const timePreview = computed(() => {
  if (!selectedDate.value) return 'Válassz dátumot'

  const date = new Date(selectedDate.value)
  const dayLabel = availableDays.value.find((d) => d.value === selectedDate.value)?.label || ''
  const monthNames = [
    'jan',
    'febr',
    'márc',
    'ápr',
    'máj',
    'jún',
    'júl',
    'aug',
    'szept',
    'okt',
    'nov',
    'dec',
  ]

  return `${dayLabel}, ${monthNames[date.getMonth()]} ${date.getDate()}. - ${String(selectedHour.value).padStart(2, '0')}:${String(selectedMinute.value).padStart(2, '0')}`
})

const eszkozImageUrl = computed(() => {
  if (props.eszkoz?.kepUrl) {
    if (props.eszkoz.kepUrl.startsWith('http')) return props.eszkoz.kepUrl
    return `${BASE_URL}${props.eszkoz.kepUrl}`
  }
  return 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80'
})

const isFormValid = computed(() => {
  return (
    form.value.nev &&
    form.value.email &&
    form.value.telefonszam &&
    form.value.cim &&
    form.value.datum &&
    form.value.ido
  )
})

// Watchers
watch(
  () => props.isOpen,
  async (newVal) => {
    if (newVal) {
      resetForm()
      await prefillFromAuth()
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  },
)

onMounted(async () => {
  if (props.isOpen) {
    await prefillFromAuth()
  }
})

// Methods
function resetForm() {
  // Alapértelmezett idő (ma + 1 óra)
  const now = new Date()
  const defaultHour = Math.min(now.getHours() + 1, 20)

  selectedDate.value = minDate.value
  selectedHour.value = defaultHour
  selectedMinute.value = 0

  updateFormTime()

  form.value = {
    nev: '',
    email: '',
    telefonszam: '',
    cim: '',
    datum: selectedDate.value,
    ido: `${String(selectedHour.value).padStart(2, '0')}:${String(selectedMinute.value).padStart(2, '0')}`,
  }
  error.value = null
}

function selectDate(dateValue) {
  selectedDate.value = dateValue
  form.value.datum = dateValue
  updateFormTime()
}

function selectMinute(minute) {
  selectedMinute.value = minute
  updateFormTime()
}

function updateFormTime() {
  form.value.datum = selectedDate.value
  form.value.ido = `${String(selectedHour.value).padStart(2, '0')}:${String(selectedMinute.value).padStart(2, '0')}`
}

function getDefaultTime() {
  const now = new Date()
  now.setHours(now.getHours() + 1)
  now.setMinutes(0)
  return `${String(now.getHours()).padStart(2, '0')}:00`
}

async function prefillFromAuth() {
  if (authStore.isAuthenticated) {
    let profile = authStore.userProfile

    // Ha a profil üres, töltsük le a szerverről
    if (profile && (!profile.nev || !profile.cim)) {
      try {
        await authStore.fetchProfile()
        profile = authStore.userProfile
      } catch (error) {
        console.error('Failed to fetch profile:', error)
      }
    }

    if (profile) {
      form.value.nev = profile.nev || ''
      form.value.email = authStore.userEmail || ''
      form.value.telefonszam = profile.telefonszam || ''
      form.value.cim = profile.cim || ''
    }
  }
}

function handleOverlayClick() {
  if (!submitting.value) {
    handleClose()
  }
}

function handleClose() {
  if (!submitting.value) {
    emit('close')
  }
}

function handleImageError(e) {
  e.target.src = 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80'
}

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return

  submitting.value = true
  error.value = null

  try {
    // Dátum és idő összeállítása
    const foglalasKezdete = new Date(`${form.value.datum}T${form.value.ido}:00`)

    const payload = {
      eszkozID: props.eszkoz.eszkozID,
      nev: form.value.nev.trim(),
      email: form.value.email.trim().toLowerCase(),
      telefonszam: form.value.telefonszam.trim(),
      cim: form.value.cim.trim(),
      foglalasKezdete: foglalasKezdete.toISOString(),
    }

    const response = await foglalasService.create(payload)

    // Sikeres foglalás
    emit('success', {
      foglalasID: response.data.foglalasID,
      eszkoz: props.eszkoz.nev,
      kezdet: foglalasKezdete,
    })

    handleClose()
  } catch (err) {
    console.error('Foglalás hiba:', err)

    if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else if (err.response?.data?.errors) {
      const errors = Object.values(err.response.data.errors).flat()
      error.value = errors.join(' ')
    } else {
      error.value = 'Hiba történt a foglalás során. Kérjük, próbálja újra!'
    }
  } finally {
    submitting.value = false
  }
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODAL OVERLAY */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
  display: flex;
  justify-content: center;
  align-items: flex-end;
  padding: 0;
}

@media (min-width: 768px) {
  .modal-overlay {
    align-items: center;
    padding: var(--spacing-lg);
    background: rgba(61, 47, 31, 0.7);
    backdrop-filter: blur(4px);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODAL CONTAINER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-container {
  background: var(--color-surface);
  width: 100%;
  max-height: 95vh;
  overflow-y: auto;
  border-radius: var(--radius-lg) var(--radius-lg) 0 0;
  display: flex;
  flex-direction: column;
  animation: slideUp 0.3s ease-out;
}

@media (min-width: 768px) {
  .modal-container {
    max-width: 600px;
    max-height: 90vh;
    border-radius: var(--radius-lg);
    animation: scaleIn 0.2s ease-out;
  }
}

.modal-container.is-submitting {
  pointer-events: none;
  opacity: 0.8;
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

/* ═══════════════════════════════════════════════════════════════════════════ */
/* HEADER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-md);
  border-bottom: 1px solid var(--color-border);
  position: sticky;
  top: 0;
  background: var(--color-surface);
  z-index: 10;
}

.btn-back {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  background: none;
  border: none;
  color: var(--color-primary);
  font-weight: 600;
  cursor: pointer;
  padding: var(--spacing-sm);
  margin: calc(var(--spacing-sm) * -1);
}

.back-icon {
  font-size: 20px;
}

.back-text {
  display: none;
}

@media (min-width: 768px) {
  .btn-back {
    display: none;
  }
}

.modal-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--color-text);
  margin: 0;
}

@media (min-width: 768px) {
  .modal-title {
    font-size: 22px;
  }
}

.btn-close {
  display: none;
  width: 36px;
  height: 36px;
  background: none;
  border: none;
  font-size: 24px;
  color: var(--color-text-muted);
  cursor: pointer;
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

@media (min-width: 768px) {
  .btn-close {
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .btn-close:hover {
    background: var(--color-background);
    color: var(--color-text);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* ESZKÖZ INFO */
/* ═══════════════════════════════════════════════════════════════════════════ */

.eszkoz-info {
  display: flex;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--color-background);
  border-bottom: 1px solid var(--color-border);
}

.eszkoz-image {
  width: 80px;
  height: 80px;
  border-radius: var(--radius-md);
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
  min-width: 0;
}

.eszkoz-nev {
  font-size: 16px;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 4px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.eszkoz-kategoria {
  font-size: 13px;
  color: var(--color-text-muted);
  margin: 0 0 var(--spacing-sm) 0;
  text-transform: capitalize;
}

.eszkoz-ar {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.ar-value {
  font-size: 20px;
  font-weight: 700;
  color: var(--color-primary);
}

.ar-unit {
  font-size: 13px;
  color: var(--color-text-muted);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* FORM */
/* ═══════════════════════════════════════════════════════════════════════════ */

.foglalas-form {
  padding: var(--spacing-md);
  flex: 1;
  overflow-y: auto;
}

@media (min-width: 768px) {
  .foglalas-form {
    padding: var(--spacing-lg);
  }
}

.form-section {
  margin-bottom: var(--spacing-lg);
}

.section-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 var(--spacing-md) 0;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.section-icon {
  font-size: 16px;
}

.form-group {
  margin-bottom: var(--spacing-md);
}

.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text);
  margin-bottom: var(--spacing-xs);
}

.form-group input {
  width: 100%;
  padding: 12px var(--spacing-md);
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: 16px; /* Prevent zoom on iOS */
  font-family: inherit;
  background: var(--color-surface);
  color: var(--color-text);
  transition: border-color var(--transition-fast);
}

.form-group input:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-group input:disabled {
  background: var(--color-background);
  color: var(--color-text-muted);
}

.form-group input::placeholder {
  color: #9ca3af;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--spacing-md);
}

.form-hint {
  font-size: 13px;
  color: var(--color-text-muted);
  margin: var(--spacing-sm) 0 0 0;
  padding: var(--spacing-sm) var(--spacing-md);
  background: #fef3c7;
  border-radius: var(--radius-sm);
  line-height: 1.4;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MOBILBARÁT IDŐPONT VÁLASZTÓ */
/* ═══════════════════════════════════════════════════════════════════════════ */

/* Dátum választó */
/* ═══════════════════════════════════════════════════════════════════════════ */
/* MOBILBARÁT IDŐPONT VÁLASZTÓ */
/* ═══════════════════════════════════════════════════════════════════════════ */

/* Dátum választó - JAVÍTOTT */
.time-row {
  display: flex;
  gap: 12px;
  align-items: center;
}

.date-selector {
  display: grid;
  grid-template-columns: repeat(4, 1fr); /* Fix 4 oszlop */
  gap: 10px;
  margin-bottom: var(--spacing-md);
}

/* Kis mobilon 3 oszlop */
@media (max-width: 400px) {
  .date-selector {
    grid-template-columns: repeat(3, 1fr);
  }
}

.date-btn {
  padding: 12px 8px;
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all var(--transition-fast);
  text-align: center;
  min-height: 72px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 6px;
}

.date-btn:hover:not(:disabled) {
  border-color: var(--color-primary);
  background: #f0f4f0;
}

.date-btn.active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
  font-weight: 700;
  box-shadow: 0 4px 12px rgba(107, 142, 35, 0.3);
}

.date-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.date-label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  line-height: 1.2;
  /* Tördelés engedélyezése */
  word-wrap: break-word;
  overflow-wrap: break-word;
}

.date-value {
  font-size: 15px;
  font-weight: 700;
  white-space: nowrap;
}

/* Idő választó - JAVÍTOTT */
.time-selector {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-md);
}

/* Mobilon egy sorban ha elfér */
@media (min-width: 480px) {
  .time-selector {
    flex-direction: row;
  }
}

.time-group {
  flex: 1;
}

.time-label {
  display: block;
  font-size: 14px;
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: var(--spacing-xs);
}

/* ÓRA VÁLASZTÓ - Egyesített megjelenítés */
.time-select {
  width: 80px; /* ugyanaz, mint a perc gomb */
  height: 56px; /* egyezzen meg a perc gomb magasságával */

  display: flex;
  align-items: center;
  justify-content: center; /* szám TÉNYLEG középen */

  padding: 0;
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);

  font-size: 24px;
  font-weight: 600;
  font-family: inherit;

  background-color: var(--color-surface);
  color: var(--color-text);

  cursor: pointer;
  appearance: none;

  /* dropdown nyíl */
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12'%3E%3Cpath fill='%23666' d='M6 9L1 4h10z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 8px center;
}

.time-select:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(107, 142, 35, 0.1);
}

.time-select:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Option elemek középre (Firefox) */
.time-select option {
  text-align: center;
}

/* Perc gombok */
.minute-buttons {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
}

.minute-btn {
  padding: 16px 12px;
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: 20px;
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition-fast);
  font-family: inherit;
  min-height: 52px;
}

.minute-btn:hover:not(:disabled) {
  border-color: var(--color-primary);
  background: #f0f4f0;
}

.minute-btn.active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
  box-shadow: 0 4px 12px rgba(107, 142, 35, 0.3);
}

.minute-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Időpont előnézet */
.time-preview {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: #e8f5e9;
  border: 2px solid #a5d6a7;
  border-radius: var(--radius-md);
  margin-bottom: var(--spacing-md);
}

.preview-icon {
  font-size: 20px;
  flex-shrink: 0;
}

.preview-text {
  font-size: 15px;
  font-weight: 600;
  color: #1b5e20;
  flex: 1;
}

@media (max-width: 480px) {
  .date-selector {
    gap: 8px;
  }

  .time-selector {
    gap: 16px;
  }

  .minute-buttons {
    gap: 8px;
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* ERROR MESSAGE */
/* ═══════════════════════════════════════════════════════════════════════════ */

.error-message {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: var(--radius-md);
  color: #dc2626;
  font-size: 14px;
  margin-bottom: var(--spacing-md);
}

.error-icon {
  flex-shrink: 0;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* FOOTER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.modal-footer {
  display: flex;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border-top: 1px solid var(--color-border);
  background: var(--color-surface);
  position: sticky;
  bottom: 0;
}

@media (min-width: 768px) {
  .modal-footer {
    padding: var(--spacing-md) var(--spacing-lg);
    justify-content: flex-end;
  }
}

.btn-secondary,
.btn-primary {
  flex: 1;
  padding: 14px var(--spacing-lg);
  border-radius: var(--radius-md);
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  border: none;
}

@media (min-width: 768px) {
  .btn-secondary,
  .btn-primary {
    flex: none;
    min-width: 140px;
  }
}

.btn-secondary {
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  color: var(--color-text);
}

.btn-secondary:hover:not(:disabled) {
  background: var(--color-background);
}

.btn-primary {
  background: var(--color-primary);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: var(--color-primary-dark);
}

.btn-primary:disabled,
.btn-secondary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* MODAL TRANSITIONS */
/* ═══════════════════════════════════════════════════════════════════════════ */

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

.modal-enter-from .modal-container {
  transform: translateY(100%);
}

.modal-leave-to .modal-container {
  transform: translateY(100%);
}

@media (min-width: 768px) {
  .modal-enter-from .modal-container {
    transform: scale(0.95);
  }

  .modal-leave-to .modal-container {
    transform: scale(0.95);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* iOS SAFE AREA */
/* ═══════════════════════════════════════════════════════════════════════════ */

@supports (padding-bottom: env(safe-area-inset-bottom)) {
  .modal-footer {
    padding-bottom: calc(var(--spacing-md) + env(safe-area-inset-bottom));
  }
}
</style>
