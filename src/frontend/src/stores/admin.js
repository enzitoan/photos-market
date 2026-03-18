import { defineStore } from 'pinia'
import { ref } from 'vue'
import axios from 'axios'

import { getApiUrl } from '@/utils/env'

const API_URL = getApiUrl()

export const useAdminStore = defineStore('admin', () => {
  const watermarkText = ref(localStorage.getItem('watermarkText') || 'PhotosMarket © 2026')
  const watermarkOpacity = ref(parseFloat(localStorage.getItem('watermarkOpacity')) || 0.5)
  const blockedAlbums = ref(JSON.parse(localStorage.getItem('blockedAlbums')) || [])
  
  // Cargar configuración desde el backend
  async function loadConfig() {
    try {
      const response = await axios.get(`${API_URL}/api/photos/config`)
      if (response.data?.success && response.data?.data) {
        const config = response.data.data
        // El backend serializa en camelCase por defecto
        watermarkText.value = config.watermarkText || config.WatermarkText || 'PhotosMarket © 2026'
        watermarkOpacity.value = config.watermarkOpacity !== undefined ? config.watermarkOpacity : (config.WatermarkOpacity !== undefined ? config.WatermarkOpacity : 0.5)
        
        // Guardar en localStorage
        localStorage.setItem('watermarkText', watermarkText.value)
        localStorage.setItem('watermarkOpacity', watermarkOpacity.value.toString())
        
        console.log('✅ Admin config loaded:', { watermarkText: watermarkText.value, watermarkOpacity: watermarkOpacity.value })
      }
    } catch (error) {
      console.error('❌ Error loading admin config:', error)
    }
  }
  
  function setWatermarkText(text) {
    watermarkText.value = text
    localStorage.setItem('watermarkText', text)
  }
  
  function setWatermarkOpacity(opacity) {
    watermarkOpacity.value = opacity
    localStorage.setItem('watermarkOpacity', opacity.toString())
  }
  
  function blockAlbum(albumId) {
    if (!blockedAlbums.value.includes(albumId)) {
      blockedAlbums.value.push(albumId)
      saveBlockedAlbums()
    }
  }
  
  function unblockAlbum(albumId) {
    blockedAlbums.value = blockedAlbums.value.filter(id => id !== albumId)
    saveBlockedAlbums()
  }
  
  function isAlbumBlocked(albumId) {
    return blockedAlbums.value.includes(albumId)
  }
  
  function saveBlockedAlbums() {
    localStorage.setItem('blockedAlbums', JSON.stringify(blockedAlbums.value))
  }
  
  // Cargar configuración al inicializar
  loadConfig()
  
  return {
    watermarkText,
    watermarkOpacity,
    blockedAlbums,
    setWatermarkText,
    setWatermarkOpacity,
    blockAlbum,
    unblockAlbum,
    isAlbumBlocked,
    loadConfig
  }
})
