<script lang="ts">
	import { enhance } from "$app/forms";
	import { schmeconomicsApiConfig } from "$lib/config";
	import { AuthApi, type AuthModel } from "$lib/openapi";
	import { getContext, setContext } from "svelte";
    
    let name = $state("");
    let password = $state("");

    const onsubmit = async (e: SubmitEvent) => {
        e.preventDefault();

        const api = new AuthApi(schmeconomicsApiConfig());
        const res = await api.authSignInPost({ signInRequest: { name, password } });
        
        // Update the auth model with response data
        const authModel: AuthModel = {
            accessToken: res.accessToken,
            expiresOnUtc: res.expiresOnUtc,
            refreshToken: res.refreshToken,
        };
        setContext<AuthModel>('authModel', authModel);
        
        // Redirect to home page after successful login
        window.location.href = '/';
    }
</script>

<form 
    id="login-form"
    method="POST"
    {onsubmit}
>
    <label for="name">name:</label>
    <input name="name" type="text" bind:value={name}>
    
    <label for="password">password:</label>
    <input name="password" type="password" bind:value={password}>
    
    <button type="submit">Submit</button>
</form>
