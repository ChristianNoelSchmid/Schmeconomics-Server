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
    const baseURL = (typeof window !== 'undefined' && (window as any).NUXT_PUBLIC_API_BASE_URL) || "http://localhost:5153";
    
    const nonAuthApiConfig = new Configuration({ credentials: "include", basePath: baseURL });

    // Create a global loading state
    const globalLoading = useState<boolean>('globalLoading', () => false);

    const apiConfig = new Configuration({
        basePath: baseURL,
        credentials: "include",
        middleware: [{
            pre: async (context) => {
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
                
                // Show global loading indicator when API request starts
                globalLoading.value = true;
                
                return context;
            },
            post: async (context) => {
                // Hide global loading indicator when API request completes
                globalLoading.value = false;
                
                // Return the response to avoid type errors
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
