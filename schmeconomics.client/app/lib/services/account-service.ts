import { AccountApi, type AccountModel } from "../openapi";
import { getApiConfiguration } from "./auth-state";
import { useStorage, type RemovableRef } from "@vueuse/core";

export function useAccountState() {
    return useState<ReadonlyArray<AccountModel> | undefined>('accountState', () => undefined);
}

export async function refreshAccountState() {
    const config = await getApiConfiguration(true);
    const api = new AccountApi(config);
    try { useAccountState().value = await api.accountAllGet(); }
    catch { return; }
};

export async function deleteAccount(id: string) {
    const config = await getApiConfiguration(true);
    const api = new AccountApi(config);
    try { api.accountDeleteIdDelete({ id }) }
    catch { return; }

    await refreshAccountState();
}

export function useDefaultAccountId(): RemovableRef<string> {
    return useStorage<string>('defaultAccountId', null);
}

export function clearAccountState() {
    useAccountState().value = undefined;
    useDefaultAccountId().value = null;
}
