import { TransactionApi, type TransactionAccountIdPostRequest } from "../openapi";
import { getApiConfiguration } from "./auth-state";

export class TransactionService {
    async createTransaction(accountId: string, categoryId: string, amount: number, notes: string, isAddition: boolean) {
        try {
            const api = new TransactionApi(getApiConfiguration(true));
            
            // Create the refill value update request
            const createTransactionRequest: TransactionAccountIdPostRequest = {
                accountId,
                createTransactionRequest: [{
                    categoryId,
                    amount: isAddition ? amount : -amount,
                    notes,
                }]
                
            };

            await api.transactionAccountIdPost(createTransactionRequest)
        } catch (error) {
            console.error('Failed to create transaction:', error);
            throw error;
        }
    }
}