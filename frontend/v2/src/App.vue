<template>
  <div id="app">
    <header class="app-header">
      <div class="container nav-container">
        <!-- BAL oldal: logó -->
        <div class="nav-left">
          <a href="#home" class="logo">
            <img src="@/assets/tooly_logo.png" alt="Szerszámkölcsönző" class="logo-img" />
            <span>Szerszámkölcsönző</span>
          </a>
        </div>

        <!-- KÖZÉP: menüpontok -->
        <nav class="nav-center">
          <a href="#home" class="nav-link">Kezdőoldal</a>
          <a href="#szolgaltatasok" class="nav-link">Szolgáltatásaink</a>
          <a href="#velemenyek" class="nav-link">Vélemények</a>
          <a href="#rolunk" class="nav-link">Rólunk</a>

          <!-- Admin link -->
          <RouterLink v-if="authStore.isAdmin" to="/admin" class="nav-link">Admin</RouterLink>
        </nav>

        <!-- JOBB oldal: bejelentkezés/kosár -->
        <div class="nav-right">
          <!-- Auth gombok -->
        <div v-if="!authStore.isAuthenticated" class="auth-buttons">
          <button class="btn-login-icon" @click="openLoginModal" title="Belépés">
            <FontAwesomeIcon :icon="['fas', 'user']" />
          </button>
        </div>

<!-- User menü (bejelentkezve) -->
<div v-else class="user-menu">
  <button class="user-button" @click="userMenuOpen = !userMenuOpen">
    <FontAwesomeIcon :icon="['fas', 'user']" />
    {{ authStore.userEmail }}
  </button>
  <div v-if="userMenuOpen" class="dropdown">
    <RouterLink to="/profil" class="dropdown-item" @click="userMenuOpen = false">
      <FontAwesomeIcon :icon="['fas', 'clipboard-list']" />
      Foglalásaim
    </RouterLink>
    <button class="dropdown-item" @click="handleLogout">
      <FontAwesomeIcon :icon="['fas', 'right-from-bracket']" />
      Kilépés
    </button>
  </div>
</div>

          <!-- Kosár gomb -->
          <button class="cart-icon" @click="toggleCart" title="Kosár">
            <FontAwesomeIcon :icon="['fas', 'cart-shopping']" />
            <span v-if="kosarStore.itemCount > 0" class="cart-badge">{{
              kosarStore.itemCount
            }}</span>
          </button>
        </div>
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

    <!-- Kosár Sidebar -->
    <KosarSidebar :is-open="cartOpen" @close="cartOpen = false" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink, RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useKosarStore } from '@/stores/kosarStore'
import LoginModal from '@/components/auth/LoginModal.vue'
import KosarSidebar from '@/components/kosar/KosarSidebar.vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome' // ← ÚJ

const authStore = useAuthStore()
const kosarStore = useKosarStore()
const loginModalOpen = ref(false)
const userMenuOpen = ref(false)
const cartOpen = ref(false)

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
  await authStore.signOut()
}

function toggleCart() {
  cartOpen.value = !cartOpen.value
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

:root {
  --dark-blue: #1a3a52;
  --medium-blue: #2c5f7f;
  --earth-brown: #8b6f47;
  --light-brown: #a68a64;
  --forest-green: #4a6741;
  --sage-green: #121312;
  --cream: #f5f1e8;
  --light-bg: #fafaf7;
  --white: #ffffff;
  --text-dark: #2d2d2d;
  --text-light: #666;
}

body {
  background-color: var(--light-brown);
  color: var(--text-dark);
  line-height: 1.6;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.container {
  margin: 0 auto;
  padding: 0 20px;
}

/* HEADER */
.app-header {
  background-color: var(--dark-blue);
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
  padding: 1rem 5%;
}

.nav-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-left .logo {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 1.4rem;
  font-weight: bold;
  text-decoration: none;
  color: white;
}

.logo-img {
  height: 50px;
  width: auto;
  border-radius: 25%;
}

.nav-center {
  display: flex;
  gap: 24px;
}

.nav-link {
  color: var(--cream);
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s;
  font-size: 1rem;
}

.nav-link:hover {
  color: var(--sage-green);
}

.nav-right {
  display: flex;
  gap: 1.5rem;
  color: var(--cream);
  font-size: 1.2rem;
}

/* AUTH */
.auth-buttons {
  display: flex;
  gap: 12px;
  align-items: center;
}

.btn-login-icon {
  width: 40px;
  height: 40px;
  background: transparent;
  color: white; 
  border: none;
  border-radius: 50%;
  font-size: 1.3rem;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-login-icon:hover {
  color: var(--sage-green);
  transform: scale(1.1);
}

.user-menu {
  position: relative;
}

.user-button {
   display: inline-block;
    padding: 0.8rem 2rem;
    background-color: var(--earth-brown);
    color: var(--white);
    text-decoration: none;
    border-radius: 5px;
    font-weight: 600;
    transition: all 0.3s;
    border: 2px solid var(--earth-brown);
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

/*kosár*/
.cart-icon {
  position: relative;
  font-size: 1.3rem;
  background: none;
  border: none;
  cursor: pointer;
  color: white;
  transition: color 0.2s;
  padding: 8px;
}

.cart-icon:hover {
  color: var(--sage-green);
}

.cart-badge {
  position: absolute;
  top: -8px;
  right: -8px;
  background: #ef4444;
  color: white;
  font-size: 0.75rem;
  font-weight: bold;
  border-radius: 50%;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* MAIN */
.app-main {
  flex: 1;
  padding: 20px 0;
}

/* FOOTER */
.app-footer {
  background: #1f2937;
  color: white;
  padding: 24px 0;
  text-align: center;
  margin-top: 40px;
}

/* SMOOTH SCROLL */
html {
  scroll-behavior: smooth;
}
</style>
