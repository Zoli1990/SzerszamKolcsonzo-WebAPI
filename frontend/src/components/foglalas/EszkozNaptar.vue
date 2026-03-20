<template>
  <div class="naptar-container">
    <!-- Navigáció -->
    <div class="naptar-header">
      <button class="nav-btn" @click="elozoNap" :disabled="isMinDatum">◀</button>
      <span class="naptar-datum">{{ displayDatum }}</span>
      <button class="nav-btn" @click="kovetkezoNap">▶</button>
    </div>

    <div v-if="betoltes" class="naptar-loading">Betöltés...</div>
    <div v-else-if="hiba" class="naptar-hiba">{{ hiba }}</div>

    <!-- Foglalások listája -->
    <div v-else-if="foglalasok.length > 0" class="foglalas-lista">
      <template v-for="f in foglalasok" :key="f.foglalasId">
        <div :class="['foglalas-sor', f.status === 'Kiadva' ? 'kiadva' : 'foglalt']">
          <span class="ido">{{ formatIdo(f.kezdet) }} – {{ formatIdo(f.veg) }}</span>
          <span class="badge">{{ f.status === 'Kiadva' ? 'KIADVA' : 'FOGLALT' }}</span>
        </div>
        <div class="buffer-sor">
          <span class="buffer-ido">{{ formatIdo(f.veg) }} – {{ formatBufferVeg(f.veg) }}</span>
          <span class="buffer-badge">🔧 átadás</span>
        </div>
      </template>
    </div>

    <!-- Szabad nap -->
    <div v-else class="szabad-nap">
      ✅ Ezen a napon nincs foglalás
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { foglalasService } from '@/services/foglalasService'
import { on as signalROn, off as signalROff } from '@/services/signalRService'

const props = defineProps({
  eszkozId: { type: Number, required: true },
  eszkozNev: { type: String, default: '' },
  initialDatum: { type: String, default: null },
})

const kivalasztottDatum = ref(props.initialDatum ?? new Date().toISOString().split('T')[0])
const foglalasok = ref([])
const betoltes = ref(false)
const hiba = ref(null)

const maDatum = new Date().toISOString().split('T')[0]
const isMinDatum = computed(() => kivalasztottDatum.value <= maDatum)

const displayDatum = computed(() => {
  const d = new Date(kivalasztottDatum.value)
  return d.toLocaleDateString('hu-HU', { year: 'numeric', month: 'long', day: 'numeric' })
})

function formatIdo(isoString) {
  const d = new Date(isoString)
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

function formatBufferVeg(isoString) {
  const d = new Date(isoString)
  d.setMinutes(d.getMinutes() + 30)
  return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

async function betoltNaptar() {
  betoltes.value = true
  hiba.value = null
  try {
    const res = await foglalasService.getEszkozNaptar(props.eszkozId, kivalasztottDatum.value)
    foglalasok.value = res.data
  } catch {
    hiba.value = 'Nem sikerült betölteni a naptárt.'
  } finally {
    betoltes.value = false
  }
}

function elozoNap() {
  const d = new Date(kivalasztottDatum.value)
  d.setDate(d.getDate() - 1)
  kivalasztottDatum.value = d.toISOString().split('T')[0]
}

function kovetkezoNap() {
  const d = new Date(kivalasztottDatum.value)
  d.setDate(d.getDate() + 1)
  kivalasztottDatum.value = d.toISOString().split('T')[0]
}

watch(kivalasztottDatum, betoltNaptar)

// SignalR — ha változik valami az eszközön, frissítünk
function handleSignalR(adat) {
  if (adat.eszkozId === props.eszkozId) {
    betoltNaptar()
  }
}

onMounted(() => {
  betoltNaptar()
  signalROn('EszkozStatuszValtozas', handleSignalR)
})

onUnmounted(() => {
  signalROff('EszkozStatuszValtozas', handleSignalR)
})
</script>

<style scoped>
.naptar-container { background: var(--color-surface); border-radius: var(--radius-lg); border: 1px solid var(--color-border); overflow: hidden; }
.naptar-header { display: flex; align-items: center; justify-content: space-between; padding: 10px var(--spacing-md); background: var(--color-background); border-bottom: 1px solid var(--color-border); }
.naptar-datum { font-size: 14px; font-weight: 600; color: var(--color-text); }
.nav-btn { background: none; border: 1px solid var(--color-border); border-radius: var(--radius-sm); padding: 4px 10px; cursor: pointer; font-size: 14px; color: var(--color-text-muted); }
.nav-btn:hover:not(:disabled) { background: var(--color-primary); color: white; border-color: var(--color-primary); }
.nav-btn:disabled { opacity: 0.3; cursor: not-allowed; }
.foglalas-lista { display: flex; flex-direction: column; padding: 8px var(--spacing-md); gap: 6px; }
.foglalas-sor { display: flex; align-items: center; justify-content: space-between; padding: 8px 12px; border-radius: var(--radius-sm); }
.foglalas-sor.foglalt { background: #fee2e2; }
.foglalas-sor.kiadva { background: #dbeafe; }
.ido { font-size: 14px; font-weight: 600; color: var(--color-text); }
.badge { font-size: 11px; font-weight: 700; letter-spacing: 0.5px; padding: 2px 8px; border-radius: 999px; }
.foglalas-sor.foglalt .badge { background: #fca5a5; color: #7f1d1d; }
.foglalas-sor.kiadva .badge { background: #93c5fd; color: #1e3a5f; }
.buffer-sor { display: flex; align-items: center; justify-content: space-between; padding: 5px 12px; background: #fef9ec; border-left: 2px dashed #f59e0b; margin: 0 0 4px 8px; border-radius: 0 var(--radius-sm) var(--radius-sm) 0; }
.buffer-ido { font-size: 12px; color: #92400e; }
.buffer-badge { font-size: 11px; color: #92400e; font-weight: 600; }
.szabad-nap { padding: var(--spacing-md); text-align: center; color: #065f46; font-size: 14px; font-weight: 500; background: #d1fae5; margin: 8px; border-radius: var(--radius-sm); }
.naptar-loading { padding: var(--spacing-md); text-align: center; color: var(--color-text-muted); font-size: 14px; }
.naptar-hiba { padding: var(--spacing-md); text-align: center; color: #dc2626; font-size: 14px; }
</style>
