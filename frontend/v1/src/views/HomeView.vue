// ============================================================================ // 2.
src/views/HomeView.vue - FRISSÍTETT (Modal használattal) //
============================================================================

<template>
  <div class="home">
    <header class="hero">
      <h1>Szerszámkölcsönző</h1>
      <p>Béreljen minőségi szerszámokat óradíjas rendszerben!</p>
    </header>

    <!-- Kategória szűrő -->
    <div class="filter-section">
      <button
        class="filter-btn"
        :class="{ active: !eszkozStore.selectedKategoriaId }"
        @click="eszkozStore.clearFilter()"
      >
        Összes
      </button>
      <button
        v-for="kategoria in kategoriak"
        :key="kategoria.kategoriaID"
        class="filter-btn"
        :class="{ active: eszkozStore.selectedKategoriaId === kategoria.kategoriaID }"
        @click="eszkozStore.setKategoriaFilter(kategoria.kategoriaID)"
      >
        {{ kategoria.nev }}
      </button>
    </div>

    <!-- Loading state -->
    <div v-if="eszkozStore.loading" class="loading">Betöltés...</div>

    <!-- Error state -->
    <div v-else-if="eszkozStore.error" class="error">Hiba történt: {{ eszkozStore.error }}</div>

    <!-- Eszközök grid -->
    <div v-else class="eszkoz-grid">
      <EszkozCard
        v-for="eszkoz in eszkozStore.filteredEszkozok"
        :key="eszkoz.eszkozID"
        :eszkoz="eszkoz"
        @foglalas="openFoglalasModal"
      />
    </div>

    <!-- Üres állapot -->
    <div v-if="!eszkozStore.loading && eszkozStore.filteredEszkozok.length === 0" class="empty">
      Nincs megjeleníthető eszköz.
    </div>

    <!-- Foglalás Modal -->
    <FoglalasModal
      :is-open="modalOpen"
      :eszkoz="selectedEszkoz"
      @close="modalOpen = false"
      @success="handleFoglalasSuccess"
    />

    <!-- Sikeres foglalás toast -->
    <Transition name="toast">
      <div v-if="successMessage" class="success-toast">✅ {{ successMessage }}</div>
    </Transition>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useEszkozStore } from '@/stores/eszkozStore'
import { kategoriaService } from '@/services/kategoriaService'
import EszkozCard from '@/components/eszkozok/EszkozCard.vue'
import FoglalasModal from '@/components/foglalas/FoglalasModal.vue'

const eszkozStore = useEszkozStore()
const kategoriak = ref([])
const modalOpen = ref(false)
const selectedEszkoz = ref(null)
const successMessage = ref('')

onMounted(async () => {
  await eszkozStore.fetchEszkozok()

  try {
    const response = await kategoriaService.getAll()
    kategoriak.value = response.data
  } catch (error) {
    console.error('Kategóriák betöltése sikertelen:', error)
  }
})

function openFoglalasModal(eszkoz) {
  selectedEszkoz.value = eszkoz
  modalOpen.value = true
}

function handleFoglalasSuccess(data) {
  successMessage.value = `Sikeres foglalás! ${data.eszkoz} - ${data.oraSzam} óra - ${formatAr(data.koltseg)} Ft`

  // 5 másodperc múlva eltűnik
  setTimeout(() => {
    successMessage.value = ''
  }, 5000)
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
.home {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.hero {
  text-align: center;
  padding: 40px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 12px;
  margin-bottom: 40px;
}

.hero h1 {
  font-size: 48px;
  margin: 0 0 16px 0;
}

.hero p {
  font-size: 20px;
  margin: 0;
  opacity: 0.9;
}

.filter-section {
  display: flex;
  gap: 12px;
  margin-bottom: 32px;
  flex-wrap: wrap;
}

.filter-btn {
  padding: 10px 20px;
  background: white;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
}

.filter-btn:hover {
  border-color: #3b82f6;
  color: #3b82f6;
}

.filter-btn.active {
  background: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.eszkoz-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
  margin-bottom: 40px;
}

.loading,
.error,
.empty {
  text-align: center;
  padding: 60px 20px;
  font-size: 18px;
  color: #6b7280;
}

.error {
  color: #ef4444;
}

/* Success Toast */
.success-toast {
  position: fixed;
  bottom: 24px;
  right: 24px;
  background: #10b981;
  color: white;
  padding: 16px 24px;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
  font-weight: 600;
  z-index: 2000;
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
  transform: translateX(400px);
  opacity: 0;
}
</style>
