// ============================================================================
// 1. src/views/admin/AdminView.vue - Fő admin oldal (tab rendszerrel)
// ============================================================================

<template>
  <div class="admin-container">
    <div class="admin-header">
      <h1>🔧 Admin Panel</h1>
      <RouterLink to="/" class="btn-back">← Vissza a főoldalra</RouterLink>
    </div>

    <!-- Tab menü -->
    <div class="tabs">
      <button
        v-for="tab in tabs"
        :key="tab.id"
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
      <EszkozokAdmin v-else-if="activeTab === 'eszkozok'" />
      <FoglalasokAdmin v-else-if="activeTab === 'foglalasok'" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import AdminDashboard from '@/components/admin/AdminDashboard.vue'
import KategoriakAdmin from '@/components/admin/KategoriakAdmin.vue'
import EszkozokAdmin from '@/components/admin/EszkozokAdmin.vue'
import FoglalasokAdmin from '@/components/admin/FoglalasokAdmin.vue'

const authStore = useAuthStore()
const router = useRouter()
const activeTab = ref('dashboard')

const tabs = [
  { id: 'dashboard', label: 'Dashboard', icon: '📊' },
  { id: 'kategoriak', label: 'Kategóriák', icon: '📁' },
  { id: 'eszkozok', label: 'Eszközök', icon: '🔨' },
  { id: 'foglalasok', label: 'Foglalások', icon: '📋' }
]

// ÚJ: Auth ellenőrzés minden oldalbetöltéskor
onMounted(() => {
  if (!authStore.isAuthenticated || !authStore.isAdmin) {
    alert('Nincs jogosultságod az admin felülethez!')
    router.push('/')
  }
})
</script>

<style scoped>
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
</style>