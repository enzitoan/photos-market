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
  const bulkDiscountMinPhotos = ref(5) // Mínimo de fotos para descuento
  const bulkDiscountPercentage = ref(20) // Porcentaje de descuento
  
  const totalItems = computed(() => items.value.length)
  
  // Subtotal sin descuento
  const subtotal = computed(() => items.value.length * pricePerPhoto.value)
  
  // Descuento aplicable
  const discountPercentage = computed(() => {
    return totalItems.value >= bulkDiscountMinPhotos.value ? bulkDiscountPercentage.value : 0
  })
  
  const discountAmount = computed(() => {
    if (discountPercentage.value > 0) {
      return subtotal.value * (discountPercentage.value / 100)
    }
    return 0
  })
  
  // Total con descuento
  const totalAmount = computed(() => subtotal.value - discountAmount.value)
  
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
        const minPhotos = response.data.data.bulkDiscountMinPhotos || response.data.data.BulkDiscountMinPhotos || 5
        const discountPct = response.data.data.bulkDiscountPercentage || response.data.data.BulkDiscountPercentage || 20
        
        pricePerPhoto.value = price
        currency.value = curr
        bulkDiscountMinPhotos.value = minPhotos
        bulkDiscountPercentage.value = discountPct
        
        // Persistir en localStorage
        localStorage.setItem('photoPrice', price.toString())
        localStorage.setItem('currency', curr)
        localStorage.setItem('bulkDiscountMinPhotos', minPhotos.toString())
        localStorage.setItem('bulkDiscountPercentage', discountPct.toString())
        
        configLoaded.value = true
        console.log('✅ Configuración cargada desde backend:', { 
          price, 
          currency: curr, 
          minPhotos, 
          discountPct 
        })
      }
    } catch (error) {
      console.error('❌ Error loading config:', error)
      // Intentar cargar desde localStorage como fallback
      const storedPrice = localStorage.getItem('photoPrice')
      const storedCurrency = localStorage.getItem('currency')
      const storedMinPhotos = localStorage.getItem('bulkDiscountMinPhotos')
      const storedDiscountPct = localStorage.getItem('bulkDiscountPercentage')
      
      if (storedPrice) {
        pricePerPhoto.value = parseFloat(storedPrice)
        console.log('📦 Precio cargado desde localStorage:', pricePerPhoto.value)
      }
      if (storedCurrency) {
        currency.value = storedCurrency
        console.log('📦 Moneda cargada desde localStorage:', currency.value)
      }
      if (storedMinPhotos) {
        bulkDiscountMinPhotos.value = parseInt(storedMinPhotos)
        console.log('📦 Descuento mínimo cargado desde localStorage:', bulkDiscountMinPhotos.value)
      }
      if (storedDiscountPct) {
        bulkDiscountPercentage.value = parseFloat(storedDiscountPct)
        console.log('📦 Porcentaje de descuento cargado desde localStorage:', bulkDiscountPercentage.value)
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
    subtotal,
    discountPercentage,
    discountAmount,
    totalAmount,
    pricePerPhoto,
    currency,
    currencySymbol,
    configLoaded,
    bulkDiscountMinPhotos,
    bulkDiscountPercentage,
    addToCart,
    removeFromCart,
    clearCart,
    isInCart,
    loadConfig
  }
})
