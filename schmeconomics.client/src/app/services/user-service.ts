/*import { inject, Injectable } from "@angular/core";
import { ApiConfigService } from "./api-config-service";
import { UserModel, CreateUserRequest, UpdateUserRequest, UserService } from "../../openapi";
import { from, map, mergeMap, Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class UserSvc {
    private apiConfigService = inject(ApiConfigService);
    private apiRequest<T>(api_fn: (a: UserService) => Promise<T>): Observable<T> {
        return this.apiConfigService.configuration().pipe(
            mergeMap(config => {
                var api = new UserApi(config);
                return from(api_fn(api));
            })
        );
    }

    getAllUsers(): Observable<UserModel[]> {
        return this.apiRequest(api => api.userAllGet());
    }

    getUserById(id: string): Observable<UserModel> {
        return this.apiRequest(api => api.userByIdIdGet({ id }));
    }

    getUserByName(name: string): Observable<UserModel> {
        return this.apiRequest(api => api.userByNameNameGet({ name }));
    }

    createUser(createUserRequest: CreateUserRequest): Observable<UserModel> {
        return this.apiRequest(api => api.userCreatePost({ createUserRequest }));
    }

    createAdmin(): Observable<void> {
        return this.apiRequest(api => api.userCreateAdminPost());
    }

    getCurrentUser(): Observable<UserModel> {
        return this.apiRequest(api => api.userCurrentGet());
    }

    deleteUser(userId: string): Observable<UserModel> {
        return this.apiRequest(api => api.userDeleteUserIdDelete({ userId }));
    }

    updateUser(updateUserRequest: UpdateUserRequest): Observable<UserModel> {
        return this.apiRequest(api => api.userUpdatePut({ updateUserRequest }));
    }
}
*/