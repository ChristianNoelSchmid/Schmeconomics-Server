import type { TransactionModel } from "../openapi";

export function txData(categoryId: globalThis.ComputedRef<string | null>) {
    const { $api, $defaultAccountId } = useNuxtApp();

    /*
     * Separate transaction collection - useAsyncData is updated in
     * a unique way, so the ref needs to be adjusted manually.
     */
    const txs = ref<TransactionModel[]>([]);
    const page = ref<number>(0);

    /*
    * Create separate `useAsyncData`s - one for watching when either `defaultAccountId`
    * or `categoryId` update, and one for when `page` updates. If the former, the entire
    * transaction collection will refresh. If the latter, it will only load the next page.
    */
    const { refresh: loadNextPageTxs } = useAsyncData<void>(
        "transaction-list-nextpage",
        async () => {
            page.value += 1;
            txs.value = [
                ...txs.value, 
                ...await $api.transaction.transactionAccountIdGet({ 
                    accountId: $defaultAccountId.value, 
                    categoryId: categoryId.value || undefined,
                    page: page.value,
                    pageSize: 10,
                })
            ];
        }
    );
    useAsyncData<void>(
        "transaction-list-refresh",
        async () => {
            page.value = 1;
            txs.value = await $api.transaction.transactionAccountIdGet({
                accountId: $defaultAccountId.value,
                categoryId: categoryId.value || undefined,
                page: page.value,
                pageSize: 10,
            });
        },
        {
            watch: [ () => $defaultAccountId.value, () => categoryId.value ]
        }
    )

    return { txs: computed(() => txs.value), loadNextPageTxs };
}

export class TransactionService {
    async createTransaction(categoryId: string, amount: number, notes: string, isAddition: boolean) {
        const { $api, $defaultAccountId } = useNuxtApp();

        await $api.transaction.transactionAccountIdPost({
            accountId: $defaultAccountId.value,
            createTransactionRequest: [{
                categoryId,
                amount: amount * (isAddition ? 1.0 : -1.0),
                notes,
            }] 
        });
    }
    async deleteTransaction(txId: string) {
        const { $api, $defaultAccountId } = useNuxtApp();

        if($defaultAccountId != null) {
            await $api.transaction.accountIdTransactionIdDelete({ 
                accountId: $defaultAccountId.value, 
                transactionId: txId 
            });
        }
    }
}