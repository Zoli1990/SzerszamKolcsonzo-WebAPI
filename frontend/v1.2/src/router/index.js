// ============================================================================
// 5. src/router/index.js - FRISSÍTETT (Route guards)
// ============================================================================

import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('../views/admin/AdminView.vue'),
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/profil',
      name: 'profil',
      component: () => import('../views/ProfilView.vue'),
      meta: { requiresAuth: true }
    }
  ]
})

// Route guard
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // Várjuk meg az auth inicializálást
  if (authStore.loading) {
    await new Promise(resolve => {
      const checkInterval = setInterval(() => {
        if (!authStore.loading) {
          clearInterval(checkInterval)
          resolve()
        }
      }, 50)
    })
  }

  // Védett route ellenőrzés
  if (to.meta.requiresAuth) {
    if (!authStore.isAuthenticated) {
      alert('Kérjük, jelentkezz be!')
      return next({ name: 'home' })
    }
  }

  // Admin jogosultság ellenőrzés
  if (to.meta.requiresAdmin) {
    if (!authStore.isAdmin) {
      alert('Nincs jogosultságod az admin felülethez!')
      return next({ name: 'home' })
    }
  }

  next()
})

export default router
