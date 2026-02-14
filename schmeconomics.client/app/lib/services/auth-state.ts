import { Configuration, type SignInModel } from "../openapi";
import { AuthApi } from "../openapi/apis/AuthApi";

export function useSignInState(): globalThis.Ref<SignInModel | undefined, SignInModel | undefined> {
    return useState<SignInModel | undefined>("signInState", () => undefined);
}

export async function getApiConfiguration(useAccessToken: boolean) {
    const headers: { [key: string]: string } = {};
    if(useAccessToken) {
        const signInState = useSignInState();
        if(signInState.value) {
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
                // Token has expired, try to refresh it
                try {
                    const authApi = new AuthApi(
                        new Configuration({ basePath: "http://localhost:5153", credentials: "include" })
                    );
                    const refreshedToken = await authApi.authRefreshPost();
                    
                    // Update the signInState with the new token
                    signInState.value = refreshedToken;
                    headers["Authorization"] = `bearer ${refreshedToken.accessToken}`;
                } catch (error) {
                    // If refresh fails, clear the sign in state to force re-authentication
                    signInState.value = undefined;
                    throw error;
                }
            } else {
                // Token is still valid
                headers["Authorization"] = `bearer ${signInState.value.accessToken}`;
            }
        }
    }

    return new Configuration({ 
        basePath: "http://localhost:5153", 
        credentials: "include",
        headers 
    });
}
