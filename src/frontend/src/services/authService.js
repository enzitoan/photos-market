import apiClient from './httpClient'

export default {
  async getGoogleLoginUrl() {
    return apiClient.get('/auth/google-login')
  },
  
  async googleCallback(code) {
    return apiClient.post('/auth/google-callback', { code })
  },

  async registerManual(payload) {
    return apiClient.post('/auth/register-manual', payload)
  },

  async loginManual(payload) {
    return apiClient.post('/auth/login-manual', payload)
  },
  
  async photographerGoogleCallback(code) {
    return apiClient.post('/auth/photographer-google-callback', { code })
  },
  
  async adminLogin(username, password) {
    return apiClient.post('/auth/admin-login', { username, password })
  },
  
  async validateToken() {
    return apiClient.get('/auth/validate')
  },

  async completeRegistration(phone, idType, idNumber, birthDate) {
    return apiClient.post('/auth/complete-registration', { phone, idType, idNumber, birthDate })
  }
}
