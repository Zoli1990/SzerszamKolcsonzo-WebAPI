// ============================================================================
// src/stores/authStore.js - JAVÍTOTT (Összes profil adattal)
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

  // ✅ JAVÍTOTT: userProfile getter - összes mező
  const userProfile = computed(() => {
    if (!user.value) return null
    return {
      email: user.value.email,
      nev: user.value.nev || '',
      telefonszam: user.value.telefonszam || '',
      iranyitoszam: user.value.iranyitoszam || '',
      telepules: user.value.telepules || '',
      utca: user.value.utca || '',
      hazszam: user.value.hazszam || '',
      cim: user.value.cim || ''
    }
  })

  // Actions
  function initialize() {
    loading.value = true

    const savedToken = authService.getToken()
    const savedUser = authService.getUser()

    if (savedToken) {
      token.value = savedToken

      try {
        const payload = parseJwt(savedToken)

        // Token lejárat ellenőrzés
        const now = Math.floor(Date.now() / 1000)
        if (payload.exp && payload.exp < now) {
          signOut()
        } else {
          // User adatok visszaállítása
          if (savedUser) {
            user.value = savedUser
          } else {
            user.value = {
              email: payload.email,
              role: payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
              userId: payload.sub,
              nev: '',
              telefonszam: '',
              iranyitoszam: '',
              telepules: '',
              utca: '',
              hazszam: '',
              cim: ''
            }
          }
        }
      } catch (error) {
        console.error('Token dekódolási hiba:', error)
        signOut()
      }
    }

    loading.value = false
  }

  // ✅ JAVÍTOTT: signIn - összes mező mentése
  // ✅ JAVÍTOTT: signIn - Profil automatikus betöltése
async function signIn(email, password) {
  loading.value = true

  try {
    const response = await authService.login(email, password)

    token.value = response.token
    authService.saveToken(response.token)

    user.value = {
      email: response.email,
      role: response.role,
      expiresAt: response.expiresAt,
      nev: response.nev || '',
      telefonszam: response.telefonszam || '',
      iranyitoszam: response.iranyitoszam || '',
      telepules: response.telepules || '',
      utca: response.utca || '',
      hazszam: response.hazszam || '',
      cim: response.cim || ''
    }

    authService.saveUser(user.value)

    // ✅ Profil automatikus betöltése bejelentkezés után
    try {
      console.log('🔄 Fetching profile after login...')
      await fetchProfile()
      console.log('✅ Profile fetched successfully')
    } catch (profileError) {
      console.warn('⚠️ Profile fetch failed:', profileError)
      // Nem throw-oljuk, hogy a login sikeres legyen akkor is
    }

    return response
  } catch (error) {
    console.error('Bejelentkezési hiba:', error)
    throw error
  } finally {
    loading.value = false
  }
}

  // ✅ JAVÍTOTT: signUp - összes mező
  async function signUp(email, password, iranyitoszam, telepules, utca, hazszam, telefonszam) {
    loading.value = true

    try {
      const response = await authService.register(email, password, iranyitoszam, telepules, utca, hazszam, telefonszam)

      token.value = response.token
      authService.saveToken(response.token)

      user.value = {
        email: response.email,
        role: response.role,
        expiresAt: response.expiresAt,
        nev: '',
        telefonszam: response.telefonszam || '',
        iranyitoszam: response.iranyitoszam || '',
        telepules: response.telepules || '',
        utca: response.utca || '',
        hazszam: response.hazszam || '',
        cim: response.cim || ''
      }

      authService.saveUser(user.value)

      return response
    } catch (error) {
      console.error('Regisztrációs hiba:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  // ✅ JAVÍTOTT: updateProfile - összes mező
  async function updateProfile(profileData) {
    loading.value = true

    try {
      const response = await authService.updateProfile(profileData)

      user.value = {
        ...user.value,
        nev: response.nev || '',
        telefonszam: response.telefonszam || '',
        iranyitoszam: response.iranyitoszam || '',
        telepules: response.telepules || '',
        utca: response.utca || '',
        hazszam: response.hazszam || '',
        cim: response.cim || ''
      }

      authService.saveUser(user.value)

      return response
    } catch (error) {
      console.error('Profil frissítési hiba:', error)
      throw error
    } finally {
      loading.value = false
    }
  }

  // ✅ JAVÍTOTT: fetchProfile - összes mező
  async function fetchProfile() {
    try {
      const response = await authService.getProfile()

      user.value = {
        ...user.value,
        nev: response.nev || '',
        telefonszam: response.telefonszam || '',
        iranyitoszam: response.iranyitoszam || '',
        telepules: response.telepules || '',
        utca: response.utca || '',
        hazszam: response.hazszam || '',
        cim: response.cim || ''
      }

      authService.saveUser(user.value)

      return response
    } catch (error) {
      console.error('Profil lekérési hiba:', error)
      throw error
    }
  }

  // Kijelentkezés
  function signOut() {
    user.value = null
    token.value = null
    authService.removeToken()
    authService.removeUser()
  }

  // Helper: JWT token dekódolása
  function parseJwt(token) {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
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
    userProfile,
    initialize,
    signIn,
    signUp,
    signOut,
    updateProfile,
    fetchProfile
  }
})