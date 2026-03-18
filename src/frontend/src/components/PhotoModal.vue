<template>
  <!-- Modal Overlay -->
  <Teleport to="body">
    <Transition name="modal">
      <div 
        v-if="show && photo"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-80 p-4"
        @click="closeModal"
      >
        <div 
          class="photo-modal relative max-w-6xl max-h-[90vh] w-full"
          @click.stop
        >
          <!-- Close button -->
          <button
            @click="closeModal"
            class="absolute -top-12 right-0 text-white hover:text-gray-300 text-4xl font-bold z-10"
            aria-label="Cerrar"
          >
            ×
          </button>

          <!-- Photo info -->
          <div class="absolute -top-10 left-0 text-white mb-2">
            <p class="text-sm">{{ photo.filename }}</p>
            <p v-if="photo.albumTitle" class="text-xs text-gray-300 mt-1">📁 {{ photo.albumTitle }}</p>
          </div>

          <!-- Image Container with Watermark -->
          <div class="relative bg-gray-900 rounded-lg overflow-hidden">
            <img
              :src="imageUrl"
              :alt="photo.filename"
              class="w-full h-auto max-h-[80vh] object-contain"
              @error="handleImageError"
            />
            
            <!-- Watermark Overlay (cruzada) -->
            <div class="watermark-overlay" :data-watermark="watermarkText"></div>
          </div>

          <!-- Actions -->
          <div class="mt-4 flex justify-center gap-4">
            <button
              @click="toggleCart"
              :class="[
                'px-6 py-3 rounded-lg font-medium transition-colors',
                isInCart ? 'bg-red-600 hover:bg-red-700 text-white' : 'bg-white hover:bg-gray-100 text-gray-800'
              ]"
            >
              {{ isInCart ? '❌ Quitar del carrito' : '➕ Agregar al carrito' }}
            </button>
            
            <div class="px-6 py-3 bg-primary-600 text-white rounded-lg font-bold">
              {{ cartStore.currencySymbol }}{{ price }}
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed } from 'vue'
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
  }
})

const emit = defineEmits(['close'])

const cartStore = useCartStore()
const adminStore = useAdminStore()
const toast = useToast()

const isInCart = computed(() => cartStore.isInCart(props.photo.id))
const price = computed(() => {
  const priceValue = cartStore.pricePerPhoto
  return typeof priceValue === 'number' ? Math.round(priceValue).toLocaleString('es-CL') : '5000'
})
const watermarkText = computed(() => adminStore.watermarkText)

// Usar la URL base para mejor resolución en el modal
const imageUrl = computed(() => {
  // Intentar usar baseUrl para mejor calidad, fallback a thumbnailUrl
  return props.photo.baseUrl || props.photo.thumbnailUrl
})

function closeModal() {
  emit('close')
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

// Cerrar modal con tecla Escape
function handleKeydown(event) {
  if (event.key === 'Escape') {
    closeModal()
  }
}

// Agregar/remover event listener
if (typeof window !== 'undefined') {
  window.addEventListener('keydown', handleKeydown)
}
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
</style>
