// ============================================================================
// 6. src/stores/eszkozStore.js - Pinia Store (eszközök)
// ============================================================================

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { eszkozService } from '@/services/eszkozService'

export const useEszkozStore = defineStore('eszkoz', () => {
  // State
  const eszkozok = ref([])
  const loading = ref(false)
  const error = ref(null)
  const selectedKategoriaId = ref(null)

  // Getters
  const filteredEszkozok = computed(() => {
    if (!selectedKategoriaId.value) return eszkozok.value
    return eszkozok.value.filter(e => e.kategoriaID === selectedKategoriaId.value)
  })

  const elerhetoEszkozok = computed(() => {
    return eszkozok.value.filter(e => e.status === 'Elerheto')
  })

  // Actions
  async function fetchEszkozok(kategoriaId = null) {
    loading.value = true
    error.value = null
    try {
      const response = await eszkozService.getAll(kategoriaId)
      eszkozok.value = response.data
    } catch (err) {
      error.value = err.message
      console.error('Hiba az eszközök betöltése közben:', err)
    } finally {
      loading.value = false
    }
  }

  function setKategoriaFilter(kategoriaId) {
    selectedKategoriaId.value = kategoriaId
  }

  function clearFilter() {
    selectedKategoriaId.value = null
  }

  return {
    eszkozok,
    loading,
    error,
    selectedKategoriaId,
    filteredEszkozok,
    elerhetoEszkozok,
    fetchEszkozok,
    setKategoriaFilter,
    clearFilter
  }
})
