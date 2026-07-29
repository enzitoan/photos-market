import apiClient from './httpClient'

export default {
  async getAllAlbums() {
    return apiClient.get('/admin/albums')
  },
  
  async blockAlbum(googleAlbumId) {
    return apiClient.post(`/admin/albums/${googleAlbumId}/block`)
  },
  
  async unblockAlbum(googleAlbumId) {
    return apiClient.post(`/admin/albums/${googleAlbumId}/unblock`)
  },

  async setAlbumPublic(googleAlbumId) {
    return apiClient.post(`/admin/albums/${googleAlbumId}/set-public`)
  },

  async setAlbumPrivate(googleAlbumId, accessCode) {
    return apiClient.post(`/admin/albums/${googleAlbumId}/set-private`, { accessCode })
  },

  async renewAccessCode(googleAlbumId, accessCode) {
    return apiClient.post(`/admin/albums/${googleAlbumId}/renew-access-code`, { accessCode })
  },
  
  async getPhotographerSettings() {
    return apiClient.get('/admin/albums/photographer-settings')
  },
  
  async updatePhotographerSettings(settings) {
    return apiClient.put('/admin/albums/photographer-settings', settings)
  }
}
