<script setup lang="ts">
import { usePromptState } from './prompt-state';
const props = usePromptState();
</script>

<template>
  <UModal :open="props.visible">
    <template #header>
      <h2 class="text-lg">Confirm</h2> 
    </template>
    <template #body>
      <p>{{  props.data.message }}</p>
      <div class="flex mt-2 gap-x-2">
        <UButton 
          v-for="action in props.data.actions" 
          color="info"
          :key="action[0]" 
          @click="action[1](); 
            props.visible = false;
          "
        >
          {{ action[0] }}
        </UButton>
        <UButton variant="outline" color="info" @click="props.visible = false">
          Cancel
        </UButton>
      </div>
    </template>
  </UModal>
</template>