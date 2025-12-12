export * from './account.service';
import { AccountOpenApiService } from './account.service';
export * from './auth.service';
import { AuthOpenApiService } from './auth.service';
export * from './category.service';
import { CategoryOpenApiService } from './category.service';
export * from './user.service';
import { UserOpenApiService } from './user.service';
export const APIS = [AccountOpenApiService, AuthOpenApiService, CategoryOpenApiService, UserOpenApiService];
