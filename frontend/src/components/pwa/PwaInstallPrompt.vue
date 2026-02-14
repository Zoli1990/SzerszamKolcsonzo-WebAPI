<template>
  <!-- ✅ TELEPORT: Közvetlenül a body-ba renderel, nem befolyásolja a layout-ot -->
  <Teleport to="body">
    <Transition name="slide-up">
      <div
        v-if="showInstallPrompt"
        class="pwa-install-banner"
      >
        <div class="install-content">
          <div class="install-icon">📲</div>
          <div class="install-text">
            <strong>Telepítsd az alkalmazást!</strong>
            <span>Gyorsabb és könnyebb használat</span>
          </div>
        </div>

        <div class="install-actions">
          <button class="btn-later" @click="dismissPrompt">Később</button>
          <button class="btn-install" @click="installPWA">Telepítés</button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const showInstallPrompt = ref(false)
let deferredPrompt = null

// beforeinstallprompt event kezelése
function handleBeforeInstallPrompt(e) {
  console.log('📲 beforeinstallprompt fired')
  
  // Megakadályozzuk az automatikus prompt-ot
  e.preventDefault()
  
  // Elmentjük későbbi használatra
  deferredPrompt = e
  
  // Csak akkor mutatjuk, ha a user nem zárta be korábban
  const dismissed = localStorage.getItem('pwa-install-dismissed')
  const dismissedTime = dismissed ? parseInt(dismissed) : 0
  const oneWeek = 7 * 24 * 60 * 60 * 1000
  
  if (!dismissed || Date.now() - dismissedTime > oneWeek) {
    showInstallPrompt.value = true
  }
}

// Telepítés indítása
async function installPWA() {
  if (!deferredPrompt) {
    console.log('❌ No deferred prompt available')
    return
  }

  console.log('📲 Showing install prompt...')
  
  // Megjelenítjük a natív promptot
  deferredPrompt.prompt()
  
  // Megvárjuk a választ
  const { outcome } = await deferredPrompt.userChoice
  console.log(`📲 User response: ${outcome}`)
  
  // Töröljük a mentett prompt-ot
  deferredPrompt = null
  showInstallPrompt.value = false
  
  if (outcome === 'accepted') {
    console.log('✅ PWA installed!')
  }
}

// Elutasítás kezelése
function dismissPrompt() {
  showInstallPrompt.value = false
  localStorage.setItem('pwa-install-dismissed', Date.now().toString())
}

// Telepítés után eltüntetjük
function handleAppInstalled() {
  console.log('✅ PWA was installed')
  showInstallPrompt.value = false
  deferredPrompt = null
}

onMounted(() => {
  window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt)
  window.addEventListener('appinstalled', handleAppInstalled)
  
  // Ellenőrizzük, hogy standalone módban fut-e (már telepítve van)
  if (window.matchMedia('(display-mode: standalone)').matches) {
    console.log('📲 App is running in standalone mode')
    showInstallPrompt.value = false
  }
})

onUnmounted(() => {
  window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt)
  window.removeEventListener('appinstalled', handleAppInstalled)
})
</script>

<style scoped>
/* ✅ Fixed position + Teleport = garantáltan nem foglal helyet */
.pwa-install-banner {
  position: fixed;
  bottom: 80px; /* Bottom nav felett */
  left: 16px;
  right: 16px;
  background: white;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

@media (min-width: 768px) {
  .pwa-install-banner {
    bottom: 24px;
    left: auto;
    right: 24px;
    max-width: 380px;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}

.install-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.install-icon {
  font-size: 32px;
  flex-shrink: 0;
}

.install-text {
  display: flex;
  flex-direction: column;
}

.install-text strong {
  font-size: 15px;
  color: #3d2f1f;
}

.install-text span {
  font-size: 13px;
  color: #6b5d4f;
}

.install-actions {
  display: flex;
  gap: 8px;
}

.btn-later,
.btn-install {
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-later {
  background: transparent;
  color: #6b5d4f;
}

.btn-later:hover {
  background: #f5f1e8;
}

.btn-install {
  background: #6b8e23;
  color: white;
  flex: 1;
}

@media (min-width: 768px) {
  .btn-install {
    flex: none;
  }
}

.btn-install:hover {
  background: #556b1a;
}

/* Animáció */
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease;
}

.slide-up-enter-from,
.slide-up-leave-to {
  transform: translateY(100%);
  opacity: 0;
}
</style>