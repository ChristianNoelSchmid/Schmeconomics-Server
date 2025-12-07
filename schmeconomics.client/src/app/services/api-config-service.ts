import { inject, Injectable } from "@angular/core";
import { Configuration } from "../../openapi";
import { AuthService } from "./auth-service";
import { API_URL } from "../config";
import { from, map, Observable } from "rxjs";

@Injectable({ providedIn: 'root' })
export class ApiConfigService {
    private authService = inject(AuthService)
    configuration(): Observable<Configuration> { 
        return from(this.authService.getAccessKey())
            .pipe(map(accessToken => 
                new Configuration({ 
                    headers: accessToken != null ? { "Authorization": `bearer ${accessToken}` } : {},
                    basePath: API_URL, 
                    credentials: "include"
                })
            ));
    }
}