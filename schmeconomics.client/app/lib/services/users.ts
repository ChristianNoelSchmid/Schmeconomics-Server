import type { CreateUserRequest, UpdateUserRequest, UserModel } from "../openapi";
import { useSignInState as useSignInState } from "./auth";

export function userData() {
  const { $api } = useNuxtApp();
  const signInState = useSignInState();

  const { data: users, refresh, clear } = useAsyncData<UserModel[]>(
    'users-list',
    async () => {
      if(signInState.value != null) {
        return await $api.user.apiV1UserAllGet();
      }
      return [];
    },
    {
      watch: [() => signInState.value]
    }
  )

  return { users, refreshUsers: refresh, clear };
}

export class UserService {
  async getAllUsers(): Promise<UserModel[]> {
    const { $api } = useNuxtApp();
    return await $api.user.apiV1UserAllGet();
  }

  async getUserById(id: string): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.apiV1UserByIdIdGet({ id });
  }

  async getUserByName(name: string): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.apiV1UserByNameNameGet({ name });
  }

  async createUser(request: CreateUserRequest): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.apiV1UserCreatePost({ createUserRequest: request });
  }

  async updateUser(_id: string, request: UpdateUserRequest): Promise<UserModel> {
    const { $api } = useNuxtApp();

    // For update we need to create a new UpdateUserRequest with the userId
    const updateUserRequest: UpdateUserRequest = {
      ...request,
      userId: _id
    };
    return await $api.user.apiV1UserUpdatePut({ updateUserRequest });
  }

  async deleteUser(id: string): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.apiV1UserDeleteUserIdDelete({ userId: id });
  }

  async toggleUserToAccount(userId: string, accountId: string): Promise<void> {
    const { $api } = useNuxtApp();
    await $api.account.apiV1AccountToggleUserPost({ toggleUserToAccountRequest: { accountId, userId }});
  }
}
