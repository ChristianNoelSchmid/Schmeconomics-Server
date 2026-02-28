<script setup lang="ts">
import { Role, type CategoryModel, type CreateCategoryRequest, type UpdateCategoryRequest } from '~/lib/openapi';
import { useDefaultAccountId } from '~/lib/services/accounts';
import { useSignInState } from '~/lib/services/auth';
import { ref } from 'vue';
import type { CreateTransactionProp } from '~/components/CreateTransactionModal.vue';
import { showPrompt } from '~/components/prompt/prompt-state';
import { accountCategoriesData, CategoryService } from '~/lib/services/categories';

const signInState = useSignInState();
const defaultAccountId = useDefaultAccountId();

const categoryService = new CategoryService();
const { categories, refresh, clear } = accountCategoriesData();

const showCreateCategoryModal = ref(false);
const showEditCategoryModal = ref(false);
const editingCategory = ref<CategoryModel | null>(null);

const showCreateTransactionModal = ref(false);
const createTransactionProp = ref<CreateTransactionProp | undefined>(undefined);

const { $api } = useNuxtApp();

onMounted(async () => {
  if (!signInState.value) {
    navigateTo('/login');
  }
  // Redirect to accounts page if no default account is selected
  if (!defaultAccountId.value) {
    navigateTo('/accounts');
  }

  refresh();
});

onUnmounted(async () => {
  clear();
})

async function createCategory(request: CreateCategoryRequest) {
  await categoryService.createCategory(defaultAccountId.value, request);
  showCreateCategoryModal.value = false;
  await refresh();
}

async function updateCategory(categoryId: string, request: UpdateCategoryRequest) {
  await categoryService.updateCategory(categoryId, request)
  showEditCategoryModal.value = false;
  await refresh();
}

function deleteCategory(categoryId: string) {
  const catName = categories.value!.find(c => c.id == categoryId)?.name;
  showPrompt({
    message: `Are you sure you want to delete "${catName}"?`,
    actions: [
      ["Yes", async () => {
        await categoryService.deleteCategory(categoryId);
        await refresh();
      }],
    ]
  })
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
  try {
    await
      $api.transaction.transactionAccountIdPost({
        accountId: defaultAccountId.value,
        createTransactionRequest: [{
          categoryId: createTransactionProp.value!.category!.id,
          amount: amount * (isAddition ? 1.0 : -1.0),
          notes,
        }] 
      });
    showCreateTransactionModal.value = false;
    await refresh();
  } catch (error) {
    console.error('Failed to create transaction:', error);
    alert('Failed to create transaction');
  }
}

async function navigateToCategoryTxs(catId: string) {
  await navigateTo({ 
    path: '/transactions', 
    query: { categoryId: catId }
  });
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">Categories</h1>

    <div v-if="signInState?.userModel.role == Role.Admin">
      <p v-if="categories?.length == 0">No categories found. Create some with the button below.</p>
      <UButton color="primary" variant="solid" icon="i-heroicons-plus-circle" class="mb-4"
        @click="showCreateCategoryModal = true">
        New Category
      </UButton>
    </div>
    <div v-else-if="categories?.length == 0" class="text-center py-8">
      <p>No categories found.</p>
    </div>

    <!-- Categories list -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <CategoryCard @clicked="navigateToCategoryTxs(category.id)" v-for="category in categories" :key="category.id" :category="category"
        @deleteclicked="deleteCategory(category.id)" @editclicked="handleEditCategory(category)"
        @transactionclicked="isAddition => handleCreateTransaction(category, isAddition)" />
    </div>

    <!-- Modal page to create categories -->
    <CreateCategoryModal :account-id="defaultAccountId || ''" :visible="showCreateCategoryModal"
      @submitted="createCategory($event)" @closed="showCreateCategoryModal = false" />

    <!-- Modal page to edit categories -->
    <CreateCategoryModal :account-id="defaultAccountId || ''" :visible="showEditCategoryModal"
      :category-to-edit="editingCategory" @submitted="updateCategory(editingCategory!.id, $event)" @closed="showEditCategoryModal = false" />

    <!-- Modal page to create transactions -->
    <CreateTransactionModal :model="createTransactionProp" :visible="showCreateTransactionModal"
      @closed="showCreateTransactionModal = false"
      @submitted="(amount, notes, isAddition) => createTransaction(amount, notes, isAddition)" />

  </div>
</template>
