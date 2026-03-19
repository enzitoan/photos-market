<template>
  <nav class="bg-white shadow-lg">
    <div class="max-w-7xl mx-auto px-2 sm:px-4 lg:px-8">
      <div class="flex justify-between h-14 sm:h-16">
        <div class="flex items-center">
          <router-link to="/" class="flex items-center">
            <Icon name="camera" :size="24" class="text-primary-600 sm:mr-2" />
            <span class="text-lg sm:text-2xl font-bold text-primary-600 hidden sm:inline">PhotosMarket</span>
          </router-link>
        </div>
        
        <div class="flex items-center space-x-1 sm:space-x-2 md:space-x-4">
          <template v-if="authStore.isAuthenticated">
            <router-link 
              to="/albums" 
              class="text-gray-700 hover:text-primary-600 px-2 sm:px-3 py-2 rounded-md text-xs sm:text-sm font-medium flex items-center"
            >
              <span class="hidden sm:inline">Álbumes</span>
              <Icon name="image" :size="18" class="sm:hidden" />
            </router-link>
            
            <router-link 
              to="/orders" 
              class="text-gray-700 hover:text-primary-600 px-2 sm:px-3 py-2 rounded-md text-xs sm:text-sm font-medium flex items-center"
            >
              <span class="hidden sm:inline">Mis Pedidos</span>
              <Icon name="file-text" :size="18" class="sm:hidden" />
            </router-link>
            
            <CartIcon />
            
            <router-link 
              v-if="authStore.isAdmin"
              to="/admin" 
              class="bg-primary-600 text-white hover:bg-primary-700 px-2 sm:px-3 py-2 rounded-md text-xs sm:text-sm font-medium flex items-center"
            >
              <span class="hidden sm:inline">Panel Admin</span>
              <Icon name="settings" :size="18" class="sm:hidden" />
            </router-link>
            
            <div class="relative" ref="dropdown">
              <button 
                @click="showDropdown = !showDropdown"
                class="flex items-center text-gray-700 hover:text-primary-600 px-1 sm:px-2"
              >
                <span class="mr-1 sm:mr-2 text-xs sm:text-sm truncate max-w-[80px] sm:max-w-none">{{ authStore.user?.name || 'Usuario' }}</span>
                <Icon name="chevron-down" :size="16" />
              </button>
              
              <div 
                v-if="showDropdown"
                class="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-50"
              >
                <button 
                  @click="handleLogout"
                  class="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center"
                >
                  <Icon name="logout" :size="16" class="mr-2" />
                  Cerrar Sesión
                </button>
              </div>
            </div>
          </template>
          
          <template v-else>
            <router-link 
              to="/login" 
              class="bg-primary-600 text-white hover:bg-primary-700 px-3 sm:px-4 py-2 rounded-md text-xs sm:text-sm font-medium"
            >
              <span class="hidden sm:inline">Iniciar Sesión</span>
              <span class="sm:hidden">Entrar</span>
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
import Icon from './Icon.vue'

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
