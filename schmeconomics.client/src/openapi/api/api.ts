export * from './account.service';
import { AccountService } from './account.service';
export * from './auth.service';
import { AuthService } from './auth.service';
export * from './category.service';
import { CategoryService } from './category.service';
export * from './user.service';
import { UserService } from './user.service';
export const APIS = [AccountService, AuthService, CategoryService, UserService];
