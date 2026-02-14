<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import { onError } from '~/lib/form-error';
import type { CreateUserRequest, UpdateUserRequest, UserModel } from '~/lib/openapi';
import PasswordInput from './PasswordInput.vue';

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

type Schema = typeof newUserState;

function validate(state: Partial<Schema>): FormError[] {
  const errors = [];
  if ((props.userToEdit == null || updateName.value) && !state.name) 
    errors.push({ name: 'name', message: 'Required' });
  if (
    (props.userToEdit == null || updatePassword.value) &&
    state.password != state.confirmPassword
  )
    errors.push({ name: 'confirmPassword', message: 'Passwords do not match' });

  if(!state.password) {
    if (props.userToEdit == null)
      errors.push({ name: 'password', message: 'Required for new users' });
    else if(updatePassword.value)
      errors.push({ name: 'password', message: 'Please enter a new password' });
  }
  return errors;
}

function submitRequest() {
  if (props.userToEdit) {
    // Editing an existing user - only send name and password, role is not editable in this UI
    const updateRequest: UpdateUserRequest = {
      userId: props.userToEdit.id,
      name: updateName.value ? newUserState.name : null,
      password: updatePassword.value ? newUserState.password : null
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

        <UForm class="space-y-4" :state="newUserState" :validate="validate" @submit="submitRequest" @error="onError">
          <UFormField v-if="userToEdit != null" label="Edit Name">
            <UCheckbox v-model="updateName" />
          </UFormField>
          <UFormField v-if="updateName || userToEdit == null" label="Name" name="name">
            <UInput v-model="newUserState.name" />
          </UFormField>

          <UFormField v-if="userToEdit != null" label="Edit Password">
            <UCheckbox v-model="updatePassword" />
          </UFormField>
          <UFormField v-if="updatePassword || userToEdit == null" label="Password" name="password" >
            <PasswordInput v-model="newUserState.password" :show-reveal-password-button="true" /> 
          </UFormField>
          <UFormField v-if="updatePassword || userToEdit == null" label="Confirm Password" name="confirmPassword">
            <PasswordInput v-model="newUserState.confirmPassword" :show-reveal-password-button="false" />
          </UFormField>
          <div class="flex justify-end space-x-2">
            <UButton color="neutral" variant="ghost" @click="emit('closed')">
              Cancel
            </UButton>
            <UButton type="submit" color="primary" variant="solid" >
              {{ props.userToEdit ? 'Update' : 'Create' }}
            </UButton>
          </div>
        </UForm>
      </UCard>
    </template>
  </UModal>
</template>