<template>
  <div class="login-wrapper">
    <div class="login-card">
      <div class="login-logo">
        <div class="logo-icon">
          <i class="pi pi-box"></i>
        </div>
        <h2>Popsilencer Stock APP</h2>
      </div>
      <p class="login-subtitle">Sign in to manage your inventory</p>
      <form @submit.prevent="handleLogin">
        <div class="field">
          <label for="username">Username</label>
          <div class="input-wrapper">
            <i class="pi pi-user input-icon"></i>
            <InputText id="username" v-model="username" placeholder="Enter username" class="w-full" />
          </div>
        </div>
        <div class="field">
          <label for="password">Password</label>
          <div class="input-wrapper">
            <i class="pi pi-lock input-icon"></i>
            <InputText id="password" v-model="password" type="password" placeholder="Enter password" class="w-full" />
          </div>
        </div>
        <Button type="submit" label="Sign In" class="w-full login-btn" :loading="loading" />
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
.login-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, var(--pink-50) 0%, var(--rose-50) 50%, var(--purple-50) 100%);
  padding: 1rem;
}

.login-card {
  background: #fff;
  padding: 2.5rem;
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(236, 72, 153, 0.08), 0 4px 16px rgba(0, 0, 0, 0.04);
  width: 400px;
  max-width: 100%;
}

.login-logo {
  text-align: center;
  margin-bottom: 0.5rem;
}

.logo-icon {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  background: linear-gradient(135deg, var(--pink-400), var(--pink-500));
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
  box-shadow: 0 4px 16px rgba(236, 72, 153, 0.3);
}

.logo-icon i {
  font-size: 1.5rem;
  color: #fff;
}

.login-logo h2 {
  font-size: 1.3rem;
  font-weight: 700;
  background: linear-gradient(135deg, var(--pink-500), var(--rose-500));
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.login-subtitle {
  text-align: center;
  color: var(--gray-400);
  font-size: 0.9rem;
  margin-bottom: 2rem;
}

.field {
  margin-bottom: 1.25rem;
}

.field label {
  display: block;
  margin-bottom: 0.4rem;
  font-weight: 600;
  font-size: 0.85rem;
  color: var(--gray-600);
}

.input-wrapper {
  position: relative;
}

.input-icon {
  position: absolute;
  left: 0.85rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--gray-400);
  font-size: 0.9rem;
  z-index: 1;
  pointer-events: none;
}

.input-wrapper .w-full {
  padding-left: 2.5rem !important;
}

.w-full {
  width: 100%;
}

.login-btn {
  width: 100% !important;
  margin-top: 0.5rem;
  padding: 0.75rem !important;
  font-size: 0.95rem !important;
  border-radius: 12px !important;
}
</style>
