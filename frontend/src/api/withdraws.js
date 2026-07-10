import http from './http'

export const withdrawsApi = {
  list() {
    return http.get('/withdraws')
  },
  get(withdrawNo) {
    return http.get(`/withdraws/${withdrawNo}`)
  },
  getDetails(withdrawNo) {
    return http.get(`/withdraws/${withdrawNo}/details`)
  },
  nextNo() {
    return http.get('/withdraws/next-no')
  },
  save(data) {
    return http.post('/withdraws/save', data)
  },
  updateDraft(withdrawNo, data) {
    return http.put(`/withdraws/${withdrawNo}/save`, data)
  },
  confirm(withdrawNo) {
    return http.post(`/withdraws/${withdrawNo}/confirm`)
  },
  create(data) {
    return http.post('/withdraws', data)
  }
}
