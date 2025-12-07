"use client"

import { AuthApi, AuthModel, UserModel } from "@/api";
import { jwtDecode } from "jwt-decode";
import { createContext, useCallback, useContext, useState } from "react"

interface AuthContextProps {
    user: UserModel | null,
    authInfo: AuthModel | null,
    login: (authInfo: AuthModel) => void,
    logout: () => void,
    refresh: () => Promise<void>
}

const AuthContext = createContext<AuthContextProps>(undefined!);
export function AuthProvider({children}: any) {
    // Gets and sets user
    const [user, setUser] = useState<UserModel | null>(null);
    // Gets and sets auth info (access key, refresh token, and expiration time)
    const [authInfo, setAuthInfo] = useState<AuthModel | null>(null);

    const login = useCallback((authInfo: AuthModel) => {
        setAuthInfo(authInfo);
        setUser(jwtDecode(authInfo.accessToken));
    }, []);

    const logout = useCallback(() => {
        setAuthInfo(null);
        setUser(null);
    }, [])

    const refresh = useCallback(async () => {
        const res = await new AuthApi().authRefreshPost();
        login(res);
    }, [login, logout]);

    return (
        <AuthContext.Provider
            value={{
                user,
                authInfo,
                login,
                logout,
                refresh
            }}
        >
            {children}
        </AuthContext.Provider>
    )
}

export function useAuthContext(): AuthContextProps {
    const context = useContext(AuthContext);
    if(typeof context == "undefined") {
        throw new Error(`${useAuthContext.name} should be used within UserContext provider`);
    }
    return context;
}