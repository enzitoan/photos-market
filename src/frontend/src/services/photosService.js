import apiClient from './httpClient'

export default {
  async getAlbums() {
    return apiClient.get('/photos/albums')
  },
  
  async getAlbum(albumId, accessCode) {
    const url = accessCode ? `/photos/albums/${albumId}?accessCode=${encodeURIComponent(accessCode)}` : `/photos/albums/${albumId}`
    return apiClient.get(url)
  },
  
  async getAlbumPhotos(albumId, accessCode) {
    const url = accessCode ? `/photos/albums/${albumId}/photos?accessCode=${encodeURIComponent(accessCode)}` : `/photos/albums/${albumId}/photos`
    return apiClient.get(url)
  },
  
  async getPhoto(mediaItemId) {
    return apiClient.get(`/photos/${mediaItemId}`)
  }
}
