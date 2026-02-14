// ============================================================================
// 5. src/services/foglalasService.js - Foglalás API
// ============================================================================

import api from './api'

export const foglalasService = {
  // Új foglalás létrehozása
  create(data) {
    return api.post('/Foglalasok', data)
  },

  // Összes foglalás (admin)
  getAll(eszkozId = null) {
    const params = eszkozId ? { eszkozId } : {}
    return api.get('/Foglalasok', { params })
  },

  // Egy foglalás részletei
  getById(id) {
    return api.get(`/Foglalasok/${id}`)
  },

  // Foglalás lezárása (admin)
  lezaras(id) {
    return api.put(`/Foglalasok/${id}/lezaras`)
  },

  // Admin kiadás
  kiadas(id) {
    return api.put(`/Foglalasok/${id}/kiadas`)
  },

  // Admin törlés
  torles(id) {
    return api.put(`/Foglalasok/${id}/torles`)
  },
}
