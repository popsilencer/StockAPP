<template>
  <div class="wd-detail-page">
    <div class="page-header">
      <div class="header-title">
        <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="$router.push('/withdraw')" />
        <h2>Withdrawal Detail</h2>
      </div>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

    <template v-else-if="withdraw">
      <!-- Master -->
      <div class="master-card">
        <div class="master-row">
          <div class="field"><label>Withdraw No</label><div class="value wd-no">{{ withdraw.withdrawNo }}</div></div>
          <div class="field"><label>Date</label><div class="value">{{ new Date(withdraw.date).toLocaleDateString() }}</div></div>
          <div class="field"><label>Items</label><div class="value">{{ withdraw.items.length }}</div></div>
          <div class="field"><label>Total Qty</label><div class="value">{{ totalQty }}</div></div>
        </div>
        <div class="field note-field" v-if="withdraw.note"><label>Note</label><div class="value">{{ withdraw.note }}</div></div>
      </div>

      <!-- Detail -->
      <div class="list-card">
        <DataTable :value="withdraw.items" tableStyle="min-width: 100%">
          <Column field="sku" header="SKU" style="width: 140px"></Column>
          <Column field="productName" header="Product"></Column>
          <Column field="quantity" header="Qty" style="width: 100px"></Column>
        </DataTable>
      </div>
    </template>

    <div v-else class="loading">Withdrawal not found.</div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { withdrawsApi } from '../api/withdraws'

const route = useRoute()
const withdraw = ref(null)
const loading = ref(true)

const totalQty = computed(() =>
  withdraw.value ? (withdraw.value.items || []).reduce((s, i) => s + i.quantity, 0) : 0
)

onMounted(async () => {
  try {
    const { data } = await withdrawsApi.get(route.params.id)
    withdraw.value = data
  } catch { withdraw.value = null }
  finally { loading.value = false }
})
</script>

<style scoped>
.wd-detail-page { padding: 0; }
.page-header { margin-bottom: 1.5rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }

.master-card { background: linear-gradient(135deg, var(--pink-50), var(--rose-50)); border: 1px solid var(--pink-100); border-radius: 12px; padding: 1.25rem; margin-bottom: 1.5rem; }
.master-row { display: flex; gap: 2rem; flex-wrap: wrap; }
.field { display: flex; flex-direction: column; }
.field label { font-size: 0.75rem; font-weight: 600; color: var(--gray-500); text-transform: uppercase; margin-bottom: 0.25rem; }
.value { font-size: 1rem; font-weight: 600; color: var(--gray-800); }
.wd-no { color: var(--rose-600); }
.note-field { margin-top: 1rem; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
.loading { text-align: center; padding: 2rem; color: var(--gray-400); }
</style>
