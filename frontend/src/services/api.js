// ============================================================================
// 3. src/services/api.js - FRISSÍTETT (JWT token interceptor)
// ============================================================================

import axios from 'axios'
import { authService } from './authService'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
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

export default api

