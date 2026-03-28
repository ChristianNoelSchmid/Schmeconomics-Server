import type { TransactionModel } from "../openapi";

export function txData(categoryId: globalThis.ComputedRef<string | null>) {
    const { $api, $defaultAccountId } = useNuxtApp();

    /*
     * Separate transaction collection - useAsyncData is updated in
     * a unique way, so the ref needs to be adjusted manually.
     */
    const txs = ref<TransactionModel[]>([]);
    const page = ref<number>(0);
    const isLastPage = ref<boolean>(false);

    /*
    * Create separate `useAsyncData`s - one for watching when either `defaultAccountId`
    * or `categoryId` update, and one for when `page` updates. If the former, the entire
    * transaction collection will refresh. If the latter, it will only load the next page.
    */
    const { refresh: loadNextPageTxs } = useAsyncData(
        "transaction-list-nextpage",
        async () => {
            page.value += 1;
            const nextPage = await $api.transaction.apiV1TransactionAccountIdGet({ 
                accountId: $defaultAccountId.value, 
                categoryId: categoryId.value || undefined,
                page: page.value,
                pageSize: 10,
            });
            txs.value = [
                ...txs.value, 
                ...nextPage
            ];
            if(nextPage.length != 10) 
                isLastPage.value = true;
        }
    );
    const { refresh } = useAsyncData(
        "transaction-list-refresh",
        async () => {
            page.value = 1;
            const nextPage = await $api.transaction.apiV1TransactionAccountIdGet({
                accountId: $defaultAccountId.value,
                categoryId: categoryId.value || undefined,
                page: page.value,
                pageSize: 10,
            });
            txs.value = nextPage;
            if(nextPage.length != 10) 
                isLastPage.value = true;
        },
        {
            watch: [ () => $defaultAccountId.value, () => categoryId.value ]
        }
    )

    return { txs: computed(() => txs.value), loadNextPageTxs, refresh, isLastPage: computed(() => isLastPage) };
}

export class TransactionService {
    async createTransaction(categoryId: string, amount: number, notes: string, isAddition: boolean) {
        const { $api, $defaultAccountId } = useNuxtApp();

        await $api.transaction.apiV1TransactionAccountIdPost({
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
            await $api.transaction.apiV1TransactionAccountIdTransactionIdDelete({ 
                accountId: $defaultAccountId.value, 
                transactionId: txId 
            });
        }
    }
}