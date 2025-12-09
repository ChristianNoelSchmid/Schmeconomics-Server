import { ApplicationConfig, inject, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { BASE_PATH } from '../openapi';
import { HttpEvent, HttpHandlerFn, HttpRequest, provideHttpClient, withInterceptors } from '@angular/common/http';
import { map, mergeMap, Observable } from 'rxjs';
import { CredentialsService } from './services/credentials-service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    { provide: BASE_PATH, useValue: "http://localhost:5153"},
    provideHttpClient(withInterceptors([jwtInterceptor]))
  ]
};

function jwtInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
    const credsService = inject(CredentialsService);

    let sendReq = req.clone({
        credentials: "include",
        withCredentials: true,
    });

    if(req.headers.get("X-Skip-Auth") == null) {
        return credsService.getAccessKey().pipe(
            mergeMap(accessKey => {
                sendReq = sendReq.clone({
                    headers: req.headers.append("Authorization", `bearer ${accessKey}`),
                });
                return next(sendReq);
            })
        )
    }
    sendReq = sendReq.clone({
        headers: sendReq.headers.delete("X-Skip-Auth")
    });
    return next(sendReq);
}
