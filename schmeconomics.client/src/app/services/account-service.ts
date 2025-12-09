/*import { inject, Injectable } from "@angular/core";
import { AccountApi, AccountModel, ToggleUserToAccountRequest, UpdateAccountRequest } from "../../openapi";
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

    createAccount(name: string): Observable<AccountModel> {
        return this.apiConfigService.configuration()
            .pipe(map(config => new AccountApi(config)))
            .pipe(mergeMap(api => from(api.accountCreatePost({ name }))));
    }

    deleteAccount(id: string): Observable<void> {
        return this.apiConfigService.configuration()
            .pipe(map(config => new AccountApi(config)))
            .pipe(mergeMap(api => from(api.accountDeleteIdDelete({ id }))));
    }

    updateAccount(id: string, name: string): Observable<AccountModel> {
        return this.apiConfigService.configuration()
            .pipe(map(config => new AccountApi(config)))
            .pipe(mergeMap(api => from(api.accountUpdateIdPut({ 
                id, 
                updateAccountRequest: { name } 
            }))));
    }

    toggleUserToAccount(accountId: string, userId: string): Observable<void> {
        return this.apiConfigService.configuration()
            .pipe(map(config => new AccountApi(config)))
            .pipe(mergeMap(api => from(api.accountToggleUserPost({ 
                toggleUserToAccountRequest: { accountId, userId } 
            }))));
    }
}
*/