<template>
  <div class="min-h-screen flex flex-col items-center justify-center bg-gray-50">
    <div class="text-center">
      <h1 class="text-9xl font-bold text-primary-600">404</h1>
      <p class="text-2xl font-semibold text-gray-800 mt-4">Página No Encontrada</p>
      <p class="text-gray-600 mt-2 mb-8">
        La página que buscas no existe o ha sido movida.
      </p>
      <router-link to="/" class="btn btn-primary">
        Volver al Inicio
      </router-link>
    </div>
  </div>
</template>

<script setup>
// No logic needed for this simple view
</script>
