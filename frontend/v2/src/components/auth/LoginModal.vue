// ============================================================================
// 3. src/components/auth/LoginModal.vue - Login/Register Modal
// ============================================================================

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" @click="closeModal">
        <div class="modal-container" @click.stop>
          <!-- Header -->
          <div class="modal-header">
            <h2>{{ isLogin ? 'Bejelentkezés' : 'Regisztráció' }}</h2>
            <button class="btn-close" @click="closeModal">&times;</button>
          </div>

          <!-- Form -->
          <form @submit.prevent="handleSubmit" class="auth-form">
            <div class="form-group">
              <label>Email cím *</label>
              <input
                type="email"
                v-model="form.email"
                placeholder="email@example.com"
                required
              />
            </div>

            <div class="form-group">
              <label>Jelszó *</label>
              <input
                type="password"
                v-model="form.password"
                placeholder="Minimum 6 karakter"
                required
                minlength="6"
              />
            </div>

            <div v-if="!isLogin" class="form-group">
              <label>Jelszó megerősítése *</label>
              <input
                type="password"
                v-model="form.passwordConfirm"
                placeholder="Jelszó újra"
                required
              />
            </div>

            <!-- Hibaüzenet -->
            <div v-if="error" class="error-message">
              {{ error }}
            </div>

            <!-- Sikeres regisztráció -->
            <div v-if="successMessage" class="success-message">
              {{ successMessage }}
            </div>

            <!-- Gombok -->
            <button type="submit" class="btn-primary btn-full" :disabled="loading">
              {{ loading ? 'Feldolgozás...' : (isLogin ? 'Bejelentkezés' : 'Regisztráció') }}
            </button>

            <div class="form-footer">
              <button type="button" class="btn-link" @click="toggleMode">
                {{ isLogin ? 'Még nincs fiókom' : 'Már van fiókom' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const props = defineProps({
  isOpen: Boolean
})

const emit = defineEmits(['close', 'success'])

const authStore = useAuthStore()
const isLogin = ref(true)
const loading = ref(false)
const error = ref(null)
const successMessage = ref('')

const form = ref({
  email: '',
  password: '',
  passwordConfirm: ''
})

function toggleMode() {
  isLogin.value = !isLogin.value
  error.value = null
  successMessage.value = ''
}

function closeModal() {
  emit('close')
  resetForm()
}

function resetForm() {
  form.value = {
    email: '',
    password: '',
    passwordConfirm: ''
  }
  error.value = null
  successMessage.value = ''
  isLogin.value = true
}

async function handleSubmit() {
  loading.value = true
  error.value = null
  successMessage.value = ''

  try {
    if (isLogin.value) {
      // Bejelentkezés
      await authStore.signIn(form.value.email, form.value.password)
      emit('success', 'login')
      closeModal()
    } else {
      // Regisztráció
      if (form.value.password !== form.value.passwordConfirm) {
        error.value = 'A jelszavak nem egyeznek!'
        return
      }

      await authStore.signUp(form.value.email, form.value.password)
      
      successMessage.value = 'Sikeres regisztráció!'
      
      // 2 mp múlva modal bezárása
      setTimeout(() => {
        emit('success', 'register')
        closeModal()
      }, 2000)
    }
  } catch (err) {
    console.error('Auth hiba:', err)
    
    // Backend hibaüzenet megjelenítése
    if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else {
      error.value = 'Hiba történt. Próbáld újra!'
    }
  } finally {
    loading.value = false
  }
}


// Reset form amikor modal megnyílik
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    resetForm()
  }
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
  padding: 20px;
}

.modal-container {
  background: white;
  border-radius: 12px;
  max-width: 450px;
  width: 100%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h2 {
  margin: 0;
  font-size: 24px;
  color: #1f2937;
}

.btn-close {
  background: none;
  border: none;
  font-size: 32px;
  color: #6b7280;
  cursor: pointer;
  line-height: 1;
}

.auth-form {
  padding: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #374151;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}

.form-group input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.error-message {
  padding: 12px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 6px;
  color: #dc2626;
  font-size: 14px;
  margin-bottom: 20px;
}

.success-message {
  padding: 12px;
  background: #d1fae5;
  border: 1px solid #a7f3d0;
  border-radius: 6px;
  color: #065f46;
  font-size: 14px;
  margin-bottom: 20px;
}

.btn-primary {
  padding: 12px 24px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
  font-size: 16px;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-primary:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

.btn-full {
  width: 100%;
}

.form-footer {
  margin-top: 20px;
  text-align: center;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
}

.btn-link {
  background: none;
  border: none;
  color: #3b82f6;
  font-weight: 600;
  cursor: pointer;
  font-size: 14px;
}

.btn-link:hover {
  text-decoration: underline;
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>

