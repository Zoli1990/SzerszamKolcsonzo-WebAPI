// ============================================================================
// 7. src/stores/kosarStore.js - Pinia Store (kosár)
// ============================================================================

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useKosarStore = defineStore('kosar', () => {
  // State
  const items = ref([])

  // Getters
  const itemCount = computed(() => items.value.length)

  const osszesOra = computed(() => {
    return items.value.reduce((sum, item) => sum + item.oraSzam, 0)
  })

  const osszesAr = computed(() => {
    return items.value.reduce((sum, item) => {
      return sum + item.eszkoz.kiadasiAr * item.oraSzam
    }, 0)
  })

  // Actions
  function addItem(eszkoz, foglalasKezdete, foglalasVege, oraSzam) {
    // Ellenőrzés: már benne van-e?
    const exists = items.value.find(item => item.eszkoz.eszkozID === eszkoz.eszkozID)
    if (exists) {
      alert('Ez az eszköz már a kosárban van!')
      return false
    }

    items.value.push({
      eszkoz,
      foglalasKezdete,
      foglalasVege,
      oraSzam
    })
    return true
  }

  function removeItem(eszkozId) {
    const index = items.value.findIndex(item => item.eszkoz.eszkozID === eszkozId)
    if (index > -1) {
      items.value.splice(index, 1)
    }
  }

  function updateItem(eszkozId, oraSzam, foglalasKezdete, foglalasVege) {
    const item = items.value.find(item => item.eszkoz.eszkozID === eszkozId)
    if (item) {
      item.oraSzam = oraSzam
      item.foglalasKezdete = foglalasKezdete
      item.foglalasVege = foglalasVege
    }
  }

  function clear() {
    items.value = []
  }

  return {
    items,
    itemCount,
    osszesOra,
    osszesAr,
    addItem,
    removeItem,
    updateItem,
    clear
  }
})

