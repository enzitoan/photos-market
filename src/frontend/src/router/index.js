import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue')
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/LoginView.vue')
    },
    {
      path: '/callback',
      name: 'callback',
      component: () => import('@/views/auth/CallbackView.vue')
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/auth/RegisterView.vue')
    },
    {
      path: '/albums',
      name: 'albums',
      component: () => import('@/views/AlbumsView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/albums/:id',
      name: 'album-photos',
      component: () => import('@/views/AlbumPhotosView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/cart',
      name: 'cart',
      component: () => import('@/views/CartView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/orders',
      name: 'orders',
      component: () => import('@/views/OrdersView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/orders/:id',
      name: 'order-detail',
      component: () => import('@/views/OrderDetailView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/download/:token',
      name: 'download',
      component: () => import('@/views/DownloadView.vue')
    },
    {
      path: '/admin/login',
      name: 'admin-login',
      component: () => import('@/views/admin/AdminLoginView.vue')
    },
    {
      path: '/admin/google-auth',
      name: 'admin-google-auth',
      component: () => import('@/views/admin/GoogleAuthView.vue'),
      meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
      path: '/admin',
      component: () => import('@/views/admin/AdminLayout.vue'),
      meta: { requiresAuth: true, requiresAdmin: true },
      children: [
        {
          path: '',
          redirect: { name: 'admin-dashboard' }
        },
        {
          path: 'dashboard',
          name: 'admin-dashboard',
          component: () => import('@/views/admin/DashboardView.vue')
        },
        {
          path: 'albums',
          name: 'admin-albums',
          component: () => import('@/views/admin/AlbumsManagementView.vue')
        },
        {
          path: 'orders',
          name: 'admin-orders',
          component: () => import('@/views/admin/OrdersManagementView.vue')
        },
        {
          path: 'settings',
          name: 'admin-settings',
          component: () => import('@/views/admin/SettingsView.vue')
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue')
    }
  ]
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Si requiere autenticación y no está autenticado
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Si es una ruta de admin, redirigir al login de admin
    if (to.meta.requiresAdmin) {
      next({ name: 'admin-login' })
    } else {
      // Si es una ruta de cliente, redirigir al login normal
      next({ name: 'login' })
    }
  } 
  // Si requiere admin y no es admin
  else if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'admin-login' })
  } 
  else {
    next()
  }
})

export default router
