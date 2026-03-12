<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="isOpen" class="modal-overlay" data-testid="login-modal-overlay" @click="closeModal">
        <div class="modal-container" data-testid="login-modal-container" @click.stop>
          <div class="modal-header">
            <h2>{{ isLogin ? t('loginModal.login') : t('loginModal.register') }}</h2>
            <button id="btn-close-modal" class="btn-close" data-testid="btn-close-modal" @click="closeModal">&times;</button>
          </div>

          <form @submit.prevent="handleSubmit" class="auth-form">
            <div class="form-group">
              <label>{{ t('loginModal.email') }} *</label>
              <input id="login-email" type="email" v-model="form.email" data-testid="login-email" placeholder="email@example.com" required />
            </div>

            <div class="form-group">
              <label>{{ t('loginModal.password') }} *</label>
              <input id="login-password" type="password" v-model="form.password" data-testid="login-password" :placeholder="t('loginModal.passwordPlaceholder')" required minlength="6" />
            </div>

            <div v-if="!isLogin" class="form-group">
              <label>{{ t('loginModal.passwordConfirm') }} *</label>
              <input type="password" v-model="form.passwordConfirm" :placeholder="t('loginModal.passwordConfirmPlaceholder')" required />
            </div>

            <template v-if="!isLogin">
              <div class="section-divider">{{ t('loginModal.addressSection') }}</div>

              <div class="form-row">
                <div class="form-group small">
                  <label>{{ t('loginModal.zip') }} *</label>
                  <input type="text" v-model="form.iranyitoszam" placeholder="6720" required maxlength="4" pattern="[1-9][0-9]{3}" @input="onIranyitoszamChange" />
                </div>
                <div class="form-group flex-grow">
                  <label>{{ t('loginModal.city') }}</label>
                  <input type="text" v-model="form.telepules" :placeholder="t('loginModal.cityAuto')" :class="{ 'auto-filled': telepulesAutoFilled }" readonly />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group flex-grow">
                  <label>{{ t('loginModal.street') }} *</label>
                  <input type="text" v-model="form.utca" :placeholder="t('loginModal.streetPlaceholder')" required />
                </div>
                <div class="form-group small">
                  <label>{{ t('loginModal.houseNumber') }} *</label>
                  <input type="text" v-model="form.hazszam" placeholder="12/A" required />
                </div>
              </div>

              <div class="form-group">
                <label>{{ t('loginModal.phone') }}</label>
                <input type="tel" v-model="form.telefonszam" placeholder="+36301234567" />
              </div>
            </template>

            <div v-if="error" class="error-message">{{ error }}</div>
            <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

            <button id="btn-submit-login" type="submit" data-testid="btn-submit-login" class="btn-primary btn-full" :disabled="loading">
              {{ loading ? t('loginModal.processing') : isLogin ? t('loginModal.login') : t('loginModal.register') }}
            </button>

            <div class="form-footer">
              <button type="button" class="btn-link" @click="toggleMode">
                {{ isLogin ? t('loginModal.noAccount') : t('loginModal.hasAccount') }}
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
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/authStore'

const { t } = useI18n()

const props = defineProps({ isOpen: Boolean })
defineOptions({ inheritAttrs: false })
const emit = defineEmits(['close', 'success'])

const authStore = useAuthStore()
const isLogin = ref(true)
const loading = ref(false)
const error = ref(null)
const successMessage = ref('')
const telepulesAutoFilled = ref(false)

const form = ref({
  email: '', password: '', passwordConfirm: '',
  iranyitoszam: '', telepules: '', utca: '', hazszam: '', telefonszam: '',
})

async function onIranyitoszamChange() {
  const irsz = form.value.iranyitoszam
  if (irsz && irsz.length === 4 && /^[1-9]\d{3}$/.test(irsz)) {
    try {
      const telepules = await getTelepulesByIranyitoszam(irsz)
      if (telepules) { form.value.telepules = telepules; telepulesAutoFilled.value = true }
    } catch (err) { console.warn('Település keresése sikertelen:', err); telepulesAutoFilled.value = false }
  } else { form.value.telepules = ''; telepulesAutoFilled.value = false }
}

async function getTelepulesByIranyitoszam(irsz) {
  const mapping = {
    1011:'Budapest',1012:'Budapest',1013:'Budapest',1014:'Budapest',1015:'Budapest',1016:'Budapest',1051:'Budapest',1052:'Budapest',1053:'Budapest',1054:'Budapest',1055:'Budapest',1056:'Budapest',1061:'Budapest',1062:'Budapest',1063:'Budapest',1064:'Budapest',1065:'Budapest',1066:'Budapest',1067:'Budapest',1068:'Budapest',1071:'Budapest',1072:'Budapest',1073:'Budapest',1074:'Budapest',1075:'Budapest',1076:'Budapest',1077:'Budapest',1078:'Budapest',1081:'Budapest',1082:'Budapest',1083:'Budapest',1084:'Budapest',1085:'Budapest',1086:'Budapest',1087:'Budapest',1088:'Budapest',1089:'Budapest',1091:'Budapest',1092:'Budapest',1093:'Budapest',1094:'Budapest',1095:'Budapest',1096:'Budapest',1097:'Budapest',1098:'Budapest',1101:'Budapest',1102:'Budapest',1103:'Budapest',1104:'Budapest',1105:'Budapest',1106:'Budapest',1107:'Budapest',1108:'Budapest',1111:'Budapest',1112:'Budapest',1113:'Budapest',1114:'Budapest',1115:'Budapest',1116:'Budapest',1117:'Budapest',1118:'Budapest',1119:'Budapest',1121:'Budapest',1122:'Budapest',1123:'Budapest',1124:'Budapest',1125:'Budapest',1126:'Budapest',1131:'Budapest',1132:'Budapest',1133:'Budapest',1134:'Budapest',1135:'Budapest',1136:'Budapest',1137:'Budapest',1138:'Budapest',1139:'Budapest',1141:'Budapest',1142:'Budapest',1143:'Budapest',1144:'Budapest',1145:'Budapest',1146:'Budapest',1147:'Budapest',1148:'Budapest',1149:'Budapest',1151:'Budapest',1152:'Budapest',1153:'Budapest',1154:'Budapest',1155:'Budapest',1156:'Budapest',1157:'Budapest',1158:'Budapest',1161:'Budapest',1162:'Budapest',1163:'Budapest',1164:'Budapest',1165:'Budapest',1171:'Budapest',1172:'Budapest',1173:'Budapest',1174:'Budapest',1181:'Budapest',1182:'Budapest',1183:'Budapest',1184:'Budapest',1185:'Budapest',1186:'Budapest',1188:'Budapest',1191:'Budapest',1192:'Budapest',1193:'Budapest',1194:'Budapest',1195:'Budapest',1196:'Budapest',1201:'Budapest',1202:'Budapest',1203:'Budapest',1204:'Budapest',1205:'Budapest',1211:'Budapest',1212:'Budapest',1213:'Budapest',1214:'Budapest',1215:'Budapest',1221:'Budapest',1222:'Budapest',1223:'Budapest',1224:'Budapest',1225:'Budapest',1237:'Budapest',1238:'Budapest',1239:'Budapest',
    6720:'Szeged',6721:'Szeged',6722:'Szeged',6723:'Szeged',6724:'Szeged',6725:'Szeged',6726:'Szeged',6727:'Szeged',6728:'Szeged',6729:'Szeged',
    4024:'Debrecen',4025:'Debrecen',4026:'Debrecen',4027:'Debrecen',4028:'Debrecen',4029:'Debrecen',4030:'Debrecen',4031:'Debrecen',4032:'Debrecen',4033:'Debrecen',4034:'Debrecen',
    3525:'Miskolc',3526:'Miskolc',3527:'Miskolc',3528:'Miskolc',3529:'Miskolc',3530:'Miskolc',3531:'Miskolc',3532:'Miskolc',3533:'Miskolc',3534:'Miskolc',3535:'Miskolc',
    7621:'Pécs',7622:'Pécs',7623:'Pécs',7624:'Pécs',7625:'Pécs',7626:'Pécs',7627:'Pécs',7628:'Pécs',7629:'Pécs',
    9021:'Győr',9022:'Győr',9023:'Győr',9024:'Győr',9025:'Győr',9026:'Győr',9027:'Győr',9028:'Győr',9029:'Győr',
    6000:'Kecskemét',6001:'Kecskemét',5000:'Szolnok',5001:'Szolnok',5002:'Szolnok',8200:'Veszprém',8201:'Veszprém',2400:'Dunaújváros',2401:'Dunaújváros',2500:'Esztergom',2501:'Esztergom',2600:'Vác',2601:'Vác',2700:'Cegléd',2701:'Cegléd',2800:'Tatabánya',2801:'Tatabánya',2900:'Komárom',2901:'Komárom',3000:'Hatvan',3001:'Hatvan',3100:'Salgótarján',3101:'Salgótarján',3200:'Gyöngyös',3201:'Gyöngyös',3300:'Eger',3301:'Eger',4400:'Nyíregyháza',4401:'Nyíregyháza',5600:'Békéscsaba',5601:'Békéscsaba',5700:'Gyula',5900:'Orosháza',6500:'Baja',6700:'Szeged',6800:'Hódmezővásárhely',6900:'Makó',7100:'Szekszárd',7400:'Kaposvár',7401:'Kaposvár',7600:'Pécs',7700:'Mohács',8000:'Székesfehérvár',8001:'Székesfehérvár',8600:'Siófok',8800:'Nagykanizsa',8801:'Nagykanizsa',8900:'Zalaegerszeg',8901:'Zalaegerszeg',9001:'Győr',9400:'Sopron',9401:'Sopron',9700:'Szombathely',9701:'Szombathely',
  }
  return mapping[irsz] || null
}

function toggleMode() { isLogin.value = !isLogin.value; error.value = null; successMessage.value = '' }

function closeModal() { emit('close'); resetForm() }

function resetForm() {
  form.value = { email: '', password: '', passwordConfirm: '', iranyitoszam: '', telepules: '', utca: '', hazszam: '', telefonszam: '' }
  error.value = null; successMessage.value = ''; isLogin.value = true; telepulesAutoFilled.value = false
}

async function handleSubmit() {
  loading.value = true; error.value = null; successMessage.value = ''
  try {
    const normalizedEmail = form.value.email.trim().toLowerCase()
    if (isLogin.value) {
      await authStore.signIn(normalizedEmail, form.value.password)
      emit('success', 'login'); closeModal()
    } else {
      if (form.value.password !== form.value.passwordConfirm) { error.value = t('loginModal.errors.passwordMismatch'); return }
      await authStore.signUp(normalizedEmail, form.value.password, form.value.iranyitoszam, form.value.telepules, form.value.utca, form.value.hazszam, form.value.telefonszam)
      successMessage.value = t('loginModal.success.registered')
      setTimeout(() => { emit('success', 'register'); closeModal() }, 2000)
    }
  } catch (err) {
    console.error('❌ Auth hiba:', err)
    if (err.response?.data?.message) error.value = err.response.data.message
    else if (err.response?.data?.errors) error.value = Object.values(err.response.data.errors).flat().join(' ')
    else if (err.response?.data) error.value = JSON.stringify(err.response.data)
    else error.value = t('loginModal.errors.general')
  } finally {
    loading.value = false
  }
}

watch(() => props.isOpen, (newVal) => { if (newVal) resetForm() })
</script>

<style scoped>
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); display: flex; justify-content: center; align-items: center; z-index: 2000; padding: 20px; overflow-y: auto; }
.modal-container { background: white; border-radius: 12px; max-width: 500px; width: 100%; max-height: 90vh; overflow-y: auto; box-shadow: 0 20px 60px rgba(0,0,0,0.3); }
.modal-header { display: flex; justify-content: space-between; align-items: center; padding: 20px 24px; border-bottom: 1px solid #e5e7eb; position: sticky; top: 0; background: white; z-index: 10; }
.modal-header h2 { margin: 0; font-size: 24px; color: #1f2937; }
.btn-close { background: none; border: none; font-size: 32px; color: #6b7280; cursor: pointer; line-height: 1; }
.auth-form { padding: 24px; }
.form-group { margin-bottom: 20px; }
.form-group label { display: block; margin-bottom: 8px; font-weight: 600; color: #374151; font-size: 14px; }
.form-group input { width: 100%; padding: 12px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; }
.form-group input:focus { outline: none; border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
.form-group input.auto-filled { background: #f0fdf4; border-color: #86efac; }
.form-group input[readonly] { background: #f9fafb; cursor: not-allowed; }
.form-row { display: flex; gap: 12px; }
.form-row .form-group.small { width: 120px; flex-shrink: 0; }
.form-row .form-group.flex-grow { flex: 1; }
.section-divider { font-size: 14px; font-weight: 600; color: #6b7280; margin: 24px 0 16px 0; padding-bottom: 8px; border-bottom: 2px solid #e5e7eb; }
.error-message { padding: 12px; background: #fef2f2; border: 1px solid #fecaca; border-radius: 6px; color: #dc2626; font-size: 14px; margin-bottom: 20px; }
.success-message { padding: 12px; background: #d1fae5; border: 1px solid #a7f3d0; border-radius: 6px; color: #065f46; font-size: 14px; margin-bottom: 20px; }
.btn-primary { padding: 12px 24px; background: #3b82f6; color: white; border: none; border-radius: 6px; font-weight: 600; cursor: pointer; transition: background 0.2s; font-size: 16px; }
.btn-primary:hover:not(:disabled) { background: #2563eb; }
.btn-primary:disabled { background: #9ca3af; cursor: not-allowed; }
.btn-full { width: 100%; }
.form-footer { margin-top: 20px; text-align: center; padding-top: 20px; border-top: 1px solid #e5e7eb; }
.btn-link { background: none; border: none; color: #3b82f6; font-weight: 600; cursor: pointer; font-size: 14px; }
.btn-link:hover { text-decoration: underline; }
.modal-enter-active, .modal-leave-active { transition: opacity 0.3s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
@media (max-width: 768px) {
  .modal-overlay { padding: 0; align-items: flex-start; background: rgba(0,0,0,0.4); }
  .modal-container { max-width: 100%; width: 100%; min-height: 100vh; max-height: 100vh; border-radius: 0; display: flex; flex-direction: column; }
  .modal-header { padding: 16px 20px; }
  .modal-header h2 { font-size: 20px; }
  .btn-close { font-size: 28px; min-width: 44px; min-height: 44px; }
  .auth-form { flex: 1; overflow-y: auto; padding: 20px; padding-bottom: env(safe-area-inset-bottom, 20px); }
  .form-group { margin-bottom: 18px; }
  .form-group label { font-size: 15px; }
  .form-group input { font-size: 16px !important; padding: 14px 12px; border-radius: 8px; }
  .form-row { flex-direction: column; gap: 0; }
  .form-row .form-group.small { width: 100%; }
  .btn-primary { padding: 16px 24px; font-size: 17px; min-height: 52px; border-radius: 8px; }
  .btn-link { font-size: 15px; min-height: 44px; padding: 8px; }
}
@media (max-width: 375px) {
  .modal-header { padding: 14px 16px; }
  .modal-header h2 { font-size: 18px; }
  .auth-form { padding: 16px; }
  .form-group input { font-size: 16px !important; padding: 12px 10px; }
  .btn-primary { padding: 14px 20px; font-size: 16px; min-height: 50px; }
}
@media (prefers-reduced-motion: reduce) {
  .modal-enter-active, .modal-leave-active { transition: none !important; }
  .btn-primary, .btn-link, .form-group input { transition: none !important; }
}
</style>