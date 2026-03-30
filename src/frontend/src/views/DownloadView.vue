<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1 max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8 w-full">
      <LoadingSpinner v-if="loading" message="Verificando enlace de descarga..." />
      
      <div v-else-if="error" class="text-center py-12">
        <div class="text-6xl mb-4">⚠️</div>
        <h2 class="text-2xl font-bold text-gray-900 mb-2">Enlace Inválido o Expirado</h2>
        <p class="text-gray-600 mb-6">{{ error }}</p>
        <router-link to="/orders" class="btn btn-primary">
          Ver Mis Pedidos
        </router-link>
      </div>
      
      <div v-else-if="downloadInfo" class="space-y-6">
        <!-- Download Header -->
        <div class="card">
          <div class="flex items-start justify-between mb-4">
            <div>
              <h1 class="text-3xl font-bold mb-2">Descarga de Fotos</h1>
              <p class="text-gray-600">Pedido #{{ downloadInfo.orderNumber }}</p>
            </div>
            
            <span 
              class="px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800"
            >
              Pagado
            </span>
          </div>
          
          <div class="grid md:grid-cols-2 gap-4 text-sm">
            <div>
              <span class="text-gray-600">Cliente:</span>
              <p class="font-medium">{{ downloadInfo.customerName }}</p>
            </div>
            
            <div>
              <span class="text-gray-600">Email:</span>
              <p class="font-medium">{{ downloadInfo.customerEmail }}</p>
            </div>
            
            <div>
              <span class="text-gray-600">Fotos:</span>
              <p class="font-medium">{{ downloadInfo.photos?.length || downloadInfo.photoIds?.length || 0 }}</p>
            </div>
            
            <div>
              <span class="text-gray-600">Expira:</span>
              <p class="font-medium">{{ formatDate(downloadInfo.expiresAt) }}</p>
            </div>
          </div>
        </div>
        
        <!-- Individual Downloads -->
        <div class="card">
          <h2 class="text-xl font-semibold mb-4">Descargar Fotos</h2>
          
          <!-- Vista con miniaturas (si están disponibles) -->
          <div v-if="downloadInfo.photos && downloadInfo.photos.length > 0" class="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div 
              v-for="(photo, index) in downloadInfo.photos" 
              :key="photo.photoId"
              class="border rounded-lg overflow-hidden hover:border-primary-500 transition-colors group relative"
            >
              <div class="aspect-square bg-gray-100 relative">
                <img 
                  :src="photo.thumbnailUrl" 
                  :alt="photo.filename"
                  class="w-full h-full object-cover"
                  loading="lazy"
                />
                <!-- Overlay on hover -->
                <div class="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-20 transition-all flex items-center justify-center">
                  <button 
                    @click="downloadSingle(photo.photoId)"
                    class="opacity-0 group-hover:opacity-100 transition-opacity bg-primary-600 hover:bg-primary-700 text-white px-4 py-2 rounded-lg font-medium flex items-center gap-2"
                    title="Descargar foto"
                  >
                    ⬇️ Descargar
                  </button>
                </div>
              </div>
              <div class="p-3">
                <p class="font-medium text-sm truncate">{{ photo.filename || `Foto ${index + 1}` }}</p>
                <p class="text-xs text-gray-500">ID: {{ photo.photoId.substring(0, 8) }}</p>
              </div>
            </div>
          </div>
          
          <!-- Vista de lista (fallback si no hay miniaturas) -->
          <div v-else-if="downloadInfo.photoIds && downloadInfo.photoIds.length > 0" class="grid md:grid-cols-2 gap-4">
            <div 
              v-for="(photo, index) in downloadInfo.photoIds" 
              :key="photo"
              class="border rounded-lg p-4 hover:border-primary-500 transition-colors"
            >
              <div class="flex items-center justify-between">
                <div>
                  <p class="font-medium">Foto {{ index + 1 }}</p>
                  <p class="text-sm text-gray-500">{{ photo }}</p>
                </div>
                
                <button 
                  @click="downloadSingle(photo)"
                  class="text-primary-600 hover:text-primary-700"
                  title="Descargar"
                >
                  ⬇️
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Important Notes -->
        <div class="card bg-blue-50 border-blue-200">
          <h3 class="font-semibold text-blue-900 mb-2">ℹ️ Información Importante</h3>
          <ul class="text-sm text-blue-800 space-y-1">
            <li>• Las fotos se descargarán en máxima resolución disponible</li>
            <li>• Haz clic en el botón ⬇️ de cada foto para descargarla</li>
            <li>• Este enlace expira el {{ formatDate(downloadInfo.expiresAt) }}</li>
            <li>• Guarda las fotos en un lugar seguro</li>
            <li>• Si tienes problemas con la descarga, contacta al fotógrafo</li>
          </ul>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useToast } from 'vue-toastification'
import NavBar from '@/components/NavBar.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import downloadService from '@/services/downloadService'

const route = useRoute()
const toast = useToast()

const loading = ref(true)
const error = ref(null)
const downloadInfo = ref(null)

const token = route.params.token

async function loadDownloadInfo() {
  try {
    loading.value = true
    error.value = null
    downloadInfo.value = await downloadService.validateToken(token)
  } catch (err) {
    console.error('Error validating download token:', err)
    error.value = err.response?.data?.message || 'El enlace de descarga no es válido o ha expirado'
  } finally {
    loading.value = false
  }
}

async function downloadSingle(photoId) {
  try {
    toast.info('Iniciando descarga...')
    
    // Get the Google Drive URL from the backend
    const photoUrl = await downloadService.downloadSinglePhoto(token, photoId)
    
    if (!photoUrl || typeof photoUrl !== 'string') {
      throw new Error('URL de foto inválida')
    }
    
    // Open the URL in a new window for download
    window.open(photoUrl, '_blank')
    
    toast.success('Descarga iniciada')
  } catch (err) {
    console.error('Download error:', err)
    const errorMessage = err.response?.data?.message || err.message || 'Error al descargar la foto'
    toast.error(errorMessage)
  }
}

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => {
  loadDownloadInfo()
})
</script>
