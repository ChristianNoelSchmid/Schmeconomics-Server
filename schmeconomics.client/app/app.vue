<script setup lang="ts">
import PromptModal from './components/prompt/PromptModal.vue';
import GlobalLoadingIndicator from './components/GlobalLoadingIndicator.vue';
import { useSignInState } from './lib/services/auth';

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
        <NuxtLoadingIndicator :height="3" />
        <NuxtPage />
      </UMain>
    </UContainer>
    <PromptModal />
    <GlobalLoadingIndicator />
  </UApp>
</template>

<style>
@import "tailwindcss";
@import "@nuxt/ui";
</style>