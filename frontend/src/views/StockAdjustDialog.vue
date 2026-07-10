<template>
  <Dialog v-model:visible="visible" modal :header="`Stock ${initialType === 'In' ? 'In' : 'Out'} — ${product?.name}`" :style="{ width: '400px' }">
    <form @submit.prevent="submit">
      <div class="field">
        <label>Type</label>
        <div class="type-row">
          <Button type="button" label="Stock In" icon="pi pi-arrow-down"
            :severity="form.type === 'In' ? 'success' : 'secondary'"
            :outlined="form.type !== 'In'" class="flex-1" @click="form.type = 'In'" />
          <Button type="button" label="Stock Out" icon="pi pi-arrow-up"
            :severity="form.type === 'Out' ? 'danger' : 'secondary'"
            :outlined="form.type !== 'Out'" class="flex-1" @click="form.type = 'Out'" />
        </div>
      </div>
      <div class="field">
        <label for="qty">Quantity</label>
        <InputNumber id="qty" v-model="form.quantity" class="w-full" :min="1" />
      </div>
      <div class="field">
        <label for="note">Note</label>
        <Textarea id="note" v-model="form.note" class="w-full" rows="2" />
      </div>
      <p class="current-stock">Current stock: <strong>{{ product?.quantity }} {{ product?.unit }}</strong></p>
    </form>
    <template #footer>
      <Button label="Cancel" severity="secondary" outlined @click="visible = false" />
      <Button :label="`Confirm ${form.type === 'In' ? 'In' : 'Out'}`"
        :severity="form.type === 'In' ? 'success' : 'danger'" @click="submit" />
    </template>
  </Dialog>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  visible: Boolean,
  product: Object,
  initialType: { type: String, default: 'In' }
})
const emit = defineEmits(['update:visible', 'save'])

const form = ref({ type: 'In', quantity: 1, note: '' })

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

watch(() => props.initialType, (t) => { form.value.type = t }, { immediate: true })

function submit() {
  emit('save', { type: form.value.type, quantity: form.value.quantity, note: form.value.note })
}
</script>

<style scoped>
.field { margin-bottom: 0.75rem; }
.field label { display: block; margin-bottom: 0.4rem; font-weight: 600; font-size: 0.85rem; color: var(--gray-600); }
.type-row { display: flex; gap: 0.5rem; }
.flex-1 { flex: 1; }
.current-stock { color: var(--gray-400); font-size: 0.85rem; margin-top: 0.5rem; }
.w-full { width: 100%; }
</style>
