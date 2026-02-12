import { AccountApi, type AccountModel } from "../openapi";
import { getApiConfiguration } from "./auth-state";
import { useStorage } from "@vueuse/core";

export function useAccountState() {
    return useState<ReadonlyArray<AccountModel> | undefined>('accountState', () => undefined);
}

export async function refreshAccountState() {
    const api = new AccountApi(getApiConfiguration(true));
    try { useAccountState().value = await api.accountAllGet(); }
    catch { return; }
};

export async function deleteAccount(id: string) {
    const api = new AccountApi(getApiConfiguration(true));
    try { api.accountDeleteIdDelete({ id }) }
    catch { return; }

    await refreshAccountState();
}

/**
 * Creates global state for default account name, used to populate the current
 * in-use account by the web app
 */
export const useDefaultAccountName = 
    () => useStorage<string>('defaultAccountName', null);

export function clearAccountState() {
    useAccountState().value = undefined;
    useDefaultAccountName().value = null;
}