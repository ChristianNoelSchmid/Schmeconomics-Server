import { inject, Injectable } from "@angular/core";
import { AccountModel, AccountOpenApiService } from "../../openapi";
import { Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class AccountService {
    private accountOpenApiService = inject(AccountOpenApiService);

    getAccounts(): Observable<AccountModel[]> {
        return this.accountOpenApiService.accountAllGet();
    }

    createAccount(name: string): Observable<AccountModel> {
        return this.accountOpenApiService.accountCreatePost(name);
    }

    deleteAccount(id: string): Observable<void> {
        return this.accountOpenApiService.accountDeleteIdDelete(id);
    }
}