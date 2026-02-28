import { tryRefresh } from "~/lib/refresh";
import { useSignInState } from "~/lib/services/auth";

export default defineNuxtPlugin(async () => {
    const signInState = useSignInState();
    signInState.value = await tryRefresh();
});