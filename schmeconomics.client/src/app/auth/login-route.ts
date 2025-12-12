import { Component, inject } from "@angular/core";
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from "@angular/forms";
import { AuthService } from "./auth-service";

@Component({
    imports: [ReactiveFormsModule],
    template: `
        <h2>Login</h2>
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
            <label>
                Name
                <input type="text" formControlName="name" />
            </label>
            <label>
                Password
                <input type="password" formControlName="password" />
            </label>
            <button type="submit" [disabled]="!loginForm.valid">Login</button>
        </form>
        @if (errors.length > 0) {
            @for(error of errors; track error) {
                <p class="red">{{ error }}</p>
            }
        }
    `
})
export class LoginRoute {
    credsService = inject(AuthService);

    errors: string[] = []
    loginForm = new FormGroup({
        name: new FormControl('', [Validators.required]),
        password: new FormControl('', [Validators.required])
    })
    onSubmit() {
        this.errors = [];
        for(const error in this.loginForm.errors) {
            this.errors.push(error);
        }
        if(this.errors.length == 0) {
            const name = this.loginForm.value.name!;
            const password = this.loginForm.value.password!;

            this.credsService.tryLogin(name, password).subscribe();
        }
    }
}