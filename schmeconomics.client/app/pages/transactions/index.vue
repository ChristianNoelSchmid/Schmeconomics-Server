<script setup lang="ts">
import { ref, onMounted } from "vue";
import TransactionCard from "~/components/TransactionCard.vue";
import { showPrompt } from "~/components/prompt/prompt-state";
import { accountCategoriesData } from "~/lib/services/categories";
import { TransactionService, txData } from "~/lib/services/transactions";

const txService = new TransactionService();

const route = useRoute();
const categoryId = ref<string | null>(route.query.categoryId as string);
const { txs, loadNextPageTxs, refresh, isLastPage } = txData(computed(() => categoryId.value));
const { data: categories } = accountCategoriesData();
const categoryName = computed(() => categories.value?.find(c => c.id == categoryId.value)?.name || null);

// Get categoryId from route query parameters
function onDeleteTransaction(txId: string) {
  showPrompt({
    message: "Are you sure you wish to delete this transaction?",
    actions: [["Yes", async () => {
      await txService.deleteTransaction(txId);
      const indexOfTx = txs.value.findIndex(tx => tx.id == txId);
      txs.value.splice(indexOfTx, 1);
    }]]
  });
}

onMounted(async () => {
  await refresh();
});
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Transactions{{ categoryName != null ? ` for ${categoryName}` : '' }}</h1>
    
    <div v-if="txs.length === 0" class="text-center py-8">
      <p>No transactions found.</p>
    </div>
    
    <div v-else>
      <TransactionCard 
        v-for="tx in txs" 
        :key="tx.id"
        :transaction="tx"
        @deleteclicked="onDeleteTransaction(tx.id)"
      />
      
      <div v-if="!isLastPage.value" class="mt-4 text-center">
        <UButton @click="async () => await loadNextPageTxs()" color="primary" variant="solid">
          Load More
        </UButton>
      </div>
    </div>
  </div>
</template>