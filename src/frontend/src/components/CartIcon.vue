<template>
  <router-link 
    to="/cart" 
    class="relative inline-flex items-center text-gray-700 hover:text-primary-600 px-2 sm:px-3 py-2 rounded-md text-xs sm:text-sm font-medium transition-colors"
    :title="`${cartStore.totalItems} fotos en el carrito`"
  >
    <Icon name="shopping-cart" :size="20" />
    
    <span class="ml-1 hidden sm:inline">Carrito</span>
    
    <span 
      v-if="cartStore.totalItems > 0" 
      class="absolute -top-1 -right-1 bg-red-500 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs font-bold animate-pulse"
    >
      {{ cartStore.totalItems > 99 ? '99+' : cartStore.totalItems }}
    </span>
  </router-link>
</template>

<script setup>
import { useCartStore } from '@/stores/cart'
import Icon from './Icon.vue'

const cartStore = useCartStore()
</script>

<style scoped>
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}

.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
