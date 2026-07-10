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
      <div class="field">
        <label for="reorder">Reorder Level</label>
        <InputNumber id="reorder" v-model="form.reorderLevel" class="w-full" :min="0" />
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

const defaultForm = () => ({ sku: '', name: '', description: '', unit: '', quantity: 0, reorderLevel: 0 })
const form = ref(defaultForm())

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const isEdit = computed(() => !!props.product)

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
</style>
