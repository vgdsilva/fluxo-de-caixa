<script setup lang="ts">
import { loginSchema } from '@fluxo/contracts';
import { LogIn } from '@lucide/vue';
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useAuthStore } from '@/store/auth.store';

const router = useRouter();
const auth = useAuthStore();

const form = reactive({ username: auth.lastUsername, password: '' });
const errors = ref<Partial<Record<'username' | 'password', string>>>({});

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
    <h1 class="text-2xl font-semibold">Entrar</h1>
    <p class="mt-1 mb-8 text-sm text-muted-foreground">Use o usuário e a senha do seu convite.</p>

    <form class="flex flex-col gap-5" novalidate @submit.prevent="submit">
      <div class="flex flex-col gap-2">
        <Label for="username">Usuário</Label>
        <Input
          id="username"
          v-model="form.username"
          autocomplete="username"
          autocapitalize="none"
          autofocus
          :aria-invalid="!!errors.username"
          class="h-10"
        />
        <p v-if="errors.username" class="text-xs text-destructive">{{ errors.username }}</p>
      </div>

      <div class="flex flex-col gap-2">
        <Label for="password">Senha</Label>
        <Input
          id="password"
          v-model="form.password"
          type="password"
          autocomplete="current-password"
          :aria-invalid="!!errors.password"
          class="h-10"
        />
        <p v-if="errors.password" class="text-xs text-destructive">{{ errors.password }}</p>
      </div>

      <Button type="submit" size="lg" class="mt-2 h-10">
        <LogIn />
        Acessar
      </Button>
    </form>
  </AuthLayout>
</template>
