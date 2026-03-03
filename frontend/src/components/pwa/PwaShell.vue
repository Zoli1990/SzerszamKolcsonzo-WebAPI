<!-- ============================================================================ -->
<!-- src/components/pwa/PwaShell.vue — PWA Admin Shell                         -->
<!-- Login screen + bottom tab navigáció + router-view                         -->
<!-- ============================================================================ -->

<template>
  <!-- ═══════════════════════════════════════════════════════════════════ -->
  <!-- LOGIN SCREEN -->
  <!-- ═══════════════════════════════════════════════════════════════════ -->
  <div v-if="!isLoggedIn" class="pwa-login">
    <div class="login-card">
      <div class="login-header">
        <span class="login-icon">🔧</span>
        <h1>Szerszámkölcsönző</h1>
        <p class="login-subtitle">Admin PWA</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label>Email</label>
          <input
            v-model="loginForm.email"
            type="email"
            placeholder="admin@email.com"
            required
            autocomplete="email"
          />
        </div>
        <div class="form-group">
          <label>Jelszó</label>
          <input
            v-model="loginForm.password"
            type="password"
            placeholder="••••••••"
            required
            autocomplete="current-password"
          />
        </div>
        <div v-if="loginError" class="login-error">{{ loginError }}</div>
        <button type="submit" class="btn-login" :disabled="loginLoading">
          {{ loginLoading ? '⏳ Bejelentkezés...' : '🔐 Bejelentkezés' }}
        </button>
      </form>
    </div>
  </div>

  <!-- ═══════════════════════════════════════════════════════════════════ -->
  <!-- PWA APP SHELL -->
  <!-- ═══════════════════════════════════════════════════════════════════ -->
  <div v-else class="pwa-shell">
    <!-- Top bar -->
    <header class="pwa-topbar">
      <h1 class="pwa-title">🔧 Admin</h1>
      <button class="btn-logout" @click="handleLogout">🚪</button>
    </header>

    <!-- Tartalom — child route-ok renderelése -->
    <main class="pwa-content">
      <router-view />
    </main>

    <!-- Bottom tab bar -->
    <nav class="pwa-bottomnav">
      <router-link
        v-for="tab in tabs"
        :key="tab.route"
        :to="tab.route"
        class="nav-tab"
        :class="{ active: isActiveTab(tab) }"
      >
        <span class="nav-icon">{{ tab.icon }}</span>
        <span class="nav-label">{{ tab.label }}</span>
      </router-link>
    </nav>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const route = useRoute()
const authStore = useAuthStore()

// ═══════════════════════════════════════════════════════════════════════════
// LOGIN
// ═══════════════════════════════════════════════════════════════════════════
const loginForm = ref({ email: '', password: '' })
const loginLoading = ref(false)
const loginError = ref(null)

const isLoggedIn = computed(() => authStore.isAuthenticated && authStore.isAdmin)

async function handleLogin() {
  loginLoading.value = true
  loginError.value = null
  try {
    await authStore.signIn(loginForm.value.email, loginForm.value.password)
    if (!authStore.isAdmin) {
      loginError.value = 'Csak admin felhasználók léphetnek be!'
      authStore.signOut()
    }
  } catch (err) {
    loginError.value = err.response?.data?.message || 'Hibás email vagy jelszó!'
  } finally {
    loginLoading.value = false
  }
}

function handleLogout() {
  if (confirm('Biztosan kijelentkezel?')) {
    authStore.signOut()
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// BOTTOM NAV
// ═══════════════════════════════════════════════════════════════════════════
const tabs = [
  { route: '/pwa', icon: '📊', label: 'Főoldal', matchName: 'pwa-dashboard' },
  { route: '/pwa/foglalasok', icon: '📋', label: 'Foglalások', matchName: 'pwa-foglalasok' },
  { route: '/pwa/eszkozok', icon: '🔨', label: 'Eszközök', matchName: 'pwa-eszkoz' },
  { route: '/pwa/kategoriak', icon: '📁', label: 'Kategóriák', matchName: 'pwa-kategoriak' },
]

function isActiveTab(tab) {
  // Eszközök tab aktív az /eszkozok és /eszkozok/uj route-oknál is
  if (tab.matchName === 'pwa-eszkoz') {
    return route.name?.startsWith('pwa-eszkoz')
  }
  return route.name === tab.matchName
}
</script>

<style scoped>
/* ═══════════════════════════════════════════════════════════════════════════ */
/* LOGIN                                                                     */
/* ═══════════════════════════════════════════════════════════════════════════ */
.pwa-login {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background: linear-gradient(135deg, #3d2f1f 0%, #6b5d4f 100%);
}

.login-card {
  background: white;
  border-radius: 20px;
  padding: 40px 32px;
  width: 100%;
  max-width: 380px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.login-header {
  text-align: center;
  margin-bottom: 32px;
}

.login-icon {
  font-size: 48px;
  display: block;
  margin-bottom: 12px;
}

.login-header h1 {
  font-size: 22px;
  color: #3d2f1f;
  margin: 0 0 4px 0;
}

.login-subtitle {
  color: #6b5d4f;
  font-size: 14px;
  margin: 0;
}

.login-form .form-group {
  margin-bottom: 20px;
}

.login-form label {
  display: block;
  font-weight: 600;
  font-size: 14px;
  color: #3d2f1f;
  margin-bottom: 6px;
}

.login-form input {
  width: 100%;
  padding: 14px 16px;
  border: 2px solid #e8dcc8;
  border-radius: 10px;
  font-size: 16px;
  font-family: inherit;
  box-sizing: border-box;
  transition: border-color 0.2s;
}

.login-form input:focus {
  outline: none;
  border-color: #6b8e23;
}

.login-error {
  padding: 12px;
  background: #fef2f2;
  border: 2px solid #fecaca;
  border-radius: 10px;
  color: #dc2626;
  font-size: 14px;
  margin-bottom: 20px;
  text-align: center;
}

.btn-login {
  width: 100%;
  padding: 16px;
  background: #6b8e23;
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  min-height: 52px;
}

.btn-login:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* ═══════════════════════════════════════════════════════════════════════════ */
/* APP SHELL                                                                 */
/* ═══════════════════════════════════════════════════════════════════════════ */
.pwa-shell {
  min-height: 100vh;
  background: #f5f1e8;
  display: flex;
  flex-direction: column;
}

/* Top bar */
.pwa-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: white;
  border-bottom: 1px solid #e8dcc8;
  position: sticky;
  top: 0;
  z-index: 100;
}

.pwa-title {
  font-size: 20px;
  color: #3d2f1f;
  margin: 0;
}

.btn-logout {
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 10px;
  background: #fef2f2;
  font-size: 18px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Content */
.pwa-content {
  flex: 1;
  padding: 16px;
  padding-bottom: 80px; /* bottom nav space */
  overflow-y: auto;
}

/* Bottom nav */
.pwa-bottomnav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  display: flex;
  background: white;
  border-top: 1px solid #e8dcc8;
  padding: 6px 0;
  z-index: 100;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
}

.nav-tab {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  padding: 8px 4px;
  text-decoration: none;
  color: #6b5d4f;
  font-size: 11px;
  transition: color 0.2s;
  min-height: 44px;
  justify-content: center;
}

.nav-tab.active {
  color: #6b8e23;
}

.nav-icon {
  font-size: 22px;
  line-height: 1;
}

.nav-label {
  font-weight: 600;
}

/* Safe area */
@supports (padding-bottom: env(safe-area-inset-bottom)) {
  .pwa-bottomnav {
    padding-bottom: calc(6px + env(safe-area-inset-bottom));
  }
  .pwa-content {
    padding-bottom: calc(80px + env(safe-area-inset-bottom));
  }
}
</style>
