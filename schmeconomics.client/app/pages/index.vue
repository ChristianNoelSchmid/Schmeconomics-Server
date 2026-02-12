<script setup lang="ts">
import { CategoryApi, Role, type CategoryModel, type CreateCategoryRequest, type UpdateCategoryRequest } from '~/lib/openapi';
import { useAccountState, useDefaultAccountName } from '~/lib/services/account-service';
import { getApiConfiguration, useSignInState } from '~/lib/services/auth-state';
import { ref, watch } from 'vue';
import { CategoryService } from '~/lib/services/category-service';
import { TransactionService } from '~/lib/services/transaction-service';
import type { CreateTransactionProp } from '~/components/CreateTransactionModal.vue';

const signInState = useSignInState();
const accountState = useAccountState();
const account = computed(() => accountState.value?.find(a => a.name === defaultAccountName.value));

const defaultAccountName = useDefaultAccountName();

const categories = ref<CategoryModel[]>([]);

const categoryService = new CategoryService();
const transactionService = new TransactionService();

const showCreateCategoryModal = ref(false);
const showEditCategoryModal = ref(false);
const editingCategory = ref<CategoryModel | null>(null);

const showCreateTransactionModal = ref(false);
const createTransactionProp = ref<CreateTransactionProp | undefined>(undefined);

async function loadCategories() {
  if (!accountState.value || !defaultAccountName.value) {
    return;
  }

  if (!account.value) {
    return;
  }

  try {
    const api = new CategoryApi(getApiConfiguration(true));
    categories.value = await api.categoryForAccountAccountIdGet({ accountId: account.value.id });
  } catch (error) {
    console.error('Failed to load categories:', error);
  }
}

// Load categories when default account changes
watch(defaultAccountName, () => {
  if (defaultAccountName.value) {
    loadCategories();
  }
});

onMounted(() => {
  if (!signInState.value) {
    navigateTo('/login');
  }

  // Redirect to accounts page if no default account is selected
  if (!defaultAccountName.value) {
    navigateTo('/accounts');
  }

  loadCategories();
});

async function createCategory(request: CreateCategoryRequest) {
  await categoryService.createCategory(request);

  showCreateCategoryModal.value = false;
  loadCategories();
}

async function updateCategory(request: UpdateCategoryRequest) {
  if (!editingCategory.value) return;

  await categoryService.updateCategory(editingCategory.value.id, request);
  showEditCategoryModal.value = false;
  loadCategories();
}


async function deleteCategory(categoryId: string) {
  if (!confirm('Are you sure you want to delete this category?')) {
    return;
  }

  await categoryService.deleteCategory(categoryId);
  loadCategories();
}

function handleEditCategory(category: CategoryModel) {
  editingCategory.value = category;
  showEditCategoryModal.value = true;
}

function handleCreateTransaction(category: CategoryModel, isAddition: boolean) {
  createTransactionProp.value = { category, isAddition };
  showCreateTransactionModal.value = true;
}

async function createTransaction(amount: number, notes: string, isAddition: boolean) {
  if (!accountState.value || !createTransactionProp.value) return;

  try {
    const account = accountState.value.find(a => a.name == defaultAccountName.value);
    if (account)
      await transactionService.createTransaction(
        account.id,
        createTransactionProp.value.category!.id,
        amount,
        notes,
        isAddition
      );
    showCreateTransactionModal.value = false;
    loadCategories();
  } catch (error) {
    console.error('Failed to create transaction:', error);
    alert('Failed to create transaction');
  }
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">Categories</h1>

    <div v-if="signInState?.userModel.role == Role.Admin">
      <p v-if="categories.length == 0">No categories found. Create some with the button below.</p>
      <UButton color="primary" variant="solid" icon="i-heroicons-plus-circle" class="mb-4"
        @click="showCreateCategoryModal = true">
        New Category
      </UButton>
    </div>
    <div v-else-if="categories.length == 0" class="text-center py-8">
      <p>No categories found.</p>
    </div>

    <!-- Categories list -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <CategoryCard v-for="category in categories" :key="category.id" :category="category"
        @deleteclicked="deleteCategory(category.id)" @editclicked="handleEditCategory(category)"
        @transactionclicked="isAddition => handleCreateTransaction(category, isAddition)" />
    </div>

    <!-- Modal page to create categories -->
    <CreateCategoryModal :account-id="account?.id ?? ''" :visible="showCreateCategoryModal"
      @submitted="createCategory($event)" @closed="showCreateCategoryModal = false" />

    <!-- Modal page to edit categories -->
    <CreateCategoryModal :account-id="account?.id ?? ''" :visible="showEditCategoryModal"
      :category-to-edit="editingCategory" @submitted="updateCategory($event)" @closed="showEditCategoryModal = false" />

    <!-- Modal page to create transactions -->
    <CreateTransactionModal :model="createTransactionProp" :visible="showCreateTransactionModal"
      @closed="showCreateTransactionModal = false"
      @submitted="(amount, notes, isAddition) => createTransaction(amount, notes, isAddition)" />

  </div>
</template>
