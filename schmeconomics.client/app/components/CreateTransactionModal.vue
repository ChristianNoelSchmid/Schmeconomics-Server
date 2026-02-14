<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
import { onError } from '~/lib/form-error';
import type { CategoryModel } from '~/lib/openapi';

export interface CreateTransactionProp {
    category: CategoryModel,
    isAddition: boolean
}

const props = defineProps<{
    visible: boolean,
    model: CreateTransactionProp | undefined
}>();

const emit = defineEmits<{
    closed: [],
    submitted: [amount: number, notes: string, isAddition: boolean]
}>();

const transactionState = reactive({
    amount: 0,
    notes: ""
});

type Schema = typeof transactionState;

function validate(state: Partial<Schema>): FormError[] {
    const errors = [];
    if (state.amount == 0) 
        errors.push({ name: 'amount', message: 'Please input a value' });
    return errors;
}

function submitRequest() {
    if (!props.model) return;

    emit(
        'submitted',
        transactionState.amount,
        transactionState.notes,
        props.model.isAddition
    );
    transactionState.amount = 0;
    transactionState.notes = "";
}
</script>

<template>
    <UModal :open="props.visible">
        <template #content>
            <UCard>
                <template #header>
                    <h3 class="text-lg font-semibold">Create Transaction for {{ props.model?.category.name }}</h3>
                </template>

                <UForm class="space-y-4" :state="transactionState" :validate="validate" @error="onError" @submit="submitRequest">
                    <UFormField label="Amount" name="amount">
                        <CurrencyInput v-model="transactionState.amount" />
                    </UFormField>
                    <UFormField label="Notes" name="notes">
                        <UInput v-model="transactionState.notes" type="text" />
                    </UFormField>

                    <div class="flex space-x-4 justify-end">
                        <UButton color="neutral" variant="ghost" @click="emit('closed')">
                            Cancel
                        </UButton>
                        <UButton type="submit" color="info" variant="solid" :disabled="!transactionState">
                            {{ (props.model?.isAddition ?? true) ? "Add Money (+)" : "Subtract Money (-)" }}
                        </UButton>
                    </div>
                </UForm>
            </UCard>
        </template>
    </UModal>
</template>