import { UserModel } from "@/api";
import { createContext, Dispatch, SetStateAction, useContext, useState } from "react"

interface UserContextProps {
    user?: UserModel,
    setUser: Dispatch<SetStateAction<UserModel>>,
}

const UserContext = createContext<UserContextProps>(undefined!);
export function UserProvider({children}: any) {
    const [user, setUser] = useState<UserModel>(undefined!);

    return (
        <UserContext.Provider
            value={{
                user,
                setUser
            }}
        >
            {children}
        </UserContext.Provider>
    )
}

export function useUserContext(): UserContextProps {
    const context = useContext(UserContext);
    if(typeof context == "undefined") {
        throw new Error(`${useUserContext.name} should be used within UserContext provider`);
    }
    return context;
}