<template>
  <div>
    <div class="flex items-center justify-between mb-8">
      <h1 class="text-3xl font-bold">Gestión de Pedidos</h1>
      <button @click="loadOrders" class="btn btn-secondary">
        🔄 Actualizar
      </button>
    </div>
    
    <LoadingSpinner v-if="loading" message="Cargando pedidos..." />
    
    <div v-else-if="error" class="text-center py-12">
      <p class="text-red-600">{{ error }}</p>
      <button @click="loadOrders" class="btn btn-primary mt-4">
        Reintentar
      </button>
    </div>
    
    <div v-else>
      <!-- Filter Tabs -->
      <div class="flex gap-4 mb-6">
        <button 
          @click="filter = 'all'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'all' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Todos ({{ orders.length }})
        </button>
        <button 
          @click="filter = 'AwaitingPayment'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'AwaitingPayment' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Pendientes Pago ({{ awaitingOrders.length }})
        </button>
        <button 
          @click="filter = 'Processing'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'Processing' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          En Preparación ({{ processingOrders.length }})
        </button>
        <button 
          @click="filter = 'Completed'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'Completed' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Completados ({{ completedOrders.length }})
        </button>
        <button 
          @click="filter = 'Cancelled'"
          class="px-4 py-2 rounded-lg transition-colors"
          :class="filter === 'Cancelled' ? 'bg-primary-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
        >
          Cancelados ({{ cancelledOrders.length }})
        </button>
      </div>
      
      <!-- Orders Table -->
      <div v-if="filteredOrders.length === 0" class="card text-center py-12 text-gray-500">
        No hay pedidos {{ filter !== 'all' ? getStatusText(filter).toLowerCase() : '' }}
      </div>
      
      <div v-else class="card overflow-x-auto">
        <table class="w-full">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Pedido</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Fotos</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Fecha</th>
              <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            <tr v-for="order in filteredOrders" :key="order.id" class="hover:bg-gray-50">
              <td class="px-4 py-3 text-sm font-medium">#{{ order.id.substring(0, 8) }}</td>
              <td class="px-4 py-3 text-sm">
                <a :href="`mailto:${order.userEmail}`" class="text-primary-600 hover:text-primary-700">
                  {{ order.userEmail }}
                </a>
              </td>
              <td class="px-4 py-3 text-sm">{{ order.photos.length }}</td>
              <td class="px-4 py-3 text-sm font-medium">${{ Math.round(order.totalAmount).toLocaleString('es-CL') }} {{ order.currency }}</td>
              <td class="px-4 py-3 text-sm">
                <span 
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="getStatusClass(order.status)"
                >
                  {{ getStatusText(order.status) }}
                </span>
              </td>
              <td class="px-4 py-3 text-sm text-gray-500">{{ formatDate(order.createdAt) }}</td>
              <td class="px-4 py-3 text-sm">
                <div class="flex gap-2">
                  <button 
                    @click="viewOrderDetails(order)"
                    class="text-primary-600 hover:text-primary-700"
                    title="Ver detalles"
                  >
                    👁️
                  </button>
                  
                  <button 
                    v-if="order.status === 'AwaitingPayment'"
                    @click="confirmPayment(order.id)"
                    :disabled="processingOrderId === order.id"
                    class="text-green-600 hover:text-green-700 disabled:opacity-50"
                    title="Confirmar pago"
                  >
                    ✓
                  </button>
                  
                  <button 
                    v-if="order.status === 'Processing'"
                    @click="completeOrder(order.id)"
                    :disabled="processingOrderId === order.id"
                    class="text-blue-600 hover:text-blue-700 disabled:opacity-50"
                    title="Marcar como completado"
                  >
                    📦
                  </button>
                  
                  <button 
                    v-if="order.status !== 'Completed' && order.status !== 'Cancelled'"
                    @click="cancelOrder(order.id)"
                    :disabled="processingOrderId === order.id"
                    class="text-red-600 hover:text-red-700 disabled:opacity-50"
                    title="Cancelar pedido"
                  >
                    ✕
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    
    <!-- Order Details Modal -->
    <div 
      v-if="selectedOrder"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click="selectedOrder = null"
    >
      <div 
        class="bg-white rounded-lg max-w-3xl w-full max-h-[90vh] overflow-y-auto"
        @click.stop
      >
        <div class="p-6">
          <div class="flex items-start justify-between mb-6">
            <div>
              <h2 class="text-2xl font-bold mb-2">Pedido #{{ selectedOrder.id.substring(0, 8) }}</h2>
              <span 
                class="px-3 py-1 rounded-full text-sm font-medium"
                :class="getStatusClass(selectedOrder.status)"
              >
                {{ getStatusText(selectedOrder.status) }}
              </span>
            </div>
            <button @click="selectedOrder = null" class="text-gray-500 hover:text-gray-700 text-2xl">
              ×
            </button>
          </div>
          
          <!-- Customer Info -->
          <div class="bg-gray-50 rounded-lg p-4 mb-6">
            <h3 class="font-semibold mb-3">Información del Cliente</h3>
            <div class="grid md:grid-cols-2 gap-3 text-sm">
              <div>
                <span class="font-medium text-gray-700">Email:</span>
                <p class="text-gray-600">{{ selectedOrder.userEmail }}</p>
              </div>
              <div>
                <span class="font-medium text-gray-700">Fecha de Pedido:</span>
                <p class="text-gray-600">{{ formatDate(selectedOrder.createdAt) }}</p>
              </div>
              <div v-if="selectedOrder.paidAt">
                <span class="font-medium text-gray-700">Fecha de Pago:</span>
                <p class="text-gray-600 text-green-600">{{ formatDate(selectedOrder.paidAt) }}</p>
              </div>
              <div v-if="selectedOrder.paymentReference">
                <span class="font-medium text-gray-700">Nº de Transacción:</span>
                <p class="text-gray-600 font-mono">{{ selectedOrder.paymentReference }}</p>
              </div>
            </div>
          </div>
          
          <!-- Order Details -->
          <div class="mb-6">
            <h3 class="font-semibold mb-3">Detalles del Pedido</h3>
            <div class="space-y-2 text-sm">
              <div class="flex justify-between py-2 border-b">
                <span class="text-gray-700">Cantidad de fotos:</span>
                <span class="font-medium">{{ selectedOrder.photos.length }}</span>
              </div>
              <div class="flex justify-between py-2 border-b">
                <span class="text-gray-700">Moneda:</span>
                <span class="font-medium">{{ selectedOrder.currency }}</span>
              </div>
              <div class="flex justify-between py-2 border-b">
                <span class="text-gray-700">Total:</span>
                <span class="font-medium text-lg">${{ Math.round(selectedOrder.totalAmount).toLocaleString('es-CL') }} {{ selectedOrder.currency }}</span>
              </div>
            </div>
          </div>
          
          <!-- Photos -->
          <div class="mb-6">
            <h3 class="font-semibold mb-3">Fotos del Pedido ({{ selectedOrder.photos.length }})</h3>
            <div class="bg-gray-50 rounded-lg overflow-hidden">
              <div class="max-h-96 overflow-y-auto">
                <table class="w-full">
                  <thead class="bg-gray-200 sticky top-0">
                    <tr>
                      <th class="px-4 py-2 text-left text-xs font-medium text-gray-700 uppercase">#</th>
                      <th class="px-4 py-2 text-left text-xs font-medium text-gray-700 uppercase">Nombre del Álbum</th>
                      <th class="px-4 py-2 text-left text-xs font-medium text-gray-700 uppercase">Nombre de la Foto</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-gray-200">
                    <tr v-for="(photo, index) in selectedOrder.photos" :key="photo.photoId" class="hover:bg-gray-100">
                      <td class="px-4 py-2 text-sm text-gray-600">{{ index + 1 }}</td>
                      <td class="px-4 py-2 text-sm text-gray-800">{{ photo.albumTitle || 'Sin álbum' }}</td>
                      <td class="px-4 py-2 text-sm text-gray-800">{{ photo.filename }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
          
          <!-- Actions -->
          <div class="flex gap-3">
            <button 
              v-if="selectedOrder.status === 'AwaitingPayment'"
              @click="confirmPayment(selectedOrder.id); selectedOrder = null"
              class="flex-1 bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700"
            >
              ✓ Confirmar Pago
            </button>
            
            <button 
              v-if="selectedOrder.status === 'Processing'"
              @click="completeOrder(selectedOrder.id); selectedOrder = null"
              class="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
            >
              📦 Marcar como Completado
            </button>
            
            <button 
              v-if="selectedOrder.status !== 'Completed' && selectedOrder.status !== 'Cancelled'"
              @click="cancelOrder(selectedOrder.id); selectedOrder = null"
              class="flex-1 bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700"
            >
              ✕ Cancelar Pedido
            </button>
            
            <button 
              @click="selectedOrder = null"
              class="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300"
            >
              Cerrar
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ordersService from '@/services/ordersService'

const toast = useToast()

const loading = ref(true)
const error = ref(null)
const orders = ref([])
const filter = ref('all')
const selectedOrder = ref(null)
const processingOrderId = ref(null)

const awaitingOrders = computed(() => orders.value.filter(o => o.status === 'AwaitingPayment'))
const processingOrders = computed(() => orders.value.filter(o => o.status === 'Processing'))
const completedOrders = computed(() => orders.value.filter(o => o.status === 'Completed'))
const cancelledOrders = computed(() => orders.value.filter(o => o.status === 'Cancelled'))

const filteredOrders = computed(() => {
  if (filter.value === 'all') return orders.value
  return orders.value.filter(o => o.status === filter.value)
})

async function loadOrders() {
  try {
    loading.value = true
    error.value = null
    orders.value = await ordersService.getAllOrders()
  } catch (err) {
    console.error('Error loading orders:', err)
    error.value = 'Error al cargar los pedidos. Intenta nuevamente.'
    toast.error('Error al cargar los pedidos')
  } finally {
    loading.value = false
  }
}

async function confirmPayment(orderId) {
  try {
    processingOrderId.value = orderId
    await ordersService.confirmPayment(orderId, 'Admin-Confirmed')
    toast.success('Pago confirmado - Pedido en preparación')
    await loadOrders()
  } catch (err) {
    console.error('Error confirming payment:', err)
    toast.error('Error al confirmar el pago')
  } finally {
    processingOrderId.value = null
  }
}

async function completeOrder(orderId) {
  try {
    processingOrderId.value = orderId
    await ordersService.completeOrder(orderId)
    toast.success('Pedido completado - Enlace de descarga enviado al cliente')
    await loadOrders()
  } catch (err) {
    console.error('Error completing order:', err)
    toast.error(err.response?.data?.message || 'Error al completar el pedido')
  } finally {
    processingOrderId.value = null
  }
}

async function cancelOrder(orderId) {
  if (!confirm('¿Estás seguro de que deseas cancelar este pedido?')) {
    return
  }
  
  try {
    processingOrderId.value = orderId
    await ordersService.cancelOrder(orderId)
    toast.success('Pedido cancelado exitosamente')
    await loadOrders()
  } catch (err) {
    console.error('Error cancelling order:', err)
    toast.error(err.response?.data?.message || 'Error al cancelar el pedido')
  } finally {
    processingOrderId.value = null
  }
}

function viewOrderDetails(order) {
  selectedOrder.value = order
}

function handleImageError(event) {
  event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="200" height="200"%3E%3Crect width="200" height="200" fill="%23ddd"/%3E%3Ctext x="50%25" y="50%25" font-size="14" text-anchor="middle" fill="%23999"%3ENo disponible%3C/text%3E%3C/svg%3E'
}

function formatDate(dateString) {
  if (!dateString) return 'N/A'
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
  loadOrders()
})
</script>
