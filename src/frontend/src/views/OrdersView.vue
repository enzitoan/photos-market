<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8 w-full">
      <h1 class="text-2xl sm:text-3xl font-bold mb-6 sm:mb-8">Mis Pedidos</h1>
      
      <LoadingSpinner v-if="loading" message="Cargando pedidos..." />
      
      <div v-else-if="error" class="text-center py-12">
        <p class="text-red-600">{{ error }}</p>
        <button @click="loadOrders" class="btn btn-primary mt-4">
          Reintentar
        </button>
      </div>
      
      <div v-else-if="orders.length === 0" class="text-center py-12">
        <div class="text-6xl mb-4">📦</div>
        <p class="text-gray-600 text-lg mb-6">No tienes pedidos aún</p>
        <router-link to="/albums" class="btn btn-primary">
          Explorar Álbumes
        </router-link>
      </div>
      
      <div v-else class="space-y-3 sm:space-y-4">
        <div 
          v-for="order in orders" 
          :key="order.id"
          class="card hover:shadow-lg transition-shadow cursor-pointer"
          @click="viewOrderDetails(order.id)"
        >
          <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-2 sm:gap-0 mb-3 sm:mb-4">
            <div>
              <h3 class="font-semibold text-base sm:text-lg">Pedido #{{ order.id.substring(0, 8) }}</h3>
              <p class="text-xs sm:text-sm text-gray-500">{{ formatDate(order.createdAt) }}</p>
            </div>
            
            <span 
              class="px-2 sm:px-3 py-1 rounded-full text-xs sm:text-sm font-medium self-start"
              :class="getStatusClass(order.status)"
            >
              {{ getStatusText(order.status) }}
            </span>
          </div>
          
          <div class="space-y-1 sm:space-y-2 text-xs sm:text-sm">
            <div class="flex justify-between">
              <span class="text-gray-600">Fotos:</span>
              <span class="font-medium">{{ order.photos.length }}</span>
            </div>
            
            <div class="flex justify-between">
              <span class="text-gray-600">Total:</span>
              <span class="font-medium">${{ Math.round(order.totalAmount).toLocaleString('es-CL') }} {{ order.currency }}</span>
            </div>
            
            <div class="flex justify-between">
              <span class="text-gray-600">Email:</span>
              <span class="font-medium">{{ order.userEmail }}</span>
            </div>
            
            <div v-if="order.paidAt" class="flex justify-between">
              <span class="text-gray-600">Pagado:</span>
              <span class="font-medium text-green-600 text-xs sm:text-sm">{{ formatDate(order.paidAt) }}</span>
            </div>
          </div>
          
          <div class="mt-3 sm:mt-4 pt-3 sm:pt-4 border-t">
            <button
              class="text-primary-600 hover:text-primary-700 font-medium flex items-center text-sm"
              @click.stop="viewOrderDetails(order.id)"
            >
              <span>Ver detalles</span>
              <span class="ml-2">→</span>
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import NavBar from '@/components/NavBar.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import ordersService from '@/services/ordersService'

const router = useRouter()
const toast = useToast()

const loading = ref(true)
const error = ref(null)
const orders = ref([])

async function loadOrders() {
  try {
    loading.value = true
    error.value = null
    const response = await ordersService.getOrders()
    orders.value = response.data || []
  } catch (err) {
    console.error('Error loading orders:', err)
    error.value = 'Error al cargar los pedidos. Intenta nuevamente.'
    toast.error('Error al cargar los pedidos')
  } finally {
    loading.value = false
  }
}

function viewOrderDetails(orderId) {
  router.push(`/orders/${orderId}`)
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
