import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../api/auth'
import { useToast } from 'primevue/usetoast'
import router from '../router'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const username = ref(localStorage.getItem('username') || '')
  const toast = useToast()

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => username.value === 'admin')

  async function login(usernameInput, password) {
    try {
      const { data } = await authApi.login(usernameInput, password)
      token.value = data.token
      username.value = usernameInput
      localStorage.setItem('token', data.token)
      localStorage.setItem('username', usernameInput)
      router.push('/dashboard')
      toast.add({ severity: 'success', summary: 'Login successful', life: 3000 })
    } catch (err) {
      toast.add({ severity: 'error', summary: 'Login failed', detail: 'Invalid username or password', life: 3000 })
      throw err
    }
  }

  function logout() {
    token.value = ''
    username.value = ''
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    router.push('/login')
  }

  return { token, username, isLoggedIn, isAdmin, login, logout }
})
