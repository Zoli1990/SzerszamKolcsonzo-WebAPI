// ============================================================================
// src/services/kategoriaService.js - FRISSÍTETT (create, update, delete)
// ============================================================================

import api from './api'

export const kategoriaService = {
  getAll() {
    return api.get('/kategoriak')
  },

  getById(id) {
    return api.get(`/kategoriak/${id}`)
  },

  create(data) {
    return api.post('/kategoriak', data)
  },

  update(id, data) {
    return api.put(`/kategoriak/${id}`, data)
  },

  delete(id) {
    return api.delete(`/kategoriak/${id}`)
  },
}
