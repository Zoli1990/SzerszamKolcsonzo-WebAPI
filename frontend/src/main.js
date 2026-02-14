// ============================================================================
// src/main.js - Feltételes Service Worker regisztráció
// ============================================================================

import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/authStore'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

// Auth inicializálás (token visszaállítása)
const authStore = useAuthStore()
authStore.initialize()

app.mount('#app')

// ============================================================================
// SERVICE WORKER - Csak Admin PWA-n regisztráljuk
// ============================================================================

// Ellenőrizzük hogy admin-pwa route-on vagyunk-e
router.isReady().then(() => {
  const currentRoute = router.currentRoute.value
  
  // Service Worker CSAK admin-pwa route-on
  if (currentRoute.name === 'admin-pwa' && 'serviceWorker' in navigator) {
    registerAdminServiceWorker()
  } else {
    console.log('[Main] Service Worker NOT registered - not on admin-pwa route')
  }
})

// Router változás figyelése
router.afterEach((to, from) => {
  if (to.name === 'admin-pwa' && 'serviceWorker' in navigator) {
    registerAdminServiceWorker()
  }
})

async function registerAdminServiceWorker() {
  try {
    // Ellenőrizzük hogy már van-e regisztrált SW
    const existingRegistration = await navigator.serviceWorker.getRegistration()
    
    if (existingRegistration) {
      console.log('[Main] Service Worker already registered')
      return
    }

    // Regisztráljuk az admin SW-t
    const registration = await navigator.serviceWorker.register('/sw-admin.js', {
      scope: '/'
    })

    console.log('[Main] Admin Service Worker registered successfully:', registration.scope)

    // Frissítés ellenőrzése
    registration.addEventListener('updatefound', () => {
      const newWorker = registration.installing
      console.log('[Main] New Service Worker installing...')

      newWorker?.addEventListener('statechange', () => {
        if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
          console.log('[Main] New Service Worker available - please refresh')
        }
      })
    })
  } catch (error) {
    console.error('[Main] Service Worker registration failed:', error)
  }
}