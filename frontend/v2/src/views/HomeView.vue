<template>
  <div class="home">
    <!-- KEZDŐOLDAL SZEKCIÓ -->
    <section id="home" class="hero">
      <div class="slogen">
        <h3>Tooly</h3>
        <p>
          Segítünk, hogy mindig legyen, amivel dolgoznod. Bérelj minőségi szerszámokat óradíjas
          rendszerben!
        </p>
        <a href="#szolgaltatasok" class="hero-cta">Böngésszen eszközök között</a>
      </div>
    </section>

    <!-- SZOLGÁLTATÁSOK SZEKCIÓ -->
    <section id="szolgaltatasok" class="services-section">
      <h2 class="section-title">Szolgáltatásaink</h2>

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
    </section>

    <!-- VÉLEMÉNYEK SZEKCIÓ -->
    <section id="velemenyek" class="reviews-section">
      <h2 class="section-title">Ügyfeleink véleménye</h2>
      <div class="reviews-grid">
        <div class="review-card">
          <div class="stars">⭐⭐⭐⭐⭐</div>
          <p class="review-text">
            "Kiváló minőségű szerszámok, rugalmas bérlési feltételek. A fúrógépet egy hétvégére
            béreltem, tökéletesen működött!"
          </p>
          <p class="review-author">- Kovács János</p>
        </div>
        <div class="review-card">
          <div class="stars">⭐⭐⭐⭐⭐</div>
          <p class="review-text">
            "Gyors ügyintézés, barátságos kiszolgálás. A csiszológépet egész nap használtam
            problémamentesen."
          </p>
          <p class="review-author">- Nagy Éva</p>
        </div>
        <div class="review-card">
          <div class="stars">⭐⭐⭐⭐⭐</div>
          <p class="review-text">
            "Kedvező árak, széles választék. Biztosan visszatérek, ha újra szerszámra lesz
            szükségem!"
          </p>
          <p class="review-author">- Szabó Péter</p>
        </div>
      </div>
    </section>

    <!-- RÓLUNK SZEKCIÓ -->
    <section id="rolunk" class="about-section">
      <h2 class="section-title">Rólunk</h2>
      <div class="about-content">
        <div class="about-text">
          <h3>Bemutatkozás</h3>
          <p>
            Szerszámkölcsönző cégünk 2020 óta szolgálja ki ügyfeleit Szegeden és környékén. Célunk,
            hogy minőségi szerszámokat biztosítsunk kedvező áron azok számára, akiknek alkalmanként
            van szükségük speciális eszközökre.
          </p>
          <h3>Miért válasszon minket?</h3>
          <ul>
            <li>✅ Széles szerszám választék</li>
            <li>✅ Rugalmas bérlési időtartam</li>
            <li>✅ Versenyképes óradíjak</li>
            <li>✅ Karbantartott, megbízható eszközök</li>
            <li>✅ Gyors ügyintézés</li>
          </ul>
          <h3>Elérhetőség</h3>
          <p>
            📍 6720 Szeged, Példa utca 12.<br />
            📞 +36 30 123 4567<br />
            📧 info@szerszamkolcsonzo.hu
          </p>
        </div>
        <div class="about-image">
          <div class="placeholder-image">🔧 🔨 ⚙️</div>
        </div>
      </div>
    </section>

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
  margin: 0 auto;
  padding: 0;
}

/* HERO SZEKCIÓ */
.hero {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 50px 0 0 0;
  margin: 0;
  margin-top: -20px;
  margin-bottom: 40px;
  background-image: linear-gradient(rgba(0, 0, 0, 0.3), rgba(0, 0, 0, 0.3)), url('home_kep.jpg');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  scroll-margin-top: 80px;
}

.slogen {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  align-items: center;
  text-align: center;
  color: #ffffff;
  height: 100vh;
  padding-top: 0px;
  background-color: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(3px);
  padding: 3rem;
  border-radius: 15px;
}

.slogen h3 {
  font-size: 6rem;
  margin-bottom: 3rem;
  margin-top: 2rem;
  line-height: 1.3;
  text-shadow: 3px 3px 8px rgba(0, 0, 0, 0.8);
  font-weight: 700;
  color: #ffffff;
}

.slogen p {
  font-size: 2rem;
  margin-bottom: 2rem;
  color: #ffffff;
  line-height: 1.8;
  text-shadow: 2px 2px 6px rgba(0, 0, 0, 0.8);
  font-weight: 600;
}

.hero-cta {
  display: inline-block;
  padding: 14px 32px;
  background: var(--dark-blue);
  color: white;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 700;
  transition: transform 0.2s;
}

.hero-cta:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
}
/* KÖZÖS SZEKCIÓ STÍLUSOK */
section {
  margin-bottom: 80px;
  scroll-margin-top: 80px;
}

.section-title {
  font-size: 36px;
  text-align: center;
  margin-bottom: 48px;
  color: #111827;
}

section {
  margin-bottom: 80px;
  scroll-margin-top: 80px;
}

.section-title {
  font-size: 36px;
  text-align: center;
  margin-bottom: 48px;
  color: #ffffff;
  text-shadow: 2px 2px 6px rgba(0, 0, 0, 0.8);
  font-weight: 700;
}

/* SZOLGÁLTATÁSOK SZEKCIÓ */
.services-section {
  padding: 40px 0;
  background-color: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(3px);
  border-radius: 15px;
  padding: 3rem;
}

.filter-section {
  display: flex;
  gap: 12px;
  margin-bottom: 32px;
  flex-wrap: wrap;
  justify-content: center;
}

.filter-btn {
  padding: 10px 20px;
  background: rgba(0, 0, 0, 0.5);
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
  color: #ffffff;
  text-shadow: 1px 1px 3px rgba(0, 0, 0, 0.6);
}

.filter-btn:hover {
  border-color: #ffffff;
  background: rgba(255, 255, 255, 0.1);
  transform: translateY(-2px);
}

.filter-btn.active {
  background: rgba(255, 255, 255, 0.2);
  color: #ffffff;
  border-color: #ffffff;
  box-shadow: 0 4px 12px rgba(255, 255, 255, 0.2);
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
  color: #ffffff;
  text-shadow: 1px 1px 3px rgba(0, 0, 0, 0.8);
}

.error {
  color: #ff6b6b;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
}
/* VÉLEMÉNYEK SZEKCIÓ */
.reviews-section {
  padding: 60px 20px;
  background: #f9fafb;
  border-radius: 12px;
}

.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
}

.review-card {
  background: white;
  padding: 32px;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.07);
}

.stars {
  font-size: 24px;
  margin-bottom: 16px;
}

.review-text {
  font-size: 16px;
  line-height: 1.6;
  color: #374151;
  margin-bottom: 16px;
  font-style: italic;
}

.review-author {
  font-weight: 600;
  color: #6b7280;
  text-align: right;
}

/* RÓLUNK SZEKCIÓ */
.about-section {
  padding: 60px 20px;
}

.about-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 48px;
  align-items: center;
}

.about-text h3 {
  font-size: 24px;
  margin: 24px 0 16px 0;
  color: #111827;
}

.about-text h3:first-child {
  margin-top: 0;
}

.about-text p {
  font-size: 16px;
  line-height: 1.8;
  color: #4b5563;
  margin-bottom: 16px;
}

.about-text ul {
  list-style: none;
  padding: 0;
}

.about-text li {
  font-size: 16px;
  line-height: 2;
  color: #4b5563;
}

.about-image {
  display: flex;
  justify-content: center;
  align-items: center;
}

.placeholder-image {
  width: 100%;
  height: 400px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 80px;
  gap: 20px;
}

/* RESPONSIVENESS */
@media (max-width: 768px) {
  .hero h1 {
    font-size: 32px;
  }

  .hero p {
    font-size: 16px;
  }

  .section-title {
    font-size: 28px;
  }

  .about-content {
    grid-template-columns: 1fr;
  }

  .placeholder-image {
    height: 250px;
    font-size: 50px;
  }
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
