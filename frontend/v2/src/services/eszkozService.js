// ============================================================================
// 3. src/services/eszkozService.js - Eszköz API hívások
// ============================================================================

import api from './api'

export const eszkozService = {
  // Összes eszköz lekérése
  getAll(kategoriaId = null) {
    const params = kategoriaId ? { kategoriaId } : {}
    return api.get('/eszkozok', { params })
  },

  // Egy eszköz részletei
  getById(id) {
    return api.get(`/eszkozok/${id}`)
  },

  // Admin: új eszköz (később)
  create(data) {
    return api.post('/eszkozok', data)
  },

  // Admin: eszköz módosítása (később)
  update(id, data) {
    return api.put(`/eszkozok/${id}`, data)
  },

  // Admin: eszköz törlése (később)
  delete(id) {
    return api.delete(`/eszkozok/${id}`)
  }
}

