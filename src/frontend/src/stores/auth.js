import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/services/authService'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || null)
  const googleAccessToken = ref(localStorage.getItem('googleAccessToken') || null)
  
  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => {
    // Determinar si es admin basado en la respuesta del servidor
    return user.value?.isAdmin === true
  })
  
  async function login(code) {
    try {
      const response = await authService.googleCallback(code)
      
      if (response.success && response.data) {
        user.value = {
          userId: response.data.userId,
          email: response.data.email,
          name: response.data.name,
          isAdmin: response.data.isAdmin || false
        }
        token.value = response.data.token
        
        localStorage.setItem('token', response.data.token)
        localStorage.setItem('user', JSON.stringify(user.value))
        
        return true
      }
      return false
    } catch (error) {
      console.error('Login error:', error)
      return false
    }
  }

  async function photographerLogin(code) {
    try {
      const response = await authService.photographerGoogleCallback(code)
      
      if (response.success && response.data) {
        user.value = {
          userId: response.data.userId,
          email: response.data.email,
          name: response.data.name,
          isAdmin: response.data.isAdmin || false
        }
        token.value = response.data.token
        
        localStorage.setItem('token', response.data.token)
        localStorage.setItem('user', JSON.stringify(user.value))
        
        return true
      }
      return false
    } catch (error) {
      console.error('Photographer login error:', error)
      throw error
    }
  }

  async function adminLogin(username, password) {
    try {
      const response = await authService.adminLogin(username, password)
      
      if (response.success && response.data) {
        user.value = {
          userId: response.data.userId,
          email: response.data.email,
          name: response.data.name,
          isAdmin: response.data.isAdmin || false
        }
        token.value = response.data.token
        
        localStorage.setItem('token', response.data.token)
        localStorage.setItem('user', JSON.stringify(user.value))
        
        return true
      }
      return false
    } catch (error) {
      console.error('Admin login error:', error)
      return false
    }
  }
  
  function setGoogleAccessToken(accessToken) {
    googleAccessToken.value = accessToken
    localStorage.setItem('googleAccessToken', accessToken)
  }
  
  function logout() {
    user.value = null
    token.value = null
    googleAccessToken.value = null
    
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('googleAccessToken')
  }
  
  function initializeAuth() {
    const storedUser = localStorage.getItem('user')
    if (storedUser && token.value) {
      try {
        user.value = JSON.parse(storedUser)
      } catch (e) {
        logout()
      }
    }
  }
  
  return {
    user,
    token,
    googleAccessToken,
    isAuthenticated,
    isAdmin,
    login,
    photographerLogin,
    adminLogin,
    logout,
    setGoogleAccessToken,
    initializeAuth
  }
})
