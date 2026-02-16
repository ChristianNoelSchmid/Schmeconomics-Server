<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import { AccountApi, Role, type AccountModel } from '~/lib/openapi';
import { deleteAccount, refreshAccountState, useAccountState, useDefaultAccountId } from '~/lib/services/account-service';
import { getApiConfiguration, useSignInState } from '~/lib/services/auth-state';
import AccountUserManagementModal from '~/components/AccountUserManagementModal.vue';

const accounts = useAccountState();
const signInState = useSignInState();
const creatingAccount = ref(false);
const userManagementModalOpen = ref(false);

const defaultAccountId = useDefaultAccountId();
const selectedAccountId = ref<string | null>(null);

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
  defaultAccountId.value = account.id;
  navigateTo('/');
}

async function onCreateAccount() {
  const api = new AccountApi(await getApiConfiguration(true));
  try { await api.accountCreatePost({ name: createAccountState.name }); }
  catch { return; }
  creatingAccount.value = false;
  await refreshAccountState();
}

function openUserManagement(accountId: string) {
  selectedAccountId.value = accountId;
  userManagementModalOpen.value = true;
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
          <div class="flex space-x-2">
            <UButton @click="openUserManagement(account.id)">Manage Users</UButton>
            <UButton color="error" @click="onDeleteAccount($event, account.id)">Delete</UButton>
          </div>
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
    
    <AccountUserManagementModal 
      :account-id="selectedAccountId || ''"
      :visible="userManagementModalOpen"
      @closed="userManagementModalOpen = false"
    />
  </div>
</template>