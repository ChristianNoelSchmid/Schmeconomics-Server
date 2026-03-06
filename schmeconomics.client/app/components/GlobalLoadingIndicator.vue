<template>
  <div v-if="isLoading" class="fixed inset-0 bg-black opacity-50 flex items-center justify-center z-50">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mb-4"></div>
  </div>
</template>

<script setup lang="ts">
const loadingCounter = useState<number>('loadingCounter', () => 0);
const isLoading = computed(() => loadingCounter.value > 0);
</script>
