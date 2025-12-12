import { Routes } from '@angular/router';
import { LoginRoute } from './auth/login-route';
import { AccountRoute } from './accounts/account-route';
import { CategoryRoute } from './categories/category-routes';

export const routes: Routes = [{
    path: "login",
    title: "Login",
    component: LoginRoute
}, {
    path: "accounts",
    title: "Accounts",
    component: AccountRoute
}, {
    path: "categories/:accountId",
    title: "Categories",
    component: CategoryRoute
}];
