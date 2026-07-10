<template>
  <Dialog v-model:visible="visible" modal :header="`Stock ${form.type === 'In' ? 'In' : 'Out'} — ${product?.name}`" :style="{ width: '400px' }">
    <form @submit.prevent="openConfirm">
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
        :icon="form.type === 'In' ? 'pi pi-arrow-down' : 'pi pi-arrow-up'"
        :severity="form.type === 'In' ? 'success' : 'danger'" @click="openConfirm" />
    </template>

    <!-- Confirmation Dialog -->
    <Dialog v-model:visible="confirmVisible" modal :header="`Confirm Stock ${form.type === 'In' ? 'In' : 'Out'}`" :style="{ width: '380px' }">
      <div class="confirm-body">
        <i :class="form.type === 'In' ? 'pi pi-arrow-circle-down confirm-icon in' : 'pi pi-arrow-circle-up confirm-icon out'"></i>
        <p>Are you sure you want to {{ form.type === 'In' ? 'receive' : 'issue' }} <strong>{{ form.quantity }} {{ product?.unit }}</strong> of <strong>{{ product?.name }}</strong>?</p>
      </div>
      <template #footer>
        <Button label="No, cancel" severity="secondary" outlined @click="confirmVisible = false" />
        <Button label="Yes, confirm" :severity="form.type === 'In' ? 'success' : 'danger'" @click="confirmSubmit" />
      </template>
    </Dialog>
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
const confirmVisible = ref(false)

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

watch(() => props.initialType, (t) => { form.value.type = t }, { immediate: true })

function openConfirm() {
  if (!form.value.quantity || form.value.quantity < 1) return
  confirmVisible.value = true
}

function confirmSubmit() {
  confirmVisible.value = false
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
.confirm-body { display: flex; align-items: center; gap: 1rem; padding: 0.5rem 0; }
.confirm-body p { margin: 0; font-size: 0.9rem; color: var(--gray-700); }
.confirm-icon { font-size: 2.5rem; }
.confirm-icon.in { color: var(--green-500); }
.confirm-icon.out { color: var(--rose-500); }
</style>
