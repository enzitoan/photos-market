<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8 w-full">
      <h1 class="text-2xl sm:text-3xl font-bold mb-6 sm:mb-8">Carrito de Compras</h1>
      
      <div v-if="cartStore.items.length === 0" class="text-center py-12">
        <div class="text-6xl mb-4">🛒</div>
        <p class="text-gray-600 text-lg mb-6">Tu carrito está vacío</p>
        <router-link to="/albums" class="btn btn-primary">
          Explorar Álbumes
        </router-link>
      </div>
      
      <div v-else class="flex flex-col lg:grid lg:grid-cols-3 gap-6 lg:gap-8">
        <!-- Cart Items -->
        <div class="lg:col-span-2 order-2 lg:order-1">
          <div class="card mb-4">
            <h2 class="text-lg sm:text-xl font-semibold mb-4">Fotos Seleccionadas ({{ cartStore.items.length }})</h2>
            
            <div class="space-y-3 sm:space-y-4">
              <div 
                v-for="item in cartStore.items" 
                :key="item.id"
                class="flex gap-3 sm:gap-4 pb-3 sm:pb-4 border-b last:border-b-0"
              >
                <img 
                  :src="getImageUrl(item)"
                  :alt="item.filename"
                  class="w-16 h-16 sm:w-20 sm:h-20 md:w-24 md:h-24 object-cover rounded flex-shrink-0"
                  @error="handleImageError($event, item)"
                >
                
                <div class="flex-1 min-w-0">
                  <h3 class="font-medium text-sm sm:text-base truncate">{{ item.filename }}</h3>
                  <p class="text-xs sm:text-sm text-gray-500" v-if="item.albumTitle">
                    📁 {{ item.albumTitle }}
                  </p>
                  <p class="text-xs sm:text-sm text-gray-500" v-if="item.creationTime">
                    {{ formatDate(item.creationTime) }}
                  </p>
                </div>
                
                <div class="text-right flex flex-col justify-between items-end">
                  <p class="font-semibold text-base sm:text-lg whitespace-nowrap">{{ cartStore.currencySymbol }}{{ Math.round(item.price).toLocaleString('es-CL') }}</p>
                  <button 
                    @click="cartStore.removeFromCart(item.id)"
                    class="text-red-600 hover:text-red-700 text-xs sm:text-sm mt-2"
                  >
                    Eliminar
                  </button>
                </div>
              </div>
            </div>
          </div>
          
          <button 
            @click="cartStore.clearCart"
            class="text-red-600 hover:text-red-700 text-sm"
          >
            Vaciar carrito
          </button>
        </div>
        
        <!-- Order Summary -->
        <div class="lg:col-span-1 order-1 lg:order-2">
          <div class="card lg:sticky lg:top-4">
            <h2 class="text-lg sm:text-xl font-semibold mb-4">Resumen del Pedido</h2>
            
            <div class="space-y-2 mb-4 pb-4 border-b">
              <div class="flex justify-between">
                <span class="text-gray-600">Subtotal ({{ cartStore.totalItems }} fotos)</span>
                <span class="font-medium">{{ cartStore.currencySymbol }}{{ Math.round(cartStore.subtotal).toLocaleString('es-CL') }}</span>
              </div>
              
              <!-- Mostrar descuento si aplica -->
              <div v-if="cartStore.discountPercentage > 0" class="flex justify-between text-green-600">
                <span>Descuento ({{ cartStore.discountPercentage }}%)</span>
                <span>-{{ cartStore.currencySymbol }}{{ Math.round(cartStore.discountAmount).toLocaleString('es-CL') }}</span>
              </div>
              
              <!-- Mensaje informativo sobre descuento -->
              <div v-if="cartStore.discountPercentage === 0 && cartStore.totalItems > 0" class="text-sm text-blue-600 mt-2 p-2 bg-blue-50 rounded">
                💡 ¡Agrega {{ cartStore.bulkDiscountMinPhotos - cartStore.totalItems }} foto(s) más y obtén {{ cartStore.bulkDiscountPercentage }}% de descuento!
              </div>
              
              <!-- Mensaje de descuento aplicado -->
              <div v-if="cartStore.discountPercentage > 0" class="text-sm text-green-600 mt-2 p-2 bg-green-50 rounded">
                🎉 ¡Descuento de {{ cartStore.discountPercentage }}% aplicado!
              </div>
            </div>
            
            <div class="flex justify-between text-lg font-bold mb-6">
              <span>Total</span>
              <span>{{ cartStore.currencySymbol }}{{ Math.round(cartStore.totalAmount).toLocaleString('es-CL') }}</span>
            </div>
            
            <button 
              @click="handleCheckout"
              :disabled="isProcessing"
              class="w-full btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ isProcessing ? 'Procesando...' : 'Finalizar Compra' }}
            </button>
            
            <div class="mt-4 p-3 bg-blue-50 rounded-md">
              <p class="text-xs sm:text-sm text-blue-800">
                📧 <strong>Importante:</strong> Recibirás un email con:
              </p>
              <ul class="text-xs text-blue-700 mt-2 ml-5 list-disc space-y-1">
                <li>Detalles de tu pedido</li>
                <li>Instrucciones de pago</li>
                <li>Enlace de descarga (una vez confirmado el pago)</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import NavBar from '@/components/NavBar.vue'
import ordersService from '@/services/ordersService'

const router = useRouter()
const toast = useToast()
const cartStore = useCartStore()

const isProcessing = ref(false)

function getImageUrl(item) {
  if (item.thumbnailUrl) {
    return item.thumbnailUrl
  }
  if (item.baseUrl) {
    return item.baseUrl + '=w200-h200-c'
  }
  return ''
}

function handleImageError(event, item) {
  if (!event.target || event.target.dataset.errorHandled === 'true') {
    return
  }
  
  console.warn('Error loading cart image:', item.filename)
  event.target.dataset.errorHandled = 'true'
  
  // Intentar con baseUrl sin parámetros
  if (item.baseUrl && !event.target.src.includes('proxy')) {
    event.target.src = item.baseUrl
    event.target.dataset.errorHandled = 'false'
    return
  }
  
  // Usar placeholder
  event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="200" height="200"%3E%3Crect width="200" height="200" fill="%23ddd"/%3E%3Ctext x="50%25" y="50%25" font-size="14" text-anchor="middle" fill="%23999"%3ENo disponible%3C/text%3E%3C/svg%3E'
}

function formatDate(dateString) {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

async function handleCheckout() {
  try {
    isProcessing.value = true
    
    // Obtener el nombre del usuario desde el store de auth
    const authStore = useAuthStore()
    const userName = authStore.user?.name || ''
    
    // Preparar los datos de la orden según el formato que espera el backend
    const orderData = {
      userName: userName,
      photos: cartStore.items.map(item => ({
        photoId: item.id,
        mediaItemId: item.mediaItemId,
        filename: item.filename,
        baseUrl: item.baseUrl,
        albumId: item.albumId || null,
        albumTitle: item.albumTitle || null
      }))
    }
    
    const response = await ordersService.createOrder(orderData)
    
    toast.success('¡Pedido creado exitosamente! Recibirás un email con los detalles.')
    cartStore.clearCart()
    
    // Redirigir a la vista de pedidos
    router.push('/orders')
  } catch (err) {
    console.error('Checkout error:', err)
    toast.error(err.response?.data?.message || 'Error al procesar el pedido. Intenta nuevamente.')
  } finally {
    isProcessing.value = false
  }
}
</script>
