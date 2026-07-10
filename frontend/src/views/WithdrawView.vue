<template>
  <div class="withdraw-page">
    <div class="page-header">
      <div class="header-title">
        <i class="pi pi-shopping-cart title-icon"></i>
        <h2>Withdraw Products</h2>
      </div>
    </div>

    <!-- ===== MASTER ===== -->
    <div class="master-card">
      <div class="master-row">
        <div class="field">
          <label>Withdraw No</label>
          <InputText :modelValue="withdrawNo" disabled class="w-full" />
        </div>
        <div class="field">
          <label>Date</label>
          <Calendar v-model="withdrawDate" dateFormat="dd/mm/yy" :showIcon="true" class="w-full" />
        </div>
        <div class="field field-note">
          <label>Note</label>
          <InputText v-model="note" placeholder="Department / requester..." class="w-full" />
        </div>
      </div>
    </div>

    <!-- ===== DETAIL: Add item ===== -->
    <div class="add-card">
      <div class="form-row">
        <div class="field field-product">
          <label>Product</label>
          <Select v-model="selectedProduct" :options="availableProducts" optionLabel="label" optionValue="value"
            placeholder="Select product" filter class="w-full" />
        </div>
        <div class="field field-qty">
          <label>Quantity</label>
          <InputNumber v-model="qty" :min="1" class="w-full" />
        </div>
        <div class="field field-btn">
          <Button label="Add" icon="pi pi-plus" class="add-btn" @click="addItem" :disabled="!selectedProduct || !qty" />
        </div>
      </div>
    </div>

    <!-- ===== DETAIL: Items list ===== -->
    <div class="list-card" v-if="items.length">
      <DataTable :value="items" tableStyle="min-width: 100%">
        <Column header="SKU" style="width: 140px">
          <template #body="slotProps">{{ slotProps.data.sku }}</template>
        </Column>
        <Column header="Product">
          <template #body="slotProps">{{ slotProps.data.name }}</template>
        </Column>
        <Column header="In Stock" style="width: 110px">
          <template #body="slotProps">
            <span :class="{ 'low': slotProps.data.quantity <= slotProps.data.reorderLevel }">
              {{ slotProps.data.quantity }} {{ slotProps.data.unit }}
            </span>
          </template>
        </Column>
        <Column header="Withdraw Qty" style="width: 140px">
          <template #body="slotProps">
            <InputNumber v-model="slotProps.data.withdrawQty" :min="1" :max="slotProps.data.quantity" size="small" />
          </template>
        </Column>
        <Column header="" style="width: 60px">
          <template #body="slotProps">
            <Button icon="pi pi-times" severity="danger" text rounded @click="removeItem(slotProps.index)" />
          </template>
        </Column>
      </DataTable>

      <div class="summary">
        <div class="summary-info">
          <span class="summary-label">Items:</span>
          <span class="summary-value">{{ items.length }}</span>
          <span class="summary-label">Total Qty:</span>
          <span class="summary-value">{{ totalQty }}</span>
        </div>
        <Button label="Confirm Withdraw" icon="pi pi-check" severity="danger" class="confirm-btn" @click="openConfirm" />
      </div>
    </div>

    <div class="empty-state" v-else>
      <i class="pi pi-shopping-cart"></i>
      <p>No items added yet. Select products above to start a withdrawal.</p>
    </div>

    <!-- Confirm -->
    <Dialog v-model:visible="confirmVisible" modal header="Confirm Withdrawal" :style="{ width: '420px' }">
      <div class="confirm-body">
        <i class="pi pi-exclamation-triangle confirm-warn"></i>
        <div>
          <p><strong>{{ withdrawNo }}</strong></p>
          <p>You are about to withdraw <strong>{{ totalQty }}</strong> units across <strong>{{ items.length }}</strong> product(s) on {{ formatDate(withdrawDate) }}.</p>
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="confirmVisible = false" />
        <Button label="Confirm" icon="pi pi-check" severity="danger" :loading="processing" @click="processWithdraw" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useProductsStore } from '../stores/products'
import { withdrawsApi } from '../api/withdraws'
import { useToast } from 'primevue/usetoast'

const store = useProductsStore()
const toast = useToast()

const withdrawNo = ref('')
const withdrawDate = ref(new Date())
const note = ref('')
const selectedProduct = ref(null)
const qty = ref(1)
const items = ref([])
const confirmVisible = ref(false)
const processing = ref(false)

const availableProducts = computed(() =>
  store.products
    .filter(p => p.quantity > 0 && !items.value.some(i => i.id === p.id))
    .map(p => ({ label: `${p.sku} — ${p.name} (${p.quantity} ${p.unit})`, value: p }))
)

const totalQty = computed(() => items.value.reduce((sum, i) => sum + (i.withdrawQty || 0), 0))

onMounted(async () => {
  await store.fetchAll()
  await loadNextNo()
})

async function loadNextNo() {
  try {
    const { data } = await withdrawsApi.nextNo()
    withdrawNo.value = data.withdrawNo
  } catch { /* ignore */ }
}

function formatDate(d) {
  return new Date(d).toLocaleDateString()
}

function addItem() {
  if (!selectedProduct.value) return
  const p = selectedProduct.value
  if (qty.value > p.quantity) {
    toast.add({ severity: 'warn', summary: 'Exceeds stock', detail: `Only ${p.quantity} ${p.unit} available`, life: 3000 })
    return
  }
  items.value.push({ ...p, withdrawQty: qty.value })
  selectedProduct.value = null
  qty.value = 1
}

function removeItem(index) {
  items.value.splice(index, 1)
}

function openConfirm() {
  const over = items.value.find(i => i.withdrawQty > i.quantity)
  if (over) {
    toast.add({ severity: 'warn', summary: 'Invalid quantity', detail: `${over.name} exceeds stock`, life: 3000 })
    return
  }
  confirmVisible.value = true
}

async function processWithdraw() {
  processing.value = true
  try {
    const payload = {
      date: withdrawDate.value,
      note: note.value,
      items: items.value.map(i => ({ productId: i.id, quantity: i.withdrawQty }))
    }
    const { data } = await withdrawsApi.create(payload)
    toast.add({ severity: 'success', summary: 'Withdrawal complete', detail: `${data.withdrawNo} — ${data.processedCount} product(s) withdrawn`, life: 4000 })
    confirmVisible.value = false
    items.value = []
    note.value = ''
    await store.fetchAll()
    await store.fetchLowStock()
    await loadNextNo()
  } catch (err) {
    const msg = err.response?.data?.message || 'Withdrawal failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 4000 })
  } finally {
    processing.value = false
  }
}
</script>

<style scoped>
.withdraw-page { padding: 0; }
.page-header { margin-bottom: 1.5rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; }
.title-icon { font-size: 1.5rem; color: var(--rose-500); }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }

.master-card { background: linear-gradient(135deg, var(--pink-50), var(--rose-50)); border: 1px solid var(--pink-100); border-radius: 12px; padding: 1.25rem; margin-bottom: 1.5rem; }
.master-row { display: flex; gap: 1rem; flex-wrap: wrap; }
.master-row .field { flex: 1; min-width: 180px; }
.field-note { min-width: 300px !important; }
.field { display: flex; flex-direction: column; }
.field label { display: block; margin-bottom: 0.4rem; font-weight: 600; font-size: 0.85rem; color: var(--gray-600); }
.w-full { width: 100%; }

.add-card { background: #fff; border-radius: 12px; padding: 1.25rem; box-shadow: 0 1px 3px rgba(0,0,0,0.06); margin-bottom: 1.5rem; }
.form-row { display: flex; gap: 1rem; align-items: flex-end; flex-wrap: wrap; }
.field-product { flex: 1; min-width: 250px; }
.field-qty { width: 130px; }
.field-btn { padding-bottom: 0.1rem; }
.add-btn { height: 38px; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
.summary { display: flex; justify-content: space-between; align-items: center; padding: 1rem 1.25rem; border-top: 1px solid var(--gray-100); flex-wrap: wrap; gap: 1rem; }
.summary-info { display: flex; align-items: center; gap: 0.5rem; font-size: 0.9rem; }
.summary-label { color: var(--gray-500); }
.summary-value { font-weight: 700; color: var(--gray-800); margin-right: 0.75rem; }
.confirm-btn { font-size: 0.9rem; }
.low { color: var(--rose-600); font-weight: 600; }

.empty-state { text-align: center; padding: 3rem 1rem; color: var(--gray-400); }
.empty-state i { font-size: 3rem; margin-bottom: 1rem; }
.empty-state p { font-size: 0.9rem; }

.confirm-body { display: flex; gap: 1rem; padding: 0.5rem 0; align-items: flex-start; }
.confirm-warn { font-size: 2rem; color: var(--amber-500); flex-shrink: 0; }
.confirm-body p { margin-bottom: 0.4rem; font-size: 0.9rem; color: var(--gray-700); }
</style>
