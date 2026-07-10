<template>
  <div class="wd-detail-page">
    <div class="page-header">
      <div class="header-title">
        <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="$router.push('/withdraw')" />
        <h2>Withdrawal Detail</h2>
        <Tag v-if="withdraw" :value="statusLabel(withdraw.status)"
             :severity="statusSeverity(withdraw.status)" class="ml-2" />
      </div>
      <div class="header-actions" v-if="withdraw && withdraw.status !== 'Withdrawn'">
        <Button label="Edit" icon="pi pi-pencil" severity="info" outlined
                @click="$router.push(`/withdraw/${withdraw.withdrawNo}/edit`)" />
        <Button label="Confirm Withdraw" icon="pi pi-check" severity="danger"
                @click="openConfirm" :loading="processing" />
      </div>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

    <template v-else-if="withdraw">
      <!-- Master -->
      <div class="master-card">
        <div class="master-row">
          <div class="field"><label>Withdraw No</label><div class="value wd-no">{{ withdraw.withdrawNo }}</div></div>
          <div class="field"><label>Date</label><div class="value">{{ new Date(withdraw.date).toLocaleDateString() }}</div></div>
          <div class="field" v-if="withdraw.note"><label>Note</label><div class="value">{{ withdraw.note }}</div></div>
        </div>
      </div>

      <!-- Detail -->
      <div class="list-card">
        <DataTable :value="details" tableStyle="min-width: 100%">
          <Column field="sku" header="SKU" style="width: 140px"></Column>
          <Column field="productName" header="Product"></Column>
          <Column field="inStock" header="In Stock" style="width: 100px"></Column>
          <Column field="quantity" header="Withdraw Qty" style="width: 120px"></Column>
        </DataTable>
      </div>

      <div v-if="withdraw.status === 'Withdrawn'" class="read-only-notice">
        <i class="pi pi-lock"></i>
        <p>This withdrawal has been confirmed and cannot be modified.</p>
      </div>
    </template>

    <div v-else class="loading">Withdrawal not found.</div>

    <!-- Confirm dialog -->
    <Dialog v-model:visible="confirmVisible" modal header="Confirm Withdrawal" :style="{ width: '420px' }">
      <div class="confirm-body">
        <i class="pi pi-exclamation-triangle confirm-warn"></i>
        <div>
          <p><strong>{{ withdraw.withdrawNo }}</strong></p>
          <p>You are about to confirm this withdrawal. Stock will be deducted for <strong>{{ totalQty }}</strong> units across <strong>{{ details.length }}</strong> product(s).</p>
          <p class="warn-text">This action cannot be undone.</p>
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="confirmVisible = false" />
        <Button label="Confirm" icon="pi pi-check" severity="danger" :loading="processing" @click="processConfirm" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { withdrawsApi } from '../api/withdraws'
import { useToast } from 'primevue/usetoast'
import { useProductsStore } from '../stores/products'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const store = useProductsStore()

const withdraw = ref(null)
const details = ref([])
const loading = ref(true)
const processing = ref(false)
const confirmVisible = ref(false)

const totalQty = computed(() =>
  details.value.reduce((s, i) => s + i.quantity, 0)
)

const statusMap = { 0: 'Draft', 1: 'Saved', 2: 'Withdrawn' }
const severityMap = { 0: 'info', 1: 'success', 2: 'danger' }
function statusLabel(s) { return statusMap[s] ?? '—' }
function statusSeverity(s) { return severityMap[s] ?? 'secondary' }

onMounted(async () => {
  try {
    const { data } = await withdrawsApi.get(route.params.withdrawNo)
    withdraw.value = data
    const { data: detailData } = await withdrawsApi.getDetails(route.params.withdrawNo)
    details.value = detailData
  } catch { withdraw.value = null }
  finally { loading.value = false }
})

function openConfirm() {
  confirmVisible.value = true
}

async function processConfirm() {
  processing.value = true
  try {
    // If draft, save first then confirm
    if (withdraw.value.status === 'Draft') {
      const payload = {
        date: withdraw.value.date,
        note: withdraw.value.note,
        items: details.value.map(d => ({ productId: d.productId, quantity: d.quantity }))
      }
      await withdrawsApi.updateDraft(withdraw.value.withdrawNo, payload)
    }
    const { data } = await withdrawsApi.confirm(withdraw.value.withdrawNo)
    toast.add({ severity: 'success', summary: 'Withdrawal confirmed', detail: `${data.withdrawNo} — Stock deducted`, life: 4000 })
    confirmVisible.value = false
    await store.fetchAll()
    await store.fetchLowStock()
    router.push('/withdraw')
  } catch (err) {
    const msg = err.response?.data?.message || 'Confirmation failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 4000 })
  } finally {
    processing.value = false
  }
}
</script>

<style scoped>
.wd-detail-page { padding: 0; }
.page-header { margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; flex: 1; }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }
.ml-2 { margin-left: 0.5rem; }
.header-actions { display: flex; gap: 0.75rem; }

.master-card { background: linear-gradient(135deg, var(--pink-50), var(--rose-50)); border: 1px solid var(--pink-100); border-radius: 12px; padding: 1.25rem; margin-bottom: 1.5rem; }
.master-row { display: flex; gap: 2rem; flex-wrap: wrap; }
.field { display: flex; flex-direction: column; }
.field label { font-size: 0.75rem; font-weight: 600; color: var(--gray-500); text-transform: uppercase; margin-bottom: 0.25rem; }
.value { font-size: 1rem; font-weight: 600; color: var(--gray-800); }
.wd-no { color: var(--rose-600); }
.note-field { margin-top: 1rem; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
.loading { text-align: center; padding: 2rem; color: var(--gray-400); }

.read-only-notice { text-align: center; padding: 1.5rem; color: var(--gray-500); background: var(--gray-50); border-radius: 12px; margin-top: 1.5rem; }
.read-only-notice i { font-size: 2rem; margin-bottom: 0.5rem; color: var(--gray-400); }
.read-only-notice p { font-size: 0.9rem; }

.confirm-body { display: flex; gap: 1rem; padding: 0.5rem 0; align-items: flex-start; }
.confirm-warn { font-size: 2rem; color: var(--amber-500); flex-shrink: 0; }
.confirm-body p { margin-bottom: 0.4rem; font-size: 0.9rem; color: var(--gray-700); }
.warn-text { color: var(--rose-600); font-weight: 600; }
</style>
