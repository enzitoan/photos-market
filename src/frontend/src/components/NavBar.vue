<template>
  <nav class="bg-white shadow-lg">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between h-16">
        <div class="flex items-center">
          <router-link to="/" class="flex items-center">
            <span class="text-2xl font-bold text-primary-600">📸 PhotosMarket</span>
          </router-link>
        </div>
        
        <div class="flex items-center space-x-4">
          <template v-if="authStore.isAuthenticated">
            <router-link 
              to="/albums" 
              class="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium"
            >
              Álbumes
            </router-link>
            
            <router-link 
              to="/orders" 
              class="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium"
            >
              Mis Pedidos
            </router-link>
            
            <CartIcon />
            
            <router-link 
              v-if="authStore.isAdmin"
              to="/admin" 
              class="bg-primary-600 text-white hover:bg-primary-700 px-3 py-2 rounded-md text-sm font-medium"
            >
              Panel Admin
            </router-link>
            
            <div class="relative" ref="dropdown">
              <button 
                @click="showDropdown = !showDropdown"
                class="flex items-center text-gray-700 hover:text-primary-600"
              >
                <span class="mr-2">{{ authStore.user?.name || 'Usuario' }}</span>
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1  0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd"/>
                </svg>
              </button>
              
              <div 
                v-if="showDropdown"
                class="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-50"
              >
                <button 
                  @click="handleLogout"
                  class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                >
                  Cerrar Sesión
                </button>
              </div>
            </div>
          </template>
          
          <template v-else>
            <router-link 
              to="/login" 
              class="bg-primary-600 text-white hover:bg-primary-700 px-4 py-2 rounded-md text-sm font-medium"
            >
              Iniciar Sesión
            </router-link>
          </template>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import CartIcon from './CartIcon.vue'

const router = useRouter()
const authStore = useAuthStore()
const showDropdown = ref(false)
const dropdown = ref(null)

function handleLogout() {
  authStore.logout()
  router.push('/')
}

function handleClickOutside(event) {
  if (dropdown.value && !dropdown.value.contains(event.target)) {
    showDropdown.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>
