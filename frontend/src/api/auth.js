import http from './http'

export const authApi = {
  login(username, password) {
    return http.post('/auth/login', { username, password })
  },
  changePassword(currentPassword, newPassword) {
    return http.post('/auth/change-password', { currentPassword, newPassword })
  }
}
