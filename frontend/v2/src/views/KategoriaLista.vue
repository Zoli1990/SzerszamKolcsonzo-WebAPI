<template>
  <div class="kategoriak">
    <h1>Kategóriák</h1>

    <!-- Ha még tölt az adat -->
    <p v-if="loading">Betöltés...</p>

    <!-- Ha hiba történt -->
    <p v-if="error" class="error-message">Hiba: {{ error }}</p>
    <!-- Ha van adat -->
    <ul v-if="kategoriak.length > 0">
      <li v-for="kategoria in kategoriak" :key="kategoria.id">
        {{ kategoria.nev }}
      </li>
    </ul>
  </div>
</template>

<script>
// Axios importálása
import axios from 'axios'

export default {
  name: 'KategoriaLista',
  data() {
    return {
      kategoriak: [], // itt tároljuk a lekért kategóriákat
      loading: true, // betöltés állapota
      error: null, // hibaüzenet, ha valami elromlik
    }
  },
  mounted() {
    // Amint betölt a komponens, meghívjuk az API-t
    this.lekerKategorak()
  },
  methods: {
    async lekerKategorak() {
      try {
        // 🔗 Itt add meg a saját backend URL-edet!
        const response = await axios.get('https://localhost:7299/api/kategoriak')
        this.kategoriak = response.data
      } catch (err) {
        this.error = err.message
      } finally {
        this.loading = false
      }
    },
  },
}
</script>

<style scoped>
.kategoriak {
  max-width: 600px;
  margin: 30px auto;
  font-family: sans-serif;
}

h1 {
  text-align: center;
  color: #333;
}

ul {
  list-style: none;
  padding: 0;
}

li {
  padding: 8px;
  background: #f2f2f2;
  margin: 5px 0;
  border-radius: 5px;
}

.error-message {
  color: #ef4444;
  font-weight: 600;
  padding: 12px;
  background: #fee2e2;
  border-radius: 6px;
  text-align: center;
}
</style>
