<script setup lang="ts">
import { ref } from 'vue';
import { currencyFormat } from '~/formatters';
import { showPrompt } from '~/components/prompt/prompt-state';
import { accountCategoriesData } from '~/lib/services/categories';
import type { CategoryRefillValueUpdate } from '~/lib/openapi';

// State for categories and refill values
const { $api, $defaultAccountId } = useNuxtApp();
const { data: categories, refresh } = accountCategoriesData();

const isEditingRefillValues = ref(false);
const editedValues = ref<Record<string, number>>({});
const showConfirmationModal = ref(false);

function resetEditValues() {
    // Initialize edited values with current refill values
    const newValues: Record<string, number> = {};
    if (categories.value) {
        categories.value.forEach(category => 
            newValues[category.id] = category.refillValue
        );
    }

    editedValues.value = newValues;
}

// Toggle edit mode
function toggleEdit() {
    isEditingRefillValues.value = !isEditingRefillValues.value;
    resetEditValues();
}

// Calculate total difference
function calculateTotalDifference(): number {
    let diff = 0;
    categories.value?.forEach(category => {
        const originalValue = category.refillValue || 0;
        const newValue = editedValues.value[category.id];
        if (newValue !== undefined) {
            diff += newValue - originalValue;
        }
    });
    return diff;
}

// Apply changes to categories
async function applyChanges() {
    if ($defaultAccountId.value == null) return;

    // Prepare update request with correct structure
    const refillUpdates: CategoryRefillValueUpdate[] = categories.value ?
        categories.value.map(category => ({
            categoryId: category.id,
            refillValue: editedValues.value[category.id]!
        })) : [];
    
    await $api.category.apiV1CategoryUpdateRefillValuesPut({
        updateCategoriesRefillValueRequest: {
            accountId: $defaultAccountId.value,
            refillValues: refillUpdates
        }
    });

    await refresh();
    resetEditValues();
    
    showConfirmationModal.value = false;
    isEditingRefillValues.value = false;
}

// Refill categories with refill values
async function refillCategories() {
    showPrompt({
        message: "Refill all categories?",
        actions: [
            ["Yes", async () => {
                if ($defaultAccountId.value == null) return;

                await $api.category.apiV1CategoryRefillAccountIdPost({ accountId: $defaultAccountId.value });
                await refresh();
                resetEditValues();
            }]
        ]
    });
    
}

// Initialize on mount
onMounted(async () => {
    // Ensure categories are loaded before initializing
    await refresh();
    // Wait for categories to be populated before resetting values
    if (categories.value && categories.value.length > 0) {
        await nextTick();
        resetEditValues();
    }
});

// Watch for categories changes and reset edit values
watch(categories, (newCategories) => {
    if (newCategories && newCategories.length > 0) {
        resetEditValues();
    }
}, { deep: true, immediate: true });
</script>

<template>
    <div class="p-6">
        <h1 class="text-2xl font-bold mb-6">Refill Categories</h1>
        
        <!-- Action buttons -->
        <div class="flex gap-2 mb-6">
            <UButton 
                v-if="isEditingRefillValues"
                @click="showConfirmationModal = true"
            >
                Confirm 
            </UButton>
            <UButton 
                :variant="isEditingRefillValues ? 'outline' : 'solid'"
                color="info"
                @click="toggleEdit"
            >
                {{ isEditingRefillValues ? 'Cancel' : 'Adjust Values' }}
            </UButton>
            <UButton 
                @click="refillCategories"
            >
                Refill Categories
            </UButton>
        </div>

        <!-- Category list -->
        <div class="space-y-4">
            <div 
                v-for="category in categories" 
                :key="category.id"
                class="border p-4 rounded shadow-sm"
            >
                <div class="flex justify-between items-center">
                    <h2 class="text-lg font-semibold">{{ category.name }}</h2>
                </div>
                
                <div class="mt-2">
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                        Refill Value
                    </label>
                    <CurrencyInput
                        v-model="editedValues[category.id]"
                        :readonly="!isEditingRefillValues"
                    />
                </div>
            </div>
        </div>

        <!-- Confirm modal -->
        <UModal :open="showConfirmationModal">
          <template #content>
          <UCard>
            <template #header>
              <h2 class="text-xl font-bold mb-4">Confirm Changes</h2>
            </template>
                
            <p>Changes to be applied:</p>
            <ul class="mt-2 space-y-1">
              <li 
                  v-for="category in categories" 
                  :key="category.id"
                  class="flex justify-between"
              >
                <span>{{ category.name }}:</span>
                <span>{{ currencyFormat(editedValues[category.id] ?? 0) }} (from {{ currencyFormat(category.refillValue) }})</span>
              </li>
            </ul>
                
            <div class="mb-4 p-3 bg-gray-100 rounded">
              <p>Total Difference: 
                <span :class="{ 'text-red-600': calculateTotalDifference()  < 0, 'text-green-600': calculateTotalDifference() > 0 }">
                  {{ currencyFormat(calculateTotalDifference()) }}
                </span>
              </p>
            </div>
                
            <div class="flex justify-end gap-3">
              <button 
                class="px-4 py-2 border rounded hover:bg-gray-100 transition"
                @click="showConfirmationModal = false"
              >
                Cancel
              </button>
              <button 
                class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition"
                @click="applyChanges"
              >
                Confirm
              </button>
            </div>
          </UCard>
          </template>
        </UModal>
      </div>
</template>