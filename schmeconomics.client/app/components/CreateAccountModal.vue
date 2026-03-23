<script setup lang="ts">
  import type { FormError } from '@nuxt/ui';
import type { AccountModel } from '~/lib/openapi';

  const props = defineProps<{
    visible: boolean,
    editingAccount: AccountModel | null
  }>();

  const emit = defineEmits<{
    closed: [],
    submitted: [name: string]
  }>();

  const state = reactive({
    name: props.editingAccount?.name || ''
  });

  type Schema = typeof state;

  function validateAccount(state: Partial<Schema>): FormError[] {
    const errors = [];
    if(state.name == null || state.name == '') 
      errors.push({ name: 'name', message: 'Required' });

    return errors;
  }
</script>

<template>
  <UModal fullscreen :open="visible">
    <template #content>
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold">{{ editingAccount != null ? 'Edit Account' : 'Create New Account' }}</h3>
        </template>
        <UForm class="space-y-4" :validate="validateAccount" :state="state"
          @submit="emit('submitted', state.name)">
          <UFormField label="Name" name="name">
            <UInput v-model="state.name" type="text" />
          </UFormField>
          <div class="flex items-center space-x-3">
            <UButton type="submit">Submit</UButton>
            <UButton @click="emit('closed')">Cancel</UButton>
          </div>
        </UForm>
      </UCard>
    </template>
  </UModal>
</template>