// ============================================================================ // 4. src/App.vue -
FRISSÍTETT (Login gomb + User menü + kosár) //
============================================================================

<template>
  <div id="app">
    <header class="app-header">
      <div class="container">
        <RouterLink to="/" class="logo"> 🔧 Szerszámkölcsönző </RouterLink>

        <nav class="main-nav">
          <RouterLink to="/" class="nav-link">Főoldal</RouterLink>

          <!-- Admin link (csak adminnak) -->
          <RouterLink v-if="authStore.isAdmin" to="/admin" class="nav-link"> Admin </RouterLink>

          <!-- Auth gombok -->
          <div v-if="!authStore.isAuthenticated" class="auth-buttons">
            <button class="btn-login" @click="openLoginModal">👤 Belépés</button>
          </div>

          <!-- User menü (bejelentkezve) -->
          <div v-else class="user-menu">
            <button class="user-button" @click="userMenuOpen = !userMenuOpen">
              👤 {{ authStore.userEmail }}
            </button>

            <div v-if="userMenuOpen" class="dropdown">
              <RouterLink to="/profil" class="dropdown-item" @click="userMenuOpen = false">
                📋 Foglalásaim
              </RouterLink>
              <button class="dropdown-item" @click="handleLogout">🚪 Kilépés</button>
            </div>
          </div>
        </nav>
      </div>
    </header>

    <main class="app-main">
      <RouterView />
    </main>

    <footer class="app-footer">
      <div class="container">
        <p>&copy; 2025 Szerszámkölcsönző - Vizsgamunka</p>
      </div>
    </footer>

    <!-- Login Modal -->
    <LoginModal
      :is-open="loginModalOpen"
      @close="loginModalOpen = false"
      @success="handleLoginSuccess"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import LoginModal from '@/components/auth/LoginModal.vue'

const router = useRouter()
const authStore = useAuthStore()
const loginModalOpen = ref(false)
const userMenuOpen = ref(false)

onMounted(async () => {
  await authStore.initialize()
})

function openLoginModal() {
  loginModalOpen.value = true
}

function handleLoginSuccess() {
  console.log('Sikeres bejelentkezés!')
}

async function handleLogout() {
  userMenuOpen.value = false

  if (confirm('Biztosan kijelentkezel?')) {
    await authStore.signOut()
    router.push('/')
  }
}
</script>

<style>
/* ... (korábbi stílusok) ... */

.auth-buttons {
  display: flex;
  gap: 12px;
  align-items: center;
}

.btn-login {
  padding: 8px 16px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-login:hover {
  background: #2563eb;
}

.user-menu {
  position: relative;
}

.user-button {
  padding: 8px 16px;
  background: white;
  border: 2px solid #e5e7eb;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  color: #374151;
}

.dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  min-width: 200px;
  z-index: 100;
}

.dropdown-item {
  display: block;
  width: 100%;
  padding: 12px 16px;
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
  color: #374151;
  font-weight: 500;
  text-decoration: none;
  transition: background 0.2s;
}

.dropdown-item:hover {
  background: #f3f4f6;
}

.dropdown-item:first-child {
  border-radius: 8px 8px 0 0;
}

.dropdown-item:last-child {
  border-radius: 0 0 8px 8px;
}
</style>
