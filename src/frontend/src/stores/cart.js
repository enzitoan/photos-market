import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import axios from 'axios'

import { getApiUrl } from '@/utils/env'

const API_URL = getApiUrl()

export const useCartStore = defineStore('cart', () => {
  const items = ref(JSON.parse(localStorage.getItem('cart')) || [])
  const pricePerPhoto = ref(5.00) // Default inicial
  const currency = ref('CLP') // Default inicial
  const configLoaded = ref(false)
  
  const totalItems = computed(() => items.value.length)
  const totalAmount = computed(() => items.value.length * pricePerPhoto.value)
  
  const currencySymbol = computed(() => {
    const symbols = {
      'CLP': '$',
      'USD': '$',
      'EUR': '€',
      'ARS': '$',
      'MXN': '$',
      'BRL': 'R$'
    }
    return symbols[currency.value] || '$'
  })
  
  // Cargar configuración desde el backend
  async function loadConfig() {
    try {
      console.log('🔄 Cargando configuración del precio...')
      const response = await axios.get(`${API_URL}/api/photos/config`)
      console.log('📥 Respuesta del backend:', response.data)
      
      if (response.data?.success && response.data?.data) {
        // El backend serializa en camelCase por defecto
        const price = response.data.data.photoPrice || response.data.data.PhotoPrice || 5.00
        const curr = response.data.data.currency || response.data.data.Currency || 'CLP'
        pricePerPhoto.value = price
        currency.value = curr
        // Persistir en localStorage
        localStorage.setItem('photoPrice', price.toString())
        localStorage.setItem('currency', curr)
        configLoaded.value = true
        console.log('✅ Configuración cargada desde backend:', { price, currency: curr })
      }
    } catch (error) {
      console.error('❌ Error loading config:', error)
      // Intentar cargar desde localStorage como fallback
      const storedPrice = localStorage.getItem('photoPrice')
      const storedCurrency = localStorage.getItem('currency')
      if (storedPrice) {
        pricePerPhoto.value = parseFloat(storedPrice)
        console.log('📦 Precio cargado desde localStorage:', pricePerPhoto.value)
      }
      if (storedCurrency) {
        currency.value = storedCurrency
        console.log('📦 Moneda cargada desde localStorage:', currency.value)
      }
    }
  }
  
  function addToCart(photo) {
    const exists = items.value.find(item => item.id === photo.id)
    if (!exists) {
      items.value.push({
        id: photo.id,
        mediaItemId: photo.mediaItemId,
        filename: photo.filename,
        thumbnailUrl: photo.thumbnailUrl,
        baseUrl: photo.baseUrl,
        price: pricePerPhoto.value,
        albumId: photo.albumId || null,
        albumTitle: photo.albumTitle || null
      })
      saveCart()
    }
  }
  
  function removeFromCart(photoId) {
    items.value = items.value.filter(item => item.id !== photoId)
    saveCart()
  }
  
  function clearCart() {
    items.value = []
    saveCart()
  }
  
  function isInCart(photoId) {
    return items.value.some(item => item.id === photoId)
  }
  
  function saveCart() {
    localStorage.setItem('cart', JSON.stringify(items.value))
  }
  
  // Cargar configuración desde backend de forma asíncrona
  loadConfig()
  
  return {
    items,
    totalItems,
    totalAmount,
    pricePerPhoto,
    currency,
    currencySymbol,
    configLoaded,
    addToCart,
    removeFromCart,
    clearCart,
    isInCart,
    loadConfig
  }
})
