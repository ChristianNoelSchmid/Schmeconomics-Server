import { Configuration } from "@/api";
import { useAuthContext } from "@/components/auth/auth-context";

export const schmeconomicsApiConfig = () => {
    const authContext = useAuthContext();
    const headers: { [key: string]: string } = { };

    if(authContext.authInfo != null) {
        headers["Authorization"] = `bearer ${authContext.authInfo?.accessToken}`;
    }

    return new Configuration({
        basePath: "http://localhost:5153",
        headers
    });
}