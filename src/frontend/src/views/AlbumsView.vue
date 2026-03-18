<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 w-full">
      <h1 class="text-3xl font-bold mb-8">Álbumes Disponibles</h1>
      
      <LoadingSpinner v-if="loading" message="Cargando álbumes..." />
      
      <div v-else-if="error" class="text-center py-12">
        <p class="text-red-600">{{ error }}</p>
        <button @click="loadAlbums" class="btn btn-primary mt-4">
          Reintentar
        </button>
      </div>
      
      <div v-else-if="albums.length === 0" class="text-center py-12">
        <p class="text-gray-600 text-lg">No hay álbumes disponibles en este momento</p>
      </div>
      
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <AlbumCard 
          v-for="album in albums" 
          :key="album.id"
          :album="album"
          @click="goToAlbum(album.id)"
        />
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import NavBar from '@/components/NavBar.vue'
import AlbumCard from '@/components/AlbumCard.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import photosService from '@/services/photosService'

const router = useRouter()
const toast = useToast()

const loading = ref(true)
const error = ref(null)
const albums = ref([])

async function loadAlbums() {
  try {
    loading.value = true
    error.value = null
    const response = await photosService.getAlbums()
    
    // El httpClient devuelve response.data que es { success: true, data: [...] }
    // Necesitamos acceder a response.data para obtener el array de álbumes
    const albumsData = response?.data || []
    
    // Filtrar álbumes válidos (que no sean null y tengan id)
    albums.value = albumsData.filter(album => album && album.id)
    
    if (albums.value.length === 0) {
      console.warn('No se encontraron álbumes válidos')
    }
  } catch (err) {
    console.error('Error loading albums:', err)
    error.value = 'Error al cargar los álbumes. Intenta nuevamente.'
    toast.error('Error al cargar los álbumes')
  } finally {
    loading.value = false
  }
}

function goToAlbum(albumId) {
  router.push(`/albums/${albumId}`)
}

onMounted(() => {
  loadAlbums()
})
</script>
