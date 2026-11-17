<script setup lang="ts">
import { PASSWORD_MIN_LENGTH, resetPasswordSchema } from '@fluxo/contracts';
import { KeyRound } from '@lucide/vue';
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MessageFeed from '@/components/common/MessageFeed.vue';
import PasswordInput from '@/components/common/PasswordInput.vue';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { ApiError } from '@/services/api';
import { passwordResetService } from '@/services/password-reset.service';
import { useAuthStore } from '@/store/auth.store';

/** Redefinir senha pelo link que o admin gerou (/redefinir-senha?token=...). Vale 1 hora. */
const route = useRoute();
const router = useRouter();
const auth = useAuthStore();

const token = String(route.query.token ?? '');
const state = ref<'checking' | 'form' | 'invalid' | 'done'>('checking');
const username = ref<string | null>(null);
const saving = ref(false);
const form = reactive({ password: '', confirm: '' });
const errors = ref<Record<string, string>>({});
const serverError = ref<string | null>(null);

onMounted(async () => {
  try {
    const check = token
      ? await passwordResetService.check(token)
      : { valid: false, username: null };
    username.value = check.username;
    state.value = check.valid ? 'form' : 'invalid';
  } catch {
    state.value = 'invalid';
  }
});

async function submit() {
  errors.value = {};
  serverError.value = null;
  const parsed = resetPasswordSchema.safeParse({ token, password: form.password });
  if (!parsed.success) {
    for (const issue of parsed.error.issues) errors.value[String(issue.path[0])] ??= issue.message;
  }
  if (form.password !== form.confirm) errors.value.confirm = 'As senhas não conferem';
  if (!parsed.success || errors.value.confirm) return;

  saving.value = true;
  try {
    await passwordResetService.reset(parsed.data);
    state.value = 'done';
  } catch (e) {
    serverError.value = e instanceof ApiError ? e.message : 'Não foi possível trocar a senha.';
  } finally {
    saving.value = false;
  }
}

function goToLogin() {
  auth.lastUsername = username.value ?? '';
  void router.replace({ name: 'login' });
}
</script>

<template>
  <AuthLayout>
    <h1 class="mb-2 text-center text-2xl font-semibold text-neutral-900">Redefinir senha</h1>
    <p v-if="username" class="mb-8 text-center text-sm text-neutral-600">
      Usuário: <strong>{{ username }}</strong>
    </p>

    <MessageFeed
      v-if="state === 'checking'"
      :items="[{ id: 'check', label: 'Conferindo o link', status: 'running' }]"
    />

    <template v-else-if="state === 'invalid' || state === 'done'">
      <MessageFeed
        :tone="state === 'done' ? 'success' : 'error'"
        :items="[
          state === 'done'
            ? {
                id: 'done',
                label: 'Senha alterada',
                status: 'done',
                detail: 'Por segurança, todas as suas sessões foram encerradas. Entre de novo.',
              }
            : {
                id: 'invalid',
                label: 'Link inválido, expirado ou já usado',
                status: 'error',
                detail: 'Peça um link novo ao administrador. Ele vale por 1 hora.',
              },
        ]"
      />
      <Button class="mt-6 h-11 w-full" @click="goToLogin">Ir para o login</Button>
    </template>

    <form v-else class="flex flex-col gap-4" novalidate @submit.prevent="submit">
      <div class="flex flex-col gap-2">
        <Label for="password" class="font-normal text-neutral-700">
          Nova senha<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <PasswordInput
          id="password"
          v-model="form.password"
          autocomplete="new-password"
          :placeholder="`Mínimo de ${PASSWORD_MIN_LENGTH} caracteres`"
          class="h-11"
        />
        <p v-if="errors.password" class="text-xs text-destructive">{{ errors.password }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="confirm" class="font-normal text-neutral-700">
          Confirme a nova senha<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <PasswordInput
          id="confirm"
          v-model="form.confirm"
          autocomplete="new-password"
          class="h-11"
        />
        <p v-if="errors.confirm" class="text-xs text-destructive">{{ errors.confirm }}</p>
      </div>
      <p v-if="serverError" class="text-sm text-destructive" role="alert">{{ serverError }}</p>
      <Button type="submit" size="lg" class="mt-2 h-12 text-base" :disabled="saving">
        <KeyRound />
        Salvar nova senha
      </Button>
    </form>
  </AuthLayout>
</template>
