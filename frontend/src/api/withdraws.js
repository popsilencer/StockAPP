import http from './http'

export const withdrawsApi = {
  list() {
    return http.get('/withdraws')
  },
  get(id) {
    return http.get(`/withdraws/${id}`)
  },
  nextNo() {
    return http.get('/withdraws/next-no')
  },
  create(data) {
    return http.post('/withdraws', data)
  }
}
