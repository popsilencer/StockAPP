import http from './http'

export const companiesApi = {
  list() {
    return http.get('/companies')
  },
  get(id) {
    return http.get(`/companies/${id}`)
  },
  create(data) {
    return http.post('/companies', data)
  },
  update(id, data) {
    return http.put(`/companies/${id}`, data)
  },
  delete(id) {
    return http.delete(`/companies/${id}`)
  }
}
