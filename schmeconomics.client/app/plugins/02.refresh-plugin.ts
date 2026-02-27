import { tryRefresh } from "~/lib/refresh";

export default defineNuxtPlugin(async () => {
    await tryRefresh();
});