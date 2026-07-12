<template>
  <div class="dashboard-page">
    <div class="page-header">
      <h2>Dashboard</h2>
    </div>

    <div v-if="loading" class="loading">Loading…</div>

    <template v-else>
      <div class="stat-grid">
        <!-- Main highlight: total cost of all stock on hand -->
        <div class="stat-card stat-card--primary">
          <div class="stat-icon"><i class="pi pi-wallet"></i></div>
          <div class="stat-body">
            <div class="stat-label">Total Stock Cost <small>(Σ cost × qty)</small></div>
            <div class="stat-value">{{ formatMoney(totalCost) }}</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon stat-icon--blue"><i class="pi pi-box"></i></div>
          <div class="stat-body">
            <div class="stat-label">Products</div>
            <div class="stat-value">{{ productCount }}</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon stat-icon--amber"><i class="pi pi-exclamation-triangle"></i></div>
          <div class="stat-body">
            <div class="stat-label">Low Stock Items</div>
            <div class="stat-value">{{ lowStock.length }}</div>
          </div>
        </div>
      </div>

      <!-- Low stock list -->
      <div class="panel">
        <div class="panel-header">
          <h3><i class="pi pi-exclamation-triangle"></i> Low Stock</h3>
          <span class="muted">{{ lowStock.length }} item(s) at or below reorder level</span>
        </div>

        <div v-if="!lowStock.length" class="empty">No low stock items 🎉</div>

        <DataTable v-else :value="lowStock" tableStyle="min-width: 100%"
                   paginator :rows="10" :rowsPerPageOptions="[10, 20, 50]">
          <Column field="sku" header="SKU" style="width: 120px"></Column>
          <Column field="name" header="Name"></Column>
          <Column field="unit" header="Unit" style="width: 90px"></Column>
          <Column field="quantity" header="Qty" style="width: 90px">
            <template #body="slotProps">
              <Tag :value="slotProps.data.quantity" severity="danger" />
            </template>
          </Column>
          <Column field="reorderLevel" header="Reorder" style="width: 100px"></Column>
          <Column header="Cost Total" style="width: 130px">
            <template #body="slotProps">
              {{ formatMoney(slotProps.data.costTotal) }}
            </template>
          </Column>
        </DataTable>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { productsApi } from '../api/products'

const loading = ref(true)
const products = ref([])
const lowStock = ref([])

const totalCost = computed(() =>
  products.value.reduce((sum, p) => sum + (Number(p.costTotal) || 0), 0)
)
const productCount = computed(() => products.value.length)

function formatMoney(value) {
  return Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

onMounted(async () => {
  try {
    const [all, low] = await Promise.all([
      productsApi.getAll(),
      productsApi.getLowStock()
    ])
    products.value = all.data
    lowStock.value = low.data
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.dashboard-page { padding: 0; }
.dashboard-page h2 {
  font-size: 1.4rem; font-weight: 700; color: var(--gray-800);
  background: linear-gradient(135deg, var(--pink-500), var(--rose-500));
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
}
.loading { padding: 2rem; color: var(--gray-500); }

.stat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.25rem;
  margin-bottom: 1.5rem;
}

.stat-card {
  background: #fff;
  border-radius: 16px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1.1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  border: 1px solid var(--gray-100);
  transition: transform 0.2s, box-shadow 0.2s;
}
.stat-card:hover { transform: translateY(-2px); box-shadow: 0 8px 24px rgba(0,0,0,0.08); }

.stat-card--primary {
  background: linear-gradient(135deg, var(--pink-400), var(--rose-500));
  border: none;
  grid-column: span 2;
}
.stat-card--primary .stat-label,
.stat-card--primary .stat-value { color: #fff; }
.stat-card--primary .stat-label small { opacity: 0.85; }

.stat-icon {
  width: 52px; height: 52px; border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.5rem; flex-shrink: 0;
  background: var(--pink-50); color: var(--pink-600);
}
.stat-card--primary .stat-icon { background: rgba(255,255,255,0.22); color: #fff; }
.stat-icon--blue { background: var(--blue-50); color: var(--blue-500); }
.stat-icon--amber { background: var(--amber-50); color: var(--amber-500); }

.stat-body { display: flex; flex-direction: column; gap: 0.2rem; min-width: 0; }
.stat-label { font-size: 0.8rem; color: var(--gray-500); font-weight: 600; }
.stat-label small { font-weight: 400; }
.stat-value { font-size: 1.7rem; font-weight: 700; color: var(--gray-800); line-height: 1.1; word-break: break-word; }

.panel {
  background: #fff;
  border-radius: 16px;
  padding: 1.25rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  border: 1px solid var(--gray-100);
}
.panel-header {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 1rem; flex-wrap: wrap; gap: 0.5rem;
}
.panel-header h3 {
  font-size: 1.05rem; font-weight: 700; color: var(--gray-800);
  display: flex; align-items: center; gap: 0.5rem;
}
.panel-header h3 i { color: var(--amber-500); }
.muted { font-size: 0.8rem; color: var(--gray-500); }
.empty { padding: 2rem; text-align: center; color: var(--gray-500); }

@media (max-width: 640px) {
  .stat-card--primary { grid-column: span 1; }
}
</style>
