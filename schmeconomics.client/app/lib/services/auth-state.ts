import { Configuration, type SignInModel } from "../openapi";

export function useSignInState(): globalThis.Ref<SignInModel | undefined, SignInModel | undefined> {
    return useState<SignInModel | undefined>("signInState", () => undefined);
}

export function getApiConfiguration(useAccessToken: boolean) {
    const headers: { [key: string]: string } = {};
    if(useAccessToken) {
        const signInState = useSignInState();
        if(signInState.value) 
            headers["Authorization"] = `bearer ${signInState.value.accessToken}`;
    }

    return new Configuration({ 
        basePath: "http://localhost:5153", 
        credentials: "include",
        headers 
    });
}
