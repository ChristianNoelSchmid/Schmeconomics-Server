import { AuthApi, Configuration, type SignInModel } from "./openapi";
import { useSignInState } from "./services/auth";


// Refresh fetch request, wrapped in a promise-lock to ensure if multiple
// api calls are made at once, only one can request the refres token, while the
// others await the same operation
export async function tryRefresh(): Promise<SignInModel | null> {
    const { $_refreshPromise } = useNuxtApp();
    if ($_refreshPromise.value) {
        return $_refreshPromise.value;
    }

    const promise = (async () => {
        // Token has expired, try to refresh it
        const refreshConfig = new Configuration({
            basePath: "http://localhost:5153",
            credentials: "include"
        });
        try {
            const authApi = new AuthApi(refreshConfig);
            return await authApi.authRefreshPost();
        } catch (error) {
            // If refresh fails, clear the sign in state to force re-authentication
            useSignInState().value = null;
            throw error;
        } finally {
            $_refreshPromise.value = null;
        }
    })();

    $_refreshPromise.value = promise;
    return promise;
}