import apiClient from './httpClient'

export default {
  async createOrder(orderData) {
    return apiClient.post('/orders', orderData)
  },
  
  async getOrders() {
    return apiClient.get('/orders')
  },
  
  async getOrder(orderId) {
    return apiClient.get(`/orders/${orderId}`)
  },
  
  async confirmPayment(orderId, paymentReference) {
    return apiClient.post(`/orders/${orderId}/confirm-payment`, {
      orderId,
      paymentReference
    })
  },
  
  async completeOrder(orderId) {
    return apiClient.post(`/orders/${orderId}/complete`)
  },
  
  async cancelOrder(orderId) {
    return apiClient.post(`/orders/${orderId}/cancel`)
  },
  
  // Admin endpoints
  async getAllOrders() {
    const response = await apiClient.get('/orders/admin/all')
    return response.data
  }
}
