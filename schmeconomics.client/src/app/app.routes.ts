import { Routes } from '@angular/router';
import { Home } from './routes/home';
import { Login } from './routes/login';
import { Accounts } from './routes/accounts';

export const routes: Routes = [{
    path: "",
    title: "Home",
    component: Home,
}, {
    path: "login",
    title: "Login",
    component: Login
}, {
    path: "accounts",
    title: "Accounts",
    component: Accounts
}];
