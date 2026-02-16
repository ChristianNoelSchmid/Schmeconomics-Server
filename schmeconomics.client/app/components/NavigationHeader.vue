<script setup lang="ts">
import type { NavigationMenuItem, SelectMenuItem } from '@nuxt/ui';
import { AuthApi } from '~/lib/openapi';
import { getApiConfiguration as useApiConfiguration, useSignInState } from '~/lib/services/auth-state';
import { ref } from 'vue';
import { clearAccountState, useAccountState, useDefaultAccountId } from '~/lib/services/account-service';

const route = useRoute();
const signInState = useSignInState();

const accountState = useAccountState();
const defaultAccountId = useDefaultAccountId();
const navMenuOpen = ref(false);

const accountNames = computed<SelectMenuItem[]>(
  () => accountState.value?.map(a => {
    return {
      key: a.id,
      label: a.name,
    };
  }) ?? []
);

async function logout() {
  const api = new AuthApi(await useApiConfiguration(false));
  await api.authSignOutPost();
  signInState.value = undefined;
  clearAccountState();
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

function updateDefaultAccountId(accountId: string) {
  if (accountState.value) {
    defaultAccountId.value = accountId;

    if (route.path.startsWith("/accounts/")) {
      navigateTo(`/accounts/${defaultAccountId.value}`);
    }
  }
}
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
      <USelectMenu :model-value="accountNames" :items="accountNames" @update:model-value="updateDefaultAccountId($event.key)" />
    </div>
  </div>
  <UModal v-model:open="navMenuOpen">
    <template #content>
      <UNavigationMenu orientation="vertical" :items="menuItems" @click="navMenuOpen = false" />
    </template>
  </UModal>
</template>