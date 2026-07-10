<template>
  <div class="user-page">
    <div class="page-header">
      <div class="header-title">
        <i class="pi pi-users title-icon"></i>
        <h2>User</h2>
      </div>
      <Button label="Add User" icon="pi pi-plus" class="new-btn" @click="openAdd" />
    </div>

    <div class="list-card">
      <div class="toolbar">
        <InputText v-model="searchTerm" placeholder="Search by Username or Company..." class="search-input" />
        <span class="result-count" v-if="!loading">{{ filteredUsers.length }} record(s)</span>
      </div>

      <DataTable :value="filteredUsers" :loading="loading" tableStyle="min-width: 100%"
                 emptyMessage="No users found."
                 paginator :rows="10" :rowsPerPageOptions="[10, 20, 50, 100]"
                 currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
                 paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown">
        <Column field="username" header="Username" style="width: 220px">
          <template #body="slotProps">
            <span class="uname">{{ slotProps.data.username }}</span>
          </template>
        </Column>
        <Column field="companyName" header="Company">
          <template #body="slotProps">{{ slotProps.data.companyName || '—' }}</template>
        </Column>
        <Column header="Action" style="width: 180px">
          <template #body="slotProps">
            <div class="action-btns action-right">
              <Button icon="pi pi-pencil" severity="info" text rounded size="small"
                      @click="openEdit(slotProps.data)" title="Edit" />
              <Button icon="pi pi-key" severity="warn" text rounded size="small"
                      @click="openPassword(slotProps.data)" title="Reset Password" />
              <Button v-if="slotProps.data.username !== 'admin'" icon="pi pi-trash" severity="danger" text rounded size="small"
                      @click="confirmDelete(slotProps.data)" title="Delete" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>

    <!-- Form Dialog -->
    <Dialog v-model:visible="formVisible" modal :header="editingId ? 'Edit User' : 'Add User'" :style="{ width: '460px' }">
      <div class="form-grid">
        <div class="field">
          <label>Username</label>
          <InputText v-model="form.username" placeholder="Username" class="w-full" />
        </div>
        <div class="field" v-if="!editingId">
          <label>Password</label>
          <Password v-model="form.password" :feedback="false" toggleMask placeholder="Min 4 chars" class="w-full" />
        </div>
        <div class="field field-full">
          <label>Company</label>
          <Select v-model="form.companyId" :options="companies" optionLabel="companyName" optionValue="id"
            placeholder="Select company" filter showClear class="w-full" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="formVisible = false" />
        <Button label="Save" icon="pi pi-check" :loading="processing" @click="save" />
      </template>
    </Dialog>

    <!-- Reset Password Dialog -->
    <Dialog v-model:visible="passwordVisible" modal :header="`Reset Password — ${passwordTarget?.username}`" :style="{ width: '400px' }">
      <div class="field">
        <label>New Password</label>
        <Password v-model="newPassword" :feedback="false" toggleMask placeholder="Min 4 chars" class="w-full" />
      </div>
      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="passwordVisible = false" />
        <Button label="Reset" icon="pi pi-key" severity="warn" :loading="processing" @click="resetPassword" />
      </template>
    </Dialog>

    <!-- Delete Confirm -->
    <Dialog v-model:visible="deleteVisible" modal header="Delete User" :style="{ width: '400px' }">
      <div class="confirm-body">
        <i class="pi pi-exclamation-triangle confirm-warn"></i>
        <p>Delete user <strong>{{ deleteTarget?.username }}</strong>? This cannot be undone.</p>
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
import { usersApi } from '../api/users'
import { companiesApi } from '../api/companies'
import { useToast } from 'primevue/usetoast'

const toast = useToast()
const users = ref([])
const companies = ref([])
const loading = ref(false)
const searchTerm = ref('')
const formVisible = ref(false)
const passwordVisible = ref(false)
const deleteVisible = ref(false)
const editingId = ref(null)
const processing = ref(false)
const deleteTarget = ref(null)
const passwordTarget = ref(null)
const newPassword = ref('')

const form = ref({ username: '', password: '', companyId: null })

const filteredUsers = computed(() => {
  const q = searchTerm.value.trim().toLowerCase()
  if (!q) return users.value
  return users.value.filter(u =>
    (u.username || '').toLowerCase().includes(q) ||
    (u.companyName || '').toLowerCase().includes(q)
  )
})

onMounted(async () => {
  await fetchCompanies()
  await fetchList()
})

async function fetchList() {
  loading.value = true
  try {
    const { data } = await usersApi.list()
    users.value = data
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users', life: 3000 })
  } finally {
    loading.value = false
  }
}

async function fetchCompanies() {
  try {
    const { data } = await companiesApi.list()
    companies.value = data
  } catch { /* ignore */ }
}

function openAdd() {
  editingId.value = null
  form.value = { username: '', password: '', companyId: null }
  formVisible.value = true
}

function openEdit(u) {
  editingId.value = u.id
  form.value = { username: u.username, password: '', companyId: u.companyId }
  formVisible.value = true
}

async function save() {
  if (!form.value.username) {
    toast.add({ severity: 'warn', summary: 'Required', detail: 'Username is required', life: 3000 })
    return
  }
  if (!editingId.value && !form.value.password) {
    toast.add({ severity: 'warn', summary: 'Required', detail: 'Password is required', life: 3000 })
    return
  }
  processing.value = true
  try {
    if (editingId.value) {
      await usersApi.update(editingId.value, {
        username: form.value.username,
        companyId: form.value.companyId
      })
      toast.add({ severity: 'success', summary: 'User updated', life: 3000 })
    } else {
      await usersApi.create(form.value)
      toast.add({ severity: 'success', summary: 'User created', life: 3000 })
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

function openPassword(u) {
  passwordTarget.value = u
  newPassword.value = ''
  passwordVisible.value = true
}

async function resetPassword() {
  if (!newPassword.value || newPassword.value.length < 4) {
    toast.add({ severity: 'warn', summary: 'Too short', detail: 'Password must be at least 4 chars', life: 3000 })
    return
  }
  processing.value = true
  try {
    await usersApi.changePassword(passwordTarget.value.id, { newPassword: newPassword.value })
    toast.add({ severity: 'success', summary: 'Password reset', life: 3000 })
    passwordVisible.value = false
  } catch (err) {
    const msg = err.response?.data?.message || 'Reset failed'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 4000 })
  } finally {
    processing.value = false
  }
}

function confirmDelete(u) {
  deleteTarget.value = u
  deleteVisible.value = true
}

async function remove() {
  processing.value = true
  try {
    await usersApi.delete(deleteTarget.value.id)
    toast.add({ severity: 'success', summary: 'User deleted', life: 3000 })
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
.user-page { padding: 0; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; flex-wrap: wrap; gap: 0.75rem; }
.header-title { display: flex; align-items: center; gap: 0.75rem; }
.title-icon { font-size: 1.5rem; color: var(--rose-500); }
.header-title h2 { font-size: 1.3rem; font-weight: 700; color: var(--gray-800); }
.new-btn { font-size: 0.85rem; }

.list-card { background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
.toolbar { display: flex; justify-content: space-between; align-items: center; gap: 1rem; padding: 1rem 1.25rem; background: linear-gradient(135deg, var(--pink-50), var(--rose-50)); border-bottom: 1px solid var(--pink-100); flex-wrap: wrap; }
.search-input { width: 100%; max-width: 420px; }
.result-count { font-size: 0.8rem; color: var(--gray-500); font-weight: 600; white-space: nowrap; }

.uname { font-weight: 600; color: var(--rose-600); }
.action-btns { display: flex; gap: 4px; }
.action-right { justify-content: flex-end; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.field { display: flex; flex-direction: column; }
.field-full { grid-column: 1 / -1; }
.field label { display: block; margin-bottom: 0.4rem; font-weight: 600; font-size: 0.85rem; color: var(--gray-600); }
.w-full { width: 100%; }

.confirm-body { display: flex; gap: 1rem; padding: 0.5rem 0; align-items: center; }
.confirm-warn { font-size: 2rem; color: var(--amber-500); flex-shrink: 0; }
.confirm-body p { font-size: 0.9rem; color: var(--gray-700); }
</style>
