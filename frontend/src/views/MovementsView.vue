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
      <Column header="Actions" style="width: 80px">
        <template #body="slotProps">
          <Button v-if="auth.isAdmin" icon="pi pi-trash" severity="danger" rounded text @click="confirmDelete(slotProps.data)" title="Delete" />
        </template>
      </Column>
    </DataTable>

    <Dialog v-model:visible="deleteVisible" modal header="Confirm Delete" :style="{ width: '380px' }">
      <p>Delete this movement? Stock will be reversed.</p>
      <ul class="del-detail">
        <li><strong>{{ deletingMovement?.productName }}</strong> ({{ deletingMovement?.sku }})</li>
        <li>Type: <Tag :value="deletingMovement?.type" :severity="deletingMovement?.type === 'In' ? 'success' : 'danger'" /></li>
        <li>Qty: {{ deletingMovement?.quantity }}</li>
      </ul>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="deleteVisible = false" />
        <Button label="Delete" icon="pi pi-trash" severity="danger" :loading="deleting" @click="handleDelete" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useProductsStore } from '../stores/products'
import { useAuthStore } from '../stores/auth'

const store = useProductsStore()
const auth = useAuthStore()
const search = ref('')
const deleteVisible = ref(false)
const deletingMovement = ref(null)
const deleting = ref(false)

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

function confirmDelete(movement) {
  deletingMovement.value = movement
  deleteVisible.value = true
}

async function handleDelete() {
  deleting.value = true
  try {
    await store.deleteMovement(deletingMovement.value.id)
    deleteVisible.value = false
    await store.fetchMovements(null)
    await store.fetchLowStock()
  } finally {
    deleting.value = false
  }
}
</script>

<style scoped>
.movements-page { padding: 0; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.actions { display: flex; gap: 0.5rem; align-items: center; }
.search-input { width: 280px; }
.del-detail { list-style: none; padding: 0; margin: 0.75rem 0; display: flex; flex-direction: column; gap: 0.4rem; }
.del-detail li { display: flex; align-items: center; gap: 0.5rem; }
</style>
