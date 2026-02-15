<script setup lang="ts">
import { currencyFormat } from '~/formatters';
import { Role, type CategoryModel } from '~/lib/openapi';
import { useSignInState } from '~/lib/services/auth-state';

const signInState = useSignInState();
const props = defineProps<{
  category: CategoryModel
}>();
const balance = computed(() => currencyFormat(props.category.balance));

const emit = defineEmits<{
  clicked: []
  deleteclicked: []
  editclicked: []
  transactionclicked: [isAddition: boolean]
}>();
</script>

<template>
  <UCard class="relative">
    <div class="flex justify-between">
      <div class="flex flex-grow flex-col justify-center" @click="emit('clicked')">
        <h3 class="text-lg font-semibold">{{ props.category.name }}</h3>
        <p class="text-md">${{ balance }}</p>
      </div>
      <div class="flex flex-col">
        <UButton color="info" variant="outline" icon="i-heroicons-minus" size="xl"
          @click="emit('transactionclicked', false)" />
        <UButton color="neutral" variant="ghost" icon="i-heroicons-plus" size="xl"
          @click="emit('transactionclicked', true)" />
      </div>
    </div>

    <template #footer v-if="signInState?.userModel.role == Role.Admin">
      <div class="flex justify-end space-x-2">
        <UButton color="warning" variant="ghost" icon="i-heroicons-pencil" size="xl" @click.prevent="emit('editclicked')" />
        <UButton color="error" variant="ghost" icon="i-heroicons-trash" size="xl" @click.prevent="emit('deleteclicked')" />
      </div>
    </template>
  </UCard>
</template>
