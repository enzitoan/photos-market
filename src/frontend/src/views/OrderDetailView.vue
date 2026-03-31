<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 w-full">
      <!-- Botón de regreso -->
      <router-link 
        to="/orders" 
        class="inline-flex items-center text-primary-600 hover:text-primary-700 mb-6"
      >
        <span class="mr-2">←</span>
        <span>Volver a Mis Pedidos</span>
      </router-link>
      
      <LoadingSpinner v-if="loading" message="Cargando detalles del pedido..." />
      
      <div v-else-if="error" class="text-center py-12">
        <p class="text-red-600 mb-4">{{ error }}</p>
        <button @click="loadOrderDetails" class="btn btn-primary">
          Reintentar
        </button>
      </div>
      
      <div v-else-if="order" class="space-y-6">
        <!-- Header del pedido -->
        <div class="card">
          <div class="flex justify-between items-start mb-4">
            <div>
              <h1 class="text-3xl font-bold mb-2">Pedido #{{ order.id.substring(0, 8) }}</h1>
              <p class="text-gray-600">Creado el {{ formatDate(order.createdAt) }}</p>
            </div>
            
            <span 
              class="px-4 py-2 rounded-full text-sm font-medium"
              :class="getStatusClass(order.status)"
            >
              {{ getStatusText(order.status) }}
            </span>
          </div>
          
          <div class="grid md:grid-cols-2 gap-6 mt-6 pt-6 border-t">
            <div>
              <h3 class="text-sm font-semibold text-gray-500 mb-2">INFORMACIÓN DEL PEDIDO</h3>
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span class="text-gray-600">Nombre:</span>
                  <span class="font-medium">{{ order.userName || 'No especificado' }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-600">Email:</span>
                  <span class="font-medium">{{ order.userEmail }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-600">Total de fotos:</span>
                  <span class="font-medium">{{ order.photos.length }}</span>
                </div>
                <div v-if="order.subtotal" class="flex justify-between">
                  <span class="text-gray-600">Subtotal:</span>
                  <span class="font-medium">${{ Math.round(order.subtotal).toLocaleString('es-CL') }} {{ order.currency }}</span>
                </div>
                <div v-if="order.discountPercentage && order.discountPercentage > 0" class="flex justify-between text-green-600">
                  <span>Descuento ({{ order.discountPercentage }}%):</span>
                  <span>-${{ Math.round(order.discountAmount).toLocaleString('es-CL') }} {{ order.currency }}</span>
                </div>
                <div class="flex justify-between pt-2 border-t">
                  <span class="text-gray-600 font-semibold">Total a pagar:</span>
                  <span class="font-medium text-lg">${{ Math.round(order.totalAmount).toLocaleString('es-CL') }} {{ order.currency }}</span>
                </div>
              </div>
            </div>
            
            <div>
              <h3 class="text-sm font-semibold text-gray-500 mb-2">FECHAS</h3>
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span class="text-gray-600">Creado:</span>
                  <span class="font-medium">{{ formatDate(order.createdAt) }}</span>
                </div>
                <div v-if="order.paidAt" class="flex justify-between">
                  <span class="text-gray-600">Pagado:</span>
                  <span class="font-medium text-green-600">{{ formatDate(order.paidAt) }}</span>
                </div>
                <div v-else class="flex justify-between">
                  <span class="text-gray-600">Pagado:</span>
                  <span class="font-medium text-yellow-600">Pendiente</span>
                </div>
                <div v-if="order.paymentReference" class="flex justify-between pt-2 border-t">
                  <span class="text-gray-600">Nº de Transacción:</span>
                  <span class="font-medium font-mono text-sm">{{ order.paymentReference }}</span>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Acciones del pedido -->
          <div v-if="order.status === 'AwaitingPayment'" class="mt-6 pt-6 border-t">
            <div class="bg-yellow-50 p-4 rounded-lg mb-4">
              <p class="text-yellow-800 font-medium mb-2">⏳ Pago Pendiente</p>
              <p class="text-sm text-yellow-700">
                Para completar tu pedido, por favor realiza el pago y confirma la transacción.
              </p>
            </div>
            <div class="flex gap-3">
              <button 
                @click="showPaymentDialog = true"
                class="btn btn-primary"
              >
                Confirmar Pago
              </button>
              <button 
                @click="handleCancelOrder"
                class="btn bg-red-600 text-white hover:bg-red-700"
              >
                Cancelar Pedido
              </button>
            </div>
          </div>
          
          <div v-else-if="order.status === 'Processing'" class="mt-6 pt-6 border-t">
            <div class="bg-blue-50 p-4 rounded-lg mb-4">
              <p class="text-blue-800 font-medium mb-2">📦 Pedido En Preparación</p>
              <p class="text-sm text-blue-700">
                Tu pago ha sido confirmado. Estamos preparando tus fotos. Recibirás un email con el enlace de descarga cuando esté listo.
              </p>
            </div>
            <button 
              @click="handleCancelOrder"
              class="btn bg-red-600 text-white hover:bg-red-700"
            >
              Cancelar Pedido
            </button>
          </div>
          
          <div v-else-if="order.status === 'Completed'" class="mt-6 pt-6 border-t">
            <div class="bg-green-50 p-4 rounded-lg mb-4">
              <p class="text-green-800 font-medium mb-2">✓ Pedido Completado</p>
              <p class="text-sm text-green-700">
                Tu pedido ha sido procesado exitosamente.
              </p>
            </div>
            
            <!-- Botón de descarga si el link está activo -->
            <div v-if="downloadLink && !downloadLink.isExpired" class="mt-4">
              <router-link 
                :to="`/download/${downloadLink.token}`"
                class="inline-flex items-center justify-center px-6 py-3 btn btn-primary"
              >
                <span class="mr-2">⬇</span>
                <span>Descargar Fotos</span>
              </router-link>
              <p class="text-sm text-gray-600 mt-2">
                Enlace válido hasta el {{ formatDate(downloadLink.expiresAt) }}
              </p>
            </div>
            
            <!-- Mensaje si el link expiró -->
            <div v-else-if="downloadLink && downloadLink.isExpired" class="mt-4 bg-yellow-50 p-4 rounded-lg">
              <p class="text-yellow-800 font-medium mb-1">⚠️ Enlace Expirado</p>
              <p class="text-sm text-yellow-700">
                El enlace de descarga ha expirado. Por favor, contacta al soporte para obtener un nuevo enlace.
              </p>
            </div>
            
            <!-- Mensaje si aún no hay link disponible -->
            <div v-else class="mt-4 bg-blue-50 p-4 rounded-lg">
              <p class="text-blue-800 font-medium mb-1">📧 Enlace de Descarga</p>
              <p class="text-sm text-blue-700">
                El enlace de descarga ha sido enviado a tu correo electrónico. Si no lo recibes en unos minutos, revisa tu carpeta de spam o contacta al soporte.
              </p>
            </div>
          </div>
          
          <div v-else-if="order.status === 'Cancelled'" class="mt-6 pt-6 border-t">
            <div class="bg-red-50 p-4 rounded-lg">
              <p class="text-red-800 font-medium mb-2">✕ Pedido Cancelado</p>
              <p class="text-sm text-red-700">
                Este pedido ha sido cancelado.
              </p>
            </div>
          </div>
        </div>
        
        <!-- Listado de fotos -->
        <div class="card">
          <h2 class="text-2xl font-semibold mb-4">Fotos del Pedido ({{ order.photos.length }})</h2>
          
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead class="bg-gray-50 border-b">
                <tr>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-16">#</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Nombre del Álbum</th>
                  <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Nombre de la Foto</th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr 
                  v-for="(photo, index) in order.photos" 
                  :key="photo.photoId"
                  class="hover:bg-gray-50 transition-colors"
                >
                  <td class="px-4 py-3 text-sm text-gray-500">
                    {{ index + 1 }}
                  </td>
                  <td class="px-4 py-3 text-sm">
                    <div class="flex items-center">
                      <span class="mr-2">📁</span>
                      <span class="font-medium text-gray-900">{{ photo.albumTitle || 'Sin álbum' }}</span>
                    </div>
                  </td>
                  <td class="px-4 py-3 text-sm text-gray-700">
                    {{ photo.filename }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </main>
    
    <!-- Modal de confirmación de pago -->
    <div v-if="showPaymentDialog" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-lg max-w-md w-full p-6">
        <h3 class="text-xl font-semibold mb-4">Confirmar Pago</h3>
        
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Nº de transacción / operación
          </label>
          <input 
            v-model="paymentReference"
            type="text"
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500"
            placeholder="Ej: 684549824"
          >
        </div>
        
        <div class="bg-blue-50 p-3 rounded-md mb-4">
          <p class="text-sm text-blue-800">
            💡 Ingresa el Nº de transacción / operación de tu transferencia de dinero para confirmar tu pedido.
            (Puedes encontrar este número en el comprobante de pago que te entrega tu banco o plataforma de pago)
          </p>
        </div>
        
        <div class="flex gap-3">
          <button 
            @click="showPaymentDialog = false"
            class="flex-1 px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
            :disabled="isConfirming"
          >
            Cancelar
          </button>
          <button 
            @click="confirmPayment"
            class="flex-1 btn btn-primary"
            :disabled="isConfirming || !paymentReference.trim()"
          >
            {{ isConfirming ? 'Confirmando...' : 'Confirmar' }}
          </button>
        </div>
      </div>
    </div>
    
    <!-- Lightbox para ver fotos en grande -->
    <div v-if="lightboxIndex !== null" class="fixed inset-0 bg-black bg-opacity-90 flex items-center justify-center z-50 p-4">
      <button 
        @click="lightboxIndex = null"
        class="absolute top-4 right-4 text-white text-3xl hover:text-gray-300"
      >
        ✕
      </button>
      
      <button 
        v-if="lightboxIndex > 0"
        @click="lightboxIndex--"
        class="absolute left-4 text-white text-3xl hover:text-gray-300"
      >
        ←
      </button>
      
      <button 
        v-if="order && lightboxIndex < order.photos.length - 1"
        @click="lightboxIndex++"
        class="absolute right-4 text-white text-3xl hover:text-gray-300"
      >
        →
      </button>
      
      <img 
        v-if="order"
        :src="getPhotoUrl(order.photos[lightboxIndex], true)"
        :alt="order.photos[lightboxIndex].filename"
        class="max-h-full max-w-full object-contain"
      >
      
      <div class="absolute bottom-4 left-0 right-0 text-center text-white">
        <p>{{ order?.photos[lightboxIndex]?.filename }}</p>
        <p class="text-sm text-gray-300">{{ lightboxIndex + 1 }} / {{ order?.photos.length }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import NavBar from '@/components/NavBar.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ordersService from '@/services/ordersService'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const loading = ref(true)
const error = ref(null)
const order = ref(null)
const downloadLink = ref(null)
const showPaymentDialog = ref(false)
const paymentReference = ref('')
const isConfirming = ref(false)
const lightboxIndex = ref(null)

async function loadOrderDetails() {
  try {
    loading.value = true
    error.value = null
    const orderId = route.params.id
    const response = await ordersService.getOrder(orderId)
    order.value = response.data
    
    // Si el pedido está completado, intentar obtener el link de descarga
    if (order.value.status === 'Completed') {
      await loadDownloadLink()
    }
  } catch (err) {
    console.error('Error loading order details:', err)
    error.value = 'Error al cargar los detalles del pedido. Intenta nuevamente.'
    toast.error('Error al cargar el pedido')
  } finally {
    loading.value = false
  }
}

async function loadDownloadLink() {
  try {
    const orderId = route.params.id
    const response = await ordersService.getOrderDownloadLink(orderId)
    
    if (response && response.success && response.data) {
      downloadLink.value = response.data
      console.log('✅ Download link loaded successfully:', downloadLink.value)
    } else {
      console.log('⚠️ No download link available')
    }
  } catch (err) {
    console.log('❌ Download link not available:', err.response?.data?.message || err.message)
  }
}

async function confirmPayment() {
  try {
    isConfirming.value = true
    const orderId = route.params.id
    await ordersService.confirmPayment(orderId, paymentReference.value)
    toast.success('¡Pago confirmado! Tu pedido está en preparación.')
    showPaymentDialog.value = false
    paymentReference.value = ''
    await loadOrderDetails() // Recargar detalles
  } catch (err) {
    console.error('Error confirming payment:', err)
    toast.error(err.response?.data?.message || 'Error al confirmar el pago. Intenta nuevamente.')
  } finally {
    isConfirming.value = false
  }
}

async function handleCancelOrder() {
  if (!confirm('¿Estás seguro de que deseas cancelar este pedido?')) {
    return
  }
  
  try {
    const orderId = route.params.id
    await ordersService.cancelOrder(orderId)
    toast.success('Pedido cancelado exitosamente')
    await loadOrderDetails()
  } catch (err) {
    console.error('Error cancelling order:', err)
    toast.error(err.response?.data?.message || 'Error al cancelar el pedido')
  }
}

function getPhotoUrl(photo, fullSize = false) {
  if (!photo.baseUrl) return ''
  // Para thumbnail usa parámetros de redimensionamiento, para full size usa =d
  return fullSize ? `${photo.baseUrl}=d` : `${photo.baseUrl}=w400-h400-c`
}

function handleImageError(event, photo) {
  if (!event.target || event.target.dataset.errorHandled === 'true') {
    return
  }
  
  console.warn('Error loading photo:', photo.filename)
  event.target.dataset.errorHandled = 'true'
  
  // Intentar con baseUrl sin parámetros
  if (photo.baseUrl && !event.target.src.includes('errorHandled')) {
    event.target.src = photo.baseUrl
    return
  }
  
  // Usar placeholder
  event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400"%3E%3Crect width="400" height="400" fill="%23ddd"/%3E%3Ctext x="50%25" y="50%25" font-size="14" text-anchor="middle" fill="%23999"%3ENo disponible%3C/text%3E%3C/svg%3E'
}

function openLightbox(index) {
  lightboxIndex.value = index
}

function formatDate(dateString) {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getStatusText(status) {
  const statusMap = {
    'AwaitingPayment': 'Pendiente de Pago',
    'PaymentConfirmed': 'Pago Confirmado',
    'Processing': 'En Preparación',
    'Completed': 'Completado',
    'Cancelled': 'Cancelado',
    'Pending': 'Pendiente'
  }
  return statusMap[status] || status
}

function getStatusClass(status) {
  const classMap = {
    'AwaitingPayment': 'bg-yellow-100 text-yellow-800',
    'PaymentConfirmed': 'bg-blue-100 text-blue-800',
    'Processing': 'bg-blue-100 text-blue-800',
    'Completed': 'bg-green-100 text-green-800',
    'Cancelled': 'bg-red-100 text-red-800',
    'Pending': 'bg-gray-100 text-gray-800'
  }
  return classMap[status] || 'bg-gray-100 text-gray-800'
}

onMounted(() => {
  loadOrderDetails()
})
</script>
