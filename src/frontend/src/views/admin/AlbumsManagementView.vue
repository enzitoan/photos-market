<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <h1 class="text-3xl font-bold">Gestión de Álbumes</h1>
      <button @click="loadAlbums" class="btn btn-secondary">
        🔄 Actualizar
      </button>
    </div>
    
    <div class="card mb-6">
      <h2 class="text-lg font-semibold mb-4">ℹ️ Información</h2>
      <p class="text-gray-600 text-sm">
        Los álbumes bloqueados no serán visibles para los clientes. 
        Puedes bloquear álbumes temporalmente o desbloquearlos cuando lo necesites.
      </p>
    </div>
    
    <LoadingSpinner v-if="loading" message="Cargando álbumes..." />
    
    <div v-else-if="error" class="text-center py-12">
      <div class="max-w-md mx-auto">
        <div class="bg-red-50 border border-red-200 rounded-lg p-6 mb-4">
          <div class="flex items-start">
            <svg class="h-6 w-6 text-red-600 mr-3 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <div class="text-left">
              <h3 class="text-sm font-medium text-red-800 mb-2">Error al cargar álbumes</h3>
              <p class="text-sm text-red-700 mb-3">{{ error }}</p>
              
              <div v-if="needsGoogleAuth" class="space-y-2">
                <p class="text-sm text-red-600 font-semibold">
                  ⚠️ Acción requerida:
                </p>
                <router-link 
                  to="/admin/google-auth"
                  class="inline-flex items-center px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <svg class="w-5 h-5 mr-2" viewBox="0 0 24 24">
                    <path fill="currentColor" d="M12.545,10.239v3.821h5.445c-0.712,2.315-2.647,3.972-5.445,3.972c-3.332,0-6.033-2.701-6.033-6.032s2.701-6.032,6.033-6.032c1.498,0,2.866,0.549,3.921,1.453l2.814-2.814C17.503,2.988,15.139,2,12.545,2C7.021,2,2.543,6.477,2.543,12s4.478,10,10.002,10c8.396,0,10.249-7.85,9.426-11.748L12.545,10.239z"/>
                  </svg>
                  Conectar Google Photos
                </router-link>
              </div>
            </div>
          </div>
        </div>
        
        <button 
          v-if="!needsGoogleAuth"
          @click="loadAlbums" 
          class="btn btn-primary"
        >
          🔄 Reintentar
        </button>
      </div>
    </div>
    
    <div v-else>
      <!-- Filter Tabs -->
      <div class="flex gap-4 mb-6">
        <button 
          @click="filter = 'all'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'all' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Todos ({{ albums.length }})
        </button>
        <button 
          @click="filter = 'active'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'active' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Activos ({{ activeAlbums.length }})
        </button>
        <button 
          @click="filter = 'blocked'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'blocked' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Bloqueados ({{ blockedAlbums.length }})
        </button>
      </div>
      
      <!-- Albums Grid -->
      <div v-if="filteredAlbums.length === 0" class="text-center py-12 text-gray-500">
        No hay álbumes {{ filter === 'blocked' ? 'bloqueados' : filter === 'active' ? 'activos' : '' }}
      </div>
      
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div 
          v-for="album in filteredAlbums" 
          :key="album.id"
          class="card hover:shadow-lg transition-shadow"
        >
          <!-- Album Image -->
          <div class="relative mb-4">
            <img 
              v-if="album.coverPhotoUrl"
              :src="album.coverPhotoUrl"
              :alt="album.title"
              class="w-full h-48 object-cover rounded-lg"
              @error="handleImageError"
            >
            <div v-else class="w-full h-48 bg-gray-200 rounded-lg flex items-center justify-center text-gray-400">
              Sin imagen
            </div>
            
            <!-- Blocked Badge -->
            <div 
              v-if="album.isBlocked"
              class="absolute top-2 right-2 bg-red-500 text-white px-3 py-1 rounded-full text-xs font-medium"
            >
              🚫 Bloqueado
            </div>
          </div>
          
          <!-- Album Info -->
          <h3 class="font-semibold text-lg mb-2">{{ album.title || 'Sin título' }}</h3>
          <p class="text-sm text-gray-600 mb-4">
            {{ album.mediaItemsCount || 0 }} fotos
          </p>
          
          <!-- Actions -->
          <div class="flex gap-2">
            <button 
              v-if="album.isBlocked"
              @click="unblockAlbum(album.googleAlbumId)"
              class="flex-1 btn btn-success"
            >
              ✓ Desbloquear
            </button>
            <button 
              v-else
              @click="blockAlbum(album.googleAlbumId)"
              class="flex-1 btn btn-danger"
            >
              🚫 Bloquear
            </button>
            
            <button 
              @click="viewAlbumDetails(album)"
              class="btn btn-secondary"
              title="Ver detalles"
            >
              👁️
            </button>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Album Details Modal -->
    <div 
      v-if="selectedAlbum"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click="selectedAlbum = null"
    >
      <div 
        class="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto"
        @click.stop
      >
        <div class="p-6">
          <div class="flex items-start justify-between mb-4">
            <h2 class="text-2xl font-bold">{{ selectedAlbum.title }}</h2>
            <button @click="selectedAlbum = null" class="text-gray-500 hover:text-gray-700 text-2xl">
              ×
            </button>
          </div>
          
          <div class="space-y-3 text-sm">
            <div>
              <span class="font-medium text-gray-700">ID:</span>
              <p class="text-gray-600 font-mono text-xs">{{ selectedAlbum.googleAlbumId }}</p>
            </div>
            
            <div>
              <span class="font-medium text-gray-700">Total de fotos:</span>
              <p class="text-gray-600">{{ selectedAlbum.mediaItemsCount || 0 }}</p>
            </div>
            
            <div>
              <span class="font-medium text-gray-700">Estado:</span>
              <p class="text-gray-600">
                {{ selectedAlbum.isBlocked ? '🚫 Bloqueado' : '✓ Activo' }}
              </p>
            </div>
            
            <div v-if="selectedAlbum.productUrl">
              <span class="font-medium text-gray-700">URL de Google Photos:</span>
              <a 
                :href="selectedAlbum.productUrl" 
                target="_blank"
                class="text-primary-600 hover:text-primary-700 text-xs break-all"
              >
                {{ selectedAlbum.productUrl }}
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import { useAdminStore } from '@/stores/admin'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import adminService from '@/services/adminService'

const toast = useToast()
const adminStore = useAdminStore()

const loading = ref(true)
const error = ref(null)
const needsGoogleAuth = ref(false)
const albums = ref([])
const filter = ref('all')
const selectedAlbum = ref(null)

const activeAlbums = computed(() => 
  albums.value.filter(album => !album.isBlocked)
)

const blockedAlbums = computed(() => 
  albums.value.filter(album => album.isBlocked)
)

const filteredAlbums = computed(() => {
  if (filter.value === 'active') return activeAlbums.value
  if (filter.value === 'blocked') return blockedAlbums.value
  return albums.value
})

async function loadAlbums() {
  try {
    loading.value = true
    error.value = null
    needsGoogleAuth.value = false
    
    const response = await adminService.getAllAlbums()
    
    if (response.success && response.data) {
      albums.value = response.data
      
      // Actualizar el store de admin con los álbumes bloqueados
      response.data.forEach(album => {
        if (album.isBlocked) {
          adminStore.blockAlbum(album.googleAlbumId)
        }
      })
    }
  } catch (err) {
    console.error('Error loading albums:', err)
    
    // Verificar si el error es por falta de autenticación de Google
    const errorMessage = err.response?.data?.message || err.message || ''
    const errorDetails = err.response?.data?.errors?.[0] || ''
    
    if (errorDetails === 'GOOGLE_AUTH_REQUIRED' ||
        errorMessage.includes('fotógrafo no ha autenticado') || 
        errorMessage.includes('Google Photos') ||
        errorMessage.includes('cuenta de Google')) {
      needsGoogleAuth.value = true
      error.value = 'Debe conectar su cuenta de Google Photos para ver los álbumes.'
    } else {
      error.value = errorMessage || 'Error al cargar los álbumes. Intenta nuevamente.'
    }
    
    toast.error(error.value)
  } finally {
    loading.value = false
  }
}

async function blockAlbum(googleAlbumId) {
  try {
    const response = await adminService.blockAlbum(googleAlbumId)
    
    if (response.success) {
      adminStore.blockAlbum(googleAlbumId)
      
      // Actualizar el álbum en la lista local
      const album = albums.value.find(a => a.googleAlbumId === googleAlbumId)
      if (album) {
        album.isBlocked = true
      }
      
      toast.success('Álbum bloqueado exitosamente')
    }
  } catch (err) {
    console.error('Error blocking album:', err)
    toast.error('Error al bloquear el álbum')
  }
}

async function unblockAlbum(googleAlbumId) {
  try {
    const response = await adminService.unblockAlbum(googleAlbumId)
    
    if (response.success) {
      adminStore.unblockAlbum(googleAlbumId)
      
      // Actualizar el álbum en la lista local
      const album = albums.value.find(a => a.googleAlbumId === googleAlbumId)
      if (album) {
        album.isBlocked = false
      }
      
      toast.success('Álbum desbloqueado exitosamente')
    }
  } catch (err) {
    console.error('Error unblocking album:', err)
    toast.error('Error al desbloquear el álbum')
  }
}

function viewAlbumDetails(album) {
  selectedAlbum.value = album
}

function handleImageError(event) {
  console.warn('Error loading album cover image')
  // Hide the image on error
  event.target.style.display = 'none'
}

onMounted(() => {
  loadAlbums()
})
</script>
