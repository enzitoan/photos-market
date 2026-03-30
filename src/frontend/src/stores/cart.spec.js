import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCartStore } from '@/stores/cart'
import axios from 'axios'

// Mock axios
vi.mock('axios')

describe('Cart Store', () => {
  beforeEach(() => {
    // Create a fresh pinia instance before each test
    setActivePinia(createPinia())
    // Clear localStorage
    localStorage.clear()
    // Reset axios mock
    vi.clearAllMocks()
  })

  it('should initialize with empty cart', () => {
    const cart = useCartStore()
    
    expect(cart.items).toEqual([])
    expect(cart.totalItems).toBe(0)
  })

  it('should add item to cart', () => {
    const cart = useCartStore()
    
    const photo = {
      id: 'photo1',
      mediaItemId: 'media1',
      filename: 'test.jpg',
      baseUrl: 'https://example.com/photo.jpg',
      albumId: 'album1',
      albumTitle: 'Test Album'
    }
    
    cart.addItem(photo)
    
    expect(cart.items).toHaveLength(1)
    expect(cart.totalItems).toBe(1)
    expect(cart.items[0].id).toBe('photo1')
  })

  it('should prevent duplicate items in cart', () => {
    const cart = useCartStore()
    
    const photo = {
      id: 'photo1',
      mediaItemId: 'media1',
      filename: 'test.jpg',
      baseUrl: 'https://example.com/photo.jpg',
      albumId: 'album1',
      albumTitle: 'Test Album'
    }
    
    cart.addItem(photo)
    cart.addItem(photo) // Try to add again
    
    expect(cart.items).toHaveLength(1)
    expect(cart.totalItems).toBe(1)
  })

  it('should calculate subtotal correctly', () => {
    const cart = useCartStore()
    cart.pricePerPhoto = 5000
    
    const photos = [
      { id: 'photo1', mediaItemId: 'media1', filename: 'test1.jpg', baseUrl: 'url1', albumId: 'album1', albumTitle: 'Album' },
      { id: 'photo2', mediaItemId: 'media2', filename: 'test2.jpg', baseUrl: 'url2', albumId: 'album1', albumTitle: 'Album' },
      { id: 'photo3', mediaItemId: 'media3', filename: 'test3.jpg', baseUrl: 'url3', albumId: 'album1', albumTitle: 'Album' }
    ]
    
    photos.forEach(photo => cart.addItem(photo))
    
    expect(cart.subtotal).toBe(15000)
  })

  it('should apply bulk discount when threshold is met', () => {
    const cart = useCartStore()
    cart.pricePerPhoto = 5000
    cart.bulkDiscountMinPhotos = 5
    cart.bulkDiscountPercentage = 20
    
    // Add 5 photos to trigger discount
    for (let i = 1; i <= 5; i++) {
      cart.addItem({
        id: `photo${i}`,
        mediaItemId: `media${i}`,
        filename: `test${i}.jpg`,
        baseUrl: `url${i}`,
        albumId: 'album1',
        albumTitle: 'Album'
      })
    }
    
    const subtotal = 25000 // 5 * 5000
    const expectedDiscount = subtotal * 0.20 // 5000
    const expectedTotal = subtotal - expectedDiscount // 20000
    
    expect(cart.subtotal).toBe(subtotal)
    expect(cart.discountPercentage).toBe(20)
    expect(cart.discountAmount).toBe(expectedDiscount)
    expect(cart.totalAmount).toBe(expectedTotal)
  })

  it('should not apply discount below threshold', () => {
    const cart = useCartStore()
    cart.pricePerPhoto = 5000
    cart.bulkDiscountMinPhotos = 5
    cart.bulkDiscountPercentage = 20
    
    // Add only 3 photos (below threshold)
    for (let i = 1; i <= 3; i++) {
      cart.addItem({
        id: `photo${i}`,
        mediaItemId: `media${i}`,
        filename: `test${i}.jpg`,
        baseUrl: `url${i}`,
        albumId: 'album1',
        albumTitle: 'Album'
      })
    }
    
    expect(cart.discountPercentage).toBe(0)
    expect(cart.discountAmount).toBe(0)
    expect(cart.totalAmount).toBe(cart.subtotal)
  })

  it('should remove item from cart', () => {
    const cart = useCartStore()
    
    const photo = {
      id: 'photo1',
      mediaItemId: 'media1',
      filename: 'test.jpg',
      baseUrl: 'https://example.com/photo.jpg',
      albumId: 'album1',
      albumTitle: 'Test Album'
    }
    
    cart.addItem(photo)
    expect(cart.items).toHaveLength(1)
    
    cart.removeItem('photo1')
    expect(cart.items).toHaveLength(0)
    expect(cart.totalItems).toBe(0)
  })

  it('should clear cart', () => {
    const cart = useCartStore()
    
    // Add multiple items
    for (let i = 1; i <= 3; i++) {
      cart.addItem({
        id: `photo${i}`,
        mediaItemId: `media${i}`,
        filename: `test${i}.jpg`,
        baseUrl: `url${i}`,
        albumId: 'album1',
        albumTitle: 'Album'
      })
    }
    
    expect(cart.items).toHaveLength(3)
    
    cart.clearCart()
    
    expect(cart.items).toHaveLength(0)
    expect(cart.totalItems).toBe(0)
  })

  it('should persist cart to localStorage', () => {
    const cart = useCartStore()
    
    const photo = {
      id: 'photo1',
      mediaItemId: 'media1',
      filename: 'test.jpg',
      baseUrl: 'https://example.com/photo.jpg',
      albumId: 'album1',
      albumTitle: 'Test Album'
    }
    
    cart.addItem(photo)
    
    const stored = JSON.parse(localStorage.getItem('cart') || '[]')
    expect(stored).toHaveLength(1)
    expect(stored[0].id).toBe('photo1')
  })
})
