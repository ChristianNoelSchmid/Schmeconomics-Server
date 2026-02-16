import { computedAsync } from "@vueuse/core";
import { CategoryApi, type CategoryModel, type CreateCategoryRequest, type UpdateCategoryRequest } from "../openapi";
import { getApiConfiguration } from "./auth-state";
import { useAccountState, useDefaultAccountId } from "./account-service";

export class CategoryService {
    defaultAccountCategories(): globalThis.Ref<CategoryModel[]> {
        const accountState = useAccountState();
        const defaultAccountId = useDefaultAccountId();

        return computedAsync<CategoryModel[]>(
            async () => {
                if (accountState.value && defaultAccountId.value != null) {
                    try {
                        const api = new CategoryApi(await getApiConfiguration(true));
                        return await api.categoryForAccountAccountIdGet({ accountId: defaultAccountId.value })
                            ?? [];
                        } catch (error) {
                        console.error('Failed to load categories:', error);
                    }
                }
                return [];
            }, []
        );
    }

    // Fetch categories for the default account
    async fetchCategories(accountId: string): Promise<CategoryModel[]> {
        try {
            const config = await getApiConfiguration(true);
            const api = new CategoryApi(config);

            return await api.categoryForAccountAccountIdGet({ accountId }); 
        } catch (error) {
            console.error('Error fetching categories:', error);
            return [];
        }
    }

    async createCategory(accountId: string, state: CreateCategoryRequest) {
        try {
            const config = await getApiConfiguration(true);
            const api = new CategoryApi(config);
            const request: CreateCategoryRequest = {
            accountId: accountId,
            name: state.name,
            balance: state.balance,
            refillValue: state.refillValue
            };

            await api.categoryCreatePost({ createCategoryRequest: request });
        } catch (error) {
            console.error('Failed to create category:', error);
        }
    }
    async updateCategory(categoryId: string, request: UpdateCategoryRequest) {
        try {
            const config = await getApiConfiguration(true);
            const api = new CategoryApi(config);
            await api.categoryUpdateIdPut({ id: categoryId, updateCategoryRequest: request });
        } catch (error) {
            console.error('Failed to update category:', error);
        }
    }
    async deleteCategory(categoryId: string) {
        try {
            const config = await getApiConfiguration(true);
            const api = new CategoryApi(config);
            await api.categoryDeleteIdDelete({ id: categoryId });
        } catch (error) {
            console.error('Failed to delete category:', error);
            alert('Failed to delete category');
        }
    }
}
