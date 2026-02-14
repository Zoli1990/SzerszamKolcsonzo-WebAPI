<template>
  <div id="app" :class="{ 'has-bottom-nav': showBottomNav }">
    <!-- HEADER -->
    <header class="app-header" data-testid="app-header">
      <div class="header-container">
        <!-- Logo -->
        <RouterLink to="/" class="logo" id="logo-link" data-testid="logo-link">
          <span class="logo-icon">🔧</span>
          <span class="logo-text">Szerszámkölcsönző</span>
        </RouterLink>

        <!-- Desktop navigáció -->
        <nav class="desktop-nav" data-testid="desktop-nav">
          <a href="#" class="nav-link" data-testid="nav-home" @click.prevent="handleHomeClick">Főoldal</a>
          <a href="#eszkozok" class="nav-link" data-testid="nav-eszkozok" @click.prevent="scrollToEszkozok">Eszközök</a>
          <a href="#velemenyek" class="nav-link" data-testid="nav-velemenyek" @click.prevent="scrollToVelemenyek">Vélemények</a>
          <a href="#kapcsolat" class="nav-link" data-testid="nav-kapcsolat" @click.prevent="scrollToKapcsolat">Kapcsolat</a>
          
          <RouterLink v-if="authStore.isAdmin" to="/admin" class="nav-link" data-testid="nav-admin">
            Admin
          </RouterLink>

          <!-- Értesítések -->
          <NotificationBell v-if="authStore.isAdmin" data-testid="notification-bell" />

          <!-- Auth gombok -->
          <div v-if="!authStore.isAuthenticated" class="auth-buttons" data-testid="auth-buttons">
            <button
              id="btn-login-desktop"
              class="btn-login"
              data-testid="btn-login-desktop"
              @click="openLoginModal"
            >
              👤 Belépés
            </button>
          </div>

          <!-- User menü -->
          <div v-else class="user-menu" data-testid="user-menu">
            <button
              id="user-menu-button"
              class="user-button"
              data-testid="user-menu-button"
              @click="userMenuOpen = !userMenuOpen"
            >
              👤 {{ truncatedEmail }}
            </button>

            <Transition name="dropdown">
              <div v-if="userMenuOpen" class="dropdown" data-testid="user-dropdown">
                <template v-if="!authStore.isAdmin">
                  <RouterLink
                    to="/profilom"
                    class="dropdown-item"
                    data-testid="dropdown-profilom"
                    @click="userMenuOpen = false"
                  >
                    👤 Profilom
                  </RouterLink>
                  <RouterLink
                    to="/profil"
                    class="dropdown-item"
                    data-testid="dropdown-foglalasaim"
                    @click="userMenuOpen = false"
                  >
                    📋 Foglalásaim
                  </RouterLink>
                </template>
                <button
                  id="btn-logout-desktop"
                  class="dropdown-item logout"
                  data-testid="btn-logout-desktop"
                  @click="handleLogout"
                >
                  🚪 Kilépés
                </button>
              </div>
            </Transition>
          </div>
        </nav>

        <!-- Mobil jobb oldali ikonok -->
        <div class="mobile-actions" data-testid="mobile-actions">
          <!-- Értesítések -->
          <NotificationBell v-if="authStore.isAdmin" data-testid="notification-bell-mobile" />

          <!-- Hamburger menü -->
          <button
            id="hamburger-menu"
            class="hamburger"
            data-testid="hamburger-menu"
            @click="mobileMenuOpen = !mobileMenuOpen"
            aria-label="Menü"
          >
            <span class="hamburger-line" :class="{ open: mobileMenuOpen }"></span>
            <span class="hamburger-line" :class="{ open: mobileMenuOpen }"></span>
            <span class="hamburger-line" :class="{ open: mobileMenuOpen }"></span>
          </button>
        </div>
      </div>

      <!-- Mobil menü (slide down) -->
      <Transition name="slide-down">
        <div v-if="mobileMenuOpen" class="mobile-menu" data-testid="mobile-menu">
          <a href="#" class="mobile-menu-item" data-testid="mobile-menu-home" @click.prevent="handleHomeClick">
            🏠 Főoldal
          </a>
          
          <a href="#eszkozok" class="mobile-menu-item" data-testid="mobile-menu-eszkozok" @click.prevent="scrollToEszkozok">
            🔨 Eszközök
          </a>
          
          <a href="#velemenyek" class="mobile-menu-item" data-testid="mobile-menu-velemenyek" @click.prevent="scrollToVelemenyek">
            💬 Vélemények
          </a>
          
          <a href="#kapcsolat" class="mobile-menu-item" data-testid="mobile-menu-kapcsolat" @click.prevent="scrollToKapcsolat">
            📍 Kapcsolat
          </a>

          <RouterLink
            v-if="authStore.isAdmin"
            to="/admin"
            class="mobile-menu-item"
            data-testid="mobile-menu-admin"
            @click="mobileMenuOpen = false"
          >
            ⚙️ Admin
          </RouterLink>

          <template v-if="authStore.isAuthenticated">
            <template v-if="!authStore.isAdmin">
              <RouterLink
                to="/profilom"
                class="mobile-menu-item"
                data-testid="mobile-menu-profilom"
                @click="mobileMenuOpen = false"
              >
                👤 Profilom
              </RouterLink>
              <RouterLink
                to="/profil"
                class="mobile-menu-item"
                data-testid="mobile-menu-foglalasaim"
                @click="mobileMenuOpen = false"
              >
                📋 Foglalásaim
              </RouterLink>
            </template>
            <button
              id="btn-logout-mobile"
              class="mobile-menu-item logout"
              data-testid="btn-logout-mobile"
              @click="handleLogout"
            >
              🚪 Kilépés
            </button>
          </template>

          <!-- Login gomb -->
          <button
            v-else
            id="btn-login-mobile"
            class="mobile-menu-item login"
            data-testid="btn-login-mobile"
            @click="openLoginModal"
          >
            👤 Belépés / Regisztráció
          </button>
        </div>
      </Transition>
    </header>

    <!-- MAIN CONTENT -->
    <main class="app-main" data-testid="app-main">
      <RouterView />
    </main>

    <!-- FOOTER -->
    <footer class="app-footer" data-testid="app-footer">
      <div class="footer-container">
        <p>&copy; 2025 Szerszámkölcsönző - Vizsgamunka</p>
      </div>
    </footer>

    <!-- LOGIN MODAL -->
    <LoginModal
      id="login-modal"
      :is-open="loginModalOpen"
      data-testid="login-modal"
      @close="loginModalOpen = false"
      @success="handleLoginSuccess"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { RouterLink, RouterView, useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import LoginModal from '@/components/auth/LoginModal.vue'
import NotificationBell from '@/components/NotificationBell.vue'
import { pushService } from '@/services/pushService'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// State
const loginModalOpen = ref(false)
const userMenuOpen = ref(false)
const mobileMenuOpen = ref(false)
const windowWidth = ref(window.innerWidth)

// Computed
const isMobile = computed(() => windowWidth.value < 768)

const showBottomNav = computed(() => {
  return isMobile.value && authStore.isAdmin
})

const truncatedEmail = computed(() => {
  const email = authStore.userEmail
  if (!email) return ''
  if (email.length > 20) {
    return email.substring(0, 17) + '...'
  }
  return email
})

// ═══════════════════════════════════════════════════════════════════════════
// SCROLL FUNKCIÓK
// ═══════════════════════════════════════════════════════════════════════════

function handleHomeClick(event) {
  event.preventDefault()
  mobileMenuOpen.value = false

  if (router.currentRoute.value.path === '/') {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } else {
    router.push('/').then(() => {
      window.scrollTo({ top: 0, behavior: 'smooth' })
    })
  }
}

function scrollToEszkozok() {
  mobileMenuOpen.value = false
  
  if (router.currentRoute.value.path !== '/') {
    router.push('/').then(() => {
      setTimeout(() => {
        const element = document.getElementById('eszkozok')
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'start' })
        }
      }, 100)
    })
  } else {
    const element = document.getElementById('eszkozok')
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }
}

function scrollToVelemenyek() {
  mobileMenuOpen.value = false
  
  if (router.currentRoute.value.path !== '/') {
    router.push('/').then(() => {
      setTimeout(() => {
        const element = document.getElementById('velemenyek')
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'start' })
        }
      }, 100)
    })
  } else {
    const element = document.getElementById('velemenyek')
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }
}

function scrollToKapcsolat() {
  mobileMenuOpen.value = false
  
  if (router.currentRoute.value.path !== '/') {
    router.push('/').then(() => {
      setTimeout(() => {
        const element = document.getElementById('kapcsolat')
        if (element) {
          element.scrollIntoView({ behavior: 'smooth', block: 'start' })
        }
      }, 100)
    })
  } else {
    const element = document.getElementById('kapcsolat')
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// EGYÉB FUNKCIÓK
// ═══════════════════════════════════════════════════════════════════════════

function isActiveRoute(path) {
  if (path === '/') {
    return route.path === '/'
  }
  return route.path.startsWith(path)
}

function setAdminTab(tab) {
  console.log('Admin tab:', tab)
}

function openLoginModal() {
  console.log('🔵 openLoginModal called')
  console.log('  mobileMenuOpen before:', mobileMenuOpen.value)

  loginModalOpen.value = true
  mobileMenuOpen.value = false

  console.log('  mobileMenuOpen after:', mobileMenuOpen.value)
  console.log('  loginModalOpen:', loginModalOpen.value)
}

function closeUserMenu() {
  userMenuOpen.value = false
}

function handleLoginSuccess() {
  console.log('Sikeres bejelentkezés!')
}

function handleLogout() {
  userMenuOpen.value = false
  mobileMenuOpen.value = false

  if (confirm('Biztosan kijelentkezel?')) {
    authStore.signOut()
    router.push('/')
  }
}

function handleResize() {
  windowWidth.value = window.innerWidth
  if (!isMobile.value) {
    mobileMenuOpen.value = false
  }
}

function handleDocumentClick(event) {
  const userMenu = document.querySelector('.user-menu')
  if (userMenu && !userMenu.contains(event.target)) {
    userMenuOpen.value = false
  }
}

// Route változáskor menük bezárása
watch(
  () => route.path,
  () => {
    mobileMenuOpen.value = false
    userMenuOpen.value = false
  },
)

watch(mobileMenuOpen, (newVal, oldVal) => {
  console.log('🟡 mobileMenuOpen changed:', { from: oldVal, to: newVal })
})

watch(loginModalOpen, (isOpen) => {
  if (isOpen) {
    console.log('🔴 Login modal opened - locking body scroll')
    document.body.style.overflow = 'hidden'
  } else {
    console.log('🟢 Login modal closed - unlocking body scroll')
    document.body.style.overflow = ''
  }
})

// Lifecycle
onMounted(async () => {
  await authStore.initialize()

  if (authStore.isAdmin) {
    console.log('🔧 Admin user detected, initializing PWA...')
    await pushService.registerServiceWorker()
  } else {
    console.log('👤 Regular user, PWA disabled')
  }

  window.addEventListener('resize', handleResize)
  document.addEventListener('click', handleDocumentClick)
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  document.removeEventListener('click', handleDocumentClick)
})

const vClickOutside = {
  mounted(el, binding) {
    el._clickOutside = (event) => {
      if (!(el === event.target || el.contains(event.target))) {
        binding.value()
      }
    }
    document.addEventListener('click', el._clickOutside)
  },
  unmounted(el) {
    if (el._clickOutside) {
      document.removeEventListener('click', el._clickOutside)
      delete el._clickOutside
    }
  },
}
</script>

<style>
:root {
  --header-bg: #3d2f1f;
  --color-primary: #6b8e23;
  --color-primary-dark: #556b1a;
  --color-secondary: #8b7355;
  --color-background: #f5f1e8;
  --color-surface: #ffffff;
  --color-text: #3d2f1f;
  --color-text-muted: #6b5d4f;
  --color-border: #e8dcc8;
  --color-waiting: #f59e0b;
  --color-active: #3b82f6;
  --color-success: #10b981;
  --color-danger: #ef4444;
  --header-height: 64px;
  --bottom-nav-height: 64px;
  --spacing-xs: 4px;
  --spacing-sm: 8px;
  --spacing-md: 16px;
  --spacing-lg: 24px;
  --spacing-xl: 32px;
  --radius-sm: 6px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --shadow-sm: 0 1px 3px rgba(61, 47, 31, 0.1);
  --shadow-md: 0 4px 12px rgba(61, 47, 31, 0.15);
  --shadow-lg: 0 10px 40px rgba(61, 47, 31, 0.2);
  --transition-fast: 0.15s ease;
  --transition-normal: 0.2s ease;
  --transition-slow: 0.3s ease;
}

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
  background: var(--color-background);
  color: var(--color-text);
  line-height: 1.5;
  -webkit-font-smoothing: antialiased;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

#app.has-bottom-nav .app-main {
  padding-bottom: calc(var(--bottom-nav-height) + var(--spacing-md));
}

.app-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: var(--header-bg);
  height: var(--header-height);
  box-shadow: var(--shadow-md);
}

.header-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 var(--spacing-md);
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.logo {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  text-decoration: none;
  color: white;
  font-weight: 700;
  font-size: 18px;
}

.logo-icon {
  font-size: 24px;
}

.logo-text {
  display: none;
}

@media (min-width: 480px) {
  .logo-text {
    display: inline;
  }
}

.desktop-nav {
  display: none;
  align-items: center;
  gap: var(--spacing-md);
}

@media (min-width: 768px) {
  .desktop-nav {
    display: flex;
  }
}

.nav-link {
  color: rgba(255, 255, 255, 0.85);
  text-decoration: none;
  font-weight: 500;
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: var(--radius-sm);
  transition: all var(--transition-fast);
}

.nav-link:hover {
  color: white;
  background: rgba(255, 255, 255, 0.1);
}

.nav-link.router-link-active {
  color: white;
  background: rgba(255, 255, 255, 0.15);
}

.auth-buttons {
  display: flex;
  gap: var(--spacing-sm);
}

.btn-login {
  padding: var(--spacing-sm) var(--spacing-md);
  background: var(--color-primary);
  color: white;
  border: none;
  border-radius: var(--radius-sm);
  font-weight: 600;
  cursor: pointer;
  transition: background var(--transition-fast);
}

.btn-login:hover {
  background: var(--color-primary-dark);
}

.user-menu {
  position: relative;
}

.user-button {
  padding: var(--spacing-sm) var(--spacing-md);
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: var(--radius-sm);
  font-weight: 500;
  cursor: pointer;
  color: white;
  transition: all var(--transition-fast);
}

.user-button:hover {
  background: rgba(255, 255, 255, 0.15);
}

.dropdown {
  position: absolute;
  top: calc(100% + var(--spacing-sm));
  right: 0;
  background: var(--color-surface);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  min-width: 200px;
  overflow: hidden;
  z-index: 200;
}

.dropdown-item {
  display: block;
  width: 100%;
  padding: var(--spacing-md);
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
  color: var(--color-text);
  font-weight: 500;
  text-decoration: none;
  transition: background var(--transition-fast);
  border-bottom: 1px solid var(--color-border);
}

.dropdown-item:last-child {
  border-bottom: none;
}

.dropdown-item:hover {
  background: var(--color-background);
}

.dropdown-item.logout {
  color: var(--color-danger);
}

.mobile-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

@media (min-width: 768px) {
  .mobile-actions {
    display: none;
  }
}

.hamburger {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  width: 44px;
  height: 44px;
  background: none;
  border: none;
  cursor: pointer;
  padding: var(--spacing-sm);
  gap: 5px;
}

.hamburger-line {
  width: 24px;
  height: 2px;
  background: white;
  border-radius: 2px;
  transition: all var(--transition-normal);
}

.hamburger-line.open:nth-child(1) {
  transform: rotate(45deg) translate(5px, 5px);
}

.hamburger-line.open:nth-child(2) {
  opacity: 0;
}

.hamburger-line.open:nth-child(3) {
  transform: rotate(-45deg) translate(5px, -5px);
}

.mobile-menu {
  position: absolute;
  top: var(--header-height);
  left: 0;
  right: 0;
  background: var(--color-surface);
  box-shadow: var(--shadow-lg);
  z-index: 150;
}

.mobile-menu-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  width: 100%;
  padding: var(--spacing-md) var(--spacing-lg);
  background: none;
  border: none;
  border-bottom: 1px solid var(--color-border);
  text-align: left;
  cursor: pointer;
  color: var(--color-text);
  font-size: 16px;
  font-weight: 500;
  text-decoration: none;
  transition: background var(--transition-fast);
}

.mobile-menu-item:hover {
  background: var(--color-background);
}

.mobile-menu-item.logout {
  color: var(--color-danger);
}

.mobile-menu-item.login {
  background: var(--color-primary);
  color: white;
  border-bottom: none;
}

.mobile-menu-item.login:hover {
  background: var(--color-primary-dark);
}

.mobile-overlay {
  position: fixed;
  inset: 0;
  top: var(--header-height);
  background: rgba(0, 0, 0, 0.5);
  z-index: 140;
}

.app-main {
  flex: 1;
  width: 100%;
}

.app-footer {
  background: var(--header-bg);
  color: rgba(255, 255, 255, 0.7);
  padding: var(--spacing-lg);
  text-align: center;
  font-size: 14px;
}

@media (max-width: 767px) {
  .app-footer {
    display: none;
  }
}

.footer-container {
  max-width: 1400px;
  margin: 0 auto;
}

.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: var(--bottom-nav-height);
  background: var(--color-surface);
  border-top: 1px solid var(--color-border);
  display: flex;
  justify-content: space-around;
  align-items: center;
  z-index: 100;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
}

@media (min-width: 768px) {
  .bottom-nav {
    display: none;
  }
}

.bottom-nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  padding: var(--spacing-sm);
  color: var(--color-text-muted);
  text-decoration: none;
  font-size: 11px;
  font-weight: 500;
  background: none;
  border: none;
  cursor: pointer;
  transition: color var(--transition-fast);
  min-width: 64px;
}

.bottom-nav-item:hover,
.bottom-nav-item.active {
  color: var(--color-primary);
}

.bottom-nav-icon {
  font-size: 22px;
  line-height: 1;
}

.bottom-nav-label {
  white-space: nowrap;
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all var(--transition-normal);
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.dropdown-enter-active,
.dropdown-leave-active {
  transition: all var(--transition-fast);
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-5px);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--transition-normal);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 var(--spacing-md);
}

@supports (padding-bottom: env(safe-area-inset-bottom)) {
  .bottom-nav {
    padding-bottom: env(safe-area-inset-bottom);
    height: calc(var(--bottom-nav-height) + env(safe-area-inset-bottom));
  }

  #app.has-bottom-nav .app-main {
    padding-bottom: calc(
      var(--bottom-nav-height) + env(safe-area-inset-bottom) + var(--spacing-md)
    );
  }
}
</style>