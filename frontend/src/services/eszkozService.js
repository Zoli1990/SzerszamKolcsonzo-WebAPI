// ============================================================================
// 3. src/services/eszkozService.js - Eszköz API hívások
// ============================================================================

import api from './api'

export const eszkozService = {
  // Összes eszköz lekérése (publikus DTO)
  getAll(kategoriaId = null) {
    const params = kategoriaId ? { kategoriaId } : {}
    return api.get('/eszkozok', { params })
  },

  // 🔹 ADMIN: részletes lista lekérése (EszkozDetailDto)
  getAllAdmin() {
    return api.get('/eszkozok/admin')
  },

  // Egy eszköz részletei
  getById(id) {
    return api.get(`/eszkozok/${id}`)
  },

  // Admin: új eszköz
  //create(data) {
    //return api.post('/eszkozok', data)
  //},

  // Admin: új eszköz
  create(data) {
    const formData = new FormData()
    formData.append('kategoriaID', data.kategoriaID)
    formData.append('nev', data.nev)
    formData.append('leiras', data.leiras || '')
    formData.append('vetelar', data.vetelar)
    formData.append('kiadasiAr', data.kiadasiAr)
    formData.append('beszerzesiDatum', data.beszerzesiDatum)
    return api.post('/eszkozok', formData, {
      headers: { 'Content-Type': undefined }
  })
},


  // Admin: eszköz módosítása
  update(id, data) {
    return api.put(`/eszkozok/${id}`, data)
  },

  // Admin: eszköz törlése
  delete(id) {
    return api.delete(`/eszkozok/${id}`)
  }
}
