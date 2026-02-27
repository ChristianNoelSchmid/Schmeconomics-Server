import type { CategoryModel, CreateCategoryRequest, UpdateCategoryRequest } from "../openapi";
import { useDefaultAccountId } from "./account-service";

export function accountCategoriesData() {
    const { $api } = useNuxtApp();
    const defaultAccountId = useDefaultAccountId();
    const { start, finish } = useLoadingIndicator();

    const { data: categories, refresh, clear } = useAsyncData<CategoryModel[]>(
        'categories-list',
        async () => {
            start();
            try {
                return await $api.category.categoryForAccountAccountIdGet({
                    accountId: defaultAccountId.value
                });
            } finally {
                finish();
            }
        },
        {
            watch: [() => defaultAccountId.value]
        }
    )
    return { categories, refresh, clear };
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
