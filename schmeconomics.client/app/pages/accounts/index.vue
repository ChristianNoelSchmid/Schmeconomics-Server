<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import { AccountApi, Role, type AccountModel } from '~/lib/openapi';
import { deleteAccount, refreshAccountState, useAccountState, useDefaultAccountName } from '~/lib/services/account-service';
import { getApiConfiguration, useSignInState } from '~/lib/services/auth-state';

const defaultAccountIdStorage = useDefaultAccountName();
const accounts = useAccountState();
const signInState = useSignInState();
const creatingAccount = ref(false);

async function onDeleteAccount(event: MouseEvent, id: string) {
  event.preventDefault();
  if (confirm("Are you sure you want to delete this account?")) {
    await deleteAccount(id);
    refreshAccountState();
  }
}

const createAccountState = reactive({
  name: undefined,
});

type Schema = typeof createAccountState;

function validateCreateAccount(state: Partial<Schema>): FormError[] {
  const errors = [];
  if (!state.name) errors.push({ name: 'name', message: 'Required' });
  return errors;
}

function selectAccount(event: Event, account: AccountModel) {
  event.preventDefault();
  defaultAccountIdStorage.value = account.name;
  navigateTo('/');
}

async function onCreateAccount() {
  const api = new AccountApi(getApiConfiguration(true));
  try { await api.accountCreatePost({ name: createAccountState.name }); }
  catch { return; }
  creatingAccount.value = false;
  await refreshAccountState();
}
</script>

<template>
  <div>
    <UPageList>
      <UCard v-for="account in accounts" :key="account.id" variant="outline">
        <div class="cursor-pointer" @click="selectAccount($event, account)">
          <h2>{{ account.name }}</h2>
        </div>
        <template v-if="signInState?.userModel.role == Role.Admin" #footer>
          <UButton color="error" @click="onDeleteAccount($event, account.id)">Delete</UButton>
        </template>
      </UCard>
    </UPageList>
    <UButton class="mt-4" @click="creatingAccount = true">Create Account</UButton>
    <UModal :open="creatingAccount">
      <template #content>
        <UForm class="space-y-4" :validate="validateCreateAccount" :state="createAccountState"
          @submit="onCreateAccount">
          <UFormField label="Name">
            <UInput v-model="createAccountState.name" type="text" />
          </UFormField>
          <UButton type="submit">Submit</UButton>
        </UForm>
      </template>
    </UModal>
  </div>
</template>