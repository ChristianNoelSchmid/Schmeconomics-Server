<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import type { CreateUserRequest, UpdateUserRequest, UserModel } from '~/lib/openapi';

const props = defineProps<{
  visible: boolean,
  userToEdit?: UserModel | null
}>();

const emit = defineEmits<{
  closed: [],
  submitted: [req: CreateUserRequest | UpdateUserRequest]
}>();

const newUserState = reactive({
  name: props.userToEdit?.name || '',
  password: '',
  confirmPassword: '',
});

const updateName = ref(false);
const updatePassword = ref(false);
const revealPassword = ref(false);

type Schema = typeof newUserState;

function validate(state: Partial<Schema>): FormError[] {
  const errors = [];
  if (!state.name) errors.push({ name: 'name', message: 'Required' });
  if (!state.password && !props.userToEdit) errors.push({ name: 'password', message: 'Required for new users' });
  return errors;
}

function submitRequest() {
  if (props.userToEdit) {
    // Editing an existing user - only send name and password, role is not editable in this UI
    const updateRequest: UpdateUserRequest = {
      userId: props.userToEdit.id,
      name: newUserState.name,
      password: newUserState.password || null
    };
    emit('submitted', updateRequest);
  } else {
    // Creating a new user
    const createRequest: CreateUserRequest = {
      name: newUserState.name,
      password: newUserState.password
    };
    emit('submitted', createRequest);
  }
  
  // Reset form
  newUserState.name = '';
  newUserState.password = '';
  newUserState.confirmPassword = '';
}

// Reset form when user to edit changes
watch(() => props.userToEdit, (newVal) => {
  if (newVal) {
    newUserState.name = newVal.name || '';
    // Don't reset password for editing
  } else {
    newUserState.name = '';
    newUserState.password = '';
    newUserState.confirmPassword = '';
  }
}, { immediate: true });
</script>

<template>
  <UModal :open="props.visible">
    <template #content>
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold">{{ props.userToEdit ? 'Edit User' : 'Create New User' }}</h3>
        </template>

        <UForm class="space-y-4" :state="newUserState" :validate="validate">
          <UFormField v-if="userToEdit != null" label="Edit Name">
            <UCheckbox v-model="updateName" />
          </UFormField>
          <UFormField v-if="updateName || userToEdit == null" label="Name">
            <UInput v-model="newUserState.name" />
          </UFormField>

          <UFormField v-if="userToEdit != null" label="Edit Password">
            <UCheckbox v-model="updatePassword" />
          </UFormField>
          <UFormField v-if="updatePassword || userToEdit == null" label="Password">
            <div class="flex">
              <UInput v-model="newUserState.password" :type="revealPassword ? 'text' : 'password'" />
              <UButton :icon="revealPassword ? 'i-heroicons-eye-slash' : 'i-heroicons-eye'" variant="ghost" @click="revealPassword = !revealPassword" />
            </div>
          </UFormField>
          <UFormField v-if="updatePassword || userToEdit == null" label="Confirm Password">
            <div class="flex">
              <UInput v-model="newUserState.confirmPassword" type="password" />
            </div>
          </UFormField>

        </UForm>

        <template #footer>
          <div class="flex justify-end space-x-2">
            <UButton color="neutral" variant="ghost" @click="emit('closed')">
              Cancel
            </UButton>
            <UButton color="primary" variant="solid" @click="submitRequest">
              {{ props.userToEdit ? 'Update' : 'Create' }}
            </UButton>
          </div>
        </template>
      </UCard>
    </template>
  </UModal>
</template>