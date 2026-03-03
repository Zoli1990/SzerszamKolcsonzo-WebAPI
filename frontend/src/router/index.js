// ============================================================================
// src/router/index.js - FRISSÍTETT (PWA route + standalone detection)
// ============================================================================

import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/profil',
      name: 'profil',
      component: () => import('../views/ProfilView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/profilom',
      name: 'profilom',
      component: () => import('../views/ProfilomView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/admin/eszkoz/uj',
      name: 'eszkoz-uj',
      component: () => import('../components/admin/AdminEszkozWizard.vue'),
      meta: { requiresAuth: true, requiresAdmin: true },
    },
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('../views/admin/AdminView.vue'),
      meta: { requiresAuth: true, requiresAdmin: true },
    },

    // ═══════════════════════════════════════════════════════════════════
    // PWA ADMIN — önálló mobil alkalmazás saját navigációval
    // ═══════════════════════════════════════════════════════════════════
    {
      path: '/pwa',
      component: () => import('../components/pwa/PwaShell.vue'),
      meta: { isPwa: true },
      children: [
        {
          path: '',
          name: 'pwa-dashboard',
          component: () => import('../components/pwa/PwaDashboard.vue'),
        },
        {
          path: 'foglalasok',
          name: 'pwa-foglalasok',
          component: () => import('../components/pwa/PwaFoglalasok.vue'),
        },
        {
          path: 'eszkozok',
          name: 'pwa-eszkozok',
          component: () => import('../components/pwa/PwaEszkozok.vue'),
        },
        {
          path: 'eszkozok/uj',
          name: 'pwa-eszkoz-uj',
          component: () => import('../components/admin/AdminEszkozWizard.vue'),
        },
        {
          path: 'kategoriak',
          name: 'pwa-kategoriak',
          component: () => import('../components/pwa/PwaKategoriak.vue'),
        },
      ],
    },
  ],
})

// ═══════════════════════════════════════════════════════════════════════════
// STANDALONE DETECTION
// Telepített PWA-ból indítva → ha admin → /pwa-ra irányít
// ═══════════════════════════════════════════════════════════════════════════
function isStandalonePwa() {
  return (
    window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true // iOS Safari
  )
}

// Route guard
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // Várjuk meg az auth inicializálást
  if (authStore.loading) {
    await new Promise((resolve) => {
      const checkInterval = setInterval(() => {
        if (!authStore.loading) {
          clearInterval(checkInterval)
          resolve()
        }
      }, 50)
    })
  }

  // ═══════════════════════════════════════════════════════════════════
  // STANDALONE PWA AUTO-REDIRECT
  // Ha standalone módban fut + admin + a főoldalra navigál → /pwa
  // ═══════════════════════════════════════════════════════════════════
  if (isStandalonePwa() && authStore.isAuthenticated && authStore.isAdmin && to.name === 'home') {
    return next({ name: 'pwa-dashboard' })
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
