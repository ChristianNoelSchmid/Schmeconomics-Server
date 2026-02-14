<script setup lang="ts">
import { AccountApi, type UserModel } from "~/lib/openapi";
import { getApiConfiguration } from "~/lib/services/auth-state";
import { UserService } from "~/lib/services/user-service";

const props = defineProps<{
  accountId: string;
  visible: boolean;
}>();

const emit = defineEmits<{
  closed: [];
}>();

const users = ref<UserModel[]>([]);
const selectedUsers = ref<string[]>([]);
const loading = ref(false);

async function loadUsers() {
  try {
    const userService = new UserService();
    users.value = await userService.getAllUsers();
    
    // Get current account to check which users are already associated
    const api = new AccountApi(await getApiConfiguration(true));
    const account = await api.accountAllGet();
    const currentAccount = account.find(acc => acc.id === props.accountId);
    
    if (currentAccount && currentAccount.users) {
      selectedUsers.value = currentAccount.users.map(u => u.id);
    }
  } catch (error) {
    console.error("Error loading users:", error);
  }
}

watch(() => props.visible, async (newVal) => {
  if (newVal) {
    await loadUsers();
  }
});

async function toggleUser(userId: string) {
  try {
    const api = new AccountApi(await getApiConfiguration(true));
    await api.accountToggleUserPost({
      toggleUserToAccountRequest: {
        accountId: props.accountId,
        userId
      }
    });
    
    // Update selected users list after successful toggle
    if (selectedUsers.value.includes(userId)) {
      selectedUsers.value = selectedUsers.value.filter(id => id !== userId);
    } else {
      selectedUsers.value.push(userId);
    }
  } catch (error) {
    console.error("Error toggling user:", error);
  }
}

function isUserSelected(userId: string): boolean {
  return selectedUsers.value.includes(userId);
}

function submit() {
  emit('closed');
}
</script>

<template>
  <UModal :open="props.visible" @close="emit('closed')">
    <template #content>
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold">Manage Account Users</h3>
        </template>
        
        <div v-if="loading" class="flex justify-center items-center py-8">
          <USpinner />
        </div>
        
        <div v-else class="space-y-4">
          <p>Select users to add or remove from this account:</p>
          
          <div class="space-y-2 max-h-96 overflow-y-auto">
            <UCard
              v-for="user in users"
              :key="user.id"
              :class="{ 'ring-2 ring-primary': isUserSelected(user.id) }"
            >
              <div class="flex items-center space-x-3">
                <UCheckbox 
                  :model-value="isUserSelected(user.id)"
                  @update:model-value="toggleUser(user.id)"
                />
                <div>
                  <p class="font-medium">{{ user.name }}</p>
                  <p class="text-sm text-gray-500">{{ user.role }}</p>
                </div>
              </div>
            </UCard>
          </div>
        </div>
        
        <template #footer>
          <div class="flex justify-end">
            <UButton color="primary" @click="submit">Close</UButton>
          </div>
        </template>
      </UCard>
    </template>
  </UModal>
</template>