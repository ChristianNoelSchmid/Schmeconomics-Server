import { AccountApi, AuthApi, CategoryApi, Configuration, TransactionApi, UserApi, type SignInModel } from "~/lib/openapi";
import { tryRefresh } from "~/lib/refresh";
import { useSignInState } from "~/lib/services/auth";

interface Api {
    auth: AuthApi,
    account: AccountApi,
    category: CategoryApi,
    transaction: TransactionApi,
    user: UserApi,
}

export default defineNuxtPlugin(async () => {
    const signInState = useSignInState();
    
    // Get base URL from environment variable or fallback to localhost
    const config = useRuntimeConfig();
    const apiBase = config.public.apiBase;
    
    const nonAuthApiConfig = new Configuration({ credentials: "include", basePath: apiBase });

    // Create a global loading state with a counter for concurrent requests
    const loadingCounter = useState<number>('loadingCounter', () => 0);
    const globalLoading = computed(() => loadingCounter.value > 0);

    const apiConfig = new Configuration({
        basePath: apiBase,
        credentials: "include",
        middleware: [{
            pre: async (context) => {
                // Increment loading counter when API request starts
                loadingCounter.value++;
                
                try {
                    if (!signInState.value) await tryRefresh();
                    if (!signInState.value) return;

                    // Check if token has expired
                    const expiresOnUtc = new Date(signInState.value.expiresOnUtc);

                    /*
                    * Subtract a minute from expiration time, to ensure it never
                    * sends a request the moment the token expires
                    * This would technically cause problems if requests take longer than a minute
                    * to run, but this is not expected to be encountered.
                    */
                    expiresOnUtc.setMinutes(expiresOnUtc.getMinutes() - 1);

                    const now = new Date();

                    if (expiresOnUtc < now) {
                        signInState.value = await tryRefresh();
                    }

                    if (signInState.value) {
                        context.init.headers = {
                            ...context.init.headers,
                            "Authorization": `Bearer ${signInState.value.accessToken}`
                        }
                    }
                    
                    return context;
                } catch (error) {
                    // Decrement counter if pre-processing fails
                    loadingCounter.value--;
                    throw error;
                }
            },
            post: async (context) => {
                // Decrement loading counter when API request completes
                loadingCounter.value--;
                
                // Return the response to avoid type errors
                return context.response;
            },
            onError: async (context) => {
                // Ensure counter is decremented on error
                loadingCounter.value--;
                return context.response;
            }
        }]
    });

    return {
        provide: {
            _refreshPromise: ref<Promise<SignInModel | null> | null>(null),
            api: {
                auth: new AuthApi(nonAuthApiConfig),
                account: new AccountApi(apiConfig),
                category: new CategoryApi(apiConfig),
                transaction: new TransactionApi(apiConfig),
                user: new UserApi(apiConfig),
            } satisfies Api,
            globalLoading: globalLoading
        }
    }
});
