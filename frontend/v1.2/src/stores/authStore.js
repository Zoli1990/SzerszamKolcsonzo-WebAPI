// ============================================================================
// 2. src/stores/authStore.js - FRISSÍTETT (Supabase nélkül)
// ============================================================================

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/authService'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref(null)
  const token = ref(null)
  const loading = ref(false)

  // Getters
  const isAuthenticated = computed(() => !!user.value && !!token.value)

  const isAdmin = computed(() => {
    return user.value?.role === 'Admin'
  })

  const userEmail = computed(() => user.value?.email)

  // Actions
  function initialize() {
    loading.value = true

    // Token visszaállítása localStorage-ból
    const savedToken = authService.getToken()

    if (savedToken) {
      token.value = savedToken

      // Token dekódolása (JWT payload kinyerése)
      try {
        const payload = parseJwt(savedToken)

        // Token lejárat ellenőrzés
        const now = Math.floor(Date.now() / 1000)
        if (payload.exp && payload.exp < now) {
          // Token lejárt
          signOut()
        } else {
          // User adatok visszaállítása
          user.value = {
            email: payload.email,
            role:
              payload.role ||
              payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
            userId: payload.sub,
          }
        }
      } catch (error) {
        console.error('Token dekódolási hiba:', error)
        signOut()
      }
    }

    loading.value = false
  }

  async function signIn(email, password) {
    loading.value = true

    try {
      const response = await authService.login(email, password)

      // Token tárolás
      token.value = response.token
      authService.saveToken(response.token)

      // User adatok
      user.value = {
        email: response.email,
        role: response.role,
        expiresAt: response.expiresAt,
      }

      return response
    } catch (error) {
      console.error('Bejelentkezési hiba:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  async function signUp(email, password) {
    loading.value = true

    try {
      const response = await authService.register(email, password)

      // Automatikus bejelentkezés regisztráció után
      token.value = response.token
      authService.saveToken(response.token)

      user.value = {
        email: response.email,
        role: response.role,
        expiresAt: response.expiresAt,
      }

      return response
    } catch (error) {
      console.error('Regisztrációs hiba:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  // kijelentkezés:
  function signOut() {
    user.value = null
    token.value = null
    authService.removeToken()
  }

  // Helper: JWT token dekódolása (Base64)
  function parseJwt(token) {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    )
    return JSON.parse(jsonPayload)
  }

  return {
    user,
    token,
    loading,
    isAuthenticated,
    isAdmin,
    userEmail,
    initialize,
    signIn,
    signUp,
    signOut,
  }
})
