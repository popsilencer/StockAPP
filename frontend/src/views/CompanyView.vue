<template>
  <div class="company-page">
    <div class="page-header">
      <div class="header-title">
        <i class="pi pi-building title-icon"></i>
        <h2>Company</h2>
      </div>
      <Button label="Add Company" icon="pi pi-plus" class="new-btn" @click="openAdd" />
    </div>

    <div class="list-card">
      <div class="toolbar">
        <InputText v-model="searchTerm" placeholder="Search by Tax, Company Name or Address..." class="search-input" />
        <span class="result-count" v-if="!loading">{{ filteredCompanies.length }} record(s)</span>
      </div>

      <DataTable :value="filteredCompanies" :loading="loading" tableStyle="min-width: 100%"
                 emptyMessage="No companies found."
                 paginator :rows="10" :rowsPerPageOptions="[10, 20, 50, 100]"
                 currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
                 paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown">
        <Column header="Tax" style="width: 200px">
          <template #body="slotProps">
            <span class="tax-no">{{ slotProps.data.tax }}</span>
          </template>
        </Column>
        <Column field="companyName" header="Company Name"></Column>
        <Column field="address" header="Address">
          <template #body="slotProps">{{ slotProps.data.address || '—' }}</template>
        </Column>
        <Column header="Action" style="width: 120px">
          <template #body="slotProps">
            <div class="action-btns action-right">
              <Button icon="pi pi-pencil" severity="info" text rounded size="small"
                      @click="openEdit(slotProps.data)" title="Edit" />
              <Button icon="pi pi-trash" severity="danger" text rounded size="small"
                      @click="confirmDelete(slotProps.data)" title="Delete" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <!-- Form Dialog -->
    <Dialog v-model:visible="formVisible" modal :header="editingId ? 'Edit Company' : 'Add Company'" :style="{ width: '480px' }">
      <div class="form-grid">
        <div class="field">
          <label>Tax ID</label>
          <InputText :modelValue="form.tax" @update:modelValue="onTaxInput"
            @keypress="onTaxKeypress" placeholder="13 digits" maxlength="13"
            class="w-full"
            :class="{ 'p-invalid': form.tax.length > 0 && form.tax.length !== 13 }" />
          <small class="field-error" v-if="form.tax.length > 0 && form.tax.length !== 13">
            Tax must be exactly 13 digits ({{ form.tax.length }}/13)
          </small>
        </div>
        <div class="field">
          <label>Company Name</label>
          <InputText v-model="form.companyName" placeholder="Company name" class="w-full" />
        </div>
        <div class="field field-full">
          <label>Address</label>
          <Textarea v-model="form.address" rows="3" placeholder="Address" class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="formVisible = false" />
        <Button label="Save" icon="pi pi-check" :loading="processing" @click="save" />
      </template>
    </Dialog>

    <!-- Delete Confirm -->
    <Dialog v-model:visible="deleteVisible" modal header="Delete Company" :style="{ width: '400px' }">
      <div class="confirm-body">
        <i class="pi pi-exclamation-triangle confirm-warn"></i>
        <p>Delete <strong>{{ deleteTarget?.companyName }}</strong>? This cannot be undone.</p>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="deleteVisible = false" />
        <Button label="Delete" icon="pi pi-trash" severity="danger" :loading="processing" @click="remove" />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { companiesApi } from '../api/companies'
import { useToast } from 'primevue/usetoast'

const toast = useToast()
const companies = ref([])
const loading = ref(false)
const searchTerm = ref('')
const formVisible = ref(false)
const deleteVisible = ref(false)
const editingId = ref(null)
const processing = ref(false)
const deleteTarget = ref(null)

const form = ref({ tax: '', companyName: '', address: '' })

const filteredCompanies = computed(() => {
  const q = searchTerm.value.trim().toLowerCase()
  if (!q) return companies.value
  return companies.value.filter(c =>
    (c.tax || '').toLowerCase().includes(q) ||
    (c.companyName || '').toLowerCase().includes(q) ||
    (c.address || '').toLowerCase().includes(q)
  )
})

function onTaxInput(val) {
  form.value.tax = (val || '').replace(/[^0-9]/g, '').slice(0, 13)
}

function onTaxKeypress(event) {
  if (!/[0-9]/.test(event.key)) {
    event.preventDefault()
  }
}

onMounted(fetchList)

async function fetchList() {
  loading.value = true
  try {
    const { data } = await companiesApi.list()
    companies.value = data
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load companies', life: 3000 })
  } finally {
    loading.value = false
  }
}

function openAdd() {
  editingId.value = null
  form.value = { tax: '', companyName: '', address: '' }
  formVisible.value = true
}

function openEdit(c) {
  editingId.value = c.id
  form.value = { tax: c.tax, companyName: c.companyName, address: c.address || '' }
  formVisible.value = true
}

async function save() {
  if (!/^\d{13}$/.test(form.value.tax)) {
    toast.add({ severity: 'warn', summary: 'Invalid Tax', detail: 'Tax must be exactly 13 digits', life: 3000 })
    return
  }
  if (!form.value.companyName) {
    toast.add({ severity: 'warn', summary: 'Required', detail: 'Company Name is required', life: 3000 })
    return
  }
  processing.value = true
  try {
    if (editingId.value) {
      await companiesApi.update(editingId.value, form.value)
      toast.add({ severity: 'success', summary: 'Company updated', life: 3000 })
    } else {
      await companiesApi.create(form.value)
      toast.add({ severity: 'success', summary: 'Company created', life: 3000 })
    }
    formVisible.value = false
    await fetchList()
  } catch (err) {
    const msg = err.response?.data?.message || 'Save failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 4000 })
  } finally {
    processing.value = false
  }
}

function confirmDelete(c) {
  deleteTarget.value = c
  deleteVisible.value = true
}

async function remove() {
  processing.value = true
  try {
    await companiesApi.delete(deleteTarget.value.id)
    toast.add({ severity: 'success', summary: 'Company deleted', life: 3000 })
    deleteVisible.value = false
    await fetchList()
  } catch (err) {
    const msg = err.response?.data?.message || 'Delete failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 4000 })
  } finally {
    processing.value = false
  }
}
</script>

<style scoped>
.company-page { padding: 0; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; }
.title-icon { font-size: 1.5rem; color: var(--rose-500); }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }
.new-btn { font-size: 0.85rem; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
.toolbar { display: flex; justify-content: space-between; align-items: center; gap: 1rem; padding: 1rem 1.25rem; background: linear-gradient(135deg, var(--pink-50), var(--rose-50)); border-bottom: 1px solid var(--pink-100); flex-wrap: wrap; }
.search-input { width: 100%; max-width: 420px; }
.result-count { font-size: 0.8rem; color: var(--gray-500); font-weight: 600; white-space: nowrap; }

.tax-no { font-weight: 600; color: var(--rose-600); }
.action-btns { display: flex; gap: 4px; }
.action-right { justify-content: flex-end; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.field { display: flex; flex-direction: column; }
.field-full { grid-column: 1 / -1; }
.field label { display: block; margin-bottom: 0.4rem; font-weight: 600; font-size: 0.85rem; color: var(--gray-600); }
.w-full { width: 100%; }
.field-error { color: var(--rose-600); font-size: 0.75rem; margin-top: 0.25rem; display: block; }
:deep(.p-invalid) { border-color: var(--rose-500) !important; }

.confirm-body { display: flex; gap: 1rem; padding: 0.5rem 0; align-items: center; }
.confirm-warn { font-size: 2rem; color: var(--amber-500); flex-shrink: 0; }
.confirm-body p { font-size: 0.9rem; color: var(--gray-700); }
</style>
