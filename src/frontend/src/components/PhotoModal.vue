<template>
  <!-- Modal Overlay -->
  <Teleport to="body">
    <Transition name="modal">
      <div 
        v-if="show && photo"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-80 p-2 sm:p-4"
        @click="closeModal"
      >
        <div 
          class="photo-modal relative max-w-6xl max-h-[90vh] w-full"
          @click.stop
        >
          <!-- Close button -->
          <button
            @click="closeModal"
            class="absolute -top-8 sm:-top-12 right-0 text-white hover:text-gray-300 text-3xl sm:text-4xl font-bold z-10"
            aria-label="Cerrar"
          >
            ×
          </button>

          <!-- Photo info -->
          <div class="absolute -top-8 sm:-top-10 left-0 text-white mb-2">
            <p class="text-xs sm:text-sm truncate max-w-[60vw] sm:max-w-none">{{ photo.filename }}</p>
            <p v-if="photo.albumTitle" class="text-xs text-gray-300 mt-1 hidden sm:block">📁 {{ photo.albumTitle }}</p>
          </div>

          <!-- Image Container with Watermark -->
          <div 
            class="relative bg-gray-900 rounded-lg overflow-hidden"
            @touchstart="handleTouchStart"
            @touchend="handleTouchEnd"
          >
            <img
              :src="imageUrl"
              :alt="photo.filename"
              class="w-full h-auto max-h-[80vh] object-contain select-none"
              draggable="false"
              @contextmenu.prevent
              @dragstart.prevent
              @error="handleImageError"
            />
            
            <!-- Watermark Overlay (cruzada) -->
            <div class="watermark-overlay" :data-watermark="watermarkText"></div>
            
            <!-- Navigation Buttons (visible on hover for desktop, always visible on mobile with carousel) -->
            <button
              v-if="hasPrevious"
              @click="navigatePrevious"
              class="carousel-btn carousel-btn-left"
              aria-label="Foto anterior"
            >
              <svg class="w-6 h-6 sm:w-8 sm:h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            
            <button
              v-if="hasNext"
              @click="navigateNext"
              class="carousel-btn carousel-btn-right"
              aria-label="Foto siguiente"
            >
              <svg class="w-6 h-6 sm:w-8 sm:h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </div>
          
          <!-- Photo Position Indicator -->
          <div v-if="hasMultiplePhotos" class="text-center mt-2 text-white text-sm">
            {{ photoPosition }}
          </div>

          <!-- Actions -->
          <div class="mt-3 sm:mt-4 flex flex-col sm:flex-row justify-center gap-2 sm:gap-4">
            <button
              @click="toggleCart"
              :class="[
                'px-4 py-2 sm:px-6 sm:py-3 rounded-lg font-medium transition-colors text-sm sm:text-base',
                isInCart ? 'bg-red-600 hover:bg-red-700 text-white' : 'bg-white hover:bg-gray-100 text-gray-800'
              ]"
            >
              {{ isInCart ? '❌ Quitar del carrito' : '➕ Agregar al carrito' }}
            </button>
            
            <div class="px-4 py-2 sm:px-6 sm:py-3 bg-primary-600 text-white rounded-lg font-bold text-center text-sm sm:text-base">
              {{ cartStore.currencySymbol }}{{ price }}
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed, ref, onMounted, onBeforeUnmount } from 'vue'
import { useCartStore } from '@/stores/cart'
import { useAdminStore } from '@/stores/admin'
import { useToast } from 'vue-toastification'

const props = defineProps({
  show: {
    type: Boolean,
    required: true
  },
  photo: {
    type: Object,
    required: true
  },
  photos: {
    type: Array,
    default: () => []
  },
  currentIndex: {
    type: Number,
    default: -1
  }
})

const emit = defineEmits(['close', 'navigate'])

const cartStore = useCartStore()
const adminStore = useAdminStore()
const toast = useToast()

// Touch events for swipe
const touchStartX = ref(0)
const touchEndX = ref(0)

// Computed properties
const isInCart = computed(() => cartStore.isInCart(props.photo.id))
const price = computed(() => {
  const priceValue = cartStore.pricePerPhoto
  return typeof priceValue === 'number' ? Math.round(priceValue).toLocaleString('es-CL') : '5000'
})
const watermarkText = computed(() => adminStore.watermarkText)

// Carousel navigation
const hasMultiplePhotos = computed(() => props.photos && props.photos.length > 1)
const hasPrevious = computed(() => hasMultiplePhotos.value && props.currentIndex > 0)
const hasNext = computed(() => hasMultiplePhotos.value && props.currentIndex < props.photos.length - 1)
const photoPosition = computed(() => {
  if (!hasMultiplePhotos.value || props.currentIndex === -1) return ''
  return `${props.currentIndex + 1} de ${props.photos.length}`
})

// Usar la URL base para mejor resolución en el modal
const imageUrl = computed(() => {
  // Intentar usar baseUrl para mejor calidad, fallback a thumbnailUrl
  return props.photo.baseUrl || props.photo.thumbnailUrl
})

function closeModal() {
  emit('close')
}

// Navigation functions
function navigatePrevious() {
  if (hasPrevious.value) {
    emit('navigate', props.currentIndex - 1)
  }
}

function navigateNext() {
  if (hasNext.value) {
    emit('navigate', props.currentIndex + 1)
  }
}

function toggleCart() {
  if (isInCart.value) {
    cartStore.removeFromCart(props.photo.id)
    toast.info('Foto eliminada del carrito')
  } else {
    cartStore.addToCart(props.photo)
    toast.success('Foto agregada al carrito')
  }
}

function handleImageError(event) {
  // Prevenir bucle infinito: si ya intentamos cargar el fallback, no hacer nada
  if (!event.target || event.target.dataset.errorHandled === 'true') {
    return
  }
  
  console.warn('Error loading image in modal:', imageUrl.value)
  
  // Marcar que ya manejamos este error
  event.target.dataset.errorHandled = 'true'
  
  // Intentar usar thumbnailUrl como fallback si estábamos usando baseUrl
  if (props.photo.thumbnailUrl && event.target.src !== props.photo.thumbnailUrl) {
    event.target.src = props.photo.thumbnailUrl
    event.target.dataset.errorHandled = 'false' // Permitir otro intento
    return
  }
  
  // Intentar usar el endpoint proxy del backend
  const API_URL = getApiUrl()
  const proxyUrl = `${API_URL}/api/photos/proxy/${props.photo.id}`
  if (event.target.src !== proxyUrl) {
    event.target.src = proxyUrl
    event.target.dataset.errorHandled = 'false' // Permitir otro intento
    return
  }
  
  // Si todo falla, usar placeholder
  event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="800" height="600"%3E%3Crect width="800" height="600" fill="%23333"/%3E%3Ctext x="50%25" y="50%25" font-size="24" text-anchor="middle" fill="%23999"%3EImagen no disponible%3C/text%3E%3C/svg%3E'
}

// Keyboard navigation
function handleKeydown(event) {
  if (event.key === 'Escape') {
    closeModal()
  } else if (event.key === 'ArrowLeft') {
    navigatePrevious()
  } else if (event.key === 'ArrowRight') {
    navigateNext()
  }
}

// Touch events for swipe
function handleTouchStart(event) {
  touchStartX.value = event.touches[0].clientX
}

function handleTouchEnd(event) {
  touchEndX.value = event.changedTouches[0].clientX
  handleSwipe()
}

function handleSwipe() {
  const swipeThreshold = 50 // Minimum distance for swipe in pixels
  const diff = touchStartX.value - touchEndX.value
  
  if (Math.abs(diff) > swipeThreshold) {
    if (diff > 0) {
      // Swipe left - next photo
      navigateNext()
    } else {
      // Swipe right - previous photo
      navigatePrevious()
    }
  }
}

// Lifecycle hooks
onMounted(() => {
  window.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<style scoped>
/* Transición del modal */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .photo-modal,
.modal-leave-active .photo-modal {
  transition: transform 0.3s ease;
}

.modal-enter-from .photo-modal,
.modal-leave-to .photo-modal {
  transform: scale(0.9);
}

/* Carousel Navigation Buttons */
.carousel-btn {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background-color: rgba(0, 0, 0, 0.5);
  color: white;
  border: none;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 10;
  backdrop-filter: blur(4px);
}

.carousel-btn:hover {
  background-color: rgba(0, 0, 0, 0.8);
  transform: translateY(-50%) scale(1.1);
}

.carousel-btn:active {
  transform: translateY(-50%) scale(0.95);
}

.carousel-btn-left {
  left: 10px;
}

.carousel-btn-right {
  right: 10px;
}

/* Mobile - larger buttons */
@media (max-width: 640px) {
  .carousel-btn {
    width: 48px;
    height: 48px;
  }
}

/* Desktop - show on hover */
@media (min-width: 641px) {
  .carousel-btn {
    opacity: 0;
  }
  
  .photo-modal:hover .carousel-btn {
    opacity: 1;
  }
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
</style>
