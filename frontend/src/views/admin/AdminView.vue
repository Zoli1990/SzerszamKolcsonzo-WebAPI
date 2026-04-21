<!-- ============================================================================ -->
<!-- src/views/admin/AdminView.vue - Admin oldal + PWA Install Prompt -->
<!-- ============================================================================ -->

<template>
  <div class="admin-container">
    <!-- PWA Install Banner (csak ha még nincs telepítve) -->
    <Transition name="slide-down">
      <div v-if="showPwaPrompt" class="pwa-install-banner">
        <div class="banner-content">
          <div class="banner-icon">📱</div>
          <div class="banner-text">
            <h3>Admin PWA Telepítése</h3>
            <p>
              Telepítsd az admin értesítő alkalmazást a könnyebb elérésért és push értesítésekért!
            </p>
          </div>
          <div class="banner-actions">
            <button class="btn-install" @click="installPwa">⬇️ Telepítés</button>
            <button class="btn-dismiss" @click="dismissPwaPrompt">✕</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Admin Header -->
    <div class="admin-header">
      <h1>🔧 Admin Panel</h1>
      <RouterLink to="/" class="btn-back">← Vissza a főoldalra</RouterLink>
    </div>

    <!-- Tab menü -->
    <div class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        :id="`admin-tab-${tab.id}`"
        :class="['tab', { active: activeTab === tab.id }]"
        @click="activeTab = tab.id"
      >
        {{ tab.icon }} {{ tab.label }}
      </button>
    </div>

    <!-- Tab tartalom -->
    <div class="tab-content">
      <AdminDashboard v-if="activeTab === 'dashboard'" />
      <KategoriakAdmin v-else-if="activeTab === 'kategoriak'" />

      <!-- ✅ Eszközök tab - Responsive váltás -->
      <template v-else-if="activeTab === 'eszkozok'">
        <AdminEszkozListaMobile v-if="isMobile" />
        <EszkozokAdmin v-else />
      </template>

      <FoglalasokAdmin v-else-if="activeTab === 'foglalasok'" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

// ✅ Komponensek
import AdminDashboard from '@/components/admin/AdminDashboard.vue'
import KategoriakAdmin from '@/components/admin/KategoriakAdmin.vue'
import EszkozokAdmin from '@/components/admin/EszkozokAdmin.vue'
import AdminEszkozListaMobile from '@/components/admin/AdminEszkozListaMobile.vue'
import FoglalasokAdmin from '@/components/admin/FoglalasokAdmin.vue'

// ✅ Responsive detection (natív, nincs szükség @vueuse/core-ra)
const windowWidth = ref(window.innerWidth)
const isMobile = computed(() => windowWidth.value < 768)

function handleResize() {
  windowWidth.value = window.innerWidth
}

onMounted(() => {
  window.addEventListener('resize', handleResize)

  // Auth ellenőrzés
  if (!authStore.isAuthenticated || !authStore.isAdmin) {
    alert('Nincs jogosultságod az admin felülethez!')
    router.push('/')
    return
  }

  // PWA prompt setup (csak ha admin)
  setupPwaPrompt()
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})

// ✅ State
const authStore = useAuthStore()
const router = useRouter()
const activeTab = ref('dashboard')
const showPwaPrompt = ref(false)
let deferredPrompt = null

const tabs = [
  { id: 'dashboard', label: 'Dashboard', icon: '📊' },
  { id: 'kategoriak', label: 'Kategóriák', icon: '📁' },
  { id: 'eszkozok', label: 'Eszközök', icon: '🔨' },
  { id: 'foglalasok', label: 'Foglalások', icon: '📋' },
]

// PWA Install Prompt kezelése
const setupPwaPrompt = () => {
  const isPwaInstalled =
    window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true

  if (isPwaInstalled) {
    console.log('[Admin] PWA already installed')
    return
  }

  const pwaPromptDismissed = localStorage.getItem('pwa-prompt-dismissed')
  if (pwaPromptDismissed) {
    console.log('[Admin] PWA prompt previously dismissed')
    return
  }

  window.addEventListener('beforeinstallprompt', (e) => {
    console.log('[Admin] beforeinstallprompt event fired')
    e.preventDefault()
    deferredPrompt = e
    showPwaPrompt.value = true
  })

  setTimeout(() => {
    if (!deferredPrompt && !isPwaInstalled) {
      console.log('[Admin] Manual PWA prompt (no beforeinstallprompt)')
      showPwaPrompt.value = true
    }
  }, 2000)
}

const installPwa = async () => {
  if (deferredPrompt) {
    deferredPrompt.prompt()
    const { outcome } = await deferredPrompt.userChoice
    console.log(`[Admin] User response: ${outcome}`)

    if (outcome === 'accepted') {
      console.log('[Admin] PWA install accepted')
      setTimeout(() => {
        router.push('/admin-pwa')
      }, 1000)
    }

    deferredPrompt = null
    showPwaPrompt.value = false
  } else {
    console.log('[Admin] Manual PWA installation')

    if ('serviceWorker' in navigator) {
      try {
        const registration = await navigator.serviceWorker.register('/sw-admin.js', {
          scope: '/',
        })
        console.log('[Admin] Service Worker registered:', registration.scope)

        setTimeout(() => {
          router.push('/admin-pwa')
          showPwaPrompt.value = false
        }, 500)
      } catch (error) {
        console.error('[Admin] Service Worker registration failed:', error)
        alert('Telepítés sikertelen. Próbáld meg később!')
      }
    } else {
      router.push('/admin-pwa')
      showPwaPrompt.value = false
    }
  }
}

const dismissPwaPrompt = () => {
  showPwaPrompt.value = false
  localStorage.setItem('pwa-prompt-dismissed', 'true')
  console.log('[Admin] PWA prompt dismissed')
}
</script>

<style scoped>
/* PWA Install Banner */
.pwa-install-banner {
  background: linear-gradient(135deg, #3d2f1f 0%, #2a1f15 100%);
  color: white;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  animation: slideIn 0.4s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.banner-content {
  display: flex;
  align-items: center;
  gap: 20px;
}

.banner-icon {
  font-size: 48px;
  flex-shrink: 0;
}

.banner-text {
  flex: 1;
}

.banner-text h3 {
  margin: 0 0 8px 0;
  font-size: 20px;
  font-weight: 700;
}

.banner-text p {
  margin: 0;
  font-size: 14px;
  opacity: 0.9;
  line-height: 1.5;
}

.banner-actions {
  display: flex;
  gap: 12px;
  flex-shrink: 0;
}

.btn-install {
  padding: 12px 24px;
  background: #6b8e23;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.btn-install:hover {
  background: #5a7a1e;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

.btn-dismiss {
  padding: 12px;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  font-size: 18px;
  cursor: pointer;
  transition: all 0.2s;
  min-width: 44px;
}

.btn-dismiss:hover {
  background: rgba(255, 255, 255, 0.2);
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .pwa-install-banner {
    padding: 16px;
  }

  .banner-content {
    flex-direction: column;
    text-align: center;
    gap: 16px;
  }

  .banner-icon {
    font-size: 40px;
  }

  .banner-text h3 {
    font-size: 18px;
  }

  .banner-text p {
    font-size: 14px;
  }

  .banner-actions {
    width: 100%;
    flex-direction: column;
  }

  .btn-install {
    width: 100%;
    padding: 14px 24px;
    font-size: 18px;
  }

  .btn-dismiss {
    width: 100%;
    padding: 12px;
  }
}

/* Admin Styles */
.admin-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px;
}

.admin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
}

.admin-header h1 {
  margin: 0;
  font-size: 32px;
  color: #3d2f1f;
}

.btn-back {
  padding: 10px 20px;
  background: white;
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  text-decoration: none;
  color: #3d2f1f;
  font-weight: 600;
  transition: all 0.2s;
}

.btn-back:hover {
  border-color: #6b8e23;
  color: #6b8e23;
}

.tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 24px;
  border-bottom: 2px solid #e8dcc8;
}

.tab {
  padding: 12px 24px;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  cursor: pointer;
  font-size: 16px;
  font-weight: 600;
  color: #6b5d4f;
  transition: all 0.2s;
}

.tab:hover {
  color: #6b8e23;
}

.tab.active {
  color: #6b8e23;
  border-bottom-color: #6b8e23;
}

.tab-content {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(61, 47, 31, 0.1);
  border: 1px solid #e8dcc8;
}

/* Transitions */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}

@media (max-width: 768px) {
  .admin-container {
    padding: 12px;
  }

  .admin-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
    margin-bottom: 20px;
  }

  .admin-header h1 {
    font-size: 22px;
  }

  .btn-back {
    font-size: 14px;
    padding: 8px 16px;
  }

  .tabs {
    overflow-x: auto;
    overflow-y: hidden;
    -webkit-overflow-scrolling: touch;
    scrollbar-width: none;
    -ms-overflow-style: none;
    gap: 4px;
    margin-bottom: 16px;
    padding-bottom: 8px;
  }

  .tabs::-webkit-scrollbar {
    display: none;
  }

  .tab {
    flex-shrink: 0;
    padding: 10px 16px;
    font-size: 14px;
    white-space: nowrap;
  }

  .tab-content {
    padding: 16px;
    border-radius: 8px;
  }
}

@media (max-width: 480px) {
  .admin-container {
    padding: 10px;
  }

  .admin-header h1 {
    font-size: 20px;
  }

  .tab {
    padding: 8px 14px;
    font-size: 13px;
  }

  .tab-content {
    padding: 12px;
  }
}
</style>
