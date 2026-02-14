import { CategoryApi, type CreateCategoryRequest, type UpdateCategoryRequest } from "../openapi";
import { useAccountState, useDefaultAccountName } from "./account-service";
import { getApiConfiguration } from "./auth-state";

export class CategoryService {
    async createCategory(state: CreateCategoryRequest) {
        const accountState = useAccountState();
        const defaultAccountName = useDefaultAccountName();

        if (!accountState.value || !defaultAccountName.value) {
            return;
        }

        const account = accountState.value.find(a => a.name === defaultAccountName.value);

        if (!account) {
            return;
        }

        try {
            const config = await getApiConfiguration(true);
            const api = new CategoryApi(config);
            const request: CreateCategoryRequest = {
            accountId: account.id,
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
