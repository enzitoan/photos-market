<template>
  <div>
    <h1 class="text-2xl sm:text-3xl font-bold mb-6 sm:mb-8">Dashboard</h1>
    
    <!-- Google Photos Connection Alert -->
    <div v-if="!googlePhotosConnected && !checkingGoogle" class="mb-6 bg-yellow-50 border-l-4 border-yellow-400 p-4 rounded-r-lg">
      <div class="flex items-start">
        <div class="flex-shrink-0">
          <Icon name="alert" :size="24" class="text-yellow-400" />
        </div>
        <div class="ml-3 flex-1">
          <h3 class="text-sm font-medium text-yellow-800">
            <Icon name="alert" :size="16" class="inline" /> Acción Requerida: Conectar Google Photos
          </h3>
          <div class="mt-2 text-sm text-yellow-700">
            <p>La cuenta de Google Photos no está conectada. Los clientes no podrán ver álbumes hasta que conecte la cuenta <strong>ahumada.enzo@gmail.com</strong>.</p>
          </div>
          <div class="mt-4">
            <router-link 
              to="/admin/google-auth"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <svg class="w-5 h-5 mr-2" viewBox="0 0 24 24">
                <path fill="currentColor" d="M12.545,10.239v3.821h5.445c-0.712,2.315-2.647,3.972-5.445,3.972c-3.332,0-6.033-2.701-6.033-6.032s2.701-6.032,6.033-6.032c1.498,0,2.866,0.549,3.921,1.453l2.814-2.814C17.503,2.988,15.139,2,12.545,2C7.021,2,2.543,6.477,2.543,12s4.478,10,10.002,10c8.396,0,10.249-7.85,9.426-11.748L12.545,10.239z"/>
              </svg>
              Conectar Google Photos Ahora
            </router-link>
          </div>
        </div>
      </div>
    </div>
    
    <LoadingSpinner v-if="loading" message="Cargando estadísticas..." />
    
    <div v-else>
      <!-- Stats Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 mb-6 sm:mb-8">
        <div class="card hover:shadow-lg transition-shadow">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-gray-600 text-sm font-medium">Pedidos Totales</p>
              <p class="text-2xl sm:text-3xl font-bold mt-1 text-gray-900">{{ stats.totalOrders }}</p>
              <p class="text-xs text-gray-500 mt-1">{{ stats.totalPhotos }} fotos</p>
            </div>
            <Icon name="package" :size="40" class="text-gray-400" />
          </div>
        </div>
        
        <div class="card hover:shadow-lg transition-shadow">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-gray-600 text-sm font-medium">Pedidos Pendientes</p>
              <p class="text-2xl sm:text-3xl font-bold mt-1 text-yellow-600">{{ stats.pendingOrders }}</p>
              <p class="text-xs text-gray-500 mt-1">Requieren acción</p>
            </div>
            <Icon name="clock" :size="40" class="text-yellow-400" />
          </div>
        </div>
        
        <div class="card hover:shadow-lg transition-shadow">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-gray-600 text-sm font-medium">Ingresos Totales</p>
              <p class="text-2xl sm:text-3xl font-bold mt-1 text-green-600">${{ formatCurrency(stats.totalRevenue) }}</p>
              <p class="text-xs text-gray-500 mt-1">{{ stats.confirmedOrders }} pedidos pagados</p>
            </div>
            <Icon name="dollar" :size="40" class="text-green-400" />
          </div>
        </div>
        
        <div class="card hover:shadow-lg transition-shadow">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-gray-600 text-sm font-medium">Álbumes Bloqueados</p>
              <p class="text-2xl sm:text-3xl font-bold mt-1 text-red-600">{{ adminStore.blockedAlbums.size }}</p>
              <p class="text-xs text-gray-500 mt-1">No visibles para clientes</p>
            </div>
            <Icon name="ban" :size="40" class="text-red-400" />
          </div>
        </div>
      </div>
      
      <!-- Recent Orders -->
      <div class="card mb-6">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-lg sm:text-xl font-semibold flex items-center">
            <Icon name="file-text" :size="20" class="mr-2" />
            Pedidos Recientes
          </h2>
          <router-link 
            to="/admin/orders" 
            class="text-primary-600 hover:text-primary-700 text-sm font-medium flex items-center transition-colors"
          >
            <span class="hidden sm:inline">Ver todos</span>
            <Icon name="chevron-right" :size="16" class="ml-1" />
          </router-link>
        </div>
        
        <div v-if="recentOrders.length === 0" class="text-center py-12">
          <Icon name="package" :size="64" class="mx-auto mb-4 text-gray-300" />
          <p class="text-gray-500 text-lg font-medium">No hay pedidos recientes</p>
          <p class="text-gray-400 text-sm mt-2">Los pedidos aparecerán aquí cuando los clientes realicen compras</p>
        </div>
        
        <div v-else class="overflow-x-auto -mx-4 sm:mx-0">
          <table class="w-full min-w-[640px]">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-2 sm:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID Pedido</th>
                <th class="px-2 sm:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider hidden md:table-cell">Cliente</th>
                <th class="px-2 sm:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Fotos</th>
                <th class="px-2 sm:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total</th>
                <th class="px-2 sm:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Estado</th>
                <th class="px-2 sm:px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider hidden lg:table-cell">Fecha</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 bg-white">
              <tr v-for="order in recentOrders" :key="order.id" class="hover:bg-gray-50 transition-colors">
                <td class="px-2 sm:px-4 py-3">
                  <span class="text-xs sm:text-sm font-mono text-gray-900">#{{ order.id.substring(0, 8) }}</span>
                </td>
                <td class="px-2 sm:px-4 py-3 hidden md:table-cell">
                  <div class="text-sm text-gray-900">{{ order.userEmail }}</div>
                </td>
                <td class="px-2 sm:px-4 py-3">
                  <div class="flex items-center">
                    <span class="text-xs sm:text-sm font-medium text-gray-900">{{ order.photos.length }}</span>
                    <span class="ml-1 text-xs text-gray-500 hidden sm:inline">fotos</span>
                  </div>
                </td>
                <td class="px-2 sm:px-4 py-3">
                  <span class="text-xs sm:text-sm font-semibold text-gray-900">${{ formatCurrency(order.totalAmount) }}</span>
                  <span class="text-xs text-gray-500 ml-1 hidden sm:inline">{{ order.currency }}</span>
                </td>
                <td class="px-2 sm:px-4 py-3">
                  <span 
                    class="px-2 sm:px-3 py-1 rounded-full text-xs font-semibold whitespace-nowrap"
                    :class="getStatusClass(order.status)"
                  >
                    {{ getStatusText(order.status) }}
                  </span>
                </td>
                <td class="px-2 sm:px-4 py-3 hidden lg:table-cell">
                  <div class="text-sm text-gray-900">{{ formatDate(order.createdAt) }}</div>
                  <div v-if="order.paidAt" class="text-xs text-green-600">
                    Pagado: {{ formatDate(order.paidAt) }}
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      
      <!-- Order Status Summary -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-6">
        <div class="card">
          <h2 class="text-lg sm:text-xl font-semibold mb-6 flex items-center">
            <Icon name="trending-up" :size="20" class="mr-2" />
            Resumen por Estado
          </h2>
          
          <div class="space-y-3">
            <div v-for="status in orderStatusSummary" :key="status.key" 
                 class="flex items-center justify-between p-3 rounded-lg hover:bg-gray-50 transition-colors">
              <div class="flex items-center">
                <Icon :name="status.icon" :size="24" class="mr-3" :class="status.colorClass" />
                <div>
                  <p class="text-sm font-medium text-gray-900">{{ status.label }}</p>
                  <p class="text-xs text-gray-500 hidden sm:block">{{ status.description }}</p>
                </div>
              </div>
              <div class="text-right">
                <p class="text-xl sm:text-2xl font-bold" :class="status.colorClass">{{ status.count }}</p>
                <p class="text-xs text-gray-500 hidden sm:block">pedidos</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="card">
          <h2 class="text-lg sm:text-xl font-semibold mb-6 flex items-center">
            <Icon name="info" :size="20" class="mr-2" />
            Información Rápida
          </h2>
          
          <div class="space-y-4">
            <div class="flex items-start p-3 bg-blue-50 rounded-lg">
              <Icon name="dollar" :size="20" class="text-blue-600 mt-0.5 mr-3 flex-shrink-0" />
              <div>
                <p class="text-sm font-medium text-blue-900">Promedio por Pedido</p>
                <p class="text-lg font-bold text-blue-600">${{ formatCurrency(averageOrderValue) }}</p>
              </div>
            </div>
            
            <div class="flex items-start p-3 bg-purple-50 rounded-lg">
              <Icon name="image" :size="20" class="text-purple-600 mt-0.5 mr-3 flex-shrink-0" />
              <div>
                <p class="text-sm font-medium text-purple-900">Fotos por Pedido</p>
                <p class="text-lg font-bold text-purple-600">{{ averagePhotosPerOrder }}</p>
              </div>
            </div>
            
            <div class="flex items-start p-3 bg-green-50 rounded-lg">
              <Icon name="check-circle" :size="20" class="text-green-600 mt-0.5 mr-3 flex-shrink-0" />
              <div>
                <p class="text-sm font-medium text-green-900">Tasa de Conversión</p>
                <p class="text-lg font-bold text-green-600">{{ conversionRate }}%</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAdminStore } from '@/stores/admin'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import Icon from '@/components/Icon.vue'
import ordersService from '@/services/ordersService'
import adminService from '@/services/adminService'

const adminStore = useAdminStore()

const loading = ref(true)
const allOrders = ref([])
const googlePhotosConnected = ref(true)
const checkingGoogle = ref(true)

const stats = computed(() => {
  const total = allOrders.value.length
  const pending = allOrders.value.filter(o => 
    o.status === 'Pending' || o.status === 'AwaitingPayment'
  ).length
  const confirmed = allOrders.value.filter(o => 
    o.status === 'PaymentConfirmed' || o.status === 'Processing' || o.status === 'Completed'
  )
  const revenue = confirmed.reduce((sum, o) => sum + o.totalAmount, 0)
  const totalPhotos = allOrders.value.reduce((sum, o) => sum + (o.photos?.length || 0), 0)
  
  return {
    totalOrders: total,
    pendingOrders: pending,
    totalRevenue: revenue,
    confirmedOrders: confirmed.length,
    totalPhotos: totalPhotos
  }
})

const recentOrders = computed(() => {
  return [...allOrders.value]
    .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
    .slice(0, 10)
})

const orderStatusSummary = computed(() => {
  return [
    {
      key: 'Pending',
      label: 'Pendiente',
      description: 'Recién creados',
      icon: 'clock',
      count: allOrders.value.filter(o => o.status === 'Pending').length,
      colorClass: 'text-yellow-600'
    },
    {
      key: 'AwaitingPayment',
      label: 'Esperando Pago',
      description: 'Pago en proceso',
      icon: 'dollar',
      count: allOrders.value.filter(o => o.status === 'AwaitingPayment').length,
      colorClass: 'text-orange-600'
    },
    {
      key: 'PaymentConfirmed',
      label: 'Pagado',
      description: 'Pago confirmado',
      icon: 'check-circle',
      count: allOrders.value.filter(o => o.status === 'PaymentConfirmed').length,
      colorClass: 'text-green-600'
    },
    {
      key: 'Processing',
      label: 'Procesando',
      description: 'En preparación',
      icon: 'settings',
      count: allOrders.value.filter(o => o.status === 'Processing').length,
      colorClass: 'text-blue-600'
    },
    {
      key: 'Completed',
      label: 'Completado',
      description: 'Entregados',
      icon: 'check',
      count: allOrders.value.filter(o => o.status === 'Completed').length,
      colorClass: 'text-purple-600'
    },
    {
      key: 'Cancelled',
      label: 'Cancelado',
      description: 'Cancelados',
      icon: 'x-circle',
      count: allOrders.value.filter(o => o.status === 'Cancelled').length,
      colorClass: 'text-red-600'
    }
  ]
})

const averageOrderValue = computed(() => {
  if (allOrders.value.length === 0) return 0
  const total = allOrders.value.reduce((sum, o) => sum + o.totalAmount, 0)
  return total / allOrders.value.length
})

const averagePhotosPerOrder = computed(() => {
  if (allOrders.value.length === 0) return 0
  const totalPhotos = allOrders.value.reduce((sum, o) => sum + (o.photos?.length || 0), 0)
  return Math.round(totalPhotos / allOrders.value.length)
})

const conversionRate = computed(() => {
  if (allOrders.value.length === 0) return 0
  const paidOrders = allOrders.value.filter(o => 
    o.status === 'PaymentConfirmed' || o.status === 'Processing' || o.status === 'Completed'
  ).length
  return Math.round((paidOrders / allOrders.value.length) * 100)
})

async function loadDashboardData() {
  try {
    loading.value = true
    
    // Cargar órdenes
    allOrders.value = await ordersService.getAllOrders()
    
    // Verificar conexión de Google Photos
    checkingGoogle.value = true
    try {
      const settingsResponse = await adminService.getPhotographerSettings()
      if (settingsResponse.success && settingsResponse.data) {
        googlePhotosConnected.value = settingsResponse.data.isGoogleAuthenticated || false
      }
    } catch (err) {
      console.error('Error checking Google Photos connection:', err)
      googlePhotosConnected.value = false
    } finally {
      checkingGoogle.value = false
    }
  } catch (err) {
    console.error('Error loading dashboard data:', err)
  } finally {
    loading.value = false
  }
}

function formatDate(dateString) {
  if (!dateString) return 'N/A'
  const date = new Date(dateString)
  return date.toLocaleDateString('es-CL', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatCurrency(amount) {
  if (!amount) return '0'
  return Math.round(amount).toLocaleString('es-CL')
}

function getStatusText(status) {
  const statusMap = {
    'Pending': 'Pendiente',
    'AwaitingPayment': 'Esperando Pago',
    'PaymentConfirmed': 'Pagado',
    'Processing': 'Procesando',
    'Completed': 'Completado',
    'Cancelled': 'Cancelado'
  }
  return statusMap[status] || status
}

function getStatusClass(status) {
  const classMap = {
    'Pending': 'bg-yellow-100 text-yellow-800 border border-yellow-200',
    'AwaitingPayment': 'bg-orange-100 text-orange-800 border border-orange-200',
    'PaymentConfirmed': 'bg-green-100 text-green-800 border border-green-200',
    'Processing': 'bg-blue-100 text-blue-800 border border-blue-200',
    'Completed': 'bg-purple-100 text-purple-800 border border-purple-200',
    'Cancelled': 'bg-red-100 text-red-800 border border-red-200'
  }
  return classMap[status] || 'bg-gray-100 text-gray-800 border border-gray-200'
}

onMounted(() => {
  loadDashboardData()
})
</script>
