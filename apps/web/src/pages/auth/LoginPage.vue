<script setup lang="ts">
import { loginSchema } from '@fluxo/contracts';
import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import PasswordInput from '@/components/common/PasswordInput.vue';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useAuthStore } from '@/store/auth.store';

const router = useRouter();
const auth = useAuthStore();

const form = reactive({ username: auth.lastUsername, password: '' });
const errors = ref<Partial<Record<'username' | 'password', string>>>({});
const showForgot = ref(false);

/** O botão "Entrar" só habilita com os dois campos preenchidos. */
const canSubmit = computed(() => form.username.trim() !== '' && form.password !== '');

/**
 * Valida com o mesmo schema da API (@fluxo/contracts) e passa para a tela de sessão, que faz
 * o login de fato mostrando o feed. A senha fica só na memória (store) até lá.
 */
async function submit() {
  const parsed = loginSchema.safeParse(form);
  if (!parsed.success) {
    errors.value = {};
    for (const issue of parsed.error.issues) {
      const field = issue.path[0] as 'username' | 'password';
      errors.value[field] ??= issue.message;
    }
    return;
  }
  errors.value = {};
  auth.startLogin(parsed.data);
  await router.push({ name: 'login-session' });
}
</script>

<template>
  <AuthLayout>
    <h1 class="mb-8 text-center text-2xl font-semibold text-neutral-900">Seja bem-vindo(a)!</h1>

    <form class="flex flex-col gap-5" novalidate @submit.prevent="submit">
      <div class="flex flex-col gap-2">
        <Label for="username" class="font-normal text-neutral-700">
          Usuário<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <Input
          id="username"
          v-model="form.username"
          placeholder="Informe o usuário"
          autocomplete="username"
          autocapitalize="none"
          required
          :aria-invalid="!!errors.username"
          class="h-11 rounded-md border-neutral-400 bg-white"
        />
        <p v-if="errors.username" class="text-xs text-destructive">{{ errors.username }}</p>
      </div>

      <div class="flex flex-col gap-2">
        <Label for="password" class="font-normal text-neutral-700">
          Senha<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <PasswordInput
          id="password"
          v-model="form.password"
          placeholder="Informe sua senha"
          autocomplete="current-password"
          required
          :aria-invalid="!!errors.password"
          class="h-11 rounded-md border-neutral-400 bg-white"
        />
        <p v-if="errors.password" class="text-xs text-destructive">{{ errors.password }}</p>
      </div>

      <div class="-mt-1 text-right">
        <button
          type="button"
          class="text-sm font-medium text-blue-800 underline underline-offset-2 hover:text-blue-900"
          @click="showForgot = !showForgot"
        >
          Esqueceu sua senha?
        </button>
        <p
          v-if="showForgot"
          class="mt-2 rounded-md bg-neutral-100 p-3 text-left text-sm text-neutral-700"
          role="status"
        >
          Peça ao administrador um link para redefinir a senha. Ele vale por 1 hora e só pode ser
          usado uma vez. Ao abrir o link, você escolhe a senha nova.
        </p>
      </div>

      <Button
        type="submit"
        size="lg"
        class="mt-2 h-12 rounded-md text-base disabled:bg-neutral-200 disabled:text-neutral-500 disabled:opacity-100"
        :disabled="!canSubmit"
      >
        Entrar
      </Button>
    </form>
  </AuthLayout>
</template>
