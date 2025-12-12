import { inject, Injectable } from "@angular/core";
import { AuthOpenApiService, SignInModel, UserModel } from "../../openapi";
import { catchError, map, Observable, of, shareReplay, tap, throwError } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private refreshObservable$: Observable<SignInModel | undefined> | null = null;
    private signInModel: SignInModel | undefined;
    private authService = inject(AuthOpenApiService);

    constructor() {
        this.authService.defaultHeaders 
            = this.authService.defaultHeaders.append("X-Skip-Auth", "true");
    }

    private getSignInModel(): Observable<SignInModel | undefined> {
        // If there is already and assigned SignInModel in cache, return it
        if(this.signInModel && Date.parse(this.signInModel!.expiresOnUtc) > Date.now()) {
            return of(this.signInModel);
        }

        // Next, we should try to refresh. However, we only want to refresh once
        // in the event that the derived methods of this one are called in different places
        // in the codebase.
        if(this.refreshObservable$ !== null) {
            // A refresh is already running, return the in-flight observable.
            // All subsequent callers will wait for *this* single observable to complete.
            return this.refreshObservable$; 
        }

        // At this point, there is no refresh already running.
        // Perform on and assign it to the observable
        const refreshCache$ = this.authService.authRefreshPost().pipe(
            tap(model => {
                this.signInModel = model; // Assign the cached model
                this.refreshObservable$ = null; // Refresh is complete - delete refresh model
            }),
            catchError(err => {
               this.refreshObservable$ = null;
               this.signInModel = undefined; 
               return throwError(() => err);
            }),
            shareReplay(1)
        );

        this.refreshObservable$ = refreshCache$;
        return refreshCache$;
    }

    getUser(): Observable<UserModel | undefined> {
        return this.getSignInModel().pipe(map(m => m?.userModel));
    }
    userIsAdmin(): Observable<boolean> {
        return this.getUser().pipe(map(u => u?.role == 'Admin'));
    }
    getAccessKey(): Observable<string | undefined> { 
        return this.getSignInModel().pipe(map(m => m?.accessToken));
    }
    tryLogin(name: string, password: string): Observable<void> { 
        return this.authService.authSignInPost({ name, password })
            .pipe(map(res => { this.signInModel = res; }));
    }
}
