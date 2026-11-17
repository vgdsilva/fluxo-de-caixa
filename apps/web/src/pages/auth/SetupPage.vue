<script setup lang="ts">
import { PASSWORD_MIN_LENGTH, setupSchema } from '@fluxo/contracts';
import { ShieldCheck } from '@lucide/vue';
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ApiError } from '@/services/api';
import { setupService } from '@/services/setup.service';
import { useAuthStore } from '@/store/auth.store';

/**
 * Primeiro acesso: o banco ainda está com SETUP_DB = 0 (a guarda de rotas manda para cá).
 * Cria o admin; a API marca SETUP_DB = 1 e o login segue pela tela de sessão.
 */
const router = useRouter();
const auth = useAuthStore();

const saving = ref(false);
const form = reactive({ username: '', name: '', password: '', confirm: '' });
const errors = ref<Record<string, string>>({});
const serverError = ref<string | null>(null);

async function submit() {
  errors.value = {};
  serverError.value = null;
  const parsed = setupSchema.safeParse(form);
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      errors.value[String(issue.path[0])] ??= issue.message;
    }
  }
  if (form.password !== form.confirm) errors.value.confirm = 'As senhas não conferem';
  if (!parsed.success || errors.value.confirm) return;

  saving.value = true;
  try {
    await setupService.run(parsed.data);
    auth.markSetupDone();
    auth.startLogin({ username: parsed.data.username, password: parsed.data.password });
    await router.replace({ name: 'login-session' });
  } catch (e) {
    if (e instanceof ApiError && e.status === 404) {
      // Outra pessoa concluiu o setup antes: segue para o login normal.
      auth.markSetupDone();
      await router.replace({ name: 'login' });
      return;
    }
    serverError.value = e instanceof ApiError ? e.message : 'Não foi possível criar o admin.';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <AuthLayout>
    <h1 class="text-2xl font-semibold">Primeiro acesso</h1>
    <p class="mt-1 mb-8 text-sm text-muted-foreground">
      O sistema ainda não tem administrador. Crie a sua conta de admin para começar.
    </p>

    <form class="flex flex-col gap-4" novalidate @submit.prevent="submit">
      <div class="flex flex-col gap-2">
        <Label for="name">Nome</Label>
        <Input id="name" v-model="form.name" autocomplete="name" autofocus class="h-10" />
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
      <Button type="submit" size="lg" class="mt-2 h-10" :disabled="saving">
        <ShieldCheck />
        Criar administrador
      </Button>
    </form>
  </AuthLayout>
</template>
