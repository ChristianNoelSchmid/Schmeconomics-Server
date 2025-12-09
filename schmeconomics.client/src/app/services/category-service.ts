/*import { inject, Injectable } from "@angular/core";
import { ApiConfigService } from "./api-config-service";
import { CategoryApi, CategoryModel } from "../../openapi";
import { from, map, mergeMap, Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class CategoryService {
    private apiConfigService = inject(ApiConfigService);

    getCategories(accountId: string): Observable<CategoryModel[]> {
        return this.apiConfigService.configuration()
            .pipe(map(config => new CategoryApi(config)))
            .pipe(mergeMap(api => from(api.categoryForAccountAccountIdGet({ accountId }))));
    }
}*/