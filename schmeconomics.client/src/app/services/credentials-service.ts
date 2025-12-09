import { inject, Injectable } from "@angular/core";
import { AuthModel, AuthService } from "../../openapi";
import { map, Observable, of } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class CredentialsService {
    private authModel: AuthModel | null = null
    private authService = inject(AuthService);

    constructor() {
        this.authService.defaultHeaders 
            = this.authService.defaultHeaders.append("X-Skip-Auth", "true");
    }

    getAccessKey(): Observable<string | null> { 
        // If the access key is null or is past expiration, attempt a refresh before continuing
        if(
            this.authModel == null || 
            Date.parse(this.authModel.expiresOnUtc) < Date.now()
        ) {
            return this.authService.authRefreshPost()
                .pipe(map(res => {
                    this.authModel = res;
                    return this.authModel.accessToken;
                }));
        // Otherwise, just return the access key
        } else {
            return of(this.authModel?.accessToken || null);
        }
    }
    tryLogin(name: string, password: string): Observable<void> { 
        return this.authService.authSignInPost({ name, password })
            .pipe(map(res => { this.authModel = res; }));
    }
}
