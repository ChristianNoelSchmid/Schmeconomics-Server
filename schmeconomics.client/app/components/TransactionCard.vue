<script setup lang="ts">
import { currencyFormat } from "~/formatters";
import type { TransactionModel } from "~/lib/openapi/models/TransactionModel";

defineProps<{
  transaction: TransactionModel;
}>();
</script>

<template>
  <div class="border rounded-lg p-4 mb-3 bg-white shadow-sm">
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

<style scoped>
</style>