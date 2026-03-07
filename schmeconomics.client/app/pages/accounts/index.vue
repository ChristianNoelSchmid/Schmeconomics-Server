<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import { AccountApi, Role, type AccountModel } from '~/lib/openapi';
import { accountData, AccountService, useDefaultAccountId } from '~/lib/services/accounts';
import { useSignInState } from '~/lib/services/auth';
import AccountUserManagementModal from '~/components/AccountUserManagementModal.vue';
import { showPrompt } from '~/components/prompt/prompt-state';

const { accounts, refreshAccounts: refresh } = accountData()
const accountService = new AccountService();

const signInState = useSignInState();
const editingAccount = ref<AccountModel | null>(null);
const upsertModalVisible = ref(false);
const userManagementModalOpen = ref(false);

const defaultAccountId = useDefaultAccountId();
const selectedAccountId = ref<string | null>(null);

async function onDeleteAccount(event: MouseEvent, id: string) {
  event.preventDefault();
  showPrompt({
    message: "Are you sure you want to delete this account?",
    actions: [
      ["Yes", async () => {
        await accountService.deleteAccount(id);
        refresh();
      }],
    ]
  })
}

function selectAccount(event: Event, account: AccountModel) {
  event.preventDefault();
  defaultAccountId.value = account.id;
  navigateTo('/');
}

async function onCreateAccount(name: string) {
  await accountService.createAccount(name);
  upsertModalVisible.value = false;
  await refresh();
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
    <UButton v-if="signInState?.userModel.role == Role.Admin" class="mt-4" @click="upsertModalVisible = true">Create Account</UButton>
    <CreateAccountModal :editing-account="editingAccount" :visible="upsertModalVisible" @submitted="onCreateAccount" @closed="upsertModalVisible=false" />
    
    <AccountUserManagementModal 
      :account-id="selectedAccountId || ''"
      :visible="userManagementModalOpen"
      @closed="userManagementModalOpen = false"
    />
  </div>
</template>