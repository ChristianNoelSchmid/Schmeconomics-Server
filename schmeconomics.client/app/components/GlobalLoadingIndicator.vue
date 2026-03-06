<template>
  <Teleport to="body">
    <div v-if="isLoading" class="fixed inset-0 flex items-center justify-center" style="background-color: rgba(0, 0, 0, 0); z-index: 99999; pointer-events: auto;">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
const loadingCounter = useState<number>('loadingCounter', () => 0);
const isLoading = computed(() => loadingCounter.value > 0);
</script>
