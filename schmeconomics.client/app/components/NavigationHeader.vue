<script setup lang="ts">
import type { NavigationMenuItem, SelectMenuItem } from '@nuxt/ui';
import { useSignInState } from '~/lib/services/auth';
import { ref } from 'vue';
import { accountData } from '~/lib/services/accounts';

const route = useRoute();
const signInState = useSignInState();

const { accounts, refreshAccounts: refresh } = accountData();
const { $defaultAccountId } = useNuxtApp();
const navMenuOpen = ref(false);
const chosenDefaultAccountName = computed(
  () => accounts.value?.find(a => a.id == $defaultAccountId.value)?.name
);

const accountNames = computed<SelectMenuItem[]>(
  () => accounts.value?.map(a => {
    return {
      key: a.id,
      label: a.name,
    };
  }) ?? []
);

async function logout() {
  const { $api } = useNuxtApp();
  await $api.auth.authSignOutPost();
  signInState.value = null;
  refresh();
}

const navItems = computed<NavigationMenuItem[]>(() => [
  {
    label: "Categories",
    active: route.path.endsWith("/"),
    to: "/"
  },
  {
    label: "Txs",
    active: route.path.endsWith("/transactions"),
    to: "/transactions"
  },
  {
    label: "Refill",
    active: route.path.endsWith("/refill"),
    to: "/refill"
  }
]);

const menuItems = computed<NavigationMenuItem[]>(() => [
  {
    label: "Accounts",
    to: "/accounts",
    active: route.path.endsWith("/accounts")
  },
  ...(signInState.value?.userModel.role === 'Admin' ? [{
    label: "Users",
    to: "/users",
    active: route.path.startsWith('/users')
  }] : []),
  {
    label: signInState.value ? "Logout" : "Login",
    to: '/login',
    active: route.path.startsWith('/login'),
    onSelect: async () => {
      if (signInState.value) {
        await logout();
      }
    }
  }
]);

async function updateDefaultAccountId(accountId: string) {
  $defaultAccountId.value = accountId;
  await refresh();

  if (route.path.startsWith("/accounts/")) {
    navigateTo(`/accounts/${$defaultAccountId.value}`);
  }
}

onMounted(async () => {
  await refresh();
})
</script>

<template>
  <div class="flex-column pb-5 my-4 border-b">
    <div class="flex justify-between my-4">
      <NuxtLink class="self-center" to="/">
        <h1 class="self-center">Schmeconomics</h1>
      </NuxtLink>
      <UButton color="neutral" variant="subtle" :icon="navMenuOpen ? 'i-heroicons-x-mark' : 'i-heroicons-bars-3'"
        @click="navMenuOpen = !navMenuOpen" />
    </div>
    <div class="flex justify-between">
      <UNavigationMenu orientation="horizontal" :items="navItems" />
      <USelectMenu class="w-32" :model-value="chosenDefaultAccountName" :items="accountNames" @update:model-value="updateDefaultAccountId(($event as any)?.key ?? '')" /> 
    </div>
  </div>
  <UModal v-model:open="navMenuOpen">
    <template #content>
      <UNavigationMenu orientation="vertical" :items="menuItems" @click="navMenuOpen = false" />
    </template>
  </UModal>
</template>