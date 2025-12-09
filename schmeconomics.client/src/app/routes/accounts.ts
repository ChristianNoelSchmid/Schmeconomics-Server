import { Component, effect, inject } from "@angular/core";
import { AccountModel, AccountService } from "../../openapi";
import { Observable } from "rxjs";
import { AsyncPipe } from "@angular/common";
import { RouterLink } from "@angular/router";

@Component({
    template: `
        @if(accounts$ | async; as accounts) {
            @for(account of accounts; track account.id) {
                <p><a [routerLink]="['/categories',account.id]">{{ account.name }}</a></p>
            }
        } 
        @else {
            <p>Loading...</p>
        }
    `,
    imports: [AsyncPipe, RouterLink]
})
export class Accounts {
    accountService = inject(AccountService);
    accounts$!: Observable<AccountModel[]>;

    constructor() {
        effect(() => {
            this.accounts$ = this.accountService.accountAllGet();
        });
    }
}
