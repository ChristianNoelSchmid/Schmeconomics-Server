import { Injectable } from "@angular/core";
import { AuthApi, Configuration } from "../../openapi";
import { catchError, from, map, Observable, of } from "rxjs";
import { API_URL } from "../config";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private accessKey: string | null = null
    private expiresOn = Date.now();

    getAccessKey(): Observable<string | null> { 
        // If the access key is null or is past expiration, attempt a refresh before continuing
        if(this.accessKey == null) this.accessKey = localStorage.getItem("accessKey");
        if(this.expiresOn < Date.now() || this.accessKey == null) {
            const api = new AuthApi(new Configuration({ basePath: API_URL, credentials: "include" }));
            return from(api.authRefreshPost())
                .pipe(map(res => {
                    this.accessKey = res.accessToken;
                    this.expiresOn = Date.parse(res.expiresOnUtc.toString());
                    return this.accessKey;
                }), 
                catchError((_, __) => of())
            );
        // Otherwise, just return the access key
        } else {
            return of(this.accessKey);
        }
    }
    tryLogin(name: string, password: string): Observable<void> { 
        const api = new AuthApi(new Configuration({ basePath: API_URL, credentials: "include" }));
        return from(api.authSignInPost({signInRequest: { name, password }}))
            .pipe(map(res => { 
                this.accessKey = res.accessToken;
                this.expiresOn = Date.parse(res.expiresOnUtc.toString());
            }));
    }
}
