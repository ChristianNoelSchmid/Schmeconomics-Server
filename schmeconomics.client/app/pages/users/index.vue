<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { UserService } from '~/lib/services/user-service';
import { Role, type UserModel, type CreateUserRequest, type UpdateUserRequest } from '~/lib/openapi';
import { useSignInState } from '~/lib/services/auth-state';

const signInState = useSignInState();
const userService = new UserService();

const users = ref<UserModel[]>([]);
const filteredUsers = ref<UserModel[]>([]);
const searchName = ref('');
const showCreateModal = ref(false);
const editingUser = ref<UserModel | null>(null);
const showEditModal = ref(false);

// Load all users when the component is mounted
onMounted(async () => {
  if (!signInState.value) {
    navigateTo('/login');
    return;
  }

  // Only Admin users can access this page
  if (signInState.value.userModel.role !== Role.Admin) {
    navigateTo('/');
    return;
  }

  await loadUsers();
});

async function loadUsers() {
  try {
    users.value = await userService.getAllUsers();
    filteredUsers.value = [...users.value];
  } catch (error) {
    console.error('Failed to load users:', error);
    // Handle error appropriately
  }
}

// Filter users based on search input
watch(searchName, () => {
  if (!searchName.value) {
    filteredUsers.value = [...users.value];
  } else {
    const searchTerm = searchName.value.toLowerCase();
    filteredUsers.value = users.value.filter(user => 
      user.name.toLowerCase().includes(searchTerm)
    );
  }
});

// Handle creating a new user
async function handleCreateUser(request: CreateUserRequest) {
  try {
    await userService.createUser(request);
    showCreateModal.value = false;
    await loadUsers();
  } catch (error) {
    console.error('Failed to create user:', error);
    // Handle error appropriately
  }
}

// Handle updating an existing user
async function handleUpdateUser(request: UpdateUserRequest) {
  if (!editingUser.value) return;

  try {
    await userService.updateUser(editingUser.value.id, request);
    showEditModal.value = false;
    await loadUsers();
  } catch (error) {
    console.error('Failed to update user:', error);
    // Handle error appropriately
  }
}

// Handle deleting a user
async function handleDeleteUser(userId: string) {
  if (!confirm('Are you sure you want to delete this user?')) {
    return;
  }

  try {
    await userService.deleteUser(userId);
    await loadUsers();
  } catch (error) {
    console.error('Failed to delete user:', error);
    // Handle error appropriately
  }
}

// Open the create modal
function openCreateModal() {
  editingUser.value = null;
  showCreateModal.value = true;
}

// Open the edit modal with user data
function openEditModal(user: UserModel) {
  editingUser.value = user;
  showEditModal.value = true;
}
</script>

<template>
  <div class="p-4">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold">Users Management</h1>
      <UButton color="primary" variant="solid" icon="i-heroicons-plus-circle" @click="openCreateModal">
        Add User
      </UButton>
    </div>

    <!-- Search input -->
    <div class="mb-6">
      <UInput 
        v-model="searchName" 
        placeholder="Search by name..." 
        icon="i-heroicons-magnifying-glass"
        class="w-full max-w-md"
      />
    </div>

    <!-- Users table -->
    <UCard>
      <template #header>
        <div class="flex justify-between items-center">
          <h2 class="text-lg font-semibold">Users</h2>
          <span class="text-sm text-gray-500">{{ filteredUsers.length }} users</span>
        </div>
      </template>

      <div v-if="filteredUsers.length === 0" class="text-center py-8">
        <p>No users found.</p>
      </div>

      <table v-else class="w-full text-sm">
        <thead>
          <tr class="border-b">
            <th class="text-left p-2">Name</th>
            <th class="text-left p-2">Role</th>
            <th class="text-left p-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id" class="border-b">
            <td class="p-2">{{ user.name }}</td>
            <td class="p-2">{{ user.role }}</td>
            <td class="p-2">
              <div class="flex space-x-2">
                <UButton 
                  size="md" 
                  color="warning" 
                  variant="soft" 
                  icon="i-heroicons-pencil-square" 
                  @click="openEditModal(user)"
                />
                <UButton 
                  v-if="user.id !== signInState?.userModel.id" 
                  size="md" 
                  color="error" 
                  variant="soft" 
                  icon="i-heroicons-trash" 
                  @click="handleDeleteUser(user.id)"
                />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </UCard>

    <!-- Create User Modal -->
    <UModal :open="showCreateModal">
      <template #content>
        <CreateUserModal 
          :visible="showCreateModal" 
          @submitted="handleCreateUser" 
          @closed="showCreateModal = false" 
        />
      </template>
    </UModal>

    <!-- Edit User Modal -->
    <UModal :open="showEditModal">
      <template #content>
        <CreateUserModal 
          :visible="showEditModal" 
          :user-to-edit="editingUser"
          @submitted="handleUpdateUser" 
          @closed="showEditModal = false" 
        />
      </template>
    </UModal>
  </div>
</template>

<style scoped>
/* Add any custom styles here if needed */
</style>