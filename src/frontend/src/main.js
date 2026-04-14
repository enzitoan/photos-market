import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import './assets/main.css'
import { useAuthStore } from './stores/auth'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Toast, {
  position: 'top-right',
  timeout: 3000,
  closeOnClick: true,
  pauseOnFocusLoss: false, // Changed to false to prevent sticking on mobile
  pauseOnHover: false, // Changed to false for better mobile experience
  draggable: false, // Disabled for mobile to prevent accidental dragging
  showCloseButtonOnHover: false,
  hideProgressBar: false,
  closeButton: 'button',
  icon: true,
  rtl: false,
  // Mobile-specific adjustments
  toastClassName: 'vue-toast-mobile',
  bodyClassName: 'vue-toast-body-mobile'
})

// Inicializar el estado de autenticación desde localStorage
const authStore = useAuthStore()
authStore.initializeAuth()

app.mount('#app')
