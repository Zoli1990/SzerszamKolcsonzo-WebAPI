<template>
  <Teleport to="body">
    <Transition name="sidebar">
      <div v-if="isOpen" class="cart-overlay" @click="$emit('close')">
        <div class="cart-sidebar" @click.stop>
          <div class="cart-header">
            <h2>🛒 Kosár</h2>
            <button class="close-btn" @click="$emit('close')">✕</button>
          </div>

          <div v-if="kosarStore.itemCount === 0" class="empty-cart">
            A kosár üres
          </div>

          <div v-else class="cart-content">
            <div class="cart-items">
              <div v-for="item in kosarStore.items" :key="item.eszkoz.eszkozID" class="cart-item">
                <h3>{{ item.eszkoz.nev }}</h3>
                <p class="date-range">
                  {{ formatDate(item.foglalasKezdete) }} - {{ formatDate(item.foglalasVege) }}
                </p>
                <p>{{ item.oraSzam }} óra × {{ formatAr(item.eszkoz.kiadasiAr) }} Ft/óra</p>
                <p class="item-total">{{ formatAr(item.eszkoz.kiadasiAr * item.oraSzam) }} Ft</p>
                <button class="remove-btn" @click="kosarStore.removeItem(item.eszkoz.eszkozID)">
                  Törlés
                </button>
              </div>
            </div>

            <div class="cart-footer">
              <div class="summary">
                <p>Összes óra: <strong>{{ kosarStore.osszesOra }}</strong></p>
                <p>Összes ár: <strong>{{ formatAr(kosarStore.osszesAr) }} Ft</strong></p>
              </div>
              <button class="checkout-btn" @click="handleCheckout">Foglalás véglegesítése</button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { useKosarStore } from '@/stores/kosarStore'

defineProps({
  isOpen: Boolean
})

const emit = defineEmits(['close'])

const kosarStore = useKosarStore()

function formatAr(ar) {
  return new Intl.NumberFormat('hu-HU').format(ar)
}

function formatDate(dateString) {
  return new Date(dateString).toLocaleString('hu-HU', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function handleCheckout() {
  alert('Foglalás véglegesítése - TODO')
  emit('close')
}
</script>

<style scoped>
.cart-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
  display: flex;
  justify-content: flex-end;
}

.cart-sidebar {
  width: 450px;
  max-width: 90vw;
  background: white;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.cart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 2px solid #e5e7eb;
}

.cart-header h2 {
  margin: 0;
  font-size: 24px;
}

.close-btn {
  background: none;
  border: none;
  font-size: 28px;
  cursor: pointer;
  color: #6b7280;
  line-height: 1;
}

.close-btn:hover {
  color: #111827;
}

.empty-cart {
  padding: 60px 20px;
  text-align: center;
  color: #6b7280;
  font-size: 18px;
}

.cart-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.cart-items {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.cart-item {
  background: #f9fafb;
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 12px;
}

.cart-item h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
  color: #111827;
}

.date-range {
  color: #6b7280;
  font-size: 13px;
  margin: 4px 0;
  font-weight: 600;
}

.cart-item p {
  margin: 4px 0;
  color: #6b7280;
  font-size: 14px;
}

.item-total {
  font-weight: 700;
  color: #111827;
  font-size: 18px;
  margin-top: 8px;
}

.remove-btn {
  margin-top: 12px;
  padding: 8px 16px;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: background 0.2s;
}

.remove-btn:hover {
  background: #dc2626;
}

.cart-footer {
  border-top: 2px solid #e5e7eb;
  padding: 20px;
  background: white;
}

.summary {
  margin-bottom: 16px;
}

.summary p {
  display: flex;
  justify-content: space-between;
  margin: 8px 0;
  font-size: 16px;
  color: #374151;
}

.checkout-btn {
  width: 100%;
  padding: 16px;
  background: #10b981;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
  font-size: 16px;
  transition: background 0.2s;
}

.checkout-btn:hover {
  background: #059669;
}

.sidebar-enter-active,
.sidebar-leave-active {
  transition: all 0.3s ease;
}

.sidebar-enter-from .cart-sidebar,
.sidebar-leave-to .cart-sidebar {
  transform: translateX(100%);
}

.sidebar-enter-from,
.sidebar-leave-to {
  opacity: 0;
}
</style>