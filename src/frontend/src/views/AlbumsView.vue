<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8 w-full">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6 sm:mb-8">
        <h1 class="text-2xl sm:text-3xl font-bold">Álbumes Disponibles</h1>
        <div v-if="!loading && !error && albums.length > 0" class="flex items-center gap-2">
          <label for="album-sort" class="text-sm text-gray-600">Ordenar por:</label>
          <select id="album-sort" v-model="sortBy" class="border border-gray-300 rounded-md px-3 py-1.5 text-sm bg-white">
            <option value="newest">Más recientes</option>
            <option value="name">Nombre (A-Z)</option>
          </select>
        </div>
      </div>
      
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
      
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
        <AlbumCard 
          v-for="album in sortedAlbums" 
          :key="album.id"
          :album="album"
          @click="goToAlbum(album.id)"
        />
      </div>
    </main>

    <!-- Botón para subir al inicio -->
    <transition name="fade">
      <button
        v-if="showScrollTop"
        @click="scrollToTop"
        class="fixed bottom-24 right-6 z-40 w-12 h-12 flex items-center justify-center bg-white text-primary-600 hover:bg-primary-50 border border-gray-200 rounded-full shadow-lg transition-colors"
        title="Subir al inicio"
        aria-label="Subir al inicio"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7" />
        </svg>
      </button>
    </transition>

    <!-- Floating Cart Button -->
    <FloatingCartButton />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import NavBar from '@/components/NavBar.vue'
import AlbumCard from '@/components/AlbumCard.vue'
import FloatingCartButton from '@/components/FloatingCartButton.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import photosService from '@/services/photosService'

const router = useRouter()
const toast = useToast()

const loading = ref(true)
const error = ref(null)
const albums = ref([])
const sortBy = ref('newest')
const showScrollTop = ref(false)

function handleScroll() {
  showScrollTop.value = window.scrollY > 300
}

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const sortedAlbums = computed(() => {
  const list = [...albums.value]
  const byName = (a, b) => (a.title || '').localeCompare(b.title || '', 'es', { sensitivity: 'base', numeric: true })

  if (sortBy.value === 'name') {
    return list.sort(byName)
  }

  // Más recientes primero; los álbumes sin fecha van al final, ordenados por nombre
  return list.sort((a, b) => {
    const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0
    const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0
    return dateB - dateA || byName(a, b)
  })
})

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
  window.addEventListener('scroll', handleScroll, { passive: true })
  handleScroll()
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
