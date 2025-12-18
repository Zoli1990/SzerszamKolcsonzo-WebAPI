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
              <input type="email" v-model="form.email" placeholder="email@example.com" required />
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

            <!-- ✅ Kibővített cím mezők regisztrációnál -->
            <template v-if="!isLogin">
              <div class="section-divider">Cím adatok</div>

              <div class="form-row">
                <div class="form-group small">
                  <label>Irányítószám *</label>
                  <input
                    type="text"
                    v-model="form.iranyitoszam"
                    placeholder="6720"
                    required
                    maxlength="4"
                    pattern="[1-9][0-9]{3}"
                    @input="onIranyitoszamChange"
                  />
                </div>

                <div class="form-group flex-grow">
                  <label>Település</label>
                  <input
                    type="text"
                    v-model="form.telepules"
                    placeholder="Automatikusan kitöltődik..."
                    :class="{ 'auto-filled': telepulesAutoFilled }"
                    readonly
                  />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group flex-grow">
                  <label>Utca *</label>
                  <input
                    type="text"
                    v-model="form.utca"
                    placeholder="Kossuth Lajos utca"
                    required
                  />
                </div>

                <div class="form-group small">
                  <label>Házszám *</label>
                  <input
                    type="text"
                    v-model="form.hazszam"
                    placeholder="12/A"
                    required
                  />
                </div>
              </div>

              <div class="form-group">
                <label>Telefonszám</label>
                <input
                  type="tel"
                  v-model="form.telefonszam"
                  placeholder="+36301234567"
                />
              </div>
            </template>

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
              {{ loading ? 'Feldolgozás...' : isLogin ? 'Bejelentkezés' : 'Regisztráció' }}
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
  isOpen: Boolean,
})

const emit = defineEmits(['close', 'success'])

const authStore = useAuthStore()
const isLogin = ref(true)
const loading = ref(false)
const error = ref(null)
const successMessage = ref('')
const telepulesAutoFilled = ref(false)

const form = ref({
  email: '',
  password: '',
  passwordConfirm: '',
  iranyitoszam: '',
  telepules: '',
  utca: '',
  hazszam: '',
  telefonszam: '',
})

// Irányítószám alapján település keresése
async function onIranyitoszamChange() {
  const irsz = form.value.iranyitoszam
  
  // Ha 4 számjegyű
  if (irsz && irsz.length === 4 && /^[1-9]\d{3}$/.test(irsz)) {
    try {
      // Magyar posta API vagy egyszerű mapping
      const telepules = await getTelepulesByIranyitoszam(irsz)
      if (telepules) {
        form.value.telepules = telepules
        telepulesAutoFilled.value = true
      }
    } catch (err) {
      console.warn('Település keresése sikertelen:', err)
      telepulesAutoFilled.value = false
    }
  } else {
    form.value.telepules = ''
    telepulesAutoFilled.value = false
  }
}

// Egyszerű irányítószám -> település mapping (bővíthető)
async function getTelepulesByIranyitoszam(irsz) {
  // Gyakori magyar irányítószámok
  const mapping = {
    // Budapest
    '1011': 'Budapest', '1012': 'Budapest', '1013': 'Budapest',
    '1014': 'Budapest', '1015': 'Budapest', '1016': 'Budapest',
    '1051': 'Budapest', '1052': 'Budapest', '1053': 'Budapest',
    '1054': 'Budapest', '1055': 'Budapest', '1056': 'Budapest',
    '1061': 'Budapest', '1062': 'Budapest', '1063': 'Budapest',
    '1064': 'Budapest', '1065': 'Budapest', '1066': 'Budapest',
    '1067': 'Budapest', '1068': 'Budapest', '1071': 'Budapest',
    '1072': 'Budapest', '1073': 'Budapest', '1074': 'Budapest',
    '1075': 'Budapest', '1076': 'Budapest', '1077': 'Budapest',
    '1078': 'Budapest', '1081': 'Budapest', '1082': 'Budapest',
    '1083': 'Budapest', '1084': 'Budapest', '1085': 'Budapest',
    '1086': 'Budapest', '1087': 'Budapest', '1088': 'Budapest',
    '1089': 'Budapest', '1091': 'Budapest', '1092': 'Budapest',
    '1093': 'Budapest', '1094': 'Budapest', '1095': 'Budapest',
    '1096': 'Budapest', '1097': 'Budapest', '1098': 'Budapest',
    '1101': 'Budapest', '1102': 'Budapest', '1103': 'Budapest',
    '1104': 'Budapest', '1105': 'Budapest', '1106': 'Budapest',
    '1107': 'Budapest', '1108': 'Budapest', '1111': 'Budapest',
    '1112': 'Budapest', '1113': 'Budapest', '1114': 'Budapest',
    '1115': 'Budapest', '1116': 'Budapest', '1117': 'Budapest',
    '1118': 'Budapest', '1119': 'Budapest', '1121': 'Budapest',
    '1122': 'Budapest', '1123': 'Budapest', '1124': 'Budapest',
    '1125': 'Budapest', '1126': 'Budapest', '1131': 'Budapest',
    '1132': 'Budapest', '1133': 'Budapest', '1134': 'Budapest',
    '1135': 'Budapest', '1136': 'Budapest', '1137': 'Budapest',
    '1138': 'Budapest', '1139': 'Budapest', '1141': 'Budapest',
    '1142': 'Budapest', '1143': 'Budapest', '1144': 'Budapest',
    '1145': 'Budapest', '1146': 'Budapest', '1147': 'Budapest',
    '1148': 'Budapest', '1149': 'Budapest', '1151': 'Budapest',
    '1152': 'Budapest', '1153': 'Budapest', '1154': 'Budapest',
    '1155': 'Budapest', '1156': 'Budapest', '1157': 'Budapest',
    '1158': 'Budapest', '1161': 'Budapest', '1162': 'Budapest',
    '1163': 'Budapest', '1164': 'Budapest', '1165': 'Budapest',
    '1171': 'Budapest', '1172': 'Budapest', '1173': 'Budapest',
    '1174': 'Budapest', '1181': 'Budapest', '1182': 'Budapest',
    '1183': 'Budapest', '1184': 'Budapest', '1185': 'Budapest',
    '1186': 'Budapest', '1188': 'Budapest', '1191': 'Budapest',
    '1192': 'Budapest', '1193': 'Budapest', '1194': 'Budapest',
    '1195': 'Budapest', '1196': 'Budapest', '1201': 'Budapest',
    '1202': 'Budapest', '1203': 'Budapest', '1204': 'Budapest',
    '1205': 'Budapest', '1211': 'Budapest', '1212': 'Budapest',
    '1213': 'Budapest', '1214': 'Budapest', '1215': 'Budapest',
    '1221': 'Budapest', '1222': 'Budapest', '1223': 'Budapest',
    '1224': 'Budapest', '1225': 'Budapest', '1237': 'Budapest',
    '1238': 'Budapest', '1239': 'Budapest',
    // Nagyvárosok
    '6720': 'Szeged', '6721': 'Szeged', '6722': 'Szeged', '6723': 'Szeged',
    '6724': 'Szeged', '6725': 'Szeged', '6726': 'Szeged', '6727': 'Szeged',
    '6728': 'Szeged', '6729': 'Szeged',
    '4024': 'Debrecen', '4025': 'Debrecen', '4026': 'Debrecen', '4027': 'Debrecen',
    '4028': 'Debrecen', '4029': 'Debrecen', '4030': 'Debrecen', '4031': 'Debrecen',
    '4032': 'Debrecen', '4033': 'Debrecen', '4034': 'Debrecen',
    '3525': 'Miskolc', '3526': 'Miskolc', '3527': 'Miskolc', '3528': 'Miskolc',
    '3529': 'Miskolc', '3530': 'Miskolc', '3531': 'Miskolc', '3532': 'Miskolc',
    '3533': 'Miskolc', '3534': 'Miskolc', '3535': 'Miskolc',
    '7621': 'Pécs', '7622': 'Pécs', '7623': 'Pécs', '7624': 'Pécs',
    '7625': 'Pécs', '7626': 'Pécs', '7627': 'Pécs', '7628': 'Pécs', '7629': 'Pécs',
    '9021': 'Győr', '9022': 'Győr', '9023': 'Győr', '9024': 'Győr',
    '9025': 'Győr', '9026': 'Győr', '9027': 'Győr', '9028': 'Győr', '9029': 'Győr',
    '6000': 'Kecskemét', '6001': 'Kecskemét',
    '5000': 'Szolnok', '5001': 'Szolnok', '5002': 'Szolnok',
    '8200': 'Veszprém', '8201': 'Veszprém',
    '2400': 'Dunaújváros', '2401': 'Dunaújváros',
    '2500': 'Esztergom', '2501': 'Esztergom',
    '2600': 'Vác', '2601': 'Vác',
    '2700': 'Cegléd', '2701': 'Cegléd',
    '2800': 'Tatabánya', '2801': 'Tatabánya',
    '2900': 'Komárom', '2901': 'Komárom',
    '3000': 'Hatvan', '3001': 'Hatvan',
    '3100': 'Salgótarján', '3101': 'Salgótarján',
    '3200': 'Gyöngyös', '3201': 'Gyöngyös',
    '3300': 'Eger', '3301': 'Eger',
    '3400': 'Mezőkövesd',
    '3500': 'Mezőkövesd',
    '3600': 'Ózd',
    '3700': 'Kazincbarcika',
    '3800': 'Szikszó',
    '3900': 'Szerencs',
    '4100': 'Berettyóújfalu',
    '4200': 'Hajdúszoboszló',
    '4300': 'Nyírbátor',
    '4400': 'Nyíregyháza', '4401': 'Nyíregyháza',
    '4500': 'Kisvárda',
    '4600': 'Vásárosnamény',
    '4700': 'Mátészalka',
    '5100': 'Jászberény',
    '5200': 'Törökszentmiklós',
    '5300': 'Karcag',
    '5400': 'Mezőtúr',
    '5500': 'Gyomaendrőd',
    '5600': 'Békéscsaba', '5601': 'Békéscsaba',
    '5700': 'Gyula',
    '5800': 'Mezőkovácsháza',
    '5900': 'Orosháza',
    '6100': 'Kiskunfélegyháza',
    '6200': 'Kiskőrös',
    '6300': 'Kalocsa',
    '6400': 'Kiskunhalas',
    '6500': 'Baja',
    '6600': 'Szentes',
    '6700': 'Szeged',
    '6800': 'Hódmezővásárhely',
    '6900': 'Makó',
    '7000': 'Sárbogárd',
    '7100': 'Szekszárd',
    '7200': 'Dombóvár',
    '7300': 'Komló',
    '7400': 'Kaposvár', '7401': 'Kaposvár',
    '7500': 'Nagyatád',
    '7600': 'Pécs',
    '7700': 'Mohács',
    '7800': 'Siklós',
    '7900': 'Szigetvár',
    '8000': 'Székesfehérvár', '8001': 'Székesfehérvár',
    '8100': 'Várpalota',
    '8300': 'Tapolca',
    '8360': 'Keszthely',
    '8400': 'Ajka',
    '8500': 'Pápa',
    '8600': 'Siófok',
    '8700': 'Marcali',
    '8800': 'Nagykanizsa', '8801': 'Nagykanizsa',
    '8900': 'Zalaegerszeg', '8901': 'Zalaegerszeg',
    '9001': 'Győr',
    '9100': 'Mosonmagyaróvár',
    '9200': 'Mosonmagyaróvár',
    '9300': 'Csorna',
    '9400': 'Sopron', '9401': 'Sopron',
    '9500': 'Celldömölk',
    '9600': 'Sárvár',
    '9700': 'Szombathely', '9701': 'Szombathely',
    '9800': 'Vasvár',
    '9900': 'Körmend',
  }
  
  return mapping[irsz] || null
}

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
    passwordConfirm: '',
    iranyitoszam: '',
    telepules: '',
    utca: '',
    hazszam: '',
    telefonszam: '',
  }
  error.value = null
  successMessage.value = ''
  isLogin.value = true
  telepulesAutoFilled.value = false
}

async function handleSubmit() {
  loading.value = true
  error.value = null
  successMessage.value = ''

  try {
    // Email normalizálás: trim + lowercase
    const normalizedEmail = form.value.email.trim().toLowerCase()

    if (isLogin.value) {
      // Bejelentkezés
      await authStore.signIn(normalizedEmail, form.value.password)
      emit('success', 'login')
      closeModal()
    } else {
      // Regisztráció
      if (form.value.password !== form.value.passwordConfirm) {
        error.value = 'A jelszavak nem egyeznek!'
        return
      }

      // ✅ Regisztráció kibővített cím mezőkkel
      await authStore.signUp(
        normalizedEmail, 
        form.value.password,
        form.value.iranyitoszam,
        form.value.telepules,
        form.value.utca,
        form.value.hazszam,
        form.value.telefonszam
      )

      successMessage.value = 'Sikeres regisztráció!'

      // 2 mp múlva modal bezárása
      setTimeout(() => {
        emit('success', 'register')
        closeModal()
      }, 2000)
    }
  } catch (err) {
    console.error('❌ Auth hiba:', err)

    // Backend hibaüzenet megjelenítése
    if (err.response?.data?.message) {
      error.value = err.response.data.message
    } else if (err.response?.data?.errors) {
      // Validation errors
      const errors = Object.values(err.response.data.errors).flat()
      error.value = errors.join(' ')
    } else if (err.response?.data) {
      error.value = JSON.stringify(err.response.data)
    } else {
      error.value = 'Hiba történt. Próbáld újra!'
    }
  } finally {
    loading.value = false
  }
}

// Reset form amikor modal megnyílik
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      resetForm()
    }
  },
)
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(61, 47, 31, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 2000;
  padding: 20px;
  overflow-y: auto;
}

.modal-container {
  background: #faf8f5;
  border-radius: 12px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(61, 47, 31, 0.3);
  border: 1px solid #e8dcc8;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 2px solid #e8dcc8;
  position: sticky;
  top: 0;
  background: #faf8f5;
  z-index: 10;
}

.modal-header h2 {
  margin: 0;
  font-size: 24px;
  color: #3d2f1f;
}

.btn-close {
  background: none;
  border: none;
  font-size: 32px;
  color: #6b5d4f;
  cursor: pointer;
  line-height: 1;
  transition: color 0.2s;
}

.btn-close:hover {
  color: #3d2f1f;
}

.auth-form {
  padding: 24px;
  background: white;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #3d2f1f;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 12px;
  border: 2px solid #d4c5b0;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  color: #3d2f1f;
  transition: all 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #6b8e23;
  box-shadow: 0 0 0 3px rgba(107, 142, 35, 0.1);
}

.form-group input.auto-filled {
  background: #f0f7e8;
  border-color: #7a9b57;
}

.form-group input[readonly] {
  background: #f5f1eb;
  cursor: not-allowed;
  color: #6b5d4f;
}

.form-row {
  display: flex;
  gap: 12px;
}

.form-row .form-group.small {
  width: 120px;
  flex-shrink: 0;
}

.form-row .form-group.flex-grow {
  flex: 1;
}

.section-divider {
  font-size: 14px;
  font-weight: 600;
  color: #6b5d4f;
  margin: 24px 0 16px 0;
  padding-bottom: 8px;
  border-bottom: 2px solid #e8dcc8;
}

.error-message {
  padding: 12px;
  background: #fef2f2;
  border: 2px solid #f8b4b4;
  border-radius: 8px;
  color: #b85c5c;
  font-size: 14px;
  margin-bottom: 20px;
}

.success-message {
  padding: 12px;
  background: #f0f7e8;
  border: 2px solid #a8c98a;
  border-radius: 8px;
  color: #5a7a3c;
  font-size: 14px;
  margin-bottom: 20px;
}

.btn-primary {
  padding: 12px 24px;
  background: #6b8e23;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 16px;
}

.btn-primary:hover:not(:disabled) {
  background: #5a7a3c;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(107, 142, 35, 0.3);
}

.btn-primary:disabled {
  background: #c4b5a0;
  cursor: not-allowed;
}

.btn-full {
  width: 100%;
}

.form-footer {
  margin-top: 20px;
  text-align: center;
  padding-top: 20px;
  border-top: 2px solid #e8dcc8;
}

.btn-link {
  background: none;
  border: none;
  color: #6b8e23;
  font-weight: 600;
  cursor: pointer;
  font-size: 14px;
  transition: color 0.2s;
}

.btn-link:hover {
  color: #5a7a3c;
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

/* Responsive */
@media (max-width: 500px) {
  .form-row {
    flex-direction: column;
  }
  
  .form-row .form-group.small {
    width: 100%;
  }
}
</style>