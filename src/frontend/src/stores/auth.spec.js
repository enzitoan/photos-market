import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'
import authService from '@/services/authService'

// Mock authService
vi.mock('@/services/authService', () => ({
  default: {
    googleCallback: vi.fn(),
    completeRegistration: vi.fn(),
    validateToken: vi.fn()
  }
}))

describe('Auth Store', () => {
  beforeEach(() => {
    // Create a fresh pinia instance before each test
    setActivePinia(createPinia())
    // Clear localStorage
    localStorage.clear()
    // Reset all mocks
    vi.clearAllMocks()
  })

  it('should initialize with no user when not authenticated', () => {
    const auth = useAuthStore()
    
    expect(auth.user).toBeNull()
    expect(auth.token).toBeNull()
    expect(auth.isAuthenticated).toBe(false)
  })

  it('should restore token from localStorage on init', () => {
    localStorage.setItem('token', 'test-token')
    
    const auth = useAuthStore()
    
    expect(auth.token).toBe('test-token')
  })

  it('should login successfully with complete registration', async () => {
    const auth = useAuthStore()
    
    const mockResponse = {
      success: true,
      data: {
        token: 'jwt-token',
        userId: 'user123',
        email: 'test@example.com',
        name: 'Test User',
        isAdmin: false,
        needsRegistration: false
      }
    }
    
    authService.googleCallback.mockResolvedValue(mockResponse)
    
    const result = await auth.login('auth-code')
    
    expect(result).toEqual({ needsRegistration: false })
    expect(auth.user).not.toBeNull()
    expect(auth.user.email).toBe('test@example.com')
    expect(auth.token).toBe('jwt-token')
    expect(localStorage.getItem('token')).toBe('jwt-token')
  })

  it('should handle login with pending registration', async () => {
    const auth = useAuthStore()
    
    const mockResponse = {
      success: true,
      data: {
        tempToken: 'temp-token',
        userId: 'user123',
        email: 'test@example.com',
        name: 'Test User',
        isAdmin: false,
        needsRegistration: true
      }
    }
    
    authService.googleCallback.mockResolvedValue(mockResponse)
    
    const result = await auth.login('auth-code')
    
    expect(result).toEqual({ needsRegistration: true })
    expect(auth.user.needsRegistration).toBe(true)
    expect(localStorage.getItem('tempToken')).toBe('temp-token')
  })

  it('should complete registration successfully', async () => {
    const auth = useAuthStore()
    
    // First set temp user
    auth.user = {
      userId: 'user123',
      email: 'test@example.com',
      name: 'Test User',
      needsRegistration: true
    }
    localStorage.setItem('tempToken', 'temp-token')
    
    const mockResponse = {
      success: true,
      data: {
        token: 'final-token',
        userId: 'user123',
        email: 'test@example.com',
        name: 'Test User',
        isAdmin: false
      }
    }
    
    authService.completeRegistration.mockResolvedValue(mockResponse)
    
    const result = await auth.completeRegistration('123456789', 'RUT', '12345678-9', '1990-01-01')
    
    expect(result).toBe(true)
    expect(auth.user.needsRegistration).toBeUndefined()
    expect(auth.token).toBe('final-token')
    expect(localStorage.getItem('token')).toBe('final-token')
    expect(localStorage.getItem('tempToken')).toBeNull()
  })

  it('should identify admin users correctly', () => {
    const auth = useAuthStore()
    
    auth.user = {
      userId: 'admin123',
      email: 'admin@example.com',
      name: 'Admin User',
      isAdmin: true
    }
    
    expect(auth.isAdmin).toBe(true)
  })

  it('should identify non-admin users correctly', () => {
    const auth = useAuthStore()
    
    auth.user = {
      userId: 'user123',
      email: 'user@example.com',
      name: 'Regular User',
      isAdmin: false
    }
    
    expect(auth.isAdmin).toBe(false)
  })

  it('should logout and clear all data', () => {
    const auth = useAuthStore()
    
    // Setup authenticated state
    auth.user = { userId: 'user123', email: 'test@example.com', name: 'Test' }
    auth.token = 'test-token'
    localStorage.setItem('token', 'test-token')
    localStorage.setItem('user', JSON.stringify(auth.user))
    
    auth.logout()
    
    expect(auth.user).toBeNull()
    expect(auth.token).toBeNull()
    expect(localStorage.getItem('token')).toBeNull()
    expect(localStorage.getItem('user')).toBeNull()
  })

  it('should handle login errors gracefully', async () => {
    const auth = useAuthStore()
    
    authService.googleCallback.mockRejectedValue(new Error('Network error'))
    
    const result = await auth.login('bad-code')
    
    expect(result).toBe(false)
    expect(auth.user).toBeNull()
    expect(auth.token).toBeNull()
  })
})
