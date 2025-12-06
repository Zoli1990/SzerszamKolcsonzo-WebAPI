<template>
  <div class="eszkoz-card">
    <div class="card-image">
      <img :src="imageUrl" :alt="eszkoz.nev" @error="handleImageError" />
      <span class="status-badge" :class="statusClass">
        {{ statusText }}
      </span>
    </div>

    <div class="card-content">
      <h3 class="eszkoz-nev">{{ eszkoz.nev }}</h3>
      <p class="kategoria">{{ eszkoz.kategoriaNev }}</p>

      <p class="leiras" v-if="eszkoz.leiras">
        {{ truncatedLeiras }}
      </p>

      <div class="card-footer">
        <div class="ar">
          <span class="ar-label">Bérleti díj:</span>
          <span class="ar-ertek">{{ formatAr(eszkoz.kiadasiAr) }} Ft/óra</span>
        </div>

        <button class="btn-foglalas" :disabled="!isElerheto" @click="$emit('foglalas', eszkoz)">
          {{ isElerheto ? 'Foglalás' : 'Kiadva' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  eszkoz: {
    type: Object,
    required: true,
  },
})

defineEmits(['foglalas'])

const isElerheto = computed(() => props.eszkoz.status === 'Elerheto')

const statusText = computed(() => {
  return props.eszkoz.status === 'Elerheto' ? 'Elérhető' : 'Kiadva'
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

// ✅ Valódi kép URL használata
const imageUrl = computed(() => {
  if (props.eszkoz.kepUrl) {
    return props.eszkoz.kepUrl
  }
  // Fallback: placeholder
  return `https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80`
})

function handleImageError(e) {
  // Ha nem tölt be a kép, placeholder-t használunk
  e.target.src = 'https://images.unsplash.com/photo-1572981779307-38b8cabb2407?w=400&q=80'
}

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}
</script>

<style scoped>
.eszkoz-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(61, 47, 31, 0.1);
  overflow: hidden;
  transition:
    transform 0.2s,
    box-shadow 0.2s;
}

.eszkoz-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 16px rgba(61, 47, 31, 0.2);
}

.card-image {
  position: relative;
  width: 100%;
  height: 200px;
  overflow: hidden;
  background: #f5f1e8;
}

.card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.eszkoz-card:hover .card-image img {
  transform: scale(1.05);
}

.status-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  backdrop-filter: blur(8px);
}

.status-elerheto {
  background: rgba(122, 155, 87, 0.95);
  color: white;
}

.status-kiadva {
  background: rgba(184, 92, 92, 0.95);
  color: white;
}

.card-content {
  padding: 16px;
}

.eszkoz-nev {
  font-size: 18px;
  font-weight: 600;
  margin: 0 0 8px 0;
  color: #3d2f1f;
}

.kategoria {
  font-size: 14px;
  color: #6b5d4f;
  margin: 0 0 12px 0;
  text-transform: capitalize;
}

.leiras {
  font-size: 14px;
  color: #544838;
  margin: 0 0 16px 0;
  line-height: 1.5;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #e8dcc8;
}

.ar {
  display: flex;
  flex-direction: column;
}

.ar-label {
  font-size: 12px;
  color: #6b5d4f;
}

.ar-ertek {
  font-size: 18px;
  font-weight: 700;
  color: #6b8e23;
}

.btn-foglalas {
  padding: 10px 24px;
  background: #6b8e23;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-foglalas:hover:not(:disabled) {
  background: #556b1a;
}

.btn-foglalas:disabled {
  background: #a89f8d;
  cursor: not-allowed;
}
</style>