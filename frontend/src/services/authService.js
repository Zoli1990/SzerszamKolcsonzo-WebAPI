// ============================================================================
// src/services/authService.js - JAVÍTOTT (Összes profil mezővel)
// ============================================================================

import api from './api'

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
    // Teljes cím összeállítása
    const cim = `${iranyitoszam} ${telepules}, ${utca} ${hazszam}`.trim()
    
    const response = await api.post('/auth/register', { 
      email, 
      password,
      iranyitoszam,
      telepules,
      utca,
      hazszam,
      telefonszam,
      cim
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
   * Token tárolás
   */
  saveToken(token) {
    localStorage.setItem('auth_token', token)
  },

  /**
   * Token lekérés
   */
  getToken() {
    return localStorage.getItem('auth_token')
  },

  /**
   * Token törlés
   */
  removeToken() {
    localStorage.removeItem('auth_token')
  },

  /**
   * User adatok tárolás
   */
  saveUser(user) {
    localStorage.setItem('auth_user', JSON.stringify(user))
  },

  /**
   * User adatok lekérés
   */
  getUser() {
    const userData = localStorage.getItem('auth_user')
    return userData ? JSON.parse(userData) : null
  },

  /**
   * User adatok törlés
   */
  removeUser() {
    localStorage.removeItem('auth_user')
  }
}