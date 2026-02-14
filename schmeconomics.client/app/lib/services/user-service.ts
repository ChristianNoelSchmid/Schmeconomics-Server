import { UserApi, type CreateUserRequest, type UpdateUserRequest, type UserModel } from "../openapi";
import { getApiConfiguration } from "./auth-state";

export class UserService {
  async api() { 
    return new UserApi(await getApiConfiguration(true)); 
  }

  async getAllUsers(): Promise<UserModel[]> {
    const api = await this.api();
    return await api.userAllGet();
  }

  async getUserById(id: string): Promise<UserModel> {
    const api = await this.api();
    return await api.userByIdIdGet({ id });
  }

  async getUserByName(name: string): Promise<UserModel> {
    const api = await this.api();
    return await api.userByNameNameGet({ name });
  }

  async createUser(request: CreateUserRequest): Promise<UserModel> {
    const api = await this.api();
    return await api.userCreatePost({ createUserRequest: request });
  }

  async updateUser(_id: string, request: UpdateUserRequest): Promise<UserModel> {
    // For update we need to create a new UpdateUserRequest with the userId
    const updateUserRequest: UpdateUserRequest = {
      ...request,
      userId: _id
    };
    const api = await this.api();
    return await api.userUpdatePut({ updateUserRequest });
  }

  async deleteUser(id: string): Promise<UserModel> {
    const api = await this.api();
    return await api.userDeleteUserIdDelete({ userId: id });
  }
}
