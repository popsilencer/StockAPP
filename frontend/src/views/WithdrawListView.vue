<template>
  <div class="withdraw-list-page">
    <div class="page-header">
      <div class="header-title">
        <i class="pi pi-shopping-cart title-icon"></i>
        <h2>Withdraw</h2>
      </div>
      <Button label="New Withdraw" icon="pi pi-plus" class="new-btn" @click="$router.push('/withdraw/new')" />
    </div>

    <div class="list-card">
      <div class="toolbar">
        <InputText v-model="searchTerm" placeholder="Search by Withdraw No or Note..." class="search-input" />
        <span class="result-count" v-if="!loading">{{ filteredWithdraws.length }} record(s)</span>
      </div>

      <DataTable :value="filteredWithdraws" :loading="loading" tableStyle="min-width: 100%"
                 emptyMessage="No withdrawals found."
                 paginator :rows="10" :rowsPerPageOptions="[10, 20, 50, 100]"
                 currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
                 paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown">
        <Column header="Withdraw No" style="width: 220px">
          <template #body="slotProps">
            <span class="wd-no">{{ slotProps.data.withdrawNo }}</span>
          </template>
        </Column>
        <Column header="Date" style="width: 160px">
          <template #body="slotProps">{{ new Date(slotProps.data.date).toLocaleDateString() }}</template>
        </Column>
        <Column header="Note">
          <template #body="slotProps">{{ slotProps.data.note || '—' }}</template>
        </Column>
        <Column header="Status" style="width: 110px">
          <template #body="slotProps">
            <Tag :value="statusLabel(slotProps.data.status)"
                 :severity="statusSeverity(slotProps.data.status)" />
          </template>
        </Column>
        <Column header="Action" style="width: 120px">
          <template #body="slotProps">
            <div class="action-btns action-right">
              <Button v-if="slotProps.data.status !== 'Withdrawn'" icon="pi pi-pencil" severity="info"
                      text rounded size="small" @click="$router.push(`/withdraw/${slotProps.data.withdrawNo}/edit`)"
                      title="Edit" />
              <Button icon="pi pi-eye" severity="info" text rounded size="small"
                      @click="$router.push(`/withdraw/${slotProps.data.withdrawNo}`)" title="View" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { withdrawsApi } from '../api/withdraws'
import { useToast } from 'primevue/usetoast'

const toast = useToast()
const withdraws = ref([])
const loading = ref(false)
const searchTerm = ref('')

const statusMap = { Draft: 'Draft', Saved: 'Saved', Withdrawn: 'Withdrawn' }
const severityMap = { Draft: 'info', Saved: 'success', Withdrawn: 'danger' }

function statusLabel(s) { return statusMap[s] ?? '—' }
function statusSeverity(s) { return severityMap[s] ?? 'secondary' }

const filteredWithdraws = computed(() => {
  const q = searchTerm.value.trim().toLowerCase()
  if (!q) return withdraws.value
  return withdraws.value.filter(w =>
    (w.withdrawNo || '').toLowerCase().includes(q) ||
    (w.note || '').toLowerCase().includes(q)
  )
})

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
</script>

<style scoped>
.withdraw-list-page { padding: 0; }

.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; }
.title-icon { font-size: 1.5rem; color: var(--rose-500); }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }
.new-btn { font-size: 0.85rem; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.25rem;
  background: linear-gradient(135deg, var(--pink-50), var(--rose-50));
  border-bottom: 1px solid var(--pink-100);
  flex-wrap: wrap;
}
.search-input { width: 100%; max-width: 420px; }
.result-count { font-size: 0.8rem; color: var(--gray-500); font-weight: 600; white-space: nowrap; }

.wd-no { font-weight: 600; color: var(--rose-600); }
.action-btns { display: flex; gap: 4px; }
.action-right { justify-content: flex-end; }
</style>
