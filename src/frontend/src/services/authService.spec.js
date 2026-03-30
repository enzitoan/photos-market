import { describe, it, expect, vi, beforeEach } from 'vitest'
import authService from '@/services/authService'
import apiClient from '@/services/httpClient'

// Mock httpClient
vi.mock('@/services/httpClient', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn()
  }
}))

describe('Auth Service', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should get Google login URL', async () => {
    const mockResponse = {
      data: {
        authUrl: 'https://accounts.google.com/o/oauth2/auth?client_id=test'
      }
    }
    
    apiClient.get.mockResolvedValue(mockResponse)
    
    const result = await authService.getGoogleLoginUrl()
    
    expect(apiClient.get).toHaveBeenCalledWith('/auth/google-login')
    expect(result).toEqual(mockResponse)
  })

  it('should handle Google callback', async () => {
    const code = 'test-auth-code'
    const mockResponse = {
      success: true,
      data: {
        token: 'jwt-token',
        user: { id: 'user123', email: 'test@example.com' }
      }
    }
    
    apiClient.post.mockResolvedValue(mockResponse)
    
    const result = await authService.googleCallback(code)
    
    expect(apiClient.post).toHaveBeenCalledWith('/auth/google-callback', { code })
    expect(result).toEqual(mockResponse)
  })

  it('should handle photographer Google callback', async () => {
    const code = 'photographer-auth-code'
    const mockResponse = {
      success: true,
      data: {
        token: 'jwt-token',
        user: { id: 'photographer123', email: 'photographer@example.com', role: 'Photographer' }
      }
    }
    
    apiClient.post.mockResolvedValue(mockResponse)
    
    const result = await authService.photographerGoogleCallback(code)
    
    expect(apiClient.post).toHaveBeenCalledWith('/auth/photographer-google-callback', { code })
    expect(result).toEqual(mockResponse)
  })

  it('should handle admin login', async () => {
    const username = 'admin'
    const password = 'password123'
    const mockResponse = {
      success: true,
      data: {
        token: 'admin-jwt-token',
        user: { id: 'admin123', email: 'admin@example.com', role: 'Admin' }
      }
    }
    
    apiClient.post.mockResolvedValue(mockResponse)
    
    const result = await authService.adminLogin(username, password)
    
    expect(apiClient.post).toHaveBeenCalledWith('/auth/admin-login', { username, password })
    expect(result).toEqual(mockResponse)
  })

  it('should validate token', async () => {
    const mockResponse = {
      success: true,
      data: {
        valid: true,
        user: { id: 'user123', email: 'test@example.com' }
      }
    }
    
    apiClient.get.mockResolvedValue(mockResponse)
    
    const result = await authService.validateToken()
    
    expect(apiClient.get).toHaveBeenCalledWith('/auth/validate')
    expect(result).toEqual(mockResponse)
  })

  it('should complete registration', async () => {
    const phone = '+56912345678'
    const idType = 'RUT'
    const idNumber = '12345678-9'
    const birthDate = '1990-01-01'
    
    const mockResponse = {
      success: true,
      data: {
        token: 'final-token',
        user: { 
          id: 'user123', 
          email: 'test@example.com',
          phone,
          idType,
          idNumber
        }
      }
    }
    
    apiClient.post.mockResolvedValue(mockResponse)
    
    const result = await authService.completeRegistration(phone, idType, idNumber, birthDate)
    
    expect(apiClient.post).toHaveBeenCalledWith('/auth/complete-registration', {
      phone,
      idType,
      idNumber,
      birthDate
    })
    expect(result).toEqual(mockResponse)
  })

  it('should handle API errors gracefully', async () => {
    const error = new Error('Network error')
    apiClient.get.mockRejectedValue(error)
    
    await expect(authService.getGoogleLoginUrl()).rejects.toThrow('Network error')
  })
})
