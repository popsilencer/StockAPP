import http from './http'

export const productsApi = {
  getAll(search) {
    return http.get('/products', { params: { search } })
  },
  getLowStock() {
    return http.get('/products/low-stock')
  },
  getById(id) {
    return http.get(`/products/${id}`)
  },
  create(data) {
    return http.post('/products', data)
  },
  update(id, data) {
    return http.put(`/products/${id}`, data)
  },
  delete(id) {
    return http.delete(`/products/${id}`)
  },
  createMovement(productId, data) {
    return http.post(`/products/${productId}/movements`, data)
  },
  getMovements(productId) {
    return http.get('/movements', { params: { productId } })
  },
  deleteMovement(id) {
    return http.delete(`/movements/${id}`)
  }
}
