<template>
  <Dialog v-model:visible="isOpen" modal :header="`Stock ${initialType === 'In' ? 'In' : 'Out'} — ${product?.name}`" :style="{ width: '400px' }">
    <form @submit.prevent="submit">
      <div class="field">
        <label>Type</label>
        <div class="type-row">
          <Button type="button" :label="'Stock In'" :severity="form.type === 'In' ? 'success' : 'secondary'"
            :outlined="form.type !== 'In'" @click="form.type = 'In'" class="flex-1" />
          <Button type="button" :label="'Stock Out'" :severity="form.type === 'Out' ? 'danger' : 'secondary'"
            :outlined="form.type !== 'Out'" @click="form.type = 'Out'" class="flex-1" />
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
      <Button label="Cancel" text severity="secondary" @click="close" />
      <Button :label="`Confirm ${initialType === 'In' ? 'In' : 'Out'}`" :severity="initialType === 'In' ? 'success' : 'danger'" @click="submit" />
    </template>
  </Dialog>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  visible: Boolean,
  product: Object,
  initialType: { type: String, default: 'In' }
})
const emit = defineEmits(['update:visible', 'save', 'close'])

const form = ref({ type: 'In', quantity: 1, note: '' })
const isOpen = ref(false)

watch(() => props.visible, (v) => { isOpen.value = v }, { immediate: true })
watch(() => props.initialType, (t) => { form.value.type = t }, { immediate: true })

function close() {
  isOpen.value = false
  emit('close')
  emit('update:visible', false)
}

function submit() {
  emit('save', { type: form.value.type, quantity: form.value.quantity, note: form.value.note })
}
</script>

<style scoped>
.field { margin-bottom: 0.75rem; }
.field label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.875rem; }
.type-row { display: flex; gap: 0.5rem; }
.flex-1 { flex: 1; }
.current-stock { color: var(--text-color-secondary); font-size: 0.875rem; margin-top: 0.5rem; }
.w-full { width: 100%; }
</style>
