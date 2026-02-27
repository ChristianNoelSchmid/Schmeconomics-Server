import type { CreateUserRequest, UpdateUserRequest, UserModel } from "../openapi";
import { useSignInState as useSignInState } from "./auth-state";

export function userData() {
  const { $api } = useNuxtApp();
  const signInState = useSignInState();
  const { start, finish } = useLoadingIndicator();

  const { data: users, refresh, clear } = useAsyncData<UserModel[]>(
    'users-list',
    async () => {
      start();
      try {
        if(signInState.value != null) {
          return await $api.user.userAllGet();
        }
        return [];
      } finally {
        finish();
      }
    },
    {
      watch: [() => signInState.value]
    }
  )

  return { users, refresh, clear };
}

export class UserService {
  async getAllUsers(): Promise<UserModel[]> {
    const { $api } = useNuxtApp();
    return await $api.user.userAllGet();
  }

  async getUserById(id: string): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.userByIdIdGet({ id });
  }

  async getUserByName(name: string): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.userByNameNameGet({ name });
  }

  async createUser(request: CreateUserRequest): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.userCreatePost({ createUserRequest: request });
  }

  async updateUser(_id: string, request: UpdateUserRequest): Promise<UserModel> {
    const { $api } = useNuxtApp();

    // For update we need to create a new UpdateUserRequest with the userId
    const updateUserRequest: UpdateUserRequest = {
      ...request,
      userId: _id
    };
    return await $api.user.userUpdatePut({ updateUserRequest });
  }

  async deleteUser(id: string): Promise<UserModel> {
    const { $api } = useNuxtApp();
    return await $api.user.userDeleteUserIdDelete({ userId: id });
  }
}
