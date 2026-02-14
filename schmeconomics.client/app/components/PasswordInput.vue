<script setup lang="ts">
  const props = defineProps<{
    showRevealPasswordButton: boolean
  }>();
  const model = defineModel<string>();
  const revealPassword = ref(false);
</script>

<template>
    <div class="flex">
      <UInput v-model="model" :type="revealPassword ? 'text' : 'password'" />
      <UButton v-if="props.showRevealPasswordButton" :icon="revealPassword ? 'i-heroicons-eye' : 'i-heroicons-eye-slash'" variant="ghost" @click="revealPassword = !revealPassword" />
    </div>
</template>