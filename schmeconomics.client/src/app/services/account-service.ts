import { inject, Injectable } from "@angular/core";
import { AccountApi, AccountModel } from "../../openapi";
import { from, map, mergeMap, Observable } from "rxjs";
import { ApiConfigService } from "./api-config-service";

@Injectable({
    providedIn: 'root'
})
export class AccountService {
    private apiConfigService = inject(ApiConfigService);

    getAccounts(): Observable<AccountModel[]> {
        return this.apiConfigService.configuration()
            .pipe(map(config => new AccountApi(config)))
            .pipe(mergeMap(api => from(api.accountAllGet())));
    }
}