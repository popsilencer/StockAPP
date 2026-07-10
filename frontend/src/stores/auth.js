import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../api/auth'
import { useToast } from 'primevue/usetoast'
import router from '../router'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const toast = useToast()

  const isLoggedIn = computed(() => !!token.value)

  async function login(username, password) {
    try {
      const { data } = await authApi.login(username, password)
      token.value = data.token
      localStorage.setItem('token', data.token)
      router.push('/products')
      toast.add({ severity: 'success', summary: 'Login successful', life: 3000 })
    } catch (err) {
      toast.add({ severity: 'error', summary: 'Login failed', detail: 'Invalid username or password', life: 3000 })
      throw err
    }
  }

  function logout() {
    token.value = ''
    localStorage.removeItem('token')
    router.push('/login')
  }

  return { token, isLoggedIn, login, logout }
})
