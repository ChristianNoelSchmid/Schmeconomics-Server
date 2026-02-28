<script setup lang="ts">
import { currencyFormat } from "~/formatters";
import type { TransactionModel } from "~/lib/openapi/models/TransactionModel";
import { useSignInState } from "~/lib/services/auth";

defineProps<{
  transaction: TransactionModel;
}>();

defineEmits<{
  deleteclicked: []
}>();

const signInState = useSignInState();
</script>

<template>
  <div class="border rounded-lg p-4 mb-3 bg-white shadow-sm relative">
    <button 
      v-if="signInState?.userModel.id === transaction.creatorId || signInState?.userModel?.role === 'Admin'"
      class="absolute top-0 right-0 text-red-700 hover:text-red-700"
      aria-label="Delete transaction"
      @click="$emit('deleteclicked')"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>
    
    <div class="flex justify-between items-start">
      <div>
        <p class="text-sm text-gray-500">{{ transaction.timestampUtc.toLocaleString() }}</p>
        <p class="font-medium">{{ transaction.creator }}</p>
      </div>
      <p class="text-lg font-bold" :class="transaction.amount >= 0 ? 'text-green-700' : 'text-red-700'">
        ${{ currencyFormat(transaction.amount) }}
      </p>
    </div>
    
    <div class="mt-2 flex justify-between items-center">
      <p class="text-sm text-gray-600">Category: {{ transaction.categoryName }}</p>
      <p v-if="transaction.notes" class="text-sm text-gray-500 italic">{{ transaction.notes }}</p>
    </div>
  </div>
</template>
