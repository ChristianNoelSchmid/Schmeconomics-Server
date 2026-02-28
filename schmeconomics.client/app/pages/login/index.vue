<script setup lang="ts">
import type { FormError, FormSubmitEvent } from '@nuxt/ui';
import { onError } from '~/lib/form-error';
import { accountData } from '~/lib/services/accounts';
import { useSignInState } from '~/lib/services/auth';

const { $api } = useNuxtApp();
const signInState = useSignInState();
const { refresh } = accountData();

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
  const res = await $api.auth.authSignInPost({ signInRequest: { name: event.data.name!, password: event.data.password! } });

  signInState.value = res;
  refresh();

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