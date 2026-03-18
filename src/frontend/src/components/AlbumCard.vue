<template>
  <div class="album-card card card-hover cursor-pointer" @click="$emit('select', album)">
    <div class="relative aspect-video bg-gray-200 rounded-t-lg overflow-hidden">
      <img 
        v-if="album.coverPhotoUrl"
        :src="album.coverPhotoUrl" 
        :alt="album.title"
        class="w-full h-full object-cover"
        @error="handleImageError"
      />
      <div v-else class="w-full h-full flex items-center justify-center">
        <svg class="w-16 h-16 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clip-rule="evenodd"/>
        </svg>
      </div>
      
      <!-- Badge de bloqueado -->
      <div v-if="isBlocked" class="absolute top-2 right-2 badge badge-danger">
        🔒 Bloqueado
      </div>
      
      <!-- Contador de fotos -->
      <div class="absolute bottom-2 right-2 bg-black bg-opacity-70 text-white px-2 py-1 rounded text-sm">
        {{ album.mediaItemsCount }} fotos
      </div>
    </div>
    
    <div class="p-4">
      <h3 class="text-lg font-semibold text-gray-800 truncate">{{ album.title }}</h3>
      <p v-if="isBlocked" class="text-sm text-red-600 mt-1">
        Este álbum no está disponible actualmente
      </p>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAdminStore } from '@/stores/admin'

const props = defineProps({
  album: {
    type: Object,
    required: true
  }
})

defineEmits(['select'])

const adminStore = useAdminStore()

const isBlocked = computed(() => adminStore.isAlbumBlocked(props.album.id))

function handleImageError(event) {
  // Prevenir bucle infinito: si ya intentamos cargar el fallback, no hacer nada
  if (!event.target || event.target.dataset.errorHandled === 'true') {
    return
  }
  
  // Marcar que ya manejamos este error
  event.target.dataset.errorHandled = 'true'
  
  // Remover el listener para evitar errores adicionales
  event.target.removeEventListener('error', handleImageError)
  
  // Intentar cargar imagen de placeholder
  event.target.src = 'https://via.placeholder.com/600x400?text=Álbum'
}
</script>

<style scoped>
.album-card:hover {
  transform: translateY(-4px);
}
</style>
