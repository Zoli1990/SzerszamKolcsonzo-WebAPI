<template>
  <div class="kategoriak">
    <h1>{{ t('kategoriak.title') }}</h1>

    <p v-if="loading">{{ t('kategoriak.loading') }}</p>

    <p v-if="error" style="color: red;">{{ t('kategoriak.error', { msg: error }) }}</p>

    <ul v-if="kategoriak.length > 0">
      <li v-for="kategoria in kategoriak" :key="kategoria.id">
        {{ kategoria.nev }}
      </li>
    </ul>
  </div>
</template>

<script>
// Axios importálása
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import axios from 'axios'

const { t } = useI18n()

export default {
  name: 'KategoriaLista',
  data() {
    return {
      kategoriak: [],   // itt tároljuk a lekért kategóriákat
      loading: true,    // betöltés állapota
      error: null       // hibaüzenet, ha valami elromlik
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
    }
  }
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
  color: #3d2f1f;
}

ul {
  list-style: none;
  padding: 0;
}

li {
  padding: 8px;
  background: #f5f1e8;
  margin: 5px 0;
  border-radius: 5px;
  color: #3d2f1f;
  border: 1px solid #e8dcc8;
}
</style>