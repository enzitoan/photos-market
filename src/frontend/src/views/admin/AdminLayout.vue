<template>
  <div class="min-h-screen bg-gray-100 flex">
    <!-- Sidebar -->
    <aside class="w-64 bg-white shadow-lg">
      <div class="p-6 border-b">
        <h1 class="text-2xl font-bold text-primary-600">Panel Admin</h1>
        <p class="text-sm text-gray-600 mt-1">Fotógrafo</p>
      </div>
      
      <nav class="p-4">
        <router-link 
          v-for="item in menuItems" 
          :key="item.path"
          :to="item.path"
          class="flex items-center px-4 py-3 mb-2 rounded-lg transition-colors"
          :class="isActive(item.path) ? 'bg-primary-100 text-primary-700 font-medium' : 'text-gray-700 hover:bg-gray-100'"
        >
          <span class="text-xl mr-3">{{ item.icon }}</span>
          <span>{{ item.label }}</span>
        </router-link>
      </nav>
      
      <div class="absolute bottom-0 w-64 p-4 border-t">
        <router-link to="/" class="flex items-center text-gray-600 hover:text-gray-900">
          <span class="mr-2">←</span>
          <span>Volver al sitio</span>
        </router-link>
      </div>
    </aside>
    
    <!-- Main Content -->
    <main class="flex-1 overflow-y-auto">
      <div class="max-w-7xl mx-auto px-6 py-8">
        <router-view />
      </div>
    </main>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const menuItems = [
  { path: '/admin/dashboard', label: 'Dashboard', icon: '📊' },
  { path: '/admin/orders', label: 'Pedidos', icon: '📦' },
  { path: '/admin/albums', label: 'Álbumes', icon: '📸' },
  { path: '/admin/settings', label: 'Configuración', icon: '⚙️' }
]

function isActive(path) {
  // Para la ruta del dashboard
  if (path === '/admin/dashboard') {
    return route.path === '/admin' || route.path === '/admin/dashboard'
  }
  return route.path.startsWith(path)
}
</script>
