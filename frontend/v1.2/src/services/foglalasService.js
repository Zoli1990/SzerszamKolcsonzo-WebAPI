// ============================================================================
// 5. src/services/foglalasService.js - Foglalás API
// ============================================================================

import api from './api'

export const foglalasService = {
  // Új foglalás létrehozása
  create(data) {
    return api.post('/foglalasok', data)
  },

  // Összes foglalás (admin)
  getAll(eszkozId = null) {
    const params = eszkozId ? { eszkozId } : {}
    return api.get('/foglalasok', { params })
  },

  // Egy foglalás részletei
  getById(id) {
    return api.get(`/foglalasok/${id}`)
  },

  // Foglalás lezárása (admin)
  lezaras(id) {
    return api.put(`/foglalasok/${id}/lezaras`)
  },
}
