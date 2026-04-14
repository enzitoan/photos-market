<template>
  <div
    v-if="cartStore.totalItems > 0"
    class="fixed bottom-6 right-6 z-50"
  >
    <router-link
      to="/cart"
      class="group relative flex items-center justify-center w-14 h-14 bg-primary-600 hover:bg-primary-700 text-white rounded-full shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-105"
      :title="`${cartStore.totalItems} fotos en el carrito`"
    >
      <Icon name="shopping-cart" :size="24" />

      <!-- Badge con contador -->
      <span
        class="absolute -top-2 -right-2 bg-red-500 text-white rounded-full min-w-[24px] h-6 flex items-center justify-center text-xs font-bold px-1 animate-pulse"
      >
        {{ cartStore.totalItems > 99 ? '99+' : cartStore.totalItems }}
      </span>

      <!-- Tooltip -->
      <div class="absolute bottom-full right-0 mb-2 px-3 py-1 bg-gray-800 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 transition-opacity duration-200 whitespace-nowrap">
        Ver carrito
        <div class="absolute top-full right-4 w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent border-t-gray-800"></div>
      </div>
    </router-link>
  </div>
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