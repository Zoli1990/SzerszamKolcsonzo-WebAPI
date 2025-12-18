<template>
  <section id="velemenyek" class="velemenyek-section">
    <div class="container">
      <h2 class="section-title">Mit mondanak ügyfeleink?</h2>
      <p class="section-subtitle">Több mint 500 elégedett ügyfél tapasztalata</p>

      <div class="carousel-wrapper">
        <button class="carousel-btn prev" @click="prev">‹</button>

        <div class="velemenyek-container">
          <transition-group name="slide" tag="div" class="velemenyek-track">
            <div
              v-for="(velemeny, index) in visibleVelemenyek"
              :key="velemeny.id"
              class="velemeny-card"
              :style="{ order: index }"
            >
              <div class="velemeny-header">
                <img :src="velemeny.profilkep" :alt="velemeny.nev" class="avatar" />
                <div class="user-info">
                  <h3 class="user-name">{{ velemeny.nev }}</h3>
                  <div class="rating">
                    <span
                      v-for="i in 5"
                      :key="i"
                      class="star"
                      :class="{ filled: i <= velemeny.ertekeles }"
                    >
                      ★
                    </span>
                  </div>
                </div>
              </div>

              <p class="velemeny-text">{{ velemeny.szoveg }}</p>

              <div class="velemeny-footer">
                <span class="datum">{{ formatDate(velemeny.datum) }}</span>
              </div>
            </div>
          </transition-group>
        </div>

        <button class="carousel-btn next" @click="next">›</button>
      </div>

      <!-- Indikátor pontok -->
      <div class="carousel-indicators">
        <button
          v-for="(_, index) in Math.ceil(velemenyek.length / itemsPerPage)"
          :key="index"
          class="indicator"
          :class="{ active: Math.floor(currentIndex / itemsPerPage) === index }"
          @click="goToPage(index)"
        ></button>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const currentIndex = ref(0)
const itemsPerPage = ref(3)

// Példa vélemények
const velemenyek = ref([
  {
    id: 1,
    nev: 'Kovács János',
    profilkep: 'https://i.pravatar.cc/150?img=12',
    ertekeles: 5,
    szoveg:
      'Kiváló szolgáltatás! Gyorsan és egyszerűen tudtam bérelni a fúrógépet, ami tökéletes állapotban volt. Mindenképpen ajánlom!',
    datum: '2024-11-15',
  },
  {
    id: 2,
    nev: 'Nagy Éva',
    profilkep: 'https://i.pravatar.cc/150?img=45',
    ertekeles: 4,
    szoveg:
      'Nagyon elégedett vagyok a kölcsönzéssel. Az egyetlen apró hiányosság, hogy kicsit drága volt, de cserébe minőségi eszközöket kaptam.',
    datum: '2024-11-10',
  },
  {
    id: 3,
    nev: 'Szabó Péter',
    profilkep: 'https://i.pravatar.cc/150?img=33',
    ertekeles: 5,
    szoveg:
      'Professzionális kiszolgálás, széles eszköz választék. A visszavétel is zökkenőmentes volt. Köszönöm!',
    datum: '2024-11-08',
  },
  {
    id: 4,
    nev: 'Tóth Mária',
    profilkep: 'https://i.pravatar.cc/150?img=23',
    ertekeles: 4,
    szoveg:
      'Jó élmény volt, bár a weboldalon nehezen találtam meg, amit kerestem. Viszont az ügyfélszolgálat segítőkész volt.',
    datum: '2024-11-05',
  },
  {
    id: 5,
    nev: 'Kiss Gábor',
    profilkep: 'https://i.pravatar.cc/150?img=68',
    ertekeles: 3,
    szoveg:
      'Átlagos tapasztalat. Az eszköz rendben volt, de a kommunikáció lehetne jobb. Összességében elfogadható.',
    datum: '2024-11-01',
  },
  {
    id: 6,
    nev: 'Horváth Anna',
    profilkep: 'https://i.pravatar.cc/150?img=47',
    ertekeles: 5,
    szoveg:
      'Fantasztikus! Minden szempontból tökéletes volt a kölcsönzés. Gyors, megbízható és barátságos kiszolgálás!',
    datum: '2024-10-28',
  },
])

const visibleVelemenyek = computed(() => {
  return velemenyek.value.slice(currentIndex.value, currentIndex.value + itemsPerPage.value)
})

function prev() {
  if (currentIndex.value > 0) {
    currentIndex.value -= itemsPerPage.value
  } else {
    // Végtelen loop: ugrás a végére
    currentIndex.value = velemenyek.value.length - itemsPerPage.value
  }
}

function next() {
  if (currentIndex.value < velemenyek.value.length - itemsPerPage.value) {
    currentIndex.value += itemsPerPage.value
  } else {
    // Végtelen loop: ugrás az elejére
    currentIndex.value = 0
  }
}

function goToPage(pageIndex) {
  currentIndex.value = pageIndex * itemsPerPage.value
}

function formatDate(dateString) {
  const date = new Date(dateString)
  return date.toLocaleDateString('hu-HU', { year: 'numeric', month: 'long', day: 'numeric' })
}

// Responsive - items per page változtatás
function updateItemsPerPage() {
  if (window.innerWidth < 768) {
    itemsPerPage.value = 1
  } else if (window.innerWidth < 1024) {
    itemsPerPage.value = 2
  } else {
    itemsPerPage.value = 3
  }
  // Reset index ha túlmenne
  if (currentIndex.value >= velemenyek.value.length - itemsPerPage.value) {
    currentIndex.value = Math.max(0, velemenyek.value.length - itemsPerPage.value)
  }
}

onMounted(() => {
  updateItemsPerPage()
  window.addEventListener('resize', updateItemsPerPage)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateItemsPerPage)
})
</script>

<style scoped>
.velemenyek-section {
  padding: 60px 0;
  background: linear-gradient(135deg, #f5f1e8 0%, #e8dcc8 100%);
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.section-title {
  text-align: center;
  font-size: 36px;
  font-weight: 700;
  color: #3d2f1f;
  margin: 0 0 12px 0;
}

.section-subtitle {
  text-align: center;
  font-size: 18px;
  color: #6b5d4f;
  margin: 0 0 48px 0;
}

.carousel-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  gap: 16px;
}

.carousel-btn {
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: white;
  border: 2px solid #d4c5b0;
  color: #6b8e23;
  font-size: 32px;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
  box-shadow: 0 2px 8px rgba(61, 47, 31, 0.1);
}

.carousel-btn:hover:not(:disabled) {
  background: #6b8e23;
  color: white;
  border-color: #6b8e23;
  transform: scale(1.1);
}

.carousel-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.velemenyek-container {
  flex: 1;
  overflow: hidden;
  position: relative;
}

.velemenyek-track {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
  width: 100%;
}

.velemeny-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(61, 47, 31, 0.1);
  border: 1px solid #e8dcc8;
  transition:
    transform 0.3s,
    box-shadow 0.3s;
  min-height: 280px;
}

.velemeny-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(61, 47, 31, 0.15);
}

.velemeny-header {
  display: flex;
  gap: 16px;
  align-items: center;
  margin-bottom: 16px;
}

.avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #6b8e23;
}

.user-info {
  flex: 1;
}

.user-name {
  margin: 0 0 8px 0;
  font-size: 18px;
  font-weight: 600;
  color: #3d2f1f;
}

.rating {
  display: flex;
  gap: 4px;
}

.star {
  font-size: 20px;
  color: #d4c5b0;
  transition: color 0.2s;
}

.star.filled {
  color: #f59e0b;
}

.velemeny-text {
  font-size: 15px;
  line-height: 1.6;
  color: #544838;
  margin: 0 0 16px 0;
  min-height: 80px;
}

.velemeny-footer {
  display: flex;
  justify-content: flex-end;
  padding-top: 12px;
  border-top: 1px solid #e8dcc8;
}

.datum {
  font-size: 13px;
  color: #8a7f6f;
}

.carousel-indicators {
  display: flex;
  justify-content: center;
  gap: 12px;
  margin-top: 32px;
}

.indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #d4c5b0;
  border: none;
  cursor: pointer;
  transition: all 0.3s;
  padding: 0;
}

.indicator:hover {
  background: #8b7355;
  transform: scale(1.2);
}

.indicator.active {
  background: #6b8e23;
  width: 32px;
  border-radius: 6px;
}

/* Slide animáció - MÓDOSÍTVA */
.slide-move {
  transition: all 0.5s ease;
}

.slide-enter-active {
  transition: all 0.5s ease;
}

.slide-leave-active {
  transition: all 0.5s ease;
  position: absolute;
  opacity: 0;
}

.slide-enter-from {
  opacity: 0;
}

.slide-leave-to {
  opacity: 0;
}
/* Responsive */
@media (max-width: 1024px) {
  .velemenyek-track {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .section-title {
    font-size: 28px;
  }

  .carousel-btn {
    width: 40px;
    height: 40px;
    font-size: 28px;
  }

  .velemenyek-track {
    grid-template-columns: 1fr;
  }

  .velemeny-text {
    min-height: auto;
  }
}
</style>
