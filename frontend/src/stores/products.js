import { defineStore } from 'pinia'
import { ref } from 'vue'
import { productsApi } from '../api/products'
import { useToast } from 'primevue/usetoast'

export const useProductsStore = defineStore('products', () => {
  const products = ref([])
  const lowStock = ref([])
  const movements = ref([])
  const loading = ref(false)
  const toast = useToast()

  async function fetchAll(search = '') {
    loading.value = true
    try {
      const { data } = await productsApi.getAll(search)
      products.value = data
    } catch (err) {
      toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load products', life: 3000 })
    } finally { loading.value = false }
  }

  async function fetchLowStock() {
    try { const { data } = await productsApi.getLowStock(); lowStock.value = data }
    catch { /* silent */ }
  }

  async function createProduct(dto) {
    try {
      const { data } = await productsApi.create(dto)
      products.value.push(data)
      toast.add({ severity: 'success', summary: 'Product created', life: 3000 })
      return data
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to create product'
      toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 3000 })
      throw err
    }
  }

  async function updateProduct(id, dto) {
    try {
      const { data } = await productsApi.update(id, dto)
      const idx = products.value.findIndex(p => p.id === id)
      if (idx >= 0) products.value[idx] = data
      toast.add({ severity: 'success', summary: 'Product updated', life: 3000 })
      return data
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to update product'
      toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 3000 })
      throw err
    }
  }

  async function deleteProduct(id) {
    try {
      await productsApi.delete(id)
      products.value = products.value.filter(p => p.id !== id)
      toast.add({ severity: 'success', summary: 'Product deleted', life: 3000 })
    } catch (err) {
      toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete product', life: 3000 })
      throw err
    }
  }

  async function adjustStock(productId, data) {
    try {
      await productsApi.createMovement(productId, data)
      toast.add({ severity: 'success', summary: 'Stock adjusted', life: 3000 })
      await fetchAll()
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to adjust stock'
      toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 3000 })
      throw err
    }
  }

  async function fetchMovements(productId) {
    try { const { data } = await productsApi.getMovements(productId); movements.value = data }
    catch { /* silent */ }
  }

  return { products, lowStock, movements, loading, fetchAll, fetchLowStock, createProduct, updateProduct, deleteProduct, adjustStock, fetchMovements }
})
