import type { AccountModel } from "../openapi";
import { useSignInState } from "./auth";

export function accountData() {
    const { $api } = useNuxtApp();

    const signInState = useSignInState();
    const { $defaultAccountId } = useNuxtApp();

    const { data: accounts, refresh: refreshAccounts } = useAsyncData<AccountModel[]>(
        'accounts-list',
        async () => await $api.account.accountAllGet(),
        {
            watch: [
                () => signInState.value,
                () => $defaultAccountId.value,
            ],
        }
    );

    return { accounts, refreshAccounts };
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
