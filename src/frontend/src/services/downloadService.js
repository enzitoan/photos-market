import apiClient from './httpClient'

export default {
  async validateToken(token) {
    const response = await apiClient.get(`/download/${token}`)
    return response.data
  },
  
  async getDownloadLink(token) {
    return apiClient.get(`/download/${token}`)
  },
  
  async getDownloadFiles(token) {
    return apiClient.get(`/download/${token}/files`)
  },
  
  async downloadSinglePhoto(token, photoId) {
    // Return the backend endpoint URL that will download the file directly
    // The backend now acts as a proxy and downloads from Google Drive
    const apiUrl = apiClient.defaults.baseURL || ''
    return `${apiUrl}/download/${token}/photo/${photoId}`
  }
}
