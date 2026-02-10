<template>
  <div class="home">
    <header class="hero">
      <h1>Szerszámkölcsönző</h1>
      <p>Béreljen minőségi szerszámokat óradíjas rendszerben!</p>
    </header>

    <!-- Kategória szűrő -->
    <div class="filter-section" id="eszkozok">
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

     <!-- Vélemények szekció -->
    <VelemenySzekcio id="velemenyek" />

    <!-- ============================================================================ -->
    <!-- ✅ ÚJ: Kapcsolat szekció -->
    <!-- ============================================================================ -->
    <KapcsolatSection id="kapcsolat" />

    <!-- ✅ JAVÍTVA: Foglalás Modal csak ha van kiválasztott eszköz -->
    <FoglalasModal
      v-if="selectedEszkoz"
      :is-open="modalOpen"
      :eszkoz="selectedEszkoz"
      @close="closeModal"
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
import VelemenySzekcio from '@/components/VelemenySzekcio.vue'
import KapcsolatSection from '@/components/KapcsolatSection.vue'

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

// ✅ JAVÍTVA: Modal bezárásakor eszköz nullázása
function closeModal() {
  modalOpen.value = false
  // Kis késleltetéssel nullázzuk, hogy a modal animation lefusson
  setTimeout(() => {
    selectedEszkoz.value = null
  }, 300)
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
  background: linear-gradient(135deg, #8b7355 0%, #6b5d4f 100%);
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
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
  color: #3d2f1f;
}

.filter-btn:hover {
  border-color: #6b8e23;
  color: #6b8e23;
}

.filter-btn.active {
  background: #6b8e23;
  color: white;
  border-color: #6b8e23;
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
  color: #6b5d4f;
}

.error {
  color: #b85c5c;
}

/* Success Toast */
.success-toast {
  position: fixed;
  bottom: 24px;
  right: 24px;
  background: #7a9b57;
  color: white;
  padding: 16px 24px;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(61, 47, 31, 0.3);
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