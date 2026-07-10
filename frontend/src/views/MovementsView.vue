<template>
  <div class="movements-page">
    <div class="page-header">
      <div class="actions">
        <InputText v-model="productId" placeholder="Product ID filter" class="filter-input" @keyup.enter="fetch" />
        <Button label="Filter" icon="pi pi-filter" @click="fetch" />
        <Button label="All" icon="pi pi-list" severity="secondary" @click="fetchAll" />
      </div>
    </div>
    <DataTable :value="store.movements" tableStyle="min-width: 100%">
      <Column field="id" header="ID" style="width: 60px"></Column>
      <Column field="productId" header="Product ID" style="width: 100px"></Column>
      <Column field="type" header="Type" style="width: 100px">
        <template #body="slotProps">
          <Tag :value="slotProps.data.type" :severity="slotProps.data.type === 'In' ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column field="quantity" header="Qty" style="width: 80px"></Column>
      <Column field="note" header="Note"></Column>
      <Column field="createdAt" header="Date">
        <template #body="slotProps">{{ new Date(slotProps.data.createdAt).toLocaleString() }}</template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useProductsStore } from '../stores/products'

const store = useProductsStore()
const productId = ref('')

onMounted(fetchAll)

async function fetchAll() {
  productId.value = ''
  await store.fetchMovements(null)
}

async function fetch() {
  const id = parseInt(productId.value)
  if (isNaN(id)) return
  await store.fetchMovements(id)
}
</script>

<style scoped>
.movements-page { padding: 1.5rem; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; flex-wrap: wrap; gap: 0.5rem; }
.actions { display: flex; gap: 0.5rem; align-items: center; }
.filter-input { width: 180px; }
</style>
