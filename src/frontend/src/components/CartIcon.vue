<template>
  <router-link 
    to="/cart" 
    class="relative inline-flex items-center text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors"
    :title="`${cartStore.totalItems} fotos en el carrito`"
  >
    <svg 
      class="w-6 h-6" 
      fill="none" 
      stroke="currentColor" 
      viewBox="0 0 24 24"
    >
      <path 
        stroke-linecap="round" 
        stroke-linejoin="round" 
        stroke-width="2" 
        d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
      />
    </svg>
    
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
