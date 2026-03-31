<template>
  <div class="min-h-screen flex flex-col" @contextmenu.prevent="handleRightClick">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8 w-full">
      <!-- Album Header -->
      <div class="mb-6 sm:mb-8">
        <button @click="$router.back()" class="text-primary-600 hover:text-primary-700 mb-3 sm:mb-4 flex items-center text-sm sm:text-base">
          <span class="mr-2">←</span> Volver a álbumes
        </button>
        
        <div v-if="album">
          <h1 class="text-2xl sm:text-3xl font-bold mb-2">{{ album.title }}</h1>
          <p class="text-sm sm:text-base text-gray-600">{{ photos.length }} fotos disponibles</p>
        </div>
      </div>
      
      <LoadingSpinner v-if="loading" message="Cargando fotos..." />
      
      <div v-else-if="error" class="text-center py-12">
        <p class="text-red-600">{{ error }}</p>
        <button @click="loadPhotos" class="btn btn-primary mt-4">
          Reintentar
        </button>
      </div>
      
      <div v-else-if="photos.length === 0" class="text-center py-12">
        <p class="text-gray-600 text-lg">Este álbum no contiene fotos</p>
      </div>
      
      <div v-else>
        <!-- Filters/Actions -->
        <div class="mb-4 sm:mb-6 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0">
          <div>
            <label class="inline-flex items-center cursor-pointer">
              <input 
                type="checkbox" 
                v-model="showOnlyInCart"
                class="form-checkbox h-4 w-4 sm:h-5 sm:w-5 text-primary-600"
              >
              <span class="ml-2 text-gray-700 text-sm sm:text-base">Solo en carrito</span>
            </label>
          </div>
          
          <div class="text-xs sm:text-sm text-gray-600">
            🛒 {{ cartStore.items.length }} {{ cartStore.items.length === 1 ? 'foto' : 'fotos' }} en el carrito
          </div>
        </div>
        
        <!-- Photos Grid -->
        <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-3 sm:gap-4 md:gap-6">
          <PhotoCard 
            v-for="photo in filteredPhotos" 
            :key="photo.id"
            :photo="photo"
            @view-details="openPhotoModal"
          />
        </div>
      </div>
    </main>

    <!-- Photo Modal -->
    <PhotoModal
      v-if="selectedPhoto"
      :show="showPhotoModal"
      :photo="selectedPhoto"
      :photos="filteredPhotos"
      :currentIndex="selectedPhotoIndex"
      @close="closePhotoModal"
      @navigate="navigateToPhoto"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRoute } from 'vue-router'
import { useToast } from 'vue-toastification'
import { useCartStore } from '@/stores/cart'
import NavBar from '@/components/NavBar.vue'
import PhotoCard from '@/components/PhotoCard.vue'
import PhotoModal from '@/components/PhotoModal.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import photosService from '@/services/photosService'

const route = useRoute()
const toast = useToast()
const cartStore = useCartStore()

const loading = ref(true)
const error = ref(null)
const album = ref(null)
const photos = ref([])
const showOnlyInCart = ref(false)
const showPhotoModal = ref(false)
const selectedPhoto = ref(null)
const selectedPhotoIndex = ref(-1)

const albumId = computed(() => route.params.id)

const filteredPhotos = computed(() => {
  if (!showOnlyInCart.value) return photos.value
  
  const cartPhotoIds = new Set(cartStore.items.map(item => item.id))
  return photos.value.filter(photo => cartPhotoIds.has(photo.id))
})

async function loadPhotos() {
  try {
    loading.value = true
    error.value = null
    
    // Obtener información del álbum
    const albumResponse = await photosService.getAlbum(albumId.value)
    album.value = albumResponse?.data || {
      id: albumId.value,
      title: `Álbum ${albumId.value}`
    }
    
    // Obtener fotos del álbum
    const response = await photosService.getAlbumPhotos(albumId.value)
    
    // El backend devuelve { success: true, data: [photos] }
    const photosData = response?.data || []
    
    // Filtrar fotos válidas
    photos.value = photosData.filter(photo => photo && photo.id)
  } catch (err) {
    console.error('Error loading photos:', err)
    error.value = 'Error al cargar las fotos. Intenta nuevamente.'
    toast.error('Error al cargar las fotos')
  } finally {
    loading.value = false
  }
}

function openPhotoModal(photo) {
  selectedPhoto.value = photo
  // Buscar índice en el array filtrado
  selectedPhotoIndex.value = filteredPhotos.value.findIndex(p => p.id === photo.id)
  showPhotoModal.value = true
}

function closePhotoModal() {
  showPhotoModal.value = false
  selectedPhoto.value = null
  selectedPhotoIndex.value = -1
}

function navigateToPhoto(newIndex) {
  if (newIndex >= 0 && newIndex < filteredPhotos.value.length) {
    selectedPhotoIndex.value = newIndex
    selectedPhoto.value = filteredPhotos.value[newIndex]
  }
}

// Security: Prevent right-click context menu
function handleRightClick() {
  toast.warning('Clic derecho deshabilitado para proteger las imágenes')
}

// Security: Prevent screenshot keyboard shortcuts and printing
function handleKeyDown(event) {
  // PrintScreen key
  if (event.key === 'PrintScreen') {
    event.preventDefault()
    toast.warning('Las capturas de pantalla están deshabilitadas en esta página')
    return
  }
  
  // Windows: Win + Shift + S (Snipping Tool)
  // Mac: Cmd + Shift + 3/4/5 (Screenshot shortcuts)
  if ((event.metaKey || event.ctrlKey) && event.shiftKey) {
    if (['s', '3', '4', '5'].includes(event.key.toLowerCase())) {
      event.preventDefault()
      toast.warning('Las capturas de pantalla están deshabilitadas en esta página')
      return
    }
  }
  
  // Windows: Alt + PrintScreen
  if (event.altKey && event.key === 'PrintScreen') {
    event.preventDefault()
    toast.warning('Las capturas de pantalla están deshabilitadas en esta página')
    return
  }
  
  // Prevent printing: Ctrl+P, Cmd+P
  if ((event.ctrlKey || event.metaKey) && event.key === 'p') {
    event.preventDefault()
    toast.warning('La impresión está deshabilitada en esta página')
  }
}

onMounted(async () => {
  // Cargar precio desde backend
  await cartStore.loadConfig()
  // Cargar fotos del álbum
  loadPhotos()
  
  // Add keyboard event listener for screenshot and print prevention
  window.addEventListener('keyup', handleKeyDown)
  window.addEventListener('keydown', handleKeyDown)
})

onBeforeUnmount(() => {
  // Clean up event listeners
  window.removeEventListener('keyup', handleKeyDown)
  window.removeEventListener('keydown', handleKeyDown)
})
</script>

<style scoped>
/* Prevent text/image selection */
.select-none {
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
}

/* Additional protection against dragging */
img {
  pointer-events: auto;
  -webkit-user-drag: none;
  -khtml-user-drag: none;
  -moz-user-drag: none;
  -o-user-drag: none;
  user-drag: none;
}

/* Prevent text selection on the entire page */
* {
  -webkit-touch-callout: none;
}
</style>