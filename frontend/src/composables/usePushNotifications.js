// ============================================================================
// src/composables/usePushNotifications.js
// ============================================================================

import { ref, computed, onMounted, onUnmounted } from 'vue'
import { pushService } from '@/services/pushService'
import { useAuthStore } from '@/stores/authStore'

export function usePushNotifications() {
  const authStore = useAuthStore()
  
  const isSupported = ref(false)
  const isSubscribed = ref(false)
  const permission = ref('default')
  const loading = ref(false)
  const error = ref(null)
  const updateAvailable = ref(false)

  const canSubscribe = computed(() => {
    return isSupported.value && permission.value !== 'denied'
  })

  const statusText = computed(() => {
    if (!isSupported.value) return 'Nem támogatott'
    if (permission.value === 'denied') return 'Letiltva'
    if (isSubscribed.value) return 'Bekapcsolva'
    return 'Kikapcsolva'
  })

  const statusIcon = computed(() => {
    if (!isSupported.value) return '🚫'
    if (permission.value === 'denied') return '🔕'
    if (isSubscribed.value) return '🔔'
    return '🔕'
  })

  onMounted(async () => {
    await initialize()
    window.addEventListener('sw-update-available', onUpdateAvailable)
  })

  onUnmounted(() => {
    window.removeEventListener('sw-update-available', onUpdateAvailable)
  })

  async function initialize() {
    isSupported.value = pushService.isPushSupported()
    
    if (!isSupported.value) return

    await pushService.registerServiceWorker()
    permission.value = pushService.getPermissionStatus()
    
    const status = await pushService.getSubscriptionStatus()
    isSubscribed.value = status.isSubscribed
  }

  async function subscribe() {
    if (!canSubscribe.value) {
      error.value = 'Értesítések nem engedélyezettek'
      return false
    }

    loading.value = true
    error.value = null

    try {
      const userEmail = authStore.userEmail || null
      await pushService.subscribe(userEmail)
      
      isSubscribed.value = true
      permission.value = pushService.getPermissionStatus()
      
      return true
    } catch (err) {
      error.value = err.message || 'Feliratkozás sikertelen'
      return false
    } finally {
      loading.value = false
    }
  }

  async function unsubscribe() {
    loading.value = true
    error.value = null

    try {
      await pushService.unsubscribe()
      isSubscribed.value = false
      return true
    } catch (err) {
      error.value = err.message || 'Leiratkozás sikertelen'
      return false
    } finally {
      loading.value = false
    }
  }

  async function toggleSubscription() {
    if (isSubscribed.value) {
      return unsubscribe()
    } else {
      return subscribe()
    }
  }

  async function requestPermission() {
    loading.value = true
    error.value = null

    try {
      const result = await pushService.requestPermission()
      permission.value = result
      
      if (result === 'granted') {
        await subscribe()
      }
      
      return result
    } catch (err) {
      error.value = 'Engedély kérése sikertelen'
      return 'denied'
    } finally {
      loading.value = false
    }
  }

  async function sendTestNotification() {
    if (!isSupported.value || permission.value !== 'granted') {
      error.value = 'Értesítések nem engedélyezettek'
      return
    }

    await pushService.showLocalNotification('🧪 Teszt értesítés', {
      body: 'Ez egy teszt értesítés a Szerszámkölcsönzőtől!',
      tag: 'test'
    })
  }

  function onUpdateAvailable() {
    updateAvailable.value = true
  }

  async function updateApp() {
    await pushService.updateServiceWorker()
  }

  return {
    isSupported,
    isSubscribed,
    permission,
    loading,
    error,
    updateAvailable,
    canSubscribe,
    statusText,
    statusIcon,
    initialize,
    subscribe,
    unsubscribe,
    toggleSubscription,
    requestPermission,
    sendTestNotification,
    updateApp
  }
}
