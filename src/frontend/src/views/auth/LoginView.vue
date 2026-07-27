<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div class="text-center">
        <h2 class="text-4xl font-bold text-gray-900">📸 PhotosMarket</h2>
        <p class="mt-2 text-sm text-gray-600">
          Inicia sesión con Google o crea tu cuenta manualmente
        </p>
      </div>

      <div class="card space-y-4">
        <button
          @click="handleGoogleLogin"
          :disabled="loading"
          class="w-full flex items-center justify-center px-4 py-3 border border-transparent text-base font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <svg class="w-5 h-5 mr-2" viewBox="0 0 24 24">
            <path fill="currentColor" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
            <path fill="currentColor" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
            <path fill="currentColor" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
            <path fill="currentColor" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
          </svg>
          {{ loading ? 'Conectando...' : 'Continuar con Google' }}
        </button>

        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-2 bg-gray-50 text-gray-500">o</span>
          </div>
        </div>

        <form @submit.prevent="handleManualLogin" class="space-y-4">
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700">Correo electrónico</label>
            <input id="email" v-model="manualForm.email" type="email" required class="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500" />
          </div>
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700">Contraseña</label>
            <input id="password" v-model="manualForm.password" type="password" required class="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500" />
          </div>
          <button type="submit" :disabled="loading" class="w-full flex justify-center px-4 py-3 border border-gray-300 text-base font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed">
            {{ loading ? 'Ingresando...' : 'Iniciar sesión' }}
          </button>
        </form>

        <div class="text-center text-sm text-gray-500">
          ¿No tienes cuenta? <router-link to="/register" class="text-primary-600 font-medium">Crear una cuenta</router-link>
        </div>

        <div v-if="error" class="mt-4 text-sm text-red-600 text-center">
          {{ error }}
        </div>
      </div>

      <div class="text-center text-sm text-gray-500">
        Al iniciar sesión, aceptas nuestros términos de servicio y política de privacidad
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import authService from '@/services/authService'

const router = useRouter()
const authStore = useAuthStore()
const loading = ref(false)
const error = ref(null)
const manualForm = ref({ email: '', password: '' })

async function handleGoogleLogin() {
  try {
    loading.value = true
    error.value = null

    const response = await authService.getGoogleLoginUrl()

    if (response.authUrl) {
      window.location.href = response.authUrl
    } else {
      error.value = 'No se pudo obtener la URL de autenticación'
    }
  } catch (err) {
    console.error('Login error:', err)
    error.value = 'Error al iniciar sesión. Intenta nuevamente.'
  } finally {
    loading.value = false
  }
}

async function handleManualLogin() {
  try {
    loading.value = true
    error.value = null

    const ok = await authStore.loginManual({
      email: manualForm.value.email,
      password: manualForm.value.password
    })

    if (ok) {
      router.push('/')
    } else {
      error.value = 'Credenciales inválidas'
    }
  } catch (err) {
    console.error('Manual login error:', err)
    error.value = err?.response?.data?.message || 'No se pudo iniciar sesión'
  } finally {
    loading.value = false
  }
}
</script>
