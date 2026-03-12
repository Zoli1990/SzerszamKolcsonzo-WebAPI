// ============================================================================
// 6. src/stores/eszkozStore.js - Pinia Store (eszközök)
// ============================================================================
import { startSignalR, stopSignalR } from '../services/signalRService.js'
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { eszkozService } from '@/services/eszkozService'

export const useEszkozStore = defineStore('eszkoz', () => {
  // State
  const eszkozok = ref([])
  const loading = ref(false)
  const error = ref(null)
  const selectedKategoriaId = ref(null)
  let signalrStarted = false

  // Getters
  const filteredEszkozok = computed(() => {
    if (!selectedKategoriaId.value) return eszkozok.value
    return eszkozok.value.filter((e) => e.kategoriaID === selectedKategoriaId.value)
  })

  const elerhetoEszkozok = computed(() => {
    return eszkozok.value.filter((e) => e.status === 'Elerheto')
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

  // ═══════════════════════════════════════════════════════════════════
  // SIGNALR — valós idejű státusz frissítés
  // ═══════════════════════════════════════════════════════════════════
  function initSignalR() {
    if (signalrStarted) return
    signalrStarted = true

    startSignalR((data) => {
      const id = data.eszkozId ?? data.EszkozId
      const ujStatusz = data.ujStatusz ?? data.UjStatusz

      const index = eszkozok.value.findIndex((e) => e.eszkozID === id)

      if (index !== -1) {
        eszkozok.value[index] = {
          ...eszkozok.value[index],
          status: ujStatusz,
        }

        console.log(`[Store] ${eszkozok.value[index].nev} → ${ujStatusz}`)
      } else {
        fetchEszkozok(selectedKategoriaId.value)
      }
    })
  }

  function destroySignalR() {
    signalrStarted = false
    stopSignalR()
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
    clearFilter,
    initSignalR, // ÚJ
    destroySignalR, // ÚJ
  }
})
