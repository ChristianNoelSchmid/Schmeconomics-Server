import { AccountApi, AuthApi, CategoryApi, Configuration, TransactionApi, UserApi  } from "~/lib/openapi";
import { tryRefresh } from "~/lib/refresh";
import { useSignInState } from "~/lib/services/auth-state";

interface Api {
    auth: AuthApi,
    account: AccountApi,
    category: CategoryApi,
    transaction: TransactionApi,
    user: UserApi,
}

export default defineNuxtPlugin(async () => {
    const signInState = useSignInState();
    const nonAuthApiConfig = new Configuration({ basePath: "http://localhost:5153" });
    
    const apiConfig = new Configuration({ 
        basePath: "http://localhost:5153", 
        credentials: "include",
        middleware: [{
            pre: async (context) => {
                if(!signInState.value) await tryRefresh();
                if(!signInState.value) return;

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
                    await tryRefresh();
                }

                if(signInState.value) {
                    context.init.headers = {
                        ...context.init.headers,
                        "Authorization": `Bearer ${signInState.value.accessToken}`
                    }
                }
                return context;
            },
        }]
    });

    return {
        provide: {
            api: {
                auth: new AuthApi(nonAuthApiConfig),
                account: new AccountApi(apiConfig),
                category: new CategoryApi(apiConfig),
                transaction: new TransactionApi(apiConfig),
                user: new UserApi(apiConfig),
            } satisfies Api
        }
    }
});