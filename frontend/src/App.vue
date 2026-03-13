<template>
  <div id="app" :class="{ 'has-bottom-nav': showBottomNav }">
    <!-- HEADER -->
    <header v-if="!isPwaRoute" class="app-header" data-testid="app-header">
      <div class="header-container">
        <!-- Logo -->
        <RouterLink to="/" class="logo" id="logo-link" data-testid="logo-link">
          <span class="logo-icon">🔧</span>
          <span class="logo-text">Szerszámkölcsönző</span>
        </RouterLink>

        <!-- Desktop navigáció -->
        <nav class="desktop-nav" data-testid="desktop-nav">
          <a href="#" class="nav-link" data-testid="nav-home" @click.prevent="handleHomeClick">
            {{ t('nav.home') }}
          </a>
          <a
            href="#eszkozok"
            class="nav-link"
            data-testid="nav-eszkozok"
            @click.prevent="scrollToEszkozok"
          >
            {{ t('nav.tools') }}
          </a>
          <a
            href="#velemenyek"
            class="nav-link"
            data-testid="nav-velemenyek"
            @click.prevent="scrollToVelemenyek"
          >
            {{ t('nav.reviews') }}
          </a>
          <a
            href="#kapcsolat"
            class="nav-link"
            data-testid="nav-kapcsolat"
            @click.prevent="scrollToKapcsolat"
          >
            {{ t('nav.contact') }}
          </a>

          <RouterLink v-if="authStore.isAdmin" to="/admin" class="nav-link" data-testid="nav-admin">
            Admin
          </RouterLink>

          <NotificationBell v-if="authStore.isAdmin" data-testid="notification-bell" />

          <!-- Nyelvváltó desktop -->
          <div class="lang-switcher" ref="langDropdownRef">
            <button class="lang-selected" @click="langDropdownOpen = !langDropdownOpen">
              {{ currentLang.flag }} {{ currentLang.label }}
              <span class="lang-arrow" :class="{ open: langDropdownOpen }">▾</span>
            </button>
            <Transition name="dropdown">
              <div v-if="langDropdownOpen" class="lang-dropdown">
                <button
                  v-for="lang in languages"
                  :key="lang.code"
                  class="lang-option"
                  :class="{ active: locale === lang.code }"
                  @click="switchLanguage(lang.code)"
                >
                  {{ lang.flag }} {{ lang.label }}
                </button>
              </div>
            </Transition>
          </div>

          <!-- Auth gombok -->
          <div v-if="!authStore.isAuthenticated" class="auth-buttons" data-testid="auth-buttons">
            <button
              id="btn-login-desktop"
              class="btn-login"
              data-testid="btn-login-desktop"
              @click="openLoginModal"
            >
              👤 {{ t('auth.login') }}
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
                    👤 {{ t('auth.profile') }}
                  </RouterLink>
                  <RouterLink
                    to="/profil"
                    class="dropdown-item"
                    data-testid="dropdown-foglalasaim"
                    @click="userMenuOpen = false"
                  >
                    📋 {{ t('auth.reservations') }}
                  </RouterLink>
                </template>
                <button
                  id="btn-logout-desktop"
                  class="dropdown-item logout"
                  data-testid="btn-logout-desktop"
                  @click="handleLogout"
                >
                  🚪 {{ t('auth.logout') }}
                </button>
              </div>
            </Transition>
          </div>
        </nav>

        <!-- Mobil jobb oldali ikonok -->
        <div class="mobile-actions" data-testid="mobile-actions">
          <NotificationBell v-if="authStore.isAdmin" data-testid="notification-bell-mobile" />
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

      <!-- Mobil menü -->
      <Transition name="slide-down">
        <div v-if="mobileMenuOpen" class="mobile-menu" data-testid="mobile-menu">
          <a
            href="#"
            class="mobile-menu-item"
            data-testid="mobile-menu-home"
            @click.prevent="handleHomeClick"
          >
            🏠 {{ t('nav.home') }}
          </a>

          <a
            href="#eszkozok"
            class="mobile-menu-item"
            data-testid="mobile-menu-eszkozok"
            @click.prevent="scrollToEszkozok"
          >
            🔨 Eszközök
          </a>

          <RouterLink
            v-if="authStore.isAdmin"
            :to="isMobile ? '/pwa' : '/admin'"
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
                👤 {{ t('auth.profile') }}
              </RouterLink>
              <RouterLink
                to="/profil"
                class="mobile-menu-item"
                data-testid="mobile-menu-foglalasaim"
                @click="mobileMenuOpen = false"
              >
                📋 {{ t('auth.reservations') }}
              </RouterLink>
            </template>
            <button
              id="btn-logout-mobile"
              class="mobile-menu-item logout"
              data-testid="btn-logout-mobile"
              @click="handleLogout"
            >
              🚪 {{ t('auth.logout') }}
            </button>
          </template>

          <button
            v-else
            id="btn-login-mobile"
            class="mobile-menu-item login"
            data-testid="btn-login-mobile"
            @click="openLoginModal"
          >
            👤 {{ t('auth.loginRegister') }}
          </button>

          <!-- Nyelvváltó mobil -->
          <div class="lang-switcher-mobile">
            <button
              v-for="lang in languages"
              :key="lang.code"
              class="lang-option"
              :class="{ active: locale === lang.code }"
              @click="switchLanguage(lang.code)"
            >
              {{ lang.flag }} {{ lang.label }}
            </button>
          </div>
        </div>
      </Transition>
    </header>

    <!-- MAIN CONTENT -->
    <main :class="isPwaRoute ? 'app-main-pwa' : 'app-main'" data-testid="app-main">
      <RouterView />
    </main>

    <!-- FOOTER -->
    <footer v-if="!isPwaRoute" class="app-footer" data-testid="app-footer">
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
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'
import LoginModal from '@/components/auth/LoginModal.vue'
import NotificationBell from '@/components/NotificationBell.vue'
import { pushService } from '@/services/pushService'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const { t, locale } = useI18n()

const loginModalOpen = ref(false)
const userMenuOpen = ref(false)
const mobileMenuOpen = ref(false)
const langDropdownOpen = ref(false)
const langDropdownRef = ref(null)
const windowWidth = ref(window.innerWidth)

const isMobile = computed(() => windowWidth.value < 768)
const showBottomNav = computed(() => isMobile.value && authStore.isAdmin)
const isPwaRoute = computed(() => route.meta?.isPwa === true)

const truncatedEmail = computed(() => {
  const email = authStore.userEmail
  if (!email) return ''
  return email.length > 20 ? email.substring(0, 17) + '...' : email
})

// ═══════════════════════════════════════════════════════
// NYELVVÁLTÓ
// ═══════════════════════════════════════════════════════
const languages = [
  { code: 'hu', label: 'Magyar', flag: '🇭🇺' },
  { code: 'en', label: 'English', flag: '🇬🇧' },
  { code: 'de', label: 'Deutsch', flag: '🇩🇪' },
]

const currentLang = computed(() => languages.find((l) => l.code === locale.value) || languages[0])

function switchLanguage(lang) {
  locale.value = lang
  localStorage.setItem('locale', lang)
  langDropdownOpen.value = false
  mobileMenuOpen.value = false
}

// ═══════════════════════════════════════════════════════
// SCROLL FUNKCIÓK
// ═══════════════════════════════════════════════════════
function handleHomeClick(event) {
  event.preventDefault()
  mobileMenuOpen.value = false
  if (router.currentRoute.value.path === '/') {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } else {
    router.push('/').then(() => window.scrollTo({ top: 0, behavior: 'smooth' }))
  }
}

function scrollToSection(id) {
  mobileMenuOpen.value = false
  if (router.currentRoute.value.path !== '/') {
    router.push('/').then(() => {
      setTimeout(
        () => document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' }),
        100,
      )
    })
  } else {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}

function scrollToEszkozok() {
  scrollToSection('eszkozok')
}
function scrollToVelemenyek() {
  scrollToSection('velemenyek')
}
function scrollToKapcsolat() {
  scrollToSection('kapcsolat')
}

// ═══════════════════════════════════════════════════════
// EGYÉB FUNKCIÓK
// ═══════════════════════════════════════════════════════
function openLoginModal() {
  loginModalOpen.value = true
  mobileMenuOpen.value = false
}

function handleLoginSuccess() {
  console.log('Sikeres bejelentkezés!')
  if (authStore.isAdmin && isMobile.value) {
    router.push('/pwa')
  }
}

function handleLogout() {
  userMenuOpen.value = false
  mobileMenuOpen.value = false
  if (confirm(t('auth.logoutConfirm'))) {
    authStore.signOut()
    router.push('/')
  }
}

function handleResize() {
  windowWidth.value = window.innerWidth
  if (!isMobile.value) mobileMenuOpen.value = false
}

function handleDocumentClick(event) {
  const userMenu = document.querySelector('.user-menu')
  if (userMenu && !userMenu.contains(event.target)) userMenuOpen.value = false
  if (langDropdownRef.value && !langDropdownRef.value.contains(event.target))
    langDropdownOpen.value = false
}

watch(
  () => route.path,
  () => {
    mobileMenuOpen.value = false
    userMenuOpen.value = false
    langDropdownOpen.value = false
  },
)

watch(loginModalOpen, (isOpen) => {
  document.body.style.overflow = isOpen ? 'hidden' : ''
})

onMounted(async () => {
  await authStore.initialize()
  if (authStore.isAdmin) {
    await pushService.registerServiceWorker()
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
      if (!(el === event.target || el.contains(event.target))) binding.value()
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
  font-family:
    'Segoe UI',
    system-ui,
    -apple-system,
    sans-serif;
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

.app-main {
  flex: 1;
  width: 100%;
}

.app-main-pwa {
  flex: 1;
  width: 100%;
  min-height: 100vh;
  padding: 0;
  margin: 0;
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

/* ═══════════════════════════════════════════════════════
   NYELVVÁLTÓ
   ═══════════════════════════════════════════════════════ */

.lang-switcher {
  position: relative;
  display: inline-block;
}

.lang-selected {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: var(--radius-sm);
  color: white;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  white-space: nowrap;
}

.lang-selected:hover {
  background: rgba(255, 255, 255, 0.2);
}

.lang-arrow {
  font-size: 11px;
  transition: transform var(--transition-fast);
  display: inline-block;
}

.lang-arrow.open {
  transform: rotate(180deg);
}

.lang-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  background: var(--color-surface);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  z-index: 300;
  min-width: 140px;
}

.lang-option {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  width: 100%;
  padding: var(--spacing-sm) var(--spacing-md);
  background: none;
  border: none;
  border-bottom: 1px solid var(--color-border);
  text-align: left;
  cursor: pointer;
  color: var(--color-text);
  font-size: 14px;
  font-weight: 500;
  transition: background var(--transition-fast);
}

.lang-option:last-child {
  border-bottom: none;
}
.lang-option:hover {
  background: var(--color-background);
}
.lang-option.active {
  color: var(--color-primary);
  font-weight: 700;
}

/* Mobil nyelvváltó */
.lang-switcher-mobile {
  display: flex;
  flex-direction: row;
  gap: var(--spacing-sm);
  padding: var(--spacing-md) var(--spacing-lg);
  border-bottom: 1px solid var(--color-border);
}

.lang-switcher-mobile .lang-option {
  width: auto;
  padding: 4px 10px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: 13px;
}

.lang-switcher-mobile .lang-option.active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
}

/* ═══════════════════════════════════════════════════════
   TRANSITIONS
   ═══════════════════════════════════════════════════════ */

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
