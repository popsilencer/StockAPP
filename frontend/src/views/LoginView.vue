<template>
  <div class="login-container">
    <div class="login-card">
      <h2>Stock App</h2>
      <form @submit.prevent="handleLogin">
        <div class="field">
          <label for="username">Username</label>
          <InputText id="username" v-model="username" placeholder="admin" class="w-full" />
        </div>
        <div class="field">
          <label for="password">Password</label>
          <InputText id="password" v-model="password" type="password" placeholder="admin123" class="w-full" />
        </div>
        <Button type="submit" label="Login" class="w-full" :loading="loading" />
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const username = ref('')
const password = ref('')
const loading = ref(false)

async function handleLogin() {
  loading.value = true
  try { await auth.login(username.value, password.value) }
  finally { loading.value = false }
}
</script>

<style scoped>
.login-container {
  display: flex; justify-content: center; align-items: center;
  min-height: 100vh; background: var(--surface-ground);
}
.login-card {
  background: var(--surface-card); padding: 2rem; border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1); width: 360px;
}
.login-card h2 { text-align: center; margin-bottom: 1.5rem; color: var(--text-color); }
.field { margin-bottom: 1rem; }
.field label { display: block; margin-bottom: 0.25rem; font-weight: 600; }
.w-full { width: 100%; }
</style>
