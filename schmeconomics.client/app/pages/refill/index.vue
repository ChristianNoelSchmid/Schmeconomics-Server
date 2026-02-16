<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { CategoryApi, type CategoryRefillValueUpdate } from '../../lib/openapi';
import { useDefaultAccountId } from '../../lib/services/account-service';
import { getApiConfiguration } from '../../lib/services/auth-state';
import { CategoryService } from '~/lib/services/category-service';
import { currencyFormat } from '~/formatters';

// State for categories and refill values
const categoryService = new CategoryService();
const defaultAccountId = useDefaultAccountId();
const categories = categoryService.defaultAccountCategories();

const isEditingRefillValues = ref(false);
const editedValues = ref<Record<string, number>>({});
const showConfirmationModal = ref(false);

async function resetEditValues() {
    // Initialize edited values with current refill values
    const newValues: Record<string, number> = {};
    categories.value.forEach(category => 
        newValues[category.id] = category.refillValue
    );

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
    categories.value.forEach(category => {
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
    if (defaultAccountId.value == null) return;

    try {
        const config = await getApiConfiguration(true);
        const api = new CategoryApi(config);
        
        // Prepare update request with correct structure
        const refillUpdates: CategoryRefillValueUpdate[] = categories.value.map(category => ({
            categoryId: category.id,
            refillValue: editedValues.value[category.id]!
        }));
        
        await api.categoryUpdateRefillValuesPut({
            updateCategoriesRefillValueRequest: {
                accountId: defaultAccountId.value,
                refillValues: refillUpdates
            }
        });

        await resetEditValues();
        
        showConfirmationModal.value = false;
        isEditingRefillValues.value = false;
    } catch (error) {
        console.error('Error applying changes:', error);
        showConfirmationModal.value = false;
    }
}

// Refill categories with refill values
async function refillCategories() {
    if (defaultAccountId.value == null) return;

    try {
        const config = await getApiConfiguration(true);
        const api = new CategoryApi(config);
        await api.categoryRefillAccountIdPost({ accountId: defaultAccountId.value });
    } catch (error) {
        console.error('Error refilling categories:', error);
    }
}

// Initialize on mount
watch(categories, () => resetEditValues());
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
                    <span class="text-gray-600">ID: {{ category.id }}</span>
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
        <div v-if="showConfirmationModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div class="bg-white p-6 rounded-lg shadow-xl max-w-md w-full">
                <h2 class="text-xl font-bold mb-4">Confirm Changes</h2>
                
                <div class="mb-4">
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
                </div>
                
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
            </div>
        </div>
    </div>
</template>