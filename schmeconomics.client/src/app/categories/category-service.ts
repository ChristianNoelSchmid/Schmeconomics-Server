import { inject, Injectable } from "@angular/core";
import { CategoryModel, CategoryOpenApiService, CategoryRefillValueUpdate, UpdateCategoriesRefillValueRequest } from "../../openapi";
import { Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class CategoryService {
    private categoryOpenApiService = inject(CategoryOpenApiService);

    getCategories(accountId: string): Observable<CategoryModel[]> {
        return this.categoryOpenApiService.categoryForAccountAccountIdGet(accountId);
    }

    createCategory(accountId: string, name: string, balance: number, refillValue: number): Observable<CategoryModel> {
        return this.categoryOpenApiService.categoryCreatePost({ accountId, name, balance, refillValue });
    }

    deleteCategory(id: string): Observable<void> {
        return this.categoryOpenApiService.categoryDeleteIdDelete(id);
    }

    updateOrder(accountId: string, categoryIds: string[]): Observable<CategoryModel[]> {
        return this.categoryOpenApiService.categoryUpdateOrderPut({ accountId, categoryIds });
    }

    updateRefillValues(accountId: string, refillValues: CategoryRefillValueUpdate[]) {
        return this.categoryOpenApiService.categoryUpdateRefillValuesPut({ accountId, refillValues });
    }
}