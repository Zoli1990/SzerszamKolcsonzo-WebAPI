// ============================================================================
// src/services/authService.js - PWA KÜLÖN AUTH TÁMOGATÁS
// ============================================================================
//
// A PWA (standalone mód) és a böngésző KÜLÖN localStorage kulcsokat használ:
//   - Böngésző: auth_token, auth_user
//   - PWA:      pwa_auth_token, pwa_auth_user
//
// Így a böngészős kijelentkezés NEM törli a PWA tokent és fordítva.
// ============================================================================

import api from './api'

// ═══════════════════════════════════════════════════════════════════════════
// STANDALONE DETECTION — egyszer futtatva, cache-elve
// ═══════════════════════════════════════════════════════════════════════════
const _isStandalone =
  window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true // iOS Safari

function isStandalonePwa() {
  return _isStandalone
}

// ═══════════════════════════════════════════════════════════════════════════
// STORAGE KULCSOK — PWA vs böngésző
// ═══════════════════════════════════════════════════════════════════════════
function getTokenKey() {
  return isStandalonePwa() ? 'pwa_auth_token' : 'auth_token'
}

function getUserKey() {
  return isStandalonePwa() ? 'pwa_auth_user' : 'auth_user'
}

// ═══════════════════════════════════════════════════════════════════════════
// AUTH SERVICE
// ═══════════════════════════════════════════════════════════════════════════
export const authService = {
  /**
   * Bejelentkezés
   */
  async login(email, password) {
    const response = await api.post('/auth/login', { email, password })
    return response.data
  },

  /**
   * Regisztráció kibővített cím mezőkkel
   */
  async register(email, password, iranyitoszam, telepules, utca, hazszam, telefonszam) {
    const cim = `${iranyitoszam} ${telepules}, ${utca} ${hazszam}`.trim()

    const response = await api.post('/auth/register', {
      email,
      password,
      iranyitoszam,
      telepules,
      utca,
      hazszam,
      telefonszam,
      cim,
    })
    return response.data
  },

  /**
   * Profil lekérése
   */
  async getProfile() {
    const response = await api.get('/auth/profile')
    return response.data
  },

  /**
   * Profil frissítése
   */
  async updateProfile(profileData) {
    const response = await api.put('/auth/profile', profileData)
    return response.data
  },

  /**
   * Token tárolás — PWA vs böngésző külön kulcs
   */
  saveToken(token) {
    localStorage.setItem(getTokenKey(), token)
  },

  /**
   * Token lekérés
   */
  getToken() {
    return localStorage.getItem(getTokenKey())
  },

  /**
   * Token törlés — CSAK a saját kontextusét törli
   */
  removeToken() {
    localStorage.removeItem(getTokenKey())
  },

  /**
   * User adatok tárolás
   */
  saveUser(user) {
    localStorage.setItem(getUserKey(), JSON.stringify(user))
  },

  /**
   * User adatok lekérés
   */
  getUser() {
    const userData = localStorage.getItem(getUserKey())
    return userData ? JSON.parse(userData) : null
  },

  /**
   * User adatok törlés
   */
  removeUser() {
    localStorage.removeItem(getUserKey())
  },

  /**
   * Standalone PWA mód lekérdezés (más komponensekből is használható)
   */
  isStandalonePwa() {
    return isStandalonePwa()
  },
}
