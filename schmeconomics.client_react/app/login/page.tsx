"use client"

import { AuthApi } from "@/api";
import { schmeconomicsApiConfig } from "@/auth/config";
import { useAuthContext } from "@/components/auth/auth-context";

export default function() {
    const authContext = useAuthContext();
    const tryLogin = async (formData: FormData) => {
        const api = new AuthApi(schmeconomicsApiConfig());

        const name = formData.get("name")?.valueOf().toString();
        const password = formData.get("password")?.valueOf().toString();
        
        const res = await api.authSignInPost({ 
            signInRequest: {
                name: name ?? "",
                password: password ?? ""
            }
        });

        authContext.login(res);
    };

    return (
        <form>
            <label>Name:</label>
            <input name="name" />
            <label>Password:</label>
            <input name="password" />
            <button formAction={e => tryLogin(e)}>Login</button>
        </form>
    )
}