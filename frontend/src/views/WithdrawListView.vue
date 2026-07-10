<template>
  <div class="withdraw-list-page">
    <div class="page-header">
      <div class="header-title">
        <i class="pi pi-shopping-cart title-icon"></i>
        <h2>Withdrawals</h2>
      </div>
      <Button label="New Withdraw" icon="pi pi-plus" class="new-btn" @click="$router.push('/withdraw/new')" />
    </div>

    <div class="list-card">
      <DataTable :value="withdraws" :loading="loading" tableStyle="min-width: 100%"
                 emptyMessage="No withdrawals yet.">
        <Column header="Withdraw No" style="width: 200px">
          <template #body="slotProps">
            <span class="wd-no">{{ slotProps.data.withdrawNo }}</span>
          </template>
        </Column>
        <Column header="Date" style="width: 160px">
          <template #body="slotProps">{{ new Date(slotProps.data.date).toLocaleDateString() }}</template>
        </Column>
        <Column header="Items" style="width: 80px">
          <template #body="slotProps">{{ slotProps.data.items.length }}</template>
        </Column>
        <Column header="Total Qty" style="width: 100px">
          <template #body="slotProps">{{ totalQty(slotProps.data) }}</template>
        </Column>
        <Column header="Note"></Column>
        <Column header="" style="width: 80px">
          <template #body="slotProps">
            <Button icon="pi pi-eye" severity="info" text rounded @click="$router.push(`/withdraw/${slotProps.data.id}`)" title="View" />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { withdrawsApi } from '../api/withdraws'
import { useToast } from 'primevue/usetoast'

const toast = useToast()
const withdraws = ref([])
const loading = ref(false)

onMounted(fetchList)

async function fetchList() {
  loading.value = true
  try {
    const { data } = await withdrawsApi.list()
    withdraws.value = data
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load withdrawals', life: 3000 })
  } finally {
    loading.value = false
  }
}

function totalQty(w) {
  return (w.items || []).reduce((sum, i) => sum + i.quantity, 0)
}
</script>

<style scoped>
.withdraw-list-page { padding: 0; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; }
.title-icon { font-size: 1.5rem; color: var(--rose-500); }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }
.new-btn { font-size: 0.85rem; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
.wd-no { font-weight: 600; color: var(--rose-600); }
</style>
