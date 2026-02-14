import { AuthApi } from "~/lib/openapi";
import { refreshAccountState } from "~/lib/services/account-service";
import { getApiConfiguration, useSignInState } from "~/lib/services/auth-state";

export default defineNuxtPlugin(async () => {
    // First attempt to sign in
    const signInState = useSignInState();
    const api = new AuthApi(await getApiConfiguration(false));

    try { signInState.value = await api.authRefreshPost(); } 
    catch { return; }

    await refreshAccountState();
})