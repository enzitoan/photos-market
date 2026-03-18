<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

onMounted(() => {
  // Restaurar sesión si existe token
  authStore.initializeAuth()
})
</script>

<style scoped>
#app {
  width: 100%;
  min-height: 100vh;
}
</style>
