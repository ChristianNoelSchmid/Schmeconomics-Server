import { Component, effect, inject, input, Input } from "@angular/core";
import { CategoryModel, CategoryService } from "../../openapi";
import { mergeMap, Observable } from "rxjs";
import { ActivatedRoute } from "@angular/router";
import { AsyncPipe } from "@angular/common";

@Component ({
    template: `
        @if (categories$ | async; as categories) {
            @for (category of categories; track category.id) {
                <p>{{ category.name }}</p>
            }
        }
    `,
    imports: [AsyncPipe]
})
export class Categories {
    route = inject(ActivatedRoute);
    categoryService = inject(CategoryService);
    categories$!: Observable<CategoryModel[]>;

    constructor() {
        effect(() => {
            this.categories$ = this.route.paramMap
                .pipe(mergeMap(paramMap => 
                    this.categoryService.categoryForAccountAccountIdGet(paramMap.get("accountId")!)
                ));
        });
    }
}