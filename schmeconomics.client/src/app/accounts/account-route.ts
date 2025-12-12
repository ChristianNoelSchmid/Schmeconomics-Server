import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { AuthService } from "../auth/auth-service";
import { AccountService } from "./account-service";
import { AccountModel } from "../../openapi";
import { AsyncPipe } from "@angular/common";

@Component({
    template: `
        @if(credentialsService.userIsAdmin() | async; as isAdmin) {
            @for(account of accounts(); track account.id) {
                <div class="flex justify-between">
                    <p><a [routerLink]="['/categories',account.id]">{{ account.name }}</a></p>
                    @if(isAdmin) {
                        <button (click)="onDeleteButtonClick(account.id)">X</button>
                    }
                </div> 
            }
            @if(isAdmin) {
                <button (click)="onCreateButtonClick()">Create New Account</button>
            }
        }
    `,
    imports: [AsyncPipe, RouterLink]
})
export class AccountRoute {
    credentialsService = inject(AuthService);
    accountService = inject(AccountService);
    accounts = signal<AccountModel[]>([]);

    onCreateButtonClick() {
        const newName = prompt("Enter the new account name");
        this.accountService.createAccount(newName!)
            .subscribe(account => this.accounts.update(currentAccounts => [...currentAccounts, account]));
    }

    onDeleteButtonClick(accountId: string) {
        this.accountService.deleteAccount(accountId).subscribe(
            () => this.accounts.update(
                currentAccounts => {
                    let newAccountList = [...currentAccounts];
                    newAccountList.splice(currentAccounts.findIndex(a => a.id == accountId), 1);
                    return newAccountList;
                }
            )
        );
    }

    constructor() {
        this.accountService.getAccounts()
            .subscribe(accounts => this.accounts.update(_ => accounts));
    }
}
