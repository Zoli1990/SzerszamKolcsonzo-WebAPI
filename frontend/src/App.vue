<template>
  <div id="app">
    <header class="app-header">
      <div class="container">
        <RouterLink to="/" class="logo"> 🔧 Szerszámkölcsönző </RouterLink>

        <nav class="main-nav">
         <a href="#" class="nav-link" @click.prevent="handleHomeClick">Főoldal</a>
          <!-- Eszközök link -->
          <a href="#eszkozok" class="nav-link" @click.prevent="scrollToEszkozok">Eszközök</a>
          <!-- Vélemények link -->
          <a href="#velemenyek" class="nav-link" @click.prevent="scrollToVelemenyek">Vélemények</a>
          <!-- Kapcsolat link -->
          <a href="#kapcsolat" class="nav-link" @click.prevent="scrollToKapcsolat">Kapcsolat</a>

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
              <!-- ✅ JAVÍTVA: Csak NEM admin felhasználóknak -->
              <template v-if="!authStore.isAdmin">
                <RouterLink to="/profilom" class="dropdown-item" @click="userMenuOpen = false">
                  👤 Profilom
                </RouterLink>
                <RouterLink to="/profil" class="dropdown-item" @click="userMenuOpen = false">
                  📋 Foglalásaim
                </RouterLink>
              </template>
              
              <!-- Mindenkinek (admin + user) -->
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

// Főoldal kattintás kezelése
function handleHomeClick(event) {
  event.preventDefault()

  // Ha már a főoldalon vagyunk
  if (router.currentRoute.value.path === '/') {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } else {
    // Ha más oldalon vagyunk, navigálj a főoldalra
    router.push('/').then(() => {
      window.scrollTo({ top: 0, behavior: 'smooth' })
    })
  }
}

// Scroll a véleményekhez
function scrollToVelemenyek() {
  // Ha nem a főoldalon vagyunk, először navigálj oda
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

// Scroll az eszközökhöz
function scrollToEszkozok() {
  // Ha nem a főoldalon vagyunk, először navigálj oda
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

// ============================================================================
// ✅ ÚJ: Scroll a kapcsolat szekcióhoz
// ============================================================================
function scrollToKapcsolat() {
  // Ha nem a főoldalon vagyunk, először navigálj oda
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
  background: #6b8e23;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-login:hover {
  background: #5a7a3c;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(107, 142, 35, 0.3);
}

.user-menu {
  position: relative;
}

.user-button {
  padding: 8px 16px;
  background: white;
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  color: #3d2f1f;
  transition: all 0.2s;
}

.user-button:hover {
  border-color: #6b8e23;
  color: #6b8e23;
}

.dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  background: #faf8f5;
  border-radius: 8px;
  box-shadow: 0 10px 40px rgba(61, 47, 31, 0.3);
  min-width: 200px;
  z-index: 100;
  border: 1px solid #e8dcc8;
}

.dropdown-item {
  display: block;
  width: 100%;
  padding: 12px 16px;
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
  color: #3d2f1f;
  font-weight: 500;
  text-decoration: none;
  transition: all 0.2s;
}

.dropdown-item:hover {
  background: #f0ebe3;
  color: #6b8e23;
}

.dropdown-item:first-child {
  border-radius: 8px 8px 0 0;
}

.dropdown-item:last-child {
  border-radius: 0 0 8px 8px;
}
</style>