<template>
  <div class="home">
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- HERO SZEKCIÓ - Reszponzív -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <header class="hero">
      <div class="hero-content">
        <h1 class="hero-title">
          <span class="hero-icon">🔧</span>
          <span>Szerszámkölcsönző</span>
        </h1>
        <p class="hero-subtitle">Béreljen minőségi szerszámokat óradíjas rendszerben!</p>
        
        <!-- Gyors statisztikák mobilon -->
        <div class="hero-stats">
          <div class="hero-stat">
            <span class="stat-number">{{ eszkozStore.elerhetoEszkozok.length }}</span>
            <span class="stat-label">Elérhető</span>
          </div>
          <div class="hero-stat">
            <span class="stat-number">{{ kategoriak.length }}</span>
            <span class="stat-label">Kategória</span>
          </div>
        </div>
      </div>
    </header>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- KATEGÓRIA SZŰRŐ - Horizontálisan görgethető mobilon -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="filter-wrapper">
      <div class="filter-section" ref="filterSection">
        <button
          class="filter-btn"
          :class="{ active: !eszkozStore.selectedKategoriaId }"
          @click="eszkozStore.clearFilter()"
        >
          <span class="filter-icon">📦</span>
          <span class="filter-text">Összes</span>
        </button>
        
        <button
          v-for="kategoria in kategoriak"
          :key="kategoria.kategoriaID"
          class="filter-btn"
          :class="{ active: eszkozStore.selectedKategoriaId === kategoria.kategoriaID }"
          @click="eszkozStore.setKategoriaFilter(kategoria.kategoriaID)"
        >
          <span class="filter-icon">{{ getCategoryIcon(kategoria.nev) }}</span>
          <span class="filter-text">{{ kategoria.nev }}</span>
        </button>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- TARTALOM -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <div class="content-section">
      <!-- Loading state -->
      <div v-if="eszkozStore.loading" class="state-container">
        <div class="loading-spinner"></div>
        <p>Eszközök betöltése...</p>
      </div>

      <!-- Error state -->
      <div v-else-if="eszkozStore.error" class="state-container error">
        <span class="state-icon">⚠️</span>
        <p>Hiba történt: {{ eszkozStore.error }}</p>
        <button class="btn-retry" @click="eszkozStore.fetchEszkozok()">
          🔄 Újrapróbálás
        </button>
      </div>

      <!-- Üres állapot -->
      <div v-else-if="eszkozStore.filteredEszkozok.length === 0" class="state-container empty">
        <span class="state-icon">🔍</span>
        <p>Nincs megjeleníthető eszköz ebben a kategóriában.</p>
        <button class="btn-clear-filter" @click="eszkozStore.clearFilter()">
          Összes eszköz mutatása
        </button>
      </div>

      

      <!-- Eszközök grid -->
      <div v-else class="eszkoz-grid">
        <EszkozCard
          v-for="eszkoz in eszkozStore.filteredEszkozok"
          :key="eszkoz.eszkozID"
          :eszkoz="eszkoz"
          @foglalas="openFoglalasModal"
        />
      </div>

      <!-- Eredmények száma -->
      <div v-if="!eszkozStore.loading && eszkozStore.filteredEszkozok.length > 0" class="results-count">
        {{ eszkozStore.filteredEszkozok.length }} eszköz találva
        <span v-if="eszkozStore.selectedKategoriaId">
          ({{ getSelectedKategoriaNev }})
        </span>
      </div>
    </div>

    <!-- ✅ ÚJ: Vélemények szekció -->
          <VelemenySzekcio id="velemenyek" />

    <!-- ✅ ÚJ: Kapcsolat szekció -->
          <KapcsolatSection id="kapcsolat" />

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- FOGLALÁS MODAL -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <FoglalasModal
      v-if="selectedEszkoz"
      :is-open="modalOpen"
      :eszkoz="selectedEszkoz"
      @close="closeModal"
      @success="handleFoglalasSuccess"
    />

    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <!-- SUCCESS TOAST -->
    <!-- ═══════════════════════════════════════════════════════════════════ -->
    <Transition name="toast">
      <div v-if="successMessage" class="success-toast">
        <span class="toast-icon">✅</span>
        <span class="toast-message">{{ successMessage }}</span>
        <button class="toast-close" @click="successMessage = ''">✕</button>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue'
import { useEszkozStore } from '@/stores/eszkozStore'
import { kategoriaService } from '@/services/kategoriaService'
import EszkozCard from '@/components/eszkozok/EszkozCard.vue'
import FoglalasModal from '@/components/foglalas/FoglalasModal.vue'
import VelemenySzekcio from '@/components/VelemenySzekcio.vue'
import KapcsolatSection from '@/components/KapcsolatSection.vue'

const eszkozStore = useEszkozStore()
const kategoriak = ref([])
const modalOpen = ref(false)
const selectedEszkoz = ref(null)
const successMessage = ref('')
const filterSection = ref(null)

// Computed
const getSelectedKategoriaNev = computed(() => {
  const kat = kategoriak.value.find(k => k.kategoriaID === eszkozStore.selectedKategoriaId)
  return kat ? kat.nev : ''
})

// Lifecycle
onMounted(async () => {
  await eszkozStore.fetchEszkozok()

  try {
    const response = await kategoriaService.getAll()
    kategoriak.value = response.data
  } catch (error) {
    console.error('Kategóriák betöltése sikertelen:', error)
  }
})

// Methods
function getCategoryIcon(categoryName) {
  const iconMap = {
    'kézi szerszámok': '🔨',
    'mérőműszerek': '📏',
    'kerti eszközök': '🌿',
    'elektromos': '⚡',
    'víz-gáz': '🔧',
    'festő': '🎨',
    'barkács': '🪚',
  }
  
  const lowerName = categoryName.toLowerCase()
  for (const [key, icon] of Object.entries(iconMap)) {
    if (lowerName.includes(key)) return icon
  }
  return '🔧'
}

function openFoglalasModal(eszkoz) {
  selectedEszkoz.value = eszkoz
  modalOpen.value = true
}

function closeModal() {
  modalOpen.value = false
  setTimeout(() => {
    selectedEszkoz.value = null
  }, 300)
}

function handleFoglalasSuccess(data) {
  successMessage.value = `Sikeres foglalás! ${data.eszkoz}`

  setTimeout(() => {
    successMessage.value = ''
  }, 5000)
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* CONTAINER */
/* ═══════════════════════════════════════════════════════════════════════════ */

.home {
  min-height: 100%;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* HERO SZEKCIÓ */
/* ═══════════════════════════════════════════════════════════════════════════ */

.hero {
  background: linear-gradient(135deg, #3d2f1f 0%, #6b8e23 25%, #6b8e23 75%, #3d2f1f 100%);
  color: white;
  padding: var(--spacing-lg) var(--spacing-md);
  text-align: center;
}

@media (min-width: 768px) {
  .hero {
    padding: var(--spacing-xl) var(--spacing-lg);
    border-radius: 0 0 var(--radius-lg) var(--radius-lg);
  }
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero-title {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 var(--spacing-sm) 0;
}

@media (min-width: 480px) {
  .hero-title {
    font-size: 32px;
  }
}

@media (min-width: 768px) {
  .hero-title {
    font-size: 42px;
    margin-bottom: var(--spacing-md);
  }
}

.hero-icon {
  font-size: 1.2em;
}

.hero-subtitle {
  font-size: 14px;
  margin: 0;
  opacity: 0.9;
}

@media (min-width: 768px) {
  .hero-subtitle {
    font-size: 18px;
  }
}

/* Hero statisztikák */
.hero-stats {
  display: flex;
  justify-content: center;
  gap: var(--spacing-xl);
  margin-top: var(--spacing-lg);
  padding-top: var(--spacing-md);
  border-top: 1px solid rgba(255, 255, 255, 0.2);
}

.hero-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.stat-number {
  font-size: 28px;
  font-weight: 700;
}

.stat-label {
  font-size: 12px;
  opacity: 0.8;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

@media (min-width: 768px) {
  .hero-stats {
    display: none; /* Desktopon elrejtjük */
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* KATEGÓRIA SZŰRŐ */
/* ═══════════════════════════════════════════════════════════════════════════ */

.filter-wrapper {
  position: sticky;
  top: var(--header-height);
  z-index: 50;
  background: var(--color-background);
  padding: var(--spacing-md) 0;
  border-bottom: 1px solid var(--color-border);
}

.filter-section {
  display: flex;
  gap: var(--spacing-sm);
  padding: 0 var(--spacing-md);
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.filter-section::-webkit-scrollbar {
  display: none;
}

@media (min-width: 768px) {
  .filter-wrapper {
    position: static;
    border-bottom: none;
    padding: var(--spacing-lg) var(--spacing-md);
  }
  
  .filter-section {
    max-width: 1200px;
    margin: 0 auto;
    justify-content: center;
    flex-wrap: wrap;
    overflow-x: visible;
    gap: var(--spacing-md);
  }
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  border-radius: 20px;
  cursor: pointer;
  font-weight: 600;
  font-size: 13px;
  color: var(--color-text);
  white-space: nowrap;
  transition: all var(--transition-fast);
  flex-shrink: 0;
}

.filter-btn:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.filter-btn.active {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
}

.filter-icon {
  font-size: 16px;
}

@media (min-width: 768px) {
  .filter-btn {
    padding: var(--spacing-sm) var(--spacing-lg);
    font-size: 14px;
    border-radius: var(--radius-md);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* CONTENT SECTION */
/* ═══════════════════════════════════════════════════════════════════════════ */

.content-section {
  padding: var(--spacing-md);
  max-width: 1200px;
  margin: 0 auto;
}

@media (min-width: 768px) {
  .content-section {
    padding: var(--spacing-lg);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* STATE CONTAINERS (Loading, Error, Empty) */
/* ═══════════════════════════════════════════════════════════════════════════ */

.state-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-xl) var(--spacing-md);
  text-align: center;
  min-height: 200px;
}

.state-icon {
  font-size: 48px;
  margin-bottom: var(--spacing-md);
}

.state-container p {
  font-size: 16px;
  color: var(--color-text-muted);
  margin: 0 0 var(--spacing-md) 0;
}

.state-container.error p {
  color: var(--color-danger);
}

/* Loading spinner */
.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: var(--spacing-md);
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Buttons */
.btn-retry,
.btn-clear-filter {
  padding: var(--spacing-sm) var(--spacing-lg);
  background: var(--color-primary);
  color: white;
  border: none;
  border-radius: var(--radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: background var(--transition-fast);
}

.btn-retry:hover,
.btn-clear-filter:hover {
  background: var(--color-primary-dark);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* ESZKÖZÖK GRID */
/* ═══════════════════════════════════════════════════════════════════════════ */

.eszkoz-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--spacing-md);
}

@media (min-width: 480px) {
  .eszkoz-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 768px) {
  .eszkoz-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: var(--spacing-lg);
  }
}

@media (min-width: 1024px) {
  .eszkoz-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (min-width: 1280px) {
  .eszkoz-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* RESULTS COUNT */
/* ═══════════════════════════════════════════════════════════════════════════ */

.results-count {
  text-align: center;
  padding: var(--spacing-lg) 0;
  font-size: 14px;
  color: var(--color-text-muted);
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* SUCCESS TOAST */
/* ═══════════════════════════════════════════════════════════════════════════ */

.success-toast {
  position: fixed;
  bottom: calc(var(--bottom-nav-height) + var(--spacing-md));
  left: var(--spacing-md);
  right: var(--spacing-md);
  background: var(--color-success);
  color: white;
  padding: var(--spacing-md);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  z-index: 2000;
}

@media (min-width: 768px) {
  .success-toast {
    bottom: var(--spacing-lg);
    left: auto;
    right: var(--spacing-lg);
    max-width: 400px;
  }
}

.toast-icon {
  font-size: 20px;
  flex-shrink: 0;
}

.toast-message {
  flex: 1;
  font-weight: 500;
  font-size: 14px;
}

.toast-close {
  background: none;
  border: none;
  color: white;
  font-size: 18px;
  cursor: pointer;
  padding: var(--spacing-xs);
  opacity: 0.8;
  transition: opacity var(--transition-fast);
}

.toast-close:hover {
  opacity: 1;
}

/* Toast animation */
.toast-enter-active,
.toast-leave-active {
  transition: all var(--transition-normal);
}

.toast-enter-from {
  transform: translateY(100px);
  opacity: 0;
}

.toast-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

@media (max-width: 767px) {
  .toast-leave-to {
    transform: translateY(100px);
  }
}
</style>