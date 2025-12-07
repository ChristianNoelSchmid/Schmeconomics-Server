import { Component, effect, inject } from "@angular/core";
import { AccountModel } from "../../openapi";
import { AccountService } from "../services/account-service";
import { Observable } from "rxjs";
import { AsyncPipe } from "@angular/common";

@Component({
    template: `
        @if(accounts$ | async; as accounts) {
            @for(account of accounts; track account.id) {
                <p>{{ account.name }}</p>
            }
        } 
        @else {
            <p>Loading...</p>
        }
    `,
    imports: [AsyncPipe]
})
export class Accounts {
    accountService = inject(AccountService);
    accounts$!: Observable<AccountModel[]>;

    constructor() {
        effect(() => {
            this.accounts$ = this.accountService.getAccounts();
        });
    }
}
