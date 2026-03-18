import apiClient from './httpClient'

export default {
  async getDownloadLink(token) {
    return apiClient.get(`/download/${token}`)
  },
  
  async getDownloadFiles(token) {
    return apiClient.get(`/download/${token}/files`)
  }
}
