<template>
  <div class="photo-card card card-hover relative overflow-hidden group">
    <div class="relative aspect-square bg-gray-200">
      <img 
        :src="photo.thumbnailUrl" 
        :alt="photo.filename"
        class="w-full h-full object-cover"
        @error="handleImageError"
      />
      
      <!-- Watermark Overlay -->
      <div class="watermark-overlay" :data-watermark="watermarkText"></div>
      
      <!-- Overlay con acciones - visible en móvil, hover en desktop -->
      <div 
        class="absolute inset-0 bg-black bg-opacity-40 md:bg-opacity-50 md:opacity-0 md:group-hover:opacity-100 transition-opacity duration-200 flex items-center justify-center"
      >
        <div class="flex flex-col sm:flex-row gap-2 px-2">
          <button 
            @click.stop="toggleCart"
            :class="[
              'px-3 py-2 sm:px-4 rounded-lg font-medium transition-colors text-sm sm:text-base',
              isInCart ? 'bg-red-600 hover:bg-red-700 text-white' : 'bg-white hover:bg-gray-100 text-gray-800'
            ]"
          >
            {{ isInCart ? '❌ Quitar' : '➕ Agregar' }}
          </button>
          
          <button 
            @click.stop="$emit('view-details', photo)"
            class="px-3 py-2 sm:px-4 bg-primary-600 text-white rounded-lg hover:bg-primary-700 text-sm sm:text-base"
          >
            👁️ Ver
          </button>
        </div>
      </div>
    </div>
    
    <div class="p-3">
      <p class="text-sm text-gray-600 truncate">{{ photo.filename }}</p>
      <div class="flex justify-between items-center mt-2">
        <span class="text-sm text-gray-500">
          {{ formatDate(photo.creationTime) }}
        </span>
        <span class="font-bold text-primary-600">{{ cartStore.currencySymbol }}{{ price }}</span>
      </div>
    </div>
    
    <!-- Badge si está en el carrito -->
    <div v-if="isInCart" class="absolute top-2 right-2 bg-green-600 text-white px-2 py-1 rounded-full text-xs font-bold shadow-lg">
      ✓
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useCartStore } from '@/stores/cart'
import { useAdminStore } from '@/stores/admin'
import { useToast } from 'vue-toastification'

const props = defineProps({
  photo: {
    type: Object,
    required: true
  }
})

defineEmits(['view-details'])

const cartStore = useCartStore()
const adminStore = useAdminStore()
const toast = useToast()

const isInCart = computed(() => cartStore.isInCart(props.photo.id))
const price = computed(() => {
  const priceValue = cartStore.pricePerPhoto
  return typeof priceValue === 'number' ? Math.round(priceValue).toLocaleString('es-CL') : '5000'
})
const watermarkText = computed(() => adminStore.watermarkText)

function toggleCart() {
  if (isInCart.value) {
    cartStore.removeFromCart(props.photo.id)
    toast.info('Foto eliminada del carrito')
  } else {
    cartStore.addToCart(props.photo)
    toast.success('Foto agregada al carrito')
  }
}

function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('es-ES', { 
    year: 'numeric', 
    month: 'short', 
    day: 'numeric' 
  })
}

function handleImageError(event) {
  // Prevenir bucle infinito: si ya intentamos cargar el fallback, no hacer nada
  if (!event.target || event.target.dataset.errorHandled === 'true') {
    return
  }
  
  console.warn('Error loading image:', props.photo.thumbnailUrl)
  
  // Marcar que ya manejamos este error
  event.target.dataset.errorHandled = 'true'
  
  // Intentar usar baseUrl como fallback
  if (props.photo.baseUrl && event.target.src !== props.photo.baseUrl) {
    event.target.src = props.photo.baseUrl
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
  event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect width="400" height="400" fill="%23ddd"/%3E%3Ctext x="50%25" y="50%25" font-size="18" text-anchor="middle" fill="%23999"%3EImagen no disponible%3C/text%3E%3C/svg%3E'
}
</script>

<style scoped>
.photo-card {
  cursor: pointer;
}
</style>
