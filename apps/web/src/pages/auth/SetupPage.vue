<script setup lang="ts">
import { PASSWORD_MIN_LENGTH, setupSchema } from '@fluxo/contracts';
import { ShieldCheck } from '@lucide/vue';
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MessageFeed from '@/components/common/MessageFeed.vue';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ApiError } from '@/services/api';
import { authService } from '@/services/auth.service';
import { useAuthStore } from '@/store/auth.store';

/** Primeiro acesso: cria o admin pelo link de uso único que a API escreve no log. */
const route = useRoute();
const router = useRouter();
const auth = useAuthStore();

const token = String(route.query.token ?? '');
const state = ref<'checking' | 'form' | 'invalid' | 'saving'>('checking');
const form = reactive({ username: '', name: '', password: '', confirm: '' });
const errors = ref<Record<string, string>>({});
const serverError = ref<string | null>(null);

onMounted(async () => {
  try {
    state.value = token && (await authService.setupIsValid(token)) ? 'form' : 'invalid';
  } catch {
    state.value = 'invalid';
  }
});

async function submit() {
  errors.value = {};
  serverError.value = null;
  if (form.password !== form.confirm) {
    errors.value.confirm = 'As senhas não conferem';
    return;
  }
  const parsed = setupSchema.safeParse({ ...form, token });
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      errors.value[String(issue.path[0])] ??= issue.message;
    }
    return;
  }
  state.value = 'saving';
  try {
    await authService.setup(parsed.data);
    auth.startLogin({ username: parsed.data.username, password: parsed.data.password });
    await router.replace({ name: 'login-session' });
  } catch (e) {
    state.value = 'form';
    serverError.value = e instanceof ApiError ? e.message : 'Não foi possível criar o admin.';
  }
}
</script>

<template>
  <AuthLayout>
    <h1 class="text-2xl font-semibold">Primeiro acesso</h1>
    <p class="mt-1 mb-8 text-sm text-muted-foreground">Crie a conta de administrador.</p>

    <MessageFeed
      v-if="state === 'checking'"
      :items="[{ id: 'check', label: 'Conferindo o link', status: 'running' }]"
    />

    <MessageFeed
      v-else-if="state === 'invalid'"
      tone="error"
      :items="[
        {
          id: 'invalid',
          label: 'Link inválido ou já usado',
          status: 'error',
          detail: 'O admin já existe ou a API foi reiniciada (o link muda a cada início).',
        },
      ]"
    />

    <form v-else class="flex flex-col gap-4" novalidate @submit.prevent="submit">
      <div class="flex flex-col gap-2">
        <Label for="name">Nome</Label>
        <Input id="name" v-model="form.name" autocomplete="name" class="h-10" />
        <p v-if="errors.name" class="text-xs text-destructive">{{ errors.name }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="username">Usuário</Label>
        <Input
          id="username"
          v-model="form.username"
          autocomplete="username"
          autocapitalize="none"
          class="h-10"
        />
        <p v-if="errors.username" class="text-xs text-destructive">{{ errors.username }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="password">Senha (mínimo {{ PASSWORD_MIN_LENGTH }} caracteres)</Label>
        <Input
          id="password"
          v-model="form.password"
          type="password"
          autocomplete="new-password"
          class="h-10"
        />
        <p v-if="errors.password" class="text-xs text-destructive">{{ errors.password }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="confirm">Confirme a senha</Label>
        <Input
          id="confirm"
          v-model="form.confirm"
          type="password"
          autocomplete="new-password"
          class="h-10"
        />
        <p v-if="errors.confirm" class="text-xs text-destructive">{{ errors.confirm }}</p>
      </div>
      <p v-if="serverError" class="text-sm text-destructive">{{ serverError }}</p>
      <Button type="submit" size="lg" class="mt-2 h-10" :disabled="state === 'saving'">
        <ShieldCheck />
        Criar administrador
      </Button>
    </form>
  </AuthLayout>
</template>
