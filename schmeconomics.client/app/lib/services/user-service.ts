import { UserApi, type CreateUserRequest, type UpdateUserRequest, type UserModel } from "../openapi";
import { getApiConfiguration } from "./auth-state";

export class UserService {
  private userApi: UserApi;

  constructor() {
    this.userApi = new UserApi(getApiConfiguration(true));
  }

  async getAllUsers(): Promise<UserModel[]> {
    return await this.userApi.userAllGet();
  }

  async getUserById(id: string): Promise<UserModel> {
    return await this.userApi.userByIdIdGet({ id });
  }

  async getUserByName(name: string): Promise<UserModel> {
    return await this.userApi.userByNameNameGet({ name });
  }

  async createUser(request: CreateUserRequest): Promise<UserModel> {
    return await this.userApi.userCreatePost({ createUserRequest: request });
  }

  async updateUser(_id: string, request: UpdateUserRequest): Promise<UserModel> {
    // For update we need to create a new UpdateUserRequest with the userId
    const updateUserRequest: UpdateUserRequest = {
      ...request,
      userId: _id
    };
    return await this.userApi.userUpdatePut({ updateUserRequest });
  }

  async deleteUser(id: string): Promise<UserModel> {
    return await this.userApi.userDeleteUserIdDelete({ userId: id });
  }
}