import { Injectable } from "@angular/core";
import { AuthApi, AuthModel, Configuration, UserModel } from "../../openapi";
import { catchError, from, map, Observable, of } from "rxjs";
import { API_URL } from "../config";
import { jwtDecode } from "jwt-decode";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private authModel: AuthModel | null = null

    getAccessKey(): Observable<string | null> { 
        // If the access key is null or is past expiration, attempt a refresh before continuing
        if(this.authModel == null || this.authModel.expiresOnUtc < new Date()) {
            const api = new AuthApi(new Configuration({ basePath: API_URL, credentials: "include" }));
            return from(api.authRefreshPost())
                .pipe(map(res => {
                    this.authModel = res;
                    return this.authModel.accessToken;
                }), 
                catchError((_, __) => of())
            );
        // Otherwise, just return the access key
        } else {
            return of(this.authModel.accessToken);
        }
    }
    tryLogin(name: string, password: string): Observable<void> { 
        const api = new AuthApi(new Configuration({ basePath: API_URL, credentials: "include" }));
        return from(api.authSignInPost({signInRequest: { name, password }}))
            .pipe(map(res => { 
                this.authModel = res;
            }));
    }
}
