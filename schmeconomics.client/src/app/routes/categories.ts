import { Component, input, Input } from "@angular/core";
import { CategoryModel } from "../../openapi";

@Component ({
    template: `
        @for (category of categories(); track category.id) {
            <p>{{ category.name }}</p>
        }
    `
})
export class Categories {
    categories = input<CategoryModel[]>();
}