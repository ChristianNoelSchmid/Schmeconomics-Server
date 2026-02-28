<script setup lang="ts">
import { AccountApi, type UserModel } from "~/lib/openapi";
import { accountData } from "~/lib/services/accounts";
import { userData, UserService } from "~/lib/services/users";

const props = defineProps<{
  accountId: string;
  visible: boolean;
}>();

const emit = defineEmits<{
  closed: [];
}>();

const { accounts } = accountData();
const { users, refresh } = userData();
const userService = new UserService();

const selectedUsers = computed<UserModel[]>(() => {
  const currentAccount = accounts.value?.find(acc => acc.id === props.accountId);
  if (currentAccount && currentAccount.users) {
    return currentAccount.users;
  }
  return [];
});

async function toggleUser(userId: string) {
  await userService.toggleUserToAccount(userId, props.accountId);
  refresh();
}

function isUserSelected(userId: string): boolean {
  return selectedUsers.value.some(u => u.id == userId);
}
</script>

<template>
  <UModal :open="props.visible" @close="emit('closed')">
    <template #content>
      <UCard>
        <template #header>
          <h3 class="text-lg font-semibold">Manage Account Users</h3>
        </template>
        
        <div class="space-y-4">
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
            <UButton color="primary" @click="emit('closed')">Close</UButton>
          </div>
        </template>
      </UCard>
    </template>
  </UModal>
</template>