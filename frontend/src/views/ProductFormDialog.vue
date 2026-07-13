<template>
  <Dialog v-model:visible="visible" modal :header="isEdit ? 'Edit Product' : 'Add Product'" :style="{ width: '450px' }">
    <form @submit.prevent="submit">
      <div class="field">
        <label for="sku">SKU</label>
        <InputText id="sku" v-model="form.sku" class="w-full" required />
      </div>
      <div class="field">
        <label for="name">Name</label>
        <InputText id="name" v-model="form.name" class="w-full" required />
      </div>
      <div class="field">
        <label for="description">Description</label>
        <Textarea id="description" v-model="form.description" class="w-full" rows="2" />
      </div>
      <div class="field-row">
        <div class="field">
          <label for="unit">Unit</label>
          <InputText id="unit" v-model="form.unit" class="w-full" placeholder="pcs" required />
        </div>
        <div class="field">
          <label for="quantity">Quantity</label>
          <InputNumber id="quantity" v-model="form.quantity" class="w-full" :min="0" />
        </div>
      </div>
      <div class="field-row">
        <div class="field">
          <label for="cost">Cost (per unit)</label>
          <InputNumber id="cost" v-model="form.cost" class="w-full" mode="decimal" :minFractionDigits="2" :maxFractionDigits="2" :min="0" />
        </div>
        <div class="field">
          <label for="price">Price (per unit)</label>
          <InputNumber id="price" v-model="form.price" class="w-full" mode="decimal" :minFractionDigits="2" :maxFractionDigits="2" :min="0" />
        </div>
      </div>
      <div class="field-row">
        <div class="field">
          <label for="reorder">Reorder Level</label>
          <InputNumber id="reorder" v-model="form.reorderLevel" class="w-full" :min="0" />
        </div>
      </div>
      <div class="cost-total">
        <span>Profit (price − cost):</span>
        <strong>{{ formatMoney(profit) }}</strong>
      </div>
      <div class="cost-total">
        <span>Cost Total (cost × qty):</span>
        <strong>{{ formatMoney(costTotal) }}</strong>
      </div>
    </form>
    <template #footer>
      <Button label="Cancel" severity="secondary" outlined @click="visible = false" />
      <Button :label="isEdit ? 'Save' : 'Create'" @click="submit" />
    </template>
  </Dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  visible: Boolean,
  product: Object
})
const emit = defineEmits(['update:visible', 'save'])

const defaultForm = () => ({ sku: '', name: '', description: '', unit: '', quantity: 0, cost: 0, price: 0, reorderLevel: 0 })
const form = ref(defaultForm())

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const isEdit = computed(() => !!props.product)

// Live total: recomputes whenever cost or quantity changes
const costTotal = computed(() => (Number(form.value.cost) || 0) * (Number(form.value.quantity) || 0))

// Live profit: recomputes whenever price or cost changes
const profit = computed(() => (Number(form.value.price) || 0) - (Number(form.value.cost) || 0))

function formatMoney(value) {
  const num = Number(value || 0)
  return num.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

watch(() => props.product, (p) => {
  if (p) form.value = { ...p }
  else form.value = defaultForm()
}, { immediate: true })

function submit() {
  emit('save', { ...form.value })
}
</script>

<style scoped>
.field { margin-bottom: 0.75rem; }
.field label { display: block; margin-bottom: 0.4rem; font-weight: 600; font-size: 0.85rem; color: var(--gray-600); }
.field-row { display: flex; gap: 1rem; }
.field-row .field { flex: 1; }
.w-full { width: 100%; }
.cost-total { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; padding: 0.6rem 0.8rem; background: var(--surface-100, #f1f5f9); border-radius: 6px; font-size: 0.9rem; }
.cost-total strong { color: var(--primary-color, #2563eb); }</style>
