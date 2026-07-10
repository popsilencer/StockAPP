<template>
  <div class="products-page">
    <div class="page-header">
      <div class="actions">
        <InputText v-model="search" placeholder="Search..." class="search-input" @keyup.enter="doSearch" />
        <Button label="Search" icon="pi pi-search" @click="doSearch" />
        <Button label="Add Product" icon="pi pi-plus" severity="success" @click="openAdd" />
        <Button label="Low Stock" icon="pi pi-exclamation-triangle" severity="warn" @click="showLowStock" />
      </div>
    </div>

    <DataTable :value="displayProducts" tableStyle="min-width: 100%">
      <Column field="sku" header="SKU" style="width: 120px"></Column>
      <Column field="name" header="Name"></Column>
      <Column field="unit" header="Unit" style="width: 80px"></Column>
      <Column field="quantity" header="Qty" style="width: 80px">
        <template #body="slotProps">
          <Tag :value="slotProps.data.quantity"
               :severity="slotProps.data.quantity <= slotProps.data.reorderLevel ? 'danger' : 'success'" />
        </template>
      </Column>
      <Column field="reorderLevel" header="Reorder" style="width: 100px"></Column>
      <Column header="Actions" style="width: 280px">
        <template #body="slotProps">
          <Button icon="pi pi-pencil" severity="info" rounded text @click="openEdit(slotProps.data)" title="Edit" />
          <Button icon="pi pi-arrow-down" severity="success" rounded text @click="openStock(slotProps.data, 'In')" title="Stock In" />
          <Button icon="pi pi-arrow-up" severity="danger" rounded text @click="openStock(slotProps.data, 'Out')" title="Stock Out" />
          <Button icon="pi pi-list" severity="secondary" rounded text @click="viewMovements(slotProps.data)" title="History" />
          <Button icon="pi pi-trash" severity="danger" rounded text @click="confirmDelete(slotProps.data)" title="Delete" />
        </template>
      </Column>
    </DataTable>

    <ProductFormDialog v-model:visible="formVisible" :product="editingProduct"
      @save="handleSave" @close="formVisible = false" />

    <StockAdjustDialog v-model:visible="stockVisible" :product="stockProduct" :initialType="stockType"
      @save="handleStockAdjust" @close="stockVisible = false" />

    <Dialog v-model:visible="movementsVisible" modal header="Movement History" :style="{ width: '600px' }">
      <DataTable :value="store.movements" tableStyle="min-width: 100%">
        <Column field="createdAt" header="Date">
          <template #body="slotProps">{{ new Date(slotProps.data.createdAt).toLocaleString() }}</template>
        </Column>
        <Column field="type" header="Type">
          <template #body="slotProps">
            <Tag :value="slotProps.data.type" :severity="slotProps.data.type === 'In' ? 'success' : 'danger'" />
          </template>
        </Column>
        <Column field="quantity" header="Qty"></Column>
        <Column field="note" header="Note"></Column>
      </DataTable>
    </Dialog>

    <Dialog v-model:visible="deleteVisible" modal header="Confirm Delete" :style="{ width: '350px' }">
      <p>Delete product <strong>{{ deletingProduct?.name }}</strong>? This cannot be undone.</p>
      <template #footer>
        <Button label="Cancel" text severity="secondary" @click="deleteVisible = false" />
        <Button label="Delete" severity="danger" @click="handleDelete" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useProductsStore } from '../stores/products'
import ProductFormDialog from './ProductFormDialog.vue'
import StockAdjustDialog from './StockAdjustDialog.vue'

const store = useProductsStore()
const search = ref('')
const showLow = ref(false)
const formVisible = ref(false)
const editingProduct = ref(null)
const stockVisible = ref(false)
const stockProduct = ref(null)
const stockType = ref('In')
const movementsVisible = ref(false)
const deleteVisible = ref(false)
const deletingProduct = ref(null)

const displayProducts = computed(() => {
  if (showLow.value) return store.lowStock
  return store.products
})

onMounted(async () => {
  await store.fetchAll()
  await store.fetchLowStock()
})

async function doSearch() {
  showLow.value = false
  await store.fetchAll(search.value)
}

function showLowStock() {
  showLow.value = true
}

function openAdd() {
  editingProduct.value = null
  formVisible.value = true
}

function openEdit(product) {
  editingProduct.value = { ...product }
  formVisible.value = true
}

function openStock(product, type) {
  stockProduct.value = product
  stockType.value = type
  stockVisible.value = true
}

async function handleSave(dto) {
  if (editingProduct.value) {
    await store.updateProduct(editingProduct.value.id, dto)
  } else {
    await store.createProduct(dto)
  }
  formVisible.value = false
  await store.fetchAll()
  await store.fetchLowStock()
}

async function handleStockAdjust(data) {
  await store.adjustStock(stockProduct.value.id, data)
  stockVisible.value = false
}

async function viewMovements(product) {
  await store.fetchMovements(product.id)
  movementsVisible.value = true
}

function confirmDelete(product) {
  deletingProduct.value = product
  deleteVisible.value = true
}

async function handleDelete() {
  await store.deleteProduct(deletingProduct.value.id)
  deleteVisible.value = false
  await store.fetchLowStock()
}

</script>

<style scoped>
.products-page { padding: 0; }
</style>
