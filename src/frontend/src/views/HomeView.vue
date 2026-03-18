<template>
  <div class="min-h-screen flex flex-col">
    <NavBar />
    
    <main class="flex-1">
      <!-- Hero Section -->
      <section class="bg-gradient-to-r from-primary-600 to-primary-800 text-white py-20">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h1 class="text-5xl font-bold mb-4">Bienvenido a PhotosMarket</h1>
          <p class="text-xl mb-8">Compra tus fotos favoritas en alta resolución</p>
          
          <router-link 
            v-if="!authStore.isAuthenticated"
            to="/login" 
            class="inline-block bg-white text-primary-600 px-8 py-3 rounded-lg text-lg font-semibold hover:bg-gray-100 transition-colors"
          >
            Comenzar Ahora
          </router-link>
          
          <router-link 
            v-else
            to="/albums" 
            class="inline-block bg-white text-primary-600 px-8 py-3 rounded-lg text-lg font-semibold hover:bg-gray-100 transition-colors"
          >
            Ver Álbumes
          </router-link>
        </div>
      </section>
      
      <!-- Features Section -->
      <section class="py-16 bg-white">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 class="text-3xl font-bold text-center mb-12">¿Cómo funciona?</h2>
          
          <div class="grid md:grid-cols-3 gap-8">
            <div class="text-center">
              <div class="text-5xl mb-4">🔐</div>
              <h3 class="text-xl font-semibold mb-2">1. Inicia Sesión</h3>
              <p class="text-gray-600">Autentica con tu cuenta de Google de forma segura</p>
            </div>
            
            <div class="text-center">
              <div class="text-5xl mb-4">📸</div>
              <h3 class="text-xl font-semibold mb-2">2. Explora y Selecciona</h3>
              <p class="text-gray-600">Navega por los álbumes y elige tus fotos favoritas</p>
            </div>
            
            <div class="text-center">
              <div class="text-5xl mb-4">💳</div>
              <h3 class="text-xl font-semibold mb-2">3. Compra y Descarga</h3>
              <p class="text-gray-600">Realiza el pago y una vez confirmado te enviamos el enlace de descarga</p>
            </div>
          </div>
        </div>
      </section>
      
      <!-- CTA Section -->
      <section class="bg-gray-50 py-16">
        <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 class="text-3xl font-bold mb-4">¿Listo para obtener tus fotos?</h2>
          <p class="text-xl text-gray-600 mb-8">
            Accede a tus recuerdos en máxima calidad
          </p>
          
          <router-link 
            v-if="!authStore.isAuthenticated"
            to="/login" 
            class="btn btn-primary text-lg px-8 py-3"
          >
            Iniciar Sesión con Google
          </router-link>
        </div>
      </section>

      <!-- Contact Section -->
      <section class="bg-white py-12">
        <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 class="text-2xl font-bold mb-8">Contáctanos</h2>
          
          <div class="flex justify-center items-center gap-8 flex-wrap">
            <!-- Instagram -->
            <a 
              href="https://instagram.com/egan.fotografia" 
              target="_blank"
              rel="noopener noreferrer"
              class="flex items-center gap-2 text-gray-700 hover:text-purple-600 transition-colors"
              title="Instagram"
            >
              <Instagram :size="20" />
              <span class="text-sm font-medium">@egan.fotografia</span>
            </a>
            
            <!-- Email -->
            <a 
              href="mailto:egan.fotografia.ph@gmail.com"
              class="flex items-center gap-2 text-gray-700 hover:text-blue-600 transition-colors"
              title="Email"
            >
              <Mail :size="20" />
              <span class="text-sm font-medium">egan.fotografia.ph@gmail.com</span>
            </a>
            
            <!-- WhatsApp -->
            <a 
              href="https://wa.me/56998977144" 
              target="_blank"
              rel="noopener noreferrer"
              class="flex items-center gap-2 text-gray-700 hover:text-green-600 transition-colors"
              title="WhatsApp"
            >
              <MessageCircle :size="20" />
              <span class="text-sm font-medium">+56 9 9897 7144</span>
            </a>
          </div>
        </div>
      </section>
    </main>
    
    <footer class="bg-gray-800 text-white py-8">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-4">
          <p>&copy; 2026 PhotosMarket. Todos los derechos reservados.</p>
        </div>
        <div class="text-center">
          <router-link 
            to="/admin/login" 
            class="text-gray-400 hover:text-white text-sm"
          >
            Acceso Administradores
          </router-link>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import NavBar from '@/components/NavBar.vue'
import { Instagram, Mail, MessageCircle } from 'lucide-vue-next'

const authStore = useAuthStore()
</script>
