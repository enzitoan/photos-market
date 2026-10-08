<template>
  <div class="min-h-screen flex flex-col" @contextmenu.prevent="handleRightClick">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8 w-full">
      <!-- Barra fija con el botón de volver -->
      <div class="sticky top-0 z-30 -mx-4 sm:-mx-6 lg:-mx-8 px-4 sm:px-6 lg:px-8 py-2 mb-3 sm:mb-4 bg-gray-50/95 backdrop-blur border-b border-gray-200">
        <button @click="$router.back()" class="text-primary-600 hover:text-primary-700 flex items-center text-sm sm:text-base">
          <span class="mr-2">←</span> Volver a álbumes
        </button>
      </div>

      <!-- Album Header -->
      <div class="mb-6 sm:mb-8">
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

    <!-- Botón para subir al inicio -->
    <transition name="fade">
      <button
        v-if="showScrollTop && !showPhotoModal"
        @click="scrollToTop"
        class="fixed bottom-24 right-6 z-40 w-12 h-12 flex items-center justify-center bg-white text-primary-600 hover:bg-primary-50 border border-gray-200 rounded-full shadow-lg transition-colors"
        title="Subir al inicio"
        aria-label="Subir al inicio del álbum"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7" />
        </svg>
      </button>
    </transition>

    <!-- Floating Cart Button -->
    <FloatingCartButton />
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
import FloatingCartButton from '@/components/FloatingCartButton.vue'
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
const showScrollTop = ref(false)

function handleScroll() {
  showScrollTop.value = window.scrollY > 300
}

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const albumId = computed(() => route.params.id)
const accessCode = ref(sessionStorage.getItem(`album-access-${albumId.value}`) || '')

const filteredPhotos = computed(() => {
  if (!showOnlyInCart.value) return photos.value
  
  const cartPhotoIds = new Set(cartStore.items.map(item => item.id))
  return photos.value.filter(photo => cartPhotoIds.has(photo.id))
})

async function loadPhotos() {
  try {
    loading.value = true
    error.value = null

    const currentAccessCode = accessCode.value?.trim()
    const [albumResponse, response] = await Promise.all([
      photosService.getAlbum(albumId.value, currentAccessCode),
      photosService.getAlbumPhotos(albumId.value, currentAccessCode)
    ])
    album.value = albumResponse?.data || {
      id: albumId.value,
      title: `Álbum ${albumId.value}`
    }

    const photosData = response?.data || []
    photos.value = photosData.filter(photo => photo && photo.id)
  } catch (err) {
    if (err?.response?.status === 403) {
      const enteredCode = window.prompt('Este álbum es privado. Ingresa el código de acceso:')
      if (enteredCode && enteredCode.trim()) {
        accessCode.value = enteredCode.trim()
        sessionStorage.setItem(`album-access-${albumId.value}`, accessCode.value)
        await loadPhotos()
        return
      }
      error.value = 'Se requiere un código de acceso para ver este álbum.'
      toast.error('Se requiere un código de acceso para ver este álbum')
    } else {
      console.error('Error loading photos:', err)
      error.value = 'Error al cargar las fotos. Intenta nuevamente.'
      toast.error('Error al cargar las fotos')
    }
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
  window.addEventListener('scroll', handleScroll, { passive: true })
  handleScroll()
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', handleScroll)
  // Clean up event listeners
  window.removeEventListener('keyup', handleKeyDown)
  window.removeEventListener('keydown', handleKeyDown)
})
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

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