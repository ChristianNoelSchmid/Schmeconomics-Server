import { Configuration } from "./openapi";

export function schmeconomicsApiConfig() {
    return new Configuration({
        basePath: "http://localhost:5153",
    })
}