import { inject, Injectable } from "@angular/core";
import { ApiConfigService } from "./api-config-service";
import { CategoryApi, CategoryModel } from "../../openapi";
import { from, map, Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class CategoryService {
    private apiConfigService = inject(ApiConfigService);
}