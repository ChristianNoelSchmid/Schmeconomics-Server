<script setup lang="ts">
import type { FormError, FormSubmitEvent } from '@nuxt/ui';
import { onError } from '~/lib/form-error';
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
  const api = new AuthApi(await getApiConfiguration(false));
  const res = await api.authSignInPost({ signInRequest: { name: event.data.name!, password: event.data.password! } });

  signInState.value = res;
  await refreshAccountState();

  navigateTo('/');
}
</script>

<template>
  <div class="my-4 flex justify-center">
    <UForm class="space-y-4" :validate="validate" :state="formState" @submit="onSubmit" @error="onError">
      <UFormField label="Name" name="name">
        <UInput v-model="formState.name" type="text" />
      </UFormField>
      <UFormField label="Password" name="password">
        <PasswordInput v-model="formState.password" type="password" :show-reveal-password-button="true" />
      </UFormField>
      <UButton type="submit">Submit</UButton>
    </UForm>
  </div>
</template>