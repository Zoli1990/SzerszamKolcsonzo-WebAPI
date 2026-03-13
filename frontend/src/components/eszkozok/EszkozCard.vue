<template>
  <div class="eszkoz-card">
    <div class="card-image">
      <img :src="imageUrl" :alt="eszkoz.nev" @error="handleImageError" />
      <span class="status-badge" :class="statusClass">{{ statusText }}</span>
    </div>
    <div class="card-content">
      <h3 class="eszkoz-nev">{{ eszkoz.nev }}</h3>
      <p class="kategoria">{{ eszkoz.kategoriaNev }}</p>
      <p class="leiras" v-if="eszkoz.leiras">{{ truncatedLeiras }}</p>
      <div class="card-footer">
        <div class="ar">
          <span class="ar-label">{{ t('eszkozCard.rentalFee') }}</span>
          <span class="ar-ertek">{{ formatAr(eszkoz.kiadasiAr) }} Ft/{{ t('eszkozCard.hour') }}</span>
        </div>
        <button
          id="foglalas-button"
          class="btn-foglalas"
          :disabled="!isElerheto"
          @click="$emit('foglalas', eszkoz)"
        >
          {{ isElerheto ? t('eszkozCard.reserve') : eszkoz.status === 'Kiadva' ? t('eszkozCard.statusIssued') : t('eszkozCard.statusReserved') }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const props = defineProps({
  eszkoz: {
    type: Object,
    required: true,
  },
})

defineEmits(['foglalas'])

const isElerheto = computed(() => props.eszkoz.status === 'Elerheto')

const statusText = computed(() => {
  const map = {
    Elerheto: t('eszkozCard.statusAvailable'),
    Foglalva: t('eszkozCard.statusReserved'),
    Kiadva: t('eszkozCard.statusIssued'),
    Kivonva: t('eszkozCard.statusWithdrawn'),
  }
  return map[props.eszkoz.status] || props.eszkoz.status
})

const statusClass = computed(() => {
  return props.eszkoz.status === 'Elerheto' ? 'status-elerheto' : 'status-kiadva'
})

const truncatedLeiras = computed(() => {
  if (!props.eszkoz.leiras) return ''
  return props.eszkoz.leiras.length > 100
    ? props.eszkoz.leiras.substring(0, 100) + '...'
    : props.eszkoz.leiras
})

// Csoporttagok verziója: relatív URL kezelés
const imageUrl = computed(() => {
  if (props.eszkoz.kepUrl) {
    if (props.eszkoz.kepUrl.startsWith('/')) {
      const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5265/api'
      return apiBase.replace(/\/api\/?$/, '') + props.eszkoz.kepUrl
    }
    return props.eszkoz.kepUrl
  }
  return 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80'
})

function handleImageError(e) {
  e.target.src = 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80'
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
.eszkoz-card { background: white; border-radius: 12px; box-shadow: 0 2px 8px rgba(61,47,31,0.1); overflow: hidden; transition: transform 0.2s, box-shadow 0.2s; cursor: pointer; }
.eszkoz-card:hover { transform: translateY(-4px); box-shadow: 0 4px 16px rgba(61,47,31,0.2); }
@media (max-width: 767px) { .eszkoz-card:active { transform: scale(0.98); } }
.card-image { position: relative; width: 100%; height: 200px; overflow: hidden; background: #f5f1e8; }
@media (max-width: 767px) { .card-image { height: 220px; } }
.card-image img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s ease; }
.eszkoz-card:hover .card-image img { transform: scale(1.05); }
.status-badge { position: absolute; top: 12px; right: 12px; padding: 8px 14px; border-radius: 20px; font-size: 13px; font-weight: 700; text-transform: uppercase; backdrop-filter: blur(8px); box-shadow: 0 2px 8px rgba(0,0,0,0.15); }
@media (max-width: 767px) { .status-badge { font-size: 14px; padding: 10px 16px; } }
.status-elerheto { background: rgba(122,155,87,0.95); color: white; }
.status-kiadva { background: rgba(184,92,92,0.95); color: white; }
.card-content { padding: 18px; }
@media (max-width: 767px) { .card-content { padding: 20px; } }
.eszkoz-nev { font-size: 19px; font-weight: 700; margin: 0 0 8px 0; color: #3d2f1f; line-height: 1.3; }
@media (max-width: 767px) { .eszkoz-nev { font-size: 21px; margin-bottom: 10px; } }
.kategoria { font-size: 14px; color: #6b5d4f; margin: 0 0 12px 0; text-transform: capitalize; font-weight: 500; }
@media (max-width: 767px) { .kategoria { font-size: 15px; } }
.leiras { font-size: 14px; color: #544838; margin: 0 0 16px 0; line-height: 1.6; }
@media (max-width: 767px) { .leiras { font-size: 15px; line-height: 1.7; } }
.card-footer { display: flex; justify-content: space-between; align-items: center; padding-top: 16px; border-top: 1px solid #e8dcc8; gap: 12px; }
@media (max-width: 380px) { .card-footer { flex-direction: column; align-items: stretch; gap: 14px; } }
.ar { display: flex; flex-direction: column; gap: 2px; }
.ar-label { font-size: 12px; color: #6b5d4f; font-weight: 500; }
@media (max-width: 767px) { .ar-label { font-size: 13px; } }
.ar-ertek { font-size: 20px; font-weight: 800; color: #6b8e23; }
@media (max-width: 767px) { .ar-ertek { font-size: 24px; } }
.btn-foglalas { padding: 12px 28px; background: #6b8e23; color: white; border: none; border-radius: 8px; font-size: 15px; font-weight: 700; cursor: pointer; transition: all 0.2s; white-space: nowrap; min-height: 44px; min-width: 100px; }
@media (max-width: 767px) { .btn-foglalas { font-size: 16px; padding: 14px 32px; min-height: 48px; min-width: 120px; } }
@media (max-width: 380px) { .btn-foglalas { width: 100%; } }
.btn-foglalas:hover:not(:disabled) { background: #556b1a; transform: translateY(-2px); box-shadow: 0 4px 12px rgba(107,142,35,0.3); }
.btn-foglalas:active:not(:disabled) { transform: translateY(0); }
.btn-foglalas:disabled { background: #a89f8d; cursor: not-allowed; opacity: 0.7; }
</style>