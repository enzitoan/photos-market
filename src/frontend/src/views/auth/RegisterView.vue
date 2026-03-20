<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full">
      <div class="bg-white rounded-2xl shadow-xl p-8">
        <!-- Header -->
        <div class="text-center mb-8">
          <h2 class="text-3xl font-bold text-gray-900 mb-2">Completa tu Registro</h2>
          <p class="text-gray-600">Solo necesitamos algunos datos más</p>
          <div class="mt-4 flex items-center justify-center space-x-2 text-sm text-gray-500">
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clip-rule="evenodd"/>
            </svg>
            <span>{{ userEmail }}</span>
          </div>
        </div>

        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Teléfono -->
          <div>
            <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">
              Teléfono
            </label>
            <input
              id="phone"
              v-model="formData.phone"
              type="tel"
              required
              placeholder="+56 9 1234 5678"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
              :class="{ 'border-red-500': errors.phone }"
            />
            <p v-if="errors.phone" class="mt-1 text-sm text-red-600">{{ errors.phone }}</p>
          </div>

          <!-- Tipo de Identificación -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Tipo de Identificación
            </label>
            <div class="flex gap-4">
              <label class="flex items-center cursor-pointer">
                <input
                  type="radio"
                  v-model="formData.idType"
                  value="RUT"
                  class="w-4 h-4 text-indigo-600 border-gray-300 focus:ring-indigo-500"
                />
                <span class="ml-2 text-sm text-gray-700">RUT (Chile)</span>
              </label>
              <label class="flex items-center cursor-pointer">
                <input
                  type="radio"
                  v-model="formData.idType"
                  value="DNI"
                  class="w-4 h-4 text-indigo-600 border-gray-300 focus:ring-indigo-500"
                />
                <span class="ml-2 text-sm text-gray-700">Otro DNI</span>
              </label>
            </div>
          </div>

          <!-- RUT / DNI -->
          <div>
            <label for="idNumber" class="block text-sm font-medium text-gray-700 mb-1">
              {{ formData.idType === 'RUT' ? 'RUT' : 'Número de Identificación' }}
            </label>
            <input
              id="idNumber"
              v-model="formData.idNumber"
              type="text"
              required
              :placeholder="formData.idType === 'RUT' ? '12345678-9' : 'Ej: 12345678'"
              :maxlength="formData.idType === 'RUT' ? 12 : 20"
              @input="handleIdInput"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
              :class="{ 'border-red-500': errors.idNumber }"
            />
            <p v-if="errors.idNumber" class="mt-1 text-sm text-red-600">{{ errors.idNumber }}</p>
            <p v-if="formData.idType === 'RUT'" class="mt-1 text-xs text-gray-500">
              Formato: 12345678-9 (con validación de dígito verificador)
            </p>
          </div>

          <!-- Fecha de Nacimiento -->
          <div>
            <label for="birthDate" class="block text-sm font-medium text-gray-700 mb-1">
              Fecha de Nacimiento
            </label>
            <input
              id="birthDate"
              v-model="formData.birthDate"
              type="date"
              required
              :max="maxBirthDate"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
              :class="{ 'border-red-500': errors.birthDate }"
            />
            <p v-if="errors.birthDate" class="mt-1 text-sm text-red-600">{{ errors.birthDate }}</p>
            <p class="mt-1 text-xs text-gray-500">Debes ser mayor de 18 años</p>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-semibold hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="!loading">Completar Registro</span>
            <span v-else class="flex items-center justify-center">
              <svg class="animate-spin h-5 w-5 mr-2" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/>
              </svg>
              Procesando...
            </span>
          </button>
        </form>

        <!-- Privacy Note -->
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
  // Verificar que haya un tempToken
  const tempToken = localStorage.getItem('tempToken')
  if (!tempToken) {
    toast.error('Sesión expirada. Por favor inicia sesión nuevamente.')
    router.push('/login')
  }
})

function handleIdInput() {
  // Solo formatear si es RUT
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
  } else {
    return validateDni()
  }
}

function validateRut() {
  // Limpiar el RUT de puntos y guiones para contar caracteres
  const cleanRut = formData.value.idNumber.replace(/[^0-9kK]/g, '')
  
  if (cleanRut.length < 8) {
    errors.value.idNumber = 'El RUT debe tener al menos 8 caracteres'
    return false
  }
  
  if (cleanRut.length > 9) {
    errors.value.idNumber = 'El RUT no puede tener más de 9 caracteres'
    return false
  }
  
  // Validación de formato - acepta con o sin puntos
  // Formatos válidos: 12345678-9, 12.345.678-9, 1.234.567-K, etc.
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
  // Reset errors
  errors.value = { phone: '', idNumber: '', birthDate: '' }
  
  // Validate all fields
  const phoneValid = validatePhone()
  const idNumberValid = validateIdNumber()
  const birthDateValid = validateBirthDate()
  
  if (!phoneValid || !idNumberValid || !birthDateValid) {
    toast.error('Por favor corrige los errores en el formulario')
    return
  }
  
  loading.value = true
  
  try {
    // Limpiar el RUT/DNI según el tipo
    let cleanIdNumber = formData.value.idNumber
    
    if (formData.value.idType === 'RUT') {
      // Limpiar puntos del RUT
      cleanIdNumber = formData.value.idNumber.replace(/\./g, '')
    }
    
    const success = await authStore.completeRegistration(
      formData.value.phone,
      formData.value.idType,
      cleanIdNumber,
      formData.value.birthDate
    )
    
    if (success) {
      toast.success('¡Registro completado exitosamente!')
      router.push('/albums')
    } else {
      toast.error('Error al completar el registro')
    }
  } catch (error) {
    console.error('Registration error:', error)
    const errorMessage = error.response?.data?.message || error.message || 'Error al completar el registro'
    toast.error(errorMessage)
    
    // Mostrar errores específicos si los hay
    if (error.response?.data?.errors) {
      error.response.data.errors.forEach(err => {
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
