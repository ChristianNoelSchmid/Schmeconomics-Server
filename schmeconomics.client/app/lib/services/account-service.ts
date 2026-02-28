import type { AccountModel } from "../openapi";
import { useStorage, type RemovableRef } from "@vueuse/core";

export function useDefaultAccountId(): RemovableRef<string> {
    return useStorage<string>('defaultAccountId', null);
}

export function accountData() {
    const { $api } = useNuxtApp();
    const { start, finish } = useLoadingIndicator();

    const { data: accounts, refresh, clear } = useAsyncData<AccountModel[]>(
        'accounts-list',
        async () => {
            start();
            try {
                return await $api.account.accountAllGet();
            } finally {
                finish();
            }
        }
    );

    return { accounts, refresh, clear };
}

export class AccountService {
    async createAccount(name: string) {
        const { $api } = useNuxtApp();
        await $api.account.accountCreatePost({ name });
    }

    async deleteAccount(id: string) {
        const { $api } = useNuxtApp();
        await $api.account.accountDeleteIdDelete({ id });
    }
}
