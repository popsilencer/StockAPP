<template>
  <div class="movements-page">
    <div class="page-header">
      <div class="actions">
        <InputText v-model="search" placeholder="Search SKU or product name..." class="search-input" />
      </div>
    </div>
    <DataTable :value="filteredMovements" tableStyle="min-width: 100%"
               paginator :rows="10" :rowsPerPageOptions="[10, 20, 50, 100]"
               currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
               paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown">
      <Column field="sku" header="SKU" style="width: 140px"></Column>
      <Column field="productName" header="Product"></Column>
      <Column field="type" header="Type" style="width: 110px">
        <template #body="slotProps">
          <Tag :value="slotProps.data.type" :severity="slotProps.data.type === 'In' ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column field="quantity" header="Qty" style="width: 80px"></Column>
      <Column field="note" header="Note"></Column>
      <Column field="createdAt" header="Date" style="width: 200px">
        <template #body="slotProps">{{ new Date(slotProps.data.createdAt).toLocaleString() }}</template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useProductsStore } from '../stores/products'

const store = useProductsStore()
const search = ref('')

onMounted(async () => {
  await store.fetchMovements(null)
})

const filteredMovements = computed(() => {
  if (!search.value.trim()) return store.movements
  const q = search.value.toLowerCase()
  return store.movements.filter(m =>
    (m.sku || '').toLowerCase().includes(q) ||
    (m.productName || '').toLowerCase().includes(q)
  )
})
</script>

<style scoped>
.movements-page { padding: 0; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.actions { display: flex; gap: 0.5rem; align-items: center; }
.search-input { width: 280px; }
</style>
