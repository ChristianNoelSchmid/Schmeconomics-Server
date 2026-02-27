import { AuthApi, Configuration } from "./openapi";
import { useSignInState } from "./services/auth-state";


// Refresh fetch request, wrapped in a promise-lock to ensure if multiple
// api calls are made at once, only one can request the refres token, while the
// others await the same operation
export const tryRefresh = async () => {
    const refreshPromise = ref<Promise<void> | null>(null);
    if(refreshPromise.value) return refreshPromise.value;
    refreshPromise.value = (async () => {
        // Token has expired, try to refresh it
        const refreshConfig = new Configuration({ 
            basePath: "http://localhost:5153", 
            credentials: "include" 
        });
        try { 
            const authApi = new AuthApi(refreshConfig);
            useSignInState().value = await authApi.authRefreshPost();
        } catch(error) {
            // If refresh fails, clear the sign in state to force re-authentication
            useSignInState().value = null;
            throw error;
        } finally {
            refreshPromise.value = null;
        }
    })();

    return refreshPromise;
}