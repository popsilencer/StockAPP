import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/login', name: 'Login', component: () => import('../views/LoginView.vue') },
  { path: '/withdraw', name: 'Withdraw', component: () => import('../views/WithdrawListView.vue'), meta: { requiresAuth: true } },
  { path: '/withdraw/new', name: 'WithdrawNew', component: () => import('../views/WithdrawView.vue'), meta: { requiresAuth: true } },
  { path: '/withdraw/:withdrawNo/edit', name: 'WithdrawEdit', component: () => import('../views/WithdrawView.vue'), meta: { requiresAuth: true } },
  { path: '/withdraw/:withdrawNo', name: 'WithdrawDetail', component: () => import('../views/WithdrawDetailView.vue'), meta: { requiresAuth: true } },
  { path: '/products', name: 'Products', component: () => import('../views/ProductsView.vue'), meta: { requiresAuth: true } },
  { path: '/movements', name: 'Movements', component: () => import('../views/MovementsView.vue'), meta: { requiresAuth: true } },
  { path: '/companies', name: 'Companies', component: () => import('../views/CompanyView.vue'), meta: { requiresAuth: true } },
  { path: '/users', name: 'Users', component: () => import('../views/UserView.vue'), meta: { requiresAuth: true } },
  { path: '/', redirect: '/withdraw' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from) => {
  const token = localStorage.getItem('token')
  if (to.meta.requiresAuth && !token) return { name: 'Login' }
  if (to.name === 'Login' && token) return { name: 'Withdraw' }
})

export default router
