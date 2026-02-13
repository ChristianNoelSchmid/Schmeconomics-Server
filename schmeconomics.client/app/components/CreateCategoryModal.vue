<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import type { CreateCategoryRequest, CategoryModel } from '~/lib/openapi';

const props = defineProps<{
  accountId: string,
  visible: boolean,
  categoryToEdit?: CategoryModel | null
}>();

const emit = defineEmits<{
  closed: [],
  submitted: [req: CreateCategoryRequest]
}>();

const newCategoryState = reactive({
  name: props.categoryToEdit?.name || '',
  balance: props.categoryToEdit?.balance || 0,
  refillValue: props.categoryToEdit?.refillValue || 0
});

type Schema = typeof newCategoryState;

function validate(state: Partial<Schema>): FormError[] {
  const errors = [];
  if (!state.name) errors.push({ name: 'name', message: 'Required' });
  if (!state.balance) errors.push({ name: 'balance', message: 'Required' });
  if (!state.refillValue) errors.push({ name: 'refillValue', message: 'Required' });
  return errors;
}

function submitRequest() {
  emit('submitted', {
    accountId: props.accountId,
    name: newCategoryState.name,
    balance: newCategoryState.balance,
    refillValue: newCategoryState.refillValue
  });
  newCategoryState.name = "";
  newCategoryState.balance = 0;
  newCategoryState.refillValue = 0;
}

// Reset form when category to edit changes
watch(() => props.categoryToEdit, (newVal) => {
  if (newVal) {
    newCategoryState.name = newVal.name || '';
    newCategoryState.balance = newVal.balance || 0;
    newCategoryState.refillValue = newVal.refillValue || 0;
  } else {
    newCategoryState.name = '';
    newCategoryState.balance = 0;
    newCategoryState.refillValue = 0;
  }
}, { immediate: true });
</script>

<template>
  <UModal :open="props.visible">
    <template #content>
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold">{{ props.categoryToEdit ? 'Edit Category' : 'Create New Category' }}</h3>
        </template>

        <UForm class="space-y-4" :state="newCategoryState" :validate="validate">
          <UFormField label="Name">
            <UInput v-model="newCategoryState.name" />
          </UFormField>

          <UFormField label="Balance">
            <CurrencyInput v-model="newCategoryState.balance" />
          </UFormField>

          <UFormField label="Refill Value">
            <CurrencyInput v-model="newCategoryState.refillValue" />
          </UFormField>
        </UForm>

        <template #footer>
          <div class="flex justify-end space-x-2">
            <UButton color="neutral" variant="ghost" @click="emit('closed')">
              Cancel
            </UButton>
            <UButton color="primary" variant="solid" @click="submitRequest">
              {{ props.categoryToEdit ? 'Update' : 'Create' }}
            </UButton>
          </div>
        </template>
      </UCard>
    </template>
  </UModal>
</template>
