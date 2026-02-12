<script setup lang="ts">
import type { FormError, FormSubmitEvent } from '@nuxt/ui';
import { AuthApi } from '~/lib/openapi';
import { refreshAccountState } from '~/lib/services/account-service';
import { getApiConfiguration, useSignInState } from '~/lib/services/auth-state';

const signInState = useSignInState();

const formState = reactive({
  name: undefined,
  password: undefined
});

type Schema = typeof formState;

function validate(state: Partial<Schema>): FormError[] {
  const errors = [];
  if (!state.name) errors.push({ name: 'name', message: 'Required' });
  if (!state.password) errors.push({ name: 'password', message: 'Required' });
  return errors;
}

async function onSubmit(event: FormSubmitEvent<Schema>) {
  const api = new AuthApi(getApiConfiguration(false));
  const res = await api.authSignInPost({ signInRequest: { name: event.data.name!, password: event.data.password! } });

  signInState.value = res;
  await refreshAccountState();

  navigateTo('/');
}
</script>

<template>
  <div class="my-4 flex justify-center">
    <UForm class="space-y-4" :validate="validate" :state="formState" @submit="onSubmit">
      <NumberInput />
      <UFormField label="Name">
        <UInput v-model="formState.name" type="text" />
      </UFormField>
      <UFormField label="Password">
        <UInput v-model="formState.password" type="password" />
      </UFormField>
      <UButton type="submit">Submit</UButton>
    </UForm>
  </div>
</template>