<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full">
      <div class="bg-white rounded-2xl shadow-xl p-8">
        <div class="text-center mb-8">
          <h2 class="text-3xl font-bold text-gray-900 mb-2">
            {{ hasTempSession ? 'Completa tu Registro' : 'Crea tu Cuenta' }}
          </h2>
          <p class="text-gray-600">
            {{ hasTempSession ? 'Solo necesitamos algunos datos más' : 'Regístrate manualmente y empieza a comprar fotos' }}
          </p>
          <div v-if="hasTempSession" class="mt-4 flex items-center justify-center space-x-2 text-sm text-gray-500">
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clip-rule="evenodd"/>
            </svg>
            <span>{{ userEmail }}</span>
          </div>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-6">
          <div v-if="!hasTempSession" class="space-y-4">
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700 mb-1">Nombre completo</label>
              <input id="name" v-model="formData.name" type="text" required class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" />
            </div>

            <div>
              <label for="email" class="block text-sm font-medium text-gray-700 mb-1">Correo electrónico</label>
              <input id="email" v-model="formData.email" type="email" required class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" />
            </div>

            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
              <input id="password" v-model="formData.password" type="password" required minlength="8" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" />
            </div>

            <div>
              <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">Confirmar contraseña</label>
              <input id="confirmPassword" v-model="formData.confirmPassword" type="password" required minlength="8" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" />
            </div>
          </div>

          <div>
            <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
            <input id="phone" v-model="formData.phone" type="tel" required placeholder="+56 9 1234 5678" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" :class="{ 'border-red-500': errors.phone }" />
            <p v-if="errors.phone" class="mt-1 text-sm text-red-600">{{ errors.phone }}</p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Tipo de Identificación</label>
            <div class="flex gap-4">
              <label class="flex items-center cursor-pointer">
                <input type="radio" v-model="formData.idType" value="RUT" class="w-4 h-4 text-indigo-600 border-gray-300 focus:ring-indigo-500" />
                <span class="ml-2 text-sm text-gray-700">RUT (Chile)</span>
              </label>
              <label class="flex items-center cursor-pointer">
                <input type="radio" v-model="formData.idType" value="DNI" class="w-4 h-4 text-indigo-600 border-gray-300 focus:ring-indigo-500" />
                <span class="ml-2 text-sm text-gray-700">Otro DNI</span>
              </label>
            </div>
          </div>

          <div>
            <label for="idNumber" class="block text-sm font-medium text-gray-700 mb-1">
              {{ formData.idType === 'RUT' ? 'RUT' : 'Número de Identificación' }}
            </label>
            <input id="idNumber" v-model="formData.idNumber" type="text" required :placeholder="formData.idType === 'RUT' ? '12345678-9' : 'Ej: 12345678'" :maxlength="formData.idType === 'RUT' ? 12 : 20" @input="handleIdInput" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" :class="{ 'border-red-500': errors.idNumber }" />
            <p v-if="errors.idNumber" class="mt-1 text-sm text-red-600">{{ errors.idNumber }}</p>
            <p v-if="formData.idType === 'RUT'" class="mt-1 text-xs text-gray-500">Formato: 12345678-9 (con validación de dígito verificador)</p>
          </div>

          <div>
            <label for="birthDate" class="block text-sm font-medium text-gray-700 mb-1">Fecha de Nacimiento</label>
            <input id="birthDate" v-model="formData.birthDate" type="date" required :max="maxBirthDate" class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition" :class="{ 'border-red-500': errors.birthDate }" />
            <p v-if="errors.birthDate" class="mt-1 text-sm text-red-600">{{ errors.birthDate }}</p>
            <p class="mt-1 text-xs text-gray-500">Debes ser mayor de 18 años</p>
          </div>

          <button type="submit" :disabled="loading" class="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-semibold hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition disabled:opacity-50 disabled:cursor-not-allowed">
            <span v-if="!loading">{{ hasTempSession ? 'Completar Registro' : 'Crear Cuenta' }}</span>
            <span v-else class="flex items-center justify-center">
              <svg class="animate-spin h-5 w-5 mr-2" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/>
              </svg>
              Procesando...
            </span>
          </button>
        </form>

        <div v-if="!hasTempSession" class="mt-6 text-center text-sm text-gray-500">
          ¿Ya tienes cuenta? <router-link to="/login" class="text-primary-600 font-medium">Inicia sesión</router-link>
        </div>

        <div class="mt-6 text-center text-xs text-gray-500">
          <p>🔒 Tus datos están protegidos y solo se usarán para gestionar tus pedidos</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToast } from 'vue-toastification'

const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()

const formData = ref({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: '',
  idType: 'RUT',
  idNumber: '',
  birthDate: ''
})

const errors = ref({
  phone: '',
  idNumber: '',
  birthDate: ''
})

const loading = ref(false)
const hasTempSession = computed(() => Boolean(localStorage.getItem('tempToken')))

const userEmail = computed(() => {
  const tempUser = localStorage.getItem('tempUser')
  if (tempUser) {
    return JSON.parse(tempUser).email
  }
  return ''
})

const maxBirthDate = computed(() => {
  const date = new Date()
  date.setFullYear(date.getFullYear() - 18)
  return date.toISOString().split('T')[0]
})

onMounted(() => {
  if (!hasTempSession.value) {
    return
  }
})

function handleIdInput() {
  if (formData.value.idType === 'RUT') {
    formatRut()
  }
}

function formatRut() {
  let rut = formData.value.idNumber.replace(/[^0-9kK]/g, '')

  if (rut.length > 1) {
    const body = rut.slice(0, -1)
    const dv = rut.slice(-1).toUpperCase()
    formData.value.idNumber = body.replace(/\B(?=(\d{3})+(?!\d))/g, '.') + '-' + dv
  } else {
    formData.value.idNumber = rut
  }
}

function validatePhone() {
  const phone = formData.value.phone.replace(/[^0-9+]/g, '')

  if (phone.length < 9) {
    errors.value.phone = 'El teléfono debe tener al menos 9 dígitos'
    return false
  }

  errors.value.phone = ''
  return true
}

function validateIdNumber() {
  if (formData.value.idType === 'RUT') {
    return validateRut()
  }
  return validateDni()
}

function validateRut() {
  const cleanRut = formData.value.idNumber.replace(/[^0-9kK]/g, '')

  if (cleanRut.length < 8) {
    errors.value.idNumber = 'El RUT debe tener al menos 8 caracteres'
    return false
  }

  if (cleanRut.length > 9) {
    errors.value.idNumber = 'El RUT no puede tener más de 9 caracteres'
    return false
  }

  const rutPattern = /^[\d.]+\-[0-9kK]$/
  if (!rutPattern.test(formData.value.idNumber)) {
    errors.value.idNumber = 'Formato de RUT inválido. Debe terminar en -X (ej: 12.345.678-9)'
    return false
  }

  errors.value.idNumber = ''
  return true
}

function validateDni() {
  const dni = formData.value.idNumber.trim()

  if (dni.length < 6) {
    errors.value.idNumber = 'El número de identificación debe tener al menos 6 caracteres'
    return false
  }

  errors.value.idNumber = ''
  return true
}

function validateBirthDate() {
  if (!formData.value.birthDate) {
    errors.value.birthDate = 'La fecha de nacimiento es requerida'
    return false
  }

  const birthDate = new Date(formData.value.birthDate)
  const today = new Date()
  let age = today.getFullYear() - birthDate.getFullYear()
  const monthDiff = today.getMonth() - birthDate.getMonth()

  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
    age--
  }

  if (age < 18) {
    errors.value.birthDate = 'Debes ser mayor de 18 años'
    return false
  }

  errors.value.birthDate = ''
  return true
}

async function handleSubmit() {
  errors.value = { phone: '', idNumber: '', birthDate: '' }

  const phoneValid = validatePhone()
  const idNumberValid = validateIdNumber()
  const birthDateValid = validateBirthDate()

  if (!phoneValid || !idNumberValid || !birthDateValid) {
    toast.error('Por favor corrige los errores en el formulario')
    return
  }

  if (!hasTempSession.value) {
    if (!formData.value.name.trim()) {
      toast.error('El nombre es requerido')
      return
    }

    if (!formData.value.email.trim()) {
      toast.error('El correo electrónico es requerido')
      return
    }

    if (!formData.value.password || formData.value.password.length < 8) {
      toast.error('La contraseña debe tener al menos 8 caracteres')
      return
    }

    if (formData.value.password !== formData.value.confirmPassword) {
      toast.error('Las contraseñas no coinciden')
      return
    }
  }

  loading.value = true

  try {
    let cleanIdNumber = formData.value.idNumber
    if (formData.value.idType === 'RUT') {
      cleanIdNumber = formData.value.idNumber.replace(/\./g, '')
    }

    let success = false
    if (hasTempSession.value) {
      success = await authStore.completeRegistration(
        formData.value.phone,
        formData.value.idType,
        cleanIdNumber,
        formData.value.birthDate
      )
    } else {
      success = await authStore.registerManual({
        name: formData.value.name,
        email: formData.value.email,
        password: formData.value.password,
        confirmPassword: formData.value.confirmPassword,
        phone: formData.value.phone,
        idType: formData.value.idType,
        idNumber: cleanIdNumber,
        birthDate: formData.value.birthDate
      })
    }

    if (success) {
      toast.success(hasTempSession.value ? '¡Registro completado exitosamente!' : '¡Cuenta creada exitosamente!')
      router.push('/albums')
    } else {
      toast.error(hasTempSession.value ? 'Error al completar el registro' : 'Error al crear la cuenta')
    }
  } catch (error) {
    console.error('Registration error:', error)
    const errorMessage = error.response?.data?.message || error.message || (hasTempSession.value ? 'Error al completar el registro' : 'Error al crear la cuenta')
    toast.error(errorMessage)

    if (error.response?.data?.errors) {
      error.response.data.errors.forEach((err) => {
        if (err.includes('RUT') || err.includes('DNI') || err.includes('identificación')) {
          errors.value.idNumber = err
        } else if (err.includes('teléfono') || err.includes('phone')) {
          errors.value.phone = err
        } else if (err.includes('fecha') || err.includes('edad')) {
          errors.value.birthDate = err
        }
      })
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
