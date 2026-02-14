<!-- ============================================================================ -->
<!-- src/views/admin/AdminPwaView.vue - Admin PWA (CSAK foglalások kezelése) -->
<!-- ============================================================================ -->

<template>
  <div class="admin-pwa">
    <!-- PWA Header -->
    <header class="pwa-header">
      <div class="header-content">
        <div class="header-icon">🔔</div>
        <h1>SZERSZÁMKÖLCSÖNZŐ - ADMIN PWA</h1>
      </div>
    </header>

    <!-- Main Content -->
    <main class="pwa-main">
      <AdminPwaDashboard />
    </main>

    <!-- PWA Install Prompt (ha még nincs telepítve) -->
    <PwaInstallPrompt v-if="!isPwaInstalled" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import AdminPwaDashboard from '@/components/admin/AdminPwaDashboard.vue'
import PwaInstallPrompt from '@/components/pwa/PwaInstallPrompt.vue'

const authStore = useAuthStore()
const router = useRouter()
const isPwaInstalled = ref(false)

// Auth ellenőrzés
onMounted(async () => {
  // Admin jogosultság ellenőrzés
  if (!authStore.isAuthenticated || !authStore.isAdmin) {
    alert('Nincs jogosultságod az admin PWA-hoz!')
    router.push('/')
    return
  }

  // PWA telepítve van-e?
  if (window.matchMedia('(display-mode: standalone)').matches) {
    isPwaInstalled.value = true
  }

  console.log('[Admin PWA] Loaded successfully')
})
</script>

<style scoped>
/* ============================================================================ */
/* ADMIN PWA STYLING - Mobile-first, dark header */
/* ============================================================================ */

.admin-pwa {
  min-height: 100vh;
  background: #f5f1e8;
  display: flex;
  flex-direction: column;
}

/* Header */
.pwa-header {
  background: linear-gradient(135deg, #3d2f1f 0%, #2a1f15 100%);
  color: white;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 12px;
  max-width: 1200px;
  margin: 0 auto;
}

.header-icon {
  font-size: 32px;
  animation: bell-ring 2s ease-in-out infinite;
}

@keyframes bell-ring {
  0%, 100% { transform: rotate(0deg); }
  10%, 30% { transform: rotate(-10deg); }
  20%, 40% { transform: rotate(10deg); }
}

.pwa-header h1 {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: 0.5px;
  text-transform: uppercase;
}

/* Main Content */
.pwa-main {
  flex: 1;
  padding: 0;
  max-width: 100%;
  margin: 0 auto;
  width: 100%;
}

/* Tablet & Desktop */
@media (min-width: 768px) {
  .pwa-header h1 {
    font-size: 22px;
  }

  .pwa-main {
    padding: 20px;
    max-width: 1200px;
  }
}

/* Desktop only */
@media (min-width: 1024px) {
  .pwa-header h1 {
    font-size: 24px;
  }
}
</style>