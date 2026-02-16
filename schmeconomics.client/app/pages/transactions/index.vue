<script setup lang="ts">
import { TransactionApi } from "~/lib/openapi";
import { useDefaultAccountId } from "~/lib/services/account-service";
import { getApiConfiguration } from "~/lib/services/auth-state";
import { ref, onMounted, watch } from "vue";
import TransactionCard from "~/components/TransactionCard.vue";
import type { TransactionModel } from "~/lib/openapi/models/TransactionModel";

const defaultAccountId = useDefaultAccountId();
const transactions = ref<TransactionModel[]>([]);
const loading = ref(false);
const hasMore = ref(true);
const page = ref(1);
const categoryId = ref<string | null>(null);

// Get categoryId from route query parameters
const route = useRoute();
if (route.query.categoryId) {
  categoryId.value = route.query.categoryId as string;
}

const loadTransactions = async () => {
  page.value = 1;
  transactions.value = [];
  if (defaultAccountId.value == null) return;

  try {
    loading.value = true;
    
    const config = await getApiConfiguration(true);
    const api = new TransactionApi(config);
    
    // Call the API with optional categoryId parameter
    const response = await api.transactionAccountIdGet({
      accountId: defaultAccountId.value,
      categoryId: categoryId.value || undefined,
      page: page.value,
      pageSize: 10
    });
    
    // Handle the transaction data properly
    if (response && Array.isArray(response)) {
      transactions.value = [...transactions.value, ...response];
      hasMore.value = response.length === 10; // Assuming 10 items per page
    }
    
    
  } catch (error) {
    console.error("Failed to load transactions:", error);
  } finally {
    loading.value = false;
  }
};

const loadMore = () => {
  if (!hasMore.value || loading.value) return;
  
  page.value++;
  loadTransactions();
};

// Load initial transactions when account name changes or page loads
watch(defaultAccountId, () => {
  if (defaultAccountId.value != null) {
    loadTransactions();
  }
});

onMounted(() => {
  if (defaultAccountId.value != null) {
    loadTransactions();
  }
});
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Transactions</h1>
    
    <div v-if="transactions.length === 0 && !loading" class="text-center py-8">
      <p>No transactions found.</p>
    </div>
    
    <div v-else>
      <TransactionCard 
        v-for="transaction in transactions" 
        :key="transaction.id"
        :transaction="transaction"
      />
      
      <div v-if="hasMore && !loading" class="mt-4 text-center">
        <UButton @click="loadMore" color="primary" variant="solid">
          Load More
        </UButton>
      </div>
      
      <div v-else-if="loading" class="text-center py-4">
        <p>Loading...</p>
      </div>
    </div>
  </div>
</template>