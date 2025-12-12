import { Component, inject, signal } from "@angular/core";
import { CategoryModel } from "../../openapi";
import { map, mergeMap } from "rxjs";
import { ActivatedRoute } from "@angular/router";
import { AsyncPipe } from "@angular/common";
import { CategoryService } from "./category-service";
import { AuthService as AuthService } from "../auth/auth-service";

@Component ({
    template: `
        @if(authService.userIsAdmin() | async; as isAdmin) {
            @for (category of categories(); track category.id) {
                <div class="flex flex-row justify-between">
                    <p>{{ category.name }}</p>
                    <div class="flex flex-column">
                        <button>+</button>
                        <button>-</button>
                    </div>
                    @if(isAdmin) {
                        <button (click)='deleteCategoryClick(category.id)' class="relative top-0 right-0">X</button> 
                    }
                </div>
            }
            @if(isAdmin) {
                <button (click)='createCategoryClick()'>Add Category</button> 
            }
        }
    `,
    imports: [AsyncPipe]
})
export class CategoryRoute {
    accountId!: string
    route = inject(ActivatedRoute);
    authService = inject(AuthService);
    categoryService = inject(CategoryService);
    categories = signal<CategoryModel[]>([]);

    createCategoryClick() {
        const newName = prompt("Enter the new category name");
        const newBal = prompt("Enter the new category balance");
        const newRefillVal = prompt("Enter the new category refill value");

        this.categoryService.createCategory(this.accountId, newName!, +newBal!, +newRefillVal!)
            .subscribe(category => this.categories.update(current => [...current, category]));

    } 

    deleteCategoryClick(categoryId: string) {
        this.categoryService.deleteCategory(categoryId) 
            .subscribe(() => this.categories.update(categories => {
                const newCategories = [...categories];
                newCategories.splice(newCategories.findIndex(c => c.id == categoryId), 1);
                return newCategories;
            }));
    }

    constructor() {
        this.route.paramMap
            .pipe(map(paramMap => this.accountId = paramMap.get("accountId")!))
            .pipe(mergeMap(accountId => this.categoryService.getCategories(accountId)))
            .subscribe(categories => this.categories.update(_ => categories));
    }
}