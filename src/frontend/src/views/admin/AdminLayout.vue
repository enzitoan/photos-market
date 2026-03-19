<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Mobile Header -->
    <div class="lg:hidden bg-white shadow-md sticky top-0 z-30">
      <div class="flex items-center justify-between p-4">
        <h1 class="text-xl font-bold text-primary-600">Panel Admin</h1>
        <button 
          @click="toggleSidebar"
          class="p-2 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <Icon :name="isSidebarOpen ? 'x' : 'menu'" :size="24" />
        </button>
      </div>
    </div>

    <div class="flex">
      <!-- Sidebar Overlay for Mobile -->
      <div 
        v-if="isSidebarOpen"
        @click="closeSidebar"
        class="fixed inset-0 bg-black bg-opacity-50 z-40 lg:hidden"
      ></div>

      <!-- Sidebar -->
      <aside 
        :class="[
          'bg-white shadow-lg transition-transform duration-300 ease-in-out z-50',
          'lg:relative lg:translate-x-0',
          'fixed inset-y-0 left-0 w-64',
          isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
        ]"
      >
        <!-- Desktop Header -->
        <div class="hidden lg:block p-6 border-b">
          <h1 class="text-2xl font-bold text-primary-600">Panel Admin</h1>
          <p class="text-sm text-gray-600 mt-1">Fotógrafo</p>
        </div>
        
        <nav class="p-4">
          <router-link 
            v-for="item in menuItems" 
            :key="item.path"
            :to="item.path"
            @click="closeSidebar"
            class="flex items-center px-4 py-3 mb-2 rounded-lg transition-colors"
            :class="isActive(item.path) ? 'bg-primary-100 text-primary-700 font-medium' : 'text-gray-700 hover:bg-gray-100'"
          >
            <Icon :name="item.icon" :size="20" class="mr-3" />
            <span>{{ item.label }}</span>
          </router-link>
        </nav>
        
        <div class="absolute bottom-0 w-64 p-4 border-t">
          <router-link 
            to="/" 
            @click="closeSidebar"
            class="flex items-center text-gray-600 hover:text-gray-900 transition-colors"
          >
            <Icon name="arrow-left" :size="18" class="mr-2" />
            <span>Volver al sitio</span>
          </router-link>
        </div>
      </aside>
      
      <!-- Main Content -->
      <main class="flex-1 overflow-y-auto min-h-screen">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-6 lg:py-8">
          <router-view />
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import Icon from '@/components/Icon.vue'

const route = useRoute()
const isSidebarOpen = ref(false)

const menuItems = [
  { path: '/admin/dashboard', label: 'Dashboard', icon: 'dashboard' },
  { path: '/admin/orders', label: 'Pedidos', icon: 'package' },
  { path: '/admin/albums', label: 'Álbumes', icon: 'camera' },
  { path: '/admin/settings', label: 'Configuración', icon: 'settings' }
]

function isActive(path) {
  if (path === '/admin/dashboard') {
    return route.path === '/admin' || route.path === '/admin/dashboard'
  }
  return route.path.startsWith(path)
}

function toggleSidebar() {
  isSidebarOpen.value = !isSidebarOpen.value
}

function closeSidebar() {
  isSidebarOpen.value = false
}
</script>
