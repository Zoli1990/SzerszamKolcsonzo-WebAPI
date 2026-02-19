// ============================================================================
// src/services/api.js - JWT token interceptor + BASE_URL
// ============================================================================

import axios from 'axios'
import { authService } from './authService'

// ✅ BASE_URL (CSAK szerver cím, /api nélkül!)
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

const api = axios.create({
  baseURL: `${BASE_URL}/api`,  // ← /api ITT van hozzáadva!
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor - JWT token hozzáadása
api.interceptors.request.use(
  (config) => {
    const token = authService.getToken()
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - hibakezelés
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // 401 Unauthorized - token lejárt vagy érvénytelen
    if (error.response?.status === 401) {
      // Token törlése
      authService.removeToken()
      
      // Átirányítás login-ra (opcionális)
      // window.location.href = '/'
      
      console.error('Autentikációs hiba - jelentkezz be újra!')
    }
    
    console.error('API Error:', error.response?.data || error.message)
    return Promise.reject(error)
  }
)

// ✅ Export BASE_URL is (képek URL-jéhez kell!)
export { api as default, BASE_URL }