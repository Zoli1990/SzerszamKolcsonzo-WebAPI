import api from './api'

export const authService = {
  /**
   * Bejelentkezés
   * @param {string} email 
   * @param {string} password 
   * @returns {Promise<{token: string, email: string, role: string, expiresAt: string}>}
   */
  async login(email, password) {
    const response = await api.post('/auth/login', { email, password })
    return response.data
  },

  /**
   * Regisztráció
   * @param {string} email 
   * @param {string} password 
   * @returns {Promise<{token: string, email: string, role: string, expiresAt: string}>}
   */
  async register(email, password) {
    const response = await api.post('/auth/register', { email, password })
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
  }
}