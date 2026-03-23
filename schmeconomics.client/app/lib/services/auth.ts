import type { SignInModel } from "../openapi";

export function useSignInState() {
    return useState<SignInModel | null>("signInState", () => null);
}