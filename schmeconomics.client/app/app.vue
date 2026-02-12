<script setup lang="ts">
import { useSignInState } from './lib/services/auth-state';

const signInState = useSignInState();
const router = useRoute();

onMounted(() => {
  if (!signInState.value && router.path != '/login')
    navigateTo('/login');
});
</script>

<template>
  <UApp>
    <UContainer>
      <NavigationHeader />
      <UMain>
        <NuxtPage />
      </UMain>
    </UContainer>
  </UApp>
</template>

<style>
@import "tailwindcss";
@import "@nuxt/ui";
</style>