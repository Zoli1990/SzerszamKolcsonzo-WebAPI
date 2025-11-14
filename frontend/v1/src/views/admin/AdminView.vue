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
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import AdminDashboard from '@/components/admin/AdminDashboard.vue'
import KategoriakAdmin from '@/components/admin/KategoriakAdmin.vue'
import EszkozokAdmin from '@/components/admin/EszkozokAdmin.vue'
import FoglalasokAdmin from '@/components/admin/FoglalasokAdmin.vue'

const activeTab = ref('dashboard')

const tabs = [
  { id: 'dashboard', label: 'Dashboard', icon: '📊' },
  { id: 'kategoriak', label: 'Kategóriák', icon: '📁' },
  { id: 'eszkozok', label: 'Eszközök', icon: '🔨' },
  { id: 'foglalasok', label: 'Foglalások', icon: '📋' }
]
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
  color: #1f2937;
}

.btn-back {
  padding: 10px 20px;
  background: white;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  text-decoration: none;
  color: #374151;
  font-weight: 600;
  transition: all 0.2s;
}

.btn-back:hover {
  border-color: #3b82f6;
  color: #3b82f6;
}

.tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 24px;
  border-bottom: 2px solid #e5e7eb;
}

.tab {
  padding: 12px 24px;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  cursor: pointer;
  font-size: 16px;
  font-weight: 600;
  color: #6b7280;
  transition: all 0.2s;
}

.tab:hover {
  color: #3b82f6;
}

.tab.active {
  color: #3b82f6;
  border-bottom-color: #3b82f6;
}

.tab-content {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}
</style>