<template>
  <div class="flex items-center justify-center p-8">
    <div class="text-center">
      <div class="spinner mx-auto"></div>
      <p v-if="message" class="mt-4 text-gray-600">{{ message }}</p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  message: {
    type: String,
    default: 'Cargando...'
  }
})
</script>
