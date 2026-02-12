<script setup lang="ts">
import type { FormError } from '@nuxt/ui';
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
    if (!state) errors.push({ name: 'amount', message: 'Required' });
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

                <UForm class="space-y-4" :state="transactionState" :validate="validate">
                    <UFormField label="Amount">
                        <NumberInput v-model="transactionState.amount" />
                    </UFormField>
                    <UFormField label="Notes">
                        <UInput v-model="transactionState.notes" type="text" />
                    </UFormField>

                    <div class="flex space-x-4 justify-center">
                        <UButton :color="(props.model?.isAddition ?? true) ? 'success' : 'error'" variant="solid"
                            :disabled="!transactionState" @click="submitRequest()">
                            {{ (props.model?.isAddition ?? true) ? "Add Money (+)" : "Subtract Money (-)" }}
                        </UButton>
                    </div>
                </UForm>

                <template #footer>
                    <div class="flex justify-end space-x-2">
                        <UButton color="neutral" variant="ghost" @click="emit('closed')">
                            Cancel
                        </UButton>
                    </div>
                </template>
            </UCard>
        </template>
    </UModal>
</template>