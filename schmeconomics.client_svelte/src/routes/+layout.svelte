<script lang="ts">
	import { type AuthModel } from "$lib/openapi";
    import { setContext } from "svelte";
    setContext<AuthModel | null>('authModel', null);

    const { children } = $props();
</script>

{@render children()}
