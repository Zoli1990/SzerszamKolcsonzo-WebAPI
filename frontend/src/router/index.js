// ============================================================================
// src/router/index.js - HASH MODE + Wizard route
// ============================================================================

import { createRouter, createWebHashHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHashHistory(), // HASH MODE (#/route)
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
    // ✨ ÚJ ROUTE - WIZARD
    {
      path: '/admin/eszkoz/uj',
      name: 'UjEszkoz',
      component: () => import('../components/admin/AdminEszkozWizard.vue'),
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/admin-pwa',
      name: 'admin-pwa',
      component: () => import('../views/admin/AdminPwaView.vue'),
      meta: { requiresAuth: true, requiresAdmin: true, isPwa: true }
    },
    {
      path: '/profil',
      name: 'profil',
      component: () => import('../views/ProfilView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/profilom',
      name: 'profilom',
      component: () => import('../views/ProfilomView.vue'),
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