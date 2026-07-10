import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/login', name: 'Login', component: () => import('../views/LoginView.vue') },
  { path: '/products', name: 'Products', component: () => import('../views/ProductsView.vue'), meta: { requiresAuth: true } },
  { path: '/movements', name: 'Movements', component: () => import('../views/MovementsView.vue'), meta: { requiresAuth: true } },
  { path: '/', redirect: '/products' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from) => {
  const token = localStorage.getItem('token')
  if (to.meta.requiresAuth && !token) return { name: 'Login' }
  if (to.name === 'Login' && token) return { name: 'Products' }
})

export default router
