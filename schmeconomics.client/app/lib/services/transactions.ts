import type { TransactionModel } from "../openapi";
import { useDefaultAccountId } from "./accounts";

export function txData(
    categoryId: globalThis.ComputedRef<string | null>, 
    page: globalThis.ComputedRef<number>
) {
    const { $api } = useNuxtApp();
    const defaultAccountId = useDefaultAccountId();
    const txs = ref<TransactionModel[]>([]);

    const { refresh: refreshTxs } = useAsyncData<void>(
        "transaction-list",
        async () => {
            txs.value = [
                ...txs.value, 
                ...await $api.transaction.transactionAccountIdGet({ 
                    accountId: defaultAccountId.value, 
                    categoryId: categoryId.value || undefined,
                    page: page.value,
                    pageSize: 10,
                })
            ];
        },
        {
            watch: [
                () => defaultAccountId.value,
                () => categoryId.value,
                () => page.value
            ]
        }
    );

    return { txs, refreshTxs };
}

export class TransactionService {
    async deleteTransaction(txId: string) {
        const { $api } = useNuxtApp();
        const accountId = useDefaultAccountId().value;

        if(accountId != null) {
            await $api.transaction.accountIdTransactionIdDelete({ 
                accountId, 
                transactionId: txId 
            });
        }
    }
}