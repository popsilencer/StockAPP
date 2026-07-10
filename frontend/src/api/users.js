import http from './http'

export const usersApi = {
  list() {
    return http.get('/users')
  },
  get(id) {
    return http.get(`/users/${id}`)
  },
  create(data) {
    return http.post('/users', data)
  },
  update(id, data) {
    return http.put(`/users/${id}`, data)
  },
  changePassword(id, data) {
    return http.put(`/users/${id}/password`, data)
  },
  delete(id) {
    return http.delete(`/users/${id}`)
  }
}
