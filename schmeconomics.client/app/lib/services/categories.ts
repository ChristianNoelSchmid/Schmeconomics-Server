import type { CategoryModel, CreateCategoryRequest, UpdateCategoryRequest } from "../openapi";
import { useSignInState } from "./auth";

export function accountCategoriesData() {
    const { $api, $defaultAccountId } = useNuxtApp();
    const signInState = useSignInState();

    const { data: categories, refresh } = useAsyncData<CategoryModel[]>(
        'categories-list',
        async () => await $api.category.categoryForAccountAccountIdGet({
            accountId: $defaultAccountId.value
        }),
        {
            watch: [
                () => signInState.value,
                () => $defaultAccountId.value,
            ]
        }
    )
    return { categories, refresh };
}

export class CategoryService {
    async createCategory(accountId: string, state: CreateCategoryRequest) {
        const { $api } = useNuxtApp();
        try {
            const request: CreateCategoryRequest = {
                accountId: accountId,
                name: state.name,
                balance: state.balance,
                refillValue: state.refillValue
            };

            await $api.category.categoryCreatePost({ createCategoryRequest: request });
        } catch (error) {
            console.error('Failed to create category:', error);
        }
    }
    async updateCategory(categoryId: string, request: UpdateCategoryRequest) {
        const { $api } = useNuxtApp();
        try {
            await $api.category.categoryUpdateIdPut({ id: categoryId, updateCategoryRequest: request });
        } catch (error) {
            console.error('Failed to update category:', error);
        }
    }
    async deleteCategory(categoryId: string) {
        const { $api } = useNuxtApp();
        try {
            await $api.category.categoryDeleteIdDelete({ id: categoryId });
        } catch (error) {
            console.error('Failed to delete category:', error);
            alert('Failed to delete category');
        }
    }
}
