import apiClient from './httpClient'

export default {
  async getAlbums() {
    return apiClient.get('/photos/albums')
  },
  
  async getAlbum(albumId) {
    return apiClient.get(`/photos/albums/${albumId}`)
  },
  
  async getAlbumPhotos(albumId) {
    return apiClient.get(`/photos/albums/${albumId}/photos`)
  },
  
  async getPhoto(mediaItemId) {
    return apiClient.get(`/photos/${mediaItemId}`)
  }
}
