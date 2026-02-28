import type { AccountModel } from "../openapi";
import { useStorage, type RemovableRef } from "@vueuse/core";
import { useSignInState } from "./auth";

export function useDefaultAccountId(): RemovableRef<string> {
    return useStorage<string>('defaultAccountId', null);
}

export function accountData() {
    const { $api } = useNuxtApp();

    const signInState = useSignInState();
    const defaultAccountId = useDefaultAccountId();

    const { data: accounts, refresh, clear } = useAsyncData<AccountModel[]>(
        'accounts-list',
        async () => await $api.account.accountAllGet(),
        {
            watch: [
                () => signInState.value,
                () => defaultAccountId.value,
            ]
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
