import http from './http'

export const withdrawsApi = {
  nextNo() {
    return http.get('/withdraws/next-no')
  },
  create(data) {
    return http.post('/withdraws', data)
  }
}
