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
          <Column field="quantity" header="Qty" style="width: 90px" :bodyStyle="{ textAlign: 'left' }">
            <template #body="slotProps">
              <Tag :value="slotProps.data.quantity" severity="danger" />
            </template>
          </Column>
          <Column field="reorderLevel" header="Reorder" style="width: 100px" :bodyStyle="{ textAlign: 'right' }"></Column>
          <Column header="Cost Total" style="width: 130px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">
              {{ formatMoney(slotProps.data.costTotal) }}
            </template>
          </Column>
        </DataTable>
      </div>

      <!-- ===== Withdraw search by date range ===== -->
      <div class="panel">
        <div class="panel-header">
          <h3><i class="pi pi-shopping-cart"></i> Withdrawals</h3>
          <span class="muted">{{ filteredWithdraws.length }} record(s) · {{ settledWithdraws.length }} settled (stock deducted)</span>
        </div>

        <div class="wd-summary">
          <div class="wd-sum-card wd-sum-card--price">
            <div class="wd-sum-icon"><i class="pi pi-money-bill"></i></div>
            <div class="wd-sum-body">
              <div class="wd-sum-label">Grand Total Price</div>
              <div class="wd-sum-value">{{ formatMoney(periodNetPrice) }}</div>
            </div>
          </div>
          <div class="wd-sum-card" :class="periodNetProfit < 0 ? 'wd-sum-card--neg' : 'wd-sum-card--profit'">
            <div class="wd-sum-icon"><i class="pi pi-chart-line"></i></div>
            <div class="wd-sum-body">
              <div class="wd-sum-label">Grand Tota Profit</div>
              <div class="wd-sum-value">{{ formatMoney(periodNetProfit) }}</div>
            </div>
          </div>
        </div>

        <div class="wd-filter">
          <div class="wd-filter-field">
            <label>From</label>
            <Calendar v-model="dateFrom" dateFormat="dd/mm/yy" :showIcon="true" placeholder="Start date" class="w-full" />
          </div>
          <div class="wd-filter-field">
            <label>To</label>
            <Calendar v-model="dateTo" dateFormat="dd/mm/yy" :showIcon="true" placeholder="End date" class="w-full" />
          </div>
          <div class="wd-filter-field wd-search">
            <label>Search</label>
            <InputText v-model="wdSearch" placeholder="Withdraw No or Note..." class="w-full" />
          </div>
          <div class="wd-filter-field">
            <label>Status</label>
            <Select v-model="statusFilter" :options="statusOptions" optionLabel="label" optionValue="value"
                    placeholder="All" showClear class="w-full" />
          </div>
          <Button label="Reset" severity="secondary" outlined @click="resetFilter" class="wd-reset" />
        </div>

        <div v-if="!filteredWithdraws.length" class="empty">No withdrawals in this period.</div>

        <DataTable v-if="filteredWithdraws.length" :value="filteredWithdraws" tableStyle="min-width: 100%"
                   paginator :rows="10" :rowsPerPageOptions="[10, 20, 50]">
          <Column field="withdrawNo" header="Withdraw No" style="width: 200px">
            <template #body="slotProps"><span class="wd-no">{{ slotProps.data.withdrawNo }}</span></template>
          </Column>
          <Column field="date" header="Date" style="width: 130px">
            <template #body="slotProps">{{ new Date(slotProps.data.date).toLocaleDateString() }}</template>
          </Column>
          <Column field="note" header="Note">
            <template #body="slotProps">{{ slotProps.data.note || '—' }}</template>
          </Column>
          <Column field="totalQuantity" header="Qty" style="width: 90px" :bodyStyle="{ textAlign: 'center' }" />
          <Column field="totalPrice" header="Net Price" style="width: 130px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps"><strong>{{ formatMoney(slotProps.data.totalPrice) }}</strong></template>
          </Column>
          <Column field="totalProfit" header="Net Profit" style="width: 130px" :bodyStyle="{ textAlign: 'right' }">
            <template #body="slotProps">
              <strong :class="{ 'profit-neg': slotProps.data.totalProfit < 0 }">{{ formatMoney(slotProps.data.totalProfit) }}</strong>
            </template>
          </Column>
          <Column field="status" header="Status" style="width: 120px">
            <template #body="slotProps">
              <Tag :value="slotProps.data.status"
                   :severity="slotProps.data.status === 'Withdrawn' ? 'success' : 'info'" />
            </template>
          </Column>
        </DataTable>

        <div v-if="chartData.labels.length" class="wd-chart">
          <div class="wd-chart-title">Net Price & Net Profit by Date</div>
          <Chart type="line" :data="chartData" :options="chartOptions" class="wd-chart-canvas" />
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import Chart from 'primevue/chart'
import { productsApi } from '../api/products'
import { withdrawsApi } from '../api/withdraws'

const BLUE = '#2a78d6'
const AQUA = '#1baf7a'
const INK_PRIMARY = '#0b0b0b'
const INK_MUTED = '#898781'
const GRIDLINE = '#e1e0d9'

const loading = ref(true)
const products = ref([])
const lowStock = ref([])
const withdraws = ref([])

const totalCost = computed(() =>
  products.value.reduce((sum, p) => sum + (Number(p.costTotal) || 0), 0)
)
const productCount = computed(() => products.value.length)

// Withdraw filter state
function firstOfMonth(d = new Date()) {
  return new Date(d.getFullYear(), d.getMonth(), 1)
}
function lastOfMonth(d = new Date()) {
  return new Date(d.getFullYear(), d.getMonth() + 1, 0)
}

const dateFrom = ref(firstOfMonth())
const dateTo = ref(lastOfMonth())
const wdSearch = ref('')
const statusFilter = ref(null)
const statusOptions = [
  { label: 'Draft', value: 'Draft' },
  { label: 'Saved', value: 'Saved' },
  { label: 'Withdrawn', value: 'Withdrawn' }
]

function startOfDay(d) { const x = new Date(d); x.setHours(0, 0, 0, 0); return x }
function endOfDay(d) { const x = new Date(d); x.setHours(23, 59, 59, 999); return x }

const filteredWithdraws = computed(() => {
  let list = withdraws.value
  if (dateFrom.value) {
    const f = startOfDay(dateFrom.value).getTime()
    list = list.filter(w => new Date(w.date).getTime() >= f)
  }
  if (dateTo.value) {
    const t = endOfDay(dateTo.value).getTime()
    list = list.filter(w => new Date(w.date).getTime() <= t)
  }
  const q = wdSearch.value.trim().toLowerCase()
  if (q) {
    list = list.filter(w =>
      (w.withdrawNo || '').toLowerCase().includes(q) ||
      (w.note || '').toLowerCase().includes(q)
    )
  }
  if (statusFilter.value) {
    list = list.filter(w => w.status === statusFilter.value)
  }
  // newest first
  return [...list].sort((a, b) => new Date(b.date) - new Date(a.date))
})

const periodNetPrice = computed(() =>
  settledWithdraws.value.reduce((s, w) => s + (Number(w.totalPrice) || 0), 0)
)
const periodNetProfit = computed(() =>
  settledWithdraws.value.reduce((s, w) => s + (Number(w.totalProfit) || 0), 0)
)

// Sum only counts withdrawals that actually deducted stock
const settledWithdraws = computed(() =>
  filteredWithdraws.value.filter(w => w.status === 'Withdrawn')
)

// Daily aggregation of settled withdrawals for the chart (X = date, Y = amount)
const chartData = computed(() => {
  const byDay = new Map()
  for (const w of settledWithdraws.value) {
    const key = new Date(w.date).toLocaleDateString()
    const entry = byDay.get(key) || { price: 0, profit: 0 }
    entry.price += Number(w.totalPrice) || 0
    entry.profit += Number(w.totalProfit) || 0
    byDay.set(key, entry)
  }
  const labels = [...byDay.keys()].sort((a, b) => new Date(a) - new Date(b))
  return {
    labels,
    datasets: [
      {
        label: 'Net Price',
        data: labels.map(l => byDay.get(l).price),
        borderColor: BLUE,
        backgroundColor: BLUE,
        tension: 0.35,
        borderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6
      },
      {
        label: 'Net Profit',
        data: labels.map(l => byDay.get(l).profit),
        borderColor: AQUA,
        backgroundColor: AQUA,
        tension: 0.35,
        borderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6
      }
    ]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
      align: 'end',
      labels: { color: INK_PRIMARY, usePointStyle: true, pointStyle: 'circle', boxWidth: 10, font: { size: 12 } }
    },
    tooltip: {
      callbacks: {
        label: ctx => ` ${ctx.dataset.label}: ${Number(ctx.parsed.y).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
      }
    }
  },
  scales: {
    x: {
      title: { display: true, text: 'Date', color: INK_MUTED, font: { size: 12 } },
      grid: { display: false },
      ticks: { color: INK_MUTED }
    },
    y: {
      title: { display: true, text: 'Amount', color: INK_MUTED, font: { size: 12 } },
      beginAtZero: true,
      grid: { color: GRIDLINE },
      border: { display: false },
      ticks: { color: INK_MUTED, callback: v => Number(v).toLocaleString() }
    }
  }
}

function resetFilter() {
  dateFrom.value = firstOfMonth()
  dateTo.value = lastOfMonth()
  wdSearch.value = ''
  statusFilter.value = null
}

function formatMoney(value) {
  return Number(value || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

onMounted(async () => {
  try {
    const [all, low, wd] = await Promise.all([
      productsApi.getAll(),
      productsApi.getLowStock(),
      withdrawsApi.list()
    ])
    products.value = all.data
    lowStock.value = low.data
    withdraws.value = wd.data
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
  margin-bottom: 1.5rem;
}
.panel:last-child { margin-bottom: 0; }
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

.wd-filter {
  display: flex; align-items: flex-end; gap: 0.75rem; flex-wrap: wrap;
  margin-bottom: 1rem;
}
.wd-filter-field { display: flex; flex-direction: column; gap: 0.3rem; }
.wd-filter-field label { font-size: 0.75rem; font-weight: 600; color: var(--gray-500); text-transform: uppercase; }
.wd-filter-field.wd-search { flex: 1; min-width: 200px; }
.wd-filter-field:has(.p-select) { min-width: 150px; }
.wd-reset { margin-bottom: 0.1rem; }
.wd-no { font-weight: 600; color: var(--rose-600); }
.profit-neg { color: var(--red-500, #ef4444); }

.wd-chart { height: 340px; margin-top: 1.5rem; padding: 0.5rem 0.25rem 0; }
.wd-chart-title { font-size: 0.85rem; font-weight: 700; color: var(--gray-700); margin-bottom: 0.25rem; }
.wd-chart-canvas { height: calc(100% - 1.5rem); }

.wd-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1rem;
  margin-bottom: 1.25rem;
}
.wd-sum-card {
  border-radius: 14px;
  padding: 1.1rem 1.25rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  border: 1px solid var(--gray-100);
}
.wd-sum-icon {
  width: 48px; height: 48px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.4rem; flex-shrink: 0;
}
.wd-sum-body { display: flex; flex-direction: column; gap: 0.15rem; min-width: 0; }
.wd-sum-label { font-size: 0.78rem; font-weight: 600; color: var(--gray-500); }
.wd-sum-label small { font-weight: 400; }
.wd-sum-value { font-size: 1.55rem; font-weight: 700; color: var(--gray-800); line-height: 1.1; word-break: break-word; }

.wd-sum-card--price { background: #fff; border: 2px solid var(--blue-400, #2a78d6); }
.wd-sum-card--price .wd-sum-icon { background: var(--blue-50); color: var(--blue-600); }
.wd-sum-card--price .wd-sum-value { color: var(--blue-700); }

.wd-sum-card--profit { background: linear-gradient(135deg, var(--green-50), var(--emerald-50)); border-color: var(--green-100); }
.wd-sum-card--profit .wd-sum-icon { background: var(--green-100); color: var(--green-600); }
.wd-sum-card--profit .wd-sum-value { color: var(--green-700); }

.wd-sum-card--neg { background: linear-gradient(135deg, var(--red-50), var(--rose-50)); border-color: var(--red-100); }
.wd-sum-card--neg .wd-sum-icon { background: var(--red-100); color: var(--red-600); }
.wd-sum-card--neg .wd-sum-value { color: var(--red-600); }

@media (max-width: 640px) {
  .stat-card--primary { grid-column: span 1; }
}
</style>
