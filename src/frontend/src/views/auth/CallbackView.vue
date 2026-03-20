<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="text-center">
      <div class="spinner mx-auto"></div>
      <p class="mt-4 text-gray-600">{{ message }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToast } from 'vue-toastification'

const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()
const message = ref('Completando autenticación...')

onMounted(async () => {
  const urlParams = new URLSearchParams(window.location.search)
  const code = urlParams.get('code')
  
  // Verificar si es una autenticación de fotógrafo
  const isPhotographerAuth = sessionStorage.getItem('photographerAuth') === 'true'
  
  if (code) {
    try {
      let success = false
      
      if (isPhotographerAuth) {
        message.value = 'Conectando Google Photos del fotógrafo...'
        success = await authStore.photographerLogin(code)
        
        // Limpiar la marca
        sessionStorage.removeItem('photographerAuth')
        
        if (success) {
          toast.success('¡Google Photos conectado exitosamente!')
          router.push('/admin/settings')
        } else {
          toast.error('Error al conectar Google Photos')
          router.push('/admin/google-auth')
        }
      } else {
        message.value = 'Completando autenticación...'
        success = await authStore.login(code)
        
        if (success && success.needsRegistration) {
          toast.info('Por favor completa tu registro')
          router.push('/register')
        } else if (success) {
          toast.success('¡Bienvenido!')
          router.push('/albums')
        } else {
          toast.error('Error al autenticar')
          router.push('/login')
        }
      }
    } catch (error) {
      console.error('Authentication error:', error)
      
      // Limpiar la marca en caso de error
      sessionStorage.removeItem('photographerAuth')
      
      const errorMessage = error.response?.data?.message || error.message || 'Error al autenticar'
      toast.error(errorMessage)
      
      if (isPhotographerAuth) {
        router.push('/admin/google-auth')
      } else {
        router.push('/login')
      }
    }
  } else {
    toast.error('Código de autorización no encontrado')
    
    // Limpiar la marca
    sessionStorage.removeItem('photographerAuth')
    
    if (isPhotographerAuth) {
      router.push('/admin/google-auth')
    } else {
      router.push('/login')
    }
  }
})
</script>
