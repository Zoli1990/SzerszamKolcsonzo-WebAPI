<template>
  <div class="notification-bell">
    <button 
      class="bell-button" 
      :class="{ active: isSubscribed, disabled: !canSubscribe }"
      @click="toggleMenu"
      :disabled="loading"
      :title="statusText"
    >
      <span class="bell-icon">{{ statusIcon }}</span>
      <span v-if="loading" class="loading-spinner"></span>
    </button>

    <Transition name="dropdown">
      <div v-if="menuOpen" class="notification-menu" @click.stop>
        <div class="menu-header">
          <h4>🔔 Értesítések</h4>
          <button class="close-btn" @click="menuOpen = false">×</button>
        </div>

        <div class="menu-content">
          <div class="status-section">
            <div class="status-row">
              <span class="label">Állapot:</span>
              <span :class="['status-badge', statusClass]">{{ statusText }}</span>
            </div>
            
            <div v-if="!isSupported" class="warning-text">
              ⚠️ A böngésződ nem támogatja az értesítéseket
            </div>
            
            <div v-else-if="permission === 'denied'" class="warning-text">
              ⚠️ Az értesítések le vannak tiltva a böngésző beállításaiban.
            </div>
          </div>

          <div v-if="canSubscribe" class="settings-section">
            <label class="toggle-row">
              <span>Push értesítések</span>
              <div 
                class="toggle-switch" 
                :class="{ on: isSubscribed }"
                @click="toggleSubscription"
              >
                <div class="toggle-thumb"></div>
              </div>
            </label>
          </div>

          <div v-if="isSubscribed" class="test-section">
            <button class="btn-test" @click="sendTestNotification">
              🧪 Teszt értesítés
            </button>
          </div>

          <div v-if="error" class="error-section">
            <span class="error-text">❌ {{ error }}</span>
          </div>
        </div>
      </div>
    </Transition>

    <Transition name="slide">
      <div v-if="updateAvailable" class="update-banner">
        <span>🔄 Új verzió elérhető!</span>
        <button @click="updateApp">Frissítés</button>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { usePushNotifications } from '@/composables/usePushNotifications'

const {
  isSupported,
  isSubscribed,
  permission,
  loading,
  error,
  updateAvailable,
  canSubscribe,
  statusText,
  statusIcon,
  toggleSubscription,
  sendTestNotification,
  updateApp
} = usePushNotifications()

const menuOpen = ref(false)

const statusClass = computed(() => {
  if (!isSupported.value) return 'status-unsupported'
  if (permission.value === 'denied') return 'status-denied'
  if (isSubscribed.value) return 'status-active'
  return 'status-inactive'
})

function toggleMenu() {
  menuOpen.value = !menuOpen.value
}

function handleClickOutside(event) {
  if (menuOpen.value && !event.target.closest('.notification-bell')) {
    menuOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.notification-bell {
  position: relative;
}

.bell-button {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: 2px solid #d4c5b0;
  background: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  position: relative;
}

.bell-button:hover:not(.disabled) {
  border-color: #6b8e23;
  transform: scale(1.05);
}

.bell-button.active {
  border-color: #6b8e23;
  background: #f0f7e6;
}

.bell-button.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.bell-icon {
  font-size: 20px;
}

.loading-spinner {
  position: absolute;
  width: 20px;
  height: 20px;
  border: 2px solid #e8dcc8;
  border-top-color: #6b8e23;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.notification-menu {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  width: 280px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(61, 47, 31, 0.2);
  border: 1px solid #e8dcc8;
  z-index: 1000;
  overflow: hidden;
}

.menu-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 16px;
  border-bottom: 1px solid #e8dcc8;
  background: #f5f1e8;
}

.menu-header h4 {
  margin: 0;
  font-size: 15px;
  color: #3d2f1f;
}

.close-btn {
  width: 26px;
  height: 26px;
  border: none;
  background: none;
  font-size: 22px;
  color: #6b5d4f;
  cursor: pointer;
  border-radius: 4px;
}

.close-btn:hover {
  background: #e8dcc8;
}

.menu-content {
  padding: 14px;
}

.status-section {
  margin-bottom: 14px;
}

.status-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.label {
  font-size: 13px;
  color: #6b5d4f;
}

.status-badge {
  padding: 4px 10px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 600;
}

.status-active {
  background: #d1fae5;
  color: #065f46;
}

.status-inactive {
  background: #f3f4f6;
  color: #6b7280;
}

.status-denied {
  background: #fee2e2;
  color: #991b1b;
}

.status-unsupported {
  background: #fef3c7;
  color: #92400e;
}

.warning-text {
  font-size: 11px;
  color: #92400e;
  background: #fef3c7;
  padding: 8px 10px;
  border-radius: 6px;
  margin-top: 10px;
}

.settings-section {
  border-top: 1px solid #e8dcc8;
  padding-top: 14px;
}

.toggle-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 600;
  font-size: 13px;
  color: #3d2f1f;
}

.toggle-switch {
  width: 44px;
  height: 24px;
  background: #d4c5b0;
  border-radius: 12px;
  cursor: pointer;
  position: relative;
  transition: background 0.2s;
}

.toggle-switch.on {
  background: #6b8e23;
}

.toggle-thumb {
  width: 20px;
  height: 20px;
  background: white;
  border-radius: 50%;
  position: absolute;
  top: 2px;
  left: 2px;
  transition: transform 0.2s;
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}

.toggle-switch.on .toggle-thumb {
  transform: translateX(20px);
}

.test-section {
  border-top: 1px solid #e8dcc8;
  padding-top: 14px;
  margin-top: 14px;
}

.btn-test {
  width: 100%;
  padding: 10px;
  background: #f5f1e8;
  border: 1px solid #d4c5b0;
  border-radius: 6px;
  color: #3d2f1f;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-test:hover {
  background: #e8dcc8;
}

.error-section {
  margin-top: 10px;
  padding: 8px 10px;
  background: #fee2e2;
  border-radius: 6px;
}

.error-text {
  font-size: 11px;
  color: #991b1b;
}

.update-banner {
  position: fixed;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: #3b82f6;
  color: white;
  padding: 12px 20px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 14px;
  box-shadow: 0 10px 40px rgba(59, 130, 246, 0.3);
  z-index: 9999;
  font-size: 14px;
}

.update-banner button {
  padding: 6px 14px;
  background: white;
  color: #3b82f6;
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  font-size: 13px;
}

.dropdown-enter-active,
.dropdown-leave-active {
  transition: all 0.2s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}

.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  transform: translate(-50%, 20px);
}
</style>
