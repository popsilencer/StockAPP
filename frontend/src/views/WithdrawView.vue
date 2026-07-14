<template>
  <div class="withdraw-page">
    <div class="page-header">
      <div class="header-title">
        <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="$router.push('/withdraw')" />
        <i class="pi pi-shopping-cart title-icon"></i>
        <h2>{{ isEdit ? 'Edit Withdrawal' : isView ? 'Withdrawal Detail' : 'New Withdraw' }}</h2>
        <Tag v-if="withdrawStatus !== null" :value="statusLabel(withdrawStatus)"
             :severity="statusSeverity(withdrawStatus)" class="ml-2" />
      </div>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

    <template v-else>
      <!-- ===== MASTER ===== -->
      <div class="master-card">
        <div class="master-row">
          <div class="field">
            <label>Withdraw No</label>
            <InputText :modelValue="withdrawNo" disabled class="w-full" />
          </div>
          <div class="field">
            <label>Date</label>
            <Calendar v-model="withdrawDate" dateFormat="dd/mm/yy" :showIcon="true" :disabled="isView" class="w-full" />
          </div>
          <div class="field field-note">
            <label>Note</label>
            <InputText v-model="note" placeholder="Note" :disabled="isView" class="w-full" />
          </div>
        </div>
      </div>

      <!-- ===== DETAIL: Add item (only if not view-only) ===== -->
      <div class="add-card" v-if="!isView">
        <div class="form-row">
          <div class="field field-product">
            <label>Product</label>
            <Select v-model="selectedProduct" :options="availableProducts" optionLabel="label" optionValue="value"
              placeholder="Search product by SKU or name..." filter filterPlaceholder="Type to search..."
              class="w-full" :disabled="isView" />
          </div>
          <div class="field field-qty">
            <label>Quantity</label>
            <InputNumber v-model="qty" :min="1" :disabled="isView" class="w-full" />
          </div>
          <div class="field field-btn">
            <Button label="Add" icon="pi pi-plus" class="add-btn" @click="addItem"
                    :disabled="!selectedProduct || !qty || isView" />
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
          <Column header="In Stock" style="width: 110px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">
              <span :class="{ 'low': slotProps.data.inStock <= slotProps.data.reorderLevel }">
                {{ slotProps.data.inStock }} {{ slotProps.data.unit }}
              </span>
            </template>
          </Column>
          <Column header="Withdraw Qty" style="width: 140px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">
              <InputNumber v-if="!isView" v-model="slotProps.data.withdrawQty"
                           :min="1" :max="slotProps.data.inStock" size="small" />
              <span v-else>{{ slotProps.data.withdrawQty }}</span>
            </template>
          </Column>
          <Column header="Price/Unit" style="width: 120px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">{{ formatMoney(slotProps.data.price) }}</template>
          </Column>
          <Column header="Profit/Unit" style="width: 120px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">
              <strong :class="{ 'profit-neg': (slotProps.data.profit ?? 0) < 0 }">{{ formatMoney(slotProps.data.profit) }}</strong>
            </template>
          </Column>
          <Column header="Total Price" style="width: 140px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps"><strong>{{ formatMoney(priceTotal(slotProps.data)) }}</strong></template>
          </Column>
          <Column header="Total Profit" style="width: 140px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">
              <strong :class="{ 'profit-neg': profitTotal(slotProps.data) < 0 }">{{ formatMoney(profitTotal(slotProps.data)) }}</strong>
            </template>
          </Column>
          <Column header="" style="width: 60px">
            <template #body="slotProps">
              <Button v-if="!isView" icon="pi pi-times" severity="danger" text rounded
                      @click="removeItem(slotProps.index)" />
            </template>
          </Column>
        </DataTable>

        <div class="summary">
          <div class="summary-info">
            <span class="summary-label">Items:</span>
            <span class="summary-value">{{ items.length }}</span>
            <span class="summary-label">Total Qty:</span>
            <span class="summary-value">{{ totalQty }}</span>
            <span class="summary-label">Net Price:</span>
            <span class="summary-value">{{ formatMoney(sumPriceTotal) }}</span>
            <span class="summary-label">Net Profit:</span>
            <span class="summary-value" :class="{ 'profit-neg': sumProfitTotal < 0 }">{{ formatMoney(sumProfitTotal) }}</span>
          </div>
          <div class="summary-btns" v-if="!isView">
            <Button v-if="!isEdit" label="Save Draft" icon="pi pi-save" severity="secondary" outlined
                    class="save-btn" @click="saveDraft" :loading="processing" />
            <Button v-if="isEdit" label="Update Draft" icon="pi pi-save" severity="secondary" outlined
                    class="save-btn" @click="updateDraft" :loading="processing" />
            <Button label="Confirm Withdraw" icon="pi pi-check" severity="danger"
                    class="confirm-btn" @click="openConfirm" :loading="processing" />
          </div>
        </div>
      </div>

      <div class="empty-state" v-else>
        <i class="pi pi-shopping-cart"></i>
        <p>{{ isView ? 'No items in this withdrawal.' : 'No items added yet. Select products above to start a withdrawal.' }}</p>
      </div>

      <!-- Confirm dialog -->
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
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useProductsStore } from '../stores/products'
import { withdrawsApi } from '../api/withdraws'
import { useToast } from 'primevue/usetoast'

const router = useRouter()
const route = useRoute()
const store = useProductsStore()
const toast = useToast()

const isEdit = computed(() => route.name === 'WithdrawEdit')
const isView = computed(() => route.name === 'WithdrawDetail')
const withdrawNoParam = computed(() => route.params.withdrawNo || '')

const withdrawNo = ref('')
const withdrawDate = ref(new Date())
const note = ref('')
const selectedProduct = ref(null)
const qty = ref(1)
const items = ref([])
const confirmVisible = ref(false)
const processing = ref(false)
const loading = ref(false)
const withdrawStatus = ref(null)

const statusMap = { 0: 'Draft', 1: 'Saved', 2: 'Withdrawn' }
const severityMap = { 0: 'info', 1: 'success', 2: 'danger' }
function statusLabel(s) { return statusMap[s] ?? '—' }
function statusSeverity(s) { return severityMap[s] ?? 'secondary' }

const availableProducts = computed(() =>
  store.products
    .filter(p => p.quantity > 0)
    .map(p => ({ label: `${p.sku} — ${p.name} (${p.quantity} ${p.unit})`, value: p }))
)

const totalQty = computed(() => items.value.reduce((sum, i) => sum + (i.withdrawQty || 0), 0))

function priceTotal(i) {
  return (Number(i.price) || 0) * (Number(i.withdrawQty) || 0)
}
function profitTotal(i) {
  return (Number(i.profit) || 0) * (Number(i.withdrawQty) || 0)
}
const sumPriceTotal = computed(() => items.value.reduce((s, i) => s + priceTotal(i), 0))
const sumProfitTotal = computed(() => items.value.reduce((s, i) => s + profitTotal(i), 0))

function formatMoney(value) {
  const num = Number(value || 0)
  return num.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

onMounted(async () => {
  await store.fetchAll()

  if (isEdit.value || isView.value) {
    loading.value = true
    try {
      const { data } = await withdrawsApi.get(withdrawNoParam.value)
      withdrawNo.value = data.withdrawNo
      withdrawDate.value = new Date(data.date)
      note.value = data.note || ''
      withdrawStatus.value = data.status

      const { data: detailData } = await withdrawsApi.getDetails(withdrawNoParam.value)
      items.value = detailData.map(d => {
        const product = store.products.find(p => p.id === d.productId)
        return {
          id: d.productId,
          sku: d.sku,
          name: d.productName,
          inStock: d.inStock,
          quantity: d.inStock,
          reorderLevel: product ? product.reorderLevel : 0,
          unit: product ? product.unit : '',
          withdrawQty: d.quantity,
          cost: d.cost ?? (product ? product.cost : 0),
          price: d.price ?? (product ? product.price : 0),
          profit: d.profit ?? (product ? product.profit : 0)
        }
      })
    } catch {
      toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load withdrawal', life: 3000 })
    } finally {
      loading.value = false
    }
  }
  // New withdrawal: Withdraw No will be generated on Save/Confirm
})

async function loadNextNo() {
  try {
    const { data } = await withdrawsApi.nextNo()
    withdrawNo.value = data.withdrawNo
  } catch { /* ignore */ }
}

async function generateNo() {
  if (!withdrawNo.value) {
    await loadNextNo()
  }
}

function formatDate(d) {
  return new Date(d).toLocaleDateString()
}

function addItem() {
  const p = selectedProduct.value
  if (!p || !p.id) {
    toast.add({ severity: 'warn', summary: 'Please select a product', detail: 'Choose a product from the search results', life: 3000 })
    return
  }
  if (qty.value < 1) {
    toast.add({ severity: 'warn', summary: 'Invalid quantity', detail: 'Quantity must be at least 1', life: 3000 })
    return
  }
  if (qty.value > p.quantity) {
    toast.add({ severity: 'warn', summary: 'Exceeds stock', detail: `Only ${p.quantity} ${p.unit} available`, life: 3000 })
    return
  }
  items.value.push({ ...p, withdrawQty: qty.value, inStock: p.quantity })
  selectedProduct.value = null
  qty.value = 1
}

function removeItem(index) {
  items.value.splice(index, 1)
}

async function saveDraft() {
  if (processing.value) return
  if (items.value.length === 0) {
    toast.add({ severity: 'warn', summary: 'No items', detail: 'Please add at least one product before saving', life: 3000 })
    return
  }
  processing.value = true
  try {
    await generateNo()
    const payload = {
      date: withdrawDate.value.toISOString(),
      note: note.value,
      items: items.value.map(i => ({ productId: i.id, quantity: i.withdrawQty }))
    }
    const { data } = await withdrawsApi.save(payload)
    toast.add({ severity: 'success', summary: 'Draft saved', detail: `${data.withdrawNo} — ${data.processedCount} product(s) saved`, life: 4000 })
    router.push('/withdraw')
  } catch (err) {
    const res = err.response?.data
    const msg = res?.message || res?.errors?.join(', ') || err.response?.data?.title || 'Save failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 })
  } finally {
    processing.value = false
  }
}

async function updateDraft() {
  if (processing.value) return
  processing.value = true
  try {
    const payload = {
      date: withdrawDate.value,
      note: note.value,
      items: items.value.map(i => ({ productId: i.id, quantity: i.withdrawQty }))
    }
    const { data } = await withdrawsApi.updateDraft(withdrawNo.value, payload)
    toast.add({ severity: 'success', summary: 'Draft updated', detail: `${data.withdrawNo} — ${data.processedCount} product(s) updated`, life: 4000 })
    router.push('/withdraw')
  } catch (err) {
    const msg = err.response?.data?.message || 'Update failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 4000 })
  } finally {
    processing.value = false
  }
}

function openConfirm() {
  const over = items.value.find(i => i.withdrawQty > i.inStock)
  if (over) {
    toast.add({ severity: 'warn', summary: 'Invalid quantity', detail: `${over.name} exceeds stock`, life: 3000 })
    return
  }
  confirmVisible.value = true
}

async function processWithdraw() {
  if (processing.value) return
  processing.value = true
  try {
    await generateNo()
    let data
    if (isEdit.value) {
      const payload = {
        date: withdrawDate.value,
        note: note.value,
        items: items.value.map(i => ({ productId: i.id, quantity: i.withdrawQty }))
      }
      const saveRes = await withdrawsApi.updateDraft(withdrawNo.value, payload)
      const confirmRes = await withdrawsApi.confirm(withdrawNo.value)
      data = confirmRes.data
    } else {
      const payload = {
        date: withdrawDate.value,
        note: note.value,
        items: items.value.map(i => ({ productId: i.id, quantity: i.withdrawQty }))
      }
      const res = await withdrawsApi.create(payload)
      data = res.data
    }
    toast.add({ severity: 'success', summary: 'Withdrawal complete', detail: `${data.withdrawNo} — ${data.processedCount} product(s) withdrawn`, life: 4000 })
    confirmVisible.value = false
    items.value = []
    note.value = ''
    await store.fetchAll()
    await store.fetchLowStock()
    router.push('/withdraw')
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
.header-title { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
.title-icon { font-size: 1.5rem; color: var(--rose-500); }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }
.ml-2 { margin-left: 0.5rem; }

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
.summary-btns { display: flex; gap: 0.75rem; }
.save-btn { font-size: 0.9rem; }
.confirm-btn { font-size: 0.9rem; }
.low { color: var(--rose-600); font-weight: 600; }

.empty-state { text-align: center; padding: 3rem 1rem; color: var(--gray-400); }
.empty-state i { font-size: 3rem; margin-bottom: 1rem; }
.empty-state p { font-size: 0.9rem; }

.confirm-body { display: flex; gap: 1rem; padding: 0.5rem 0; align-items: flex-start; }
.confirm-warn { font-size: 2rem; color: var(--amber-500); flex-shrink: 0; }
.confirm-body p { margin-bottom: 0.4rem; font-size: 0.9rem; color: var(--gray-700); }

.loading { text-align: center; padding: 2rem; color: var(--gray-400); }
.profit-neg { color: var(--red-500, #ef4444); }
</style>
