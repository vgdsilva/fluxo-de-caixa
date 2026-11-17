<script setup lang="ts">
import { acceptInviteSchema, PASSWORD_MIN_LENGTH } from '@fluxo/contracts';
import { UserPlus } from '@lucide/vue';
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import MessageFeed from '@/components/common/MessageFeed.vue';
import PasswordInput from '@/components/common/PasswordInput.vue';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ApiError } from '@/services/api';
import { invitesService } from '@/services/invites.service';
import { useAuthStore } from '@/store/auth.store';

/** Cadastro por convite (/convite?token=...). Depois de criar a conta, entra pelo feed de sessão. */
const route = useRoute();
const router = useRouter();
const auth = useAuthStore();

const token = String(route.query.token ?? '');
const state = ref<'checking' | 'form' | 'invalid'>('checking');
const lockedEmail = ref<string | null>(null);
const saving = ref(false);
const form = reactive({ name: '', username: '', email: '', password: '', confirm: '' });
const errors = ref<Record<string, string>>({});
const serverError = ref<string | null>(null);

onMounted(async () => {
  try {
    const check = token ? await invitesService.check(token) : { valid: false, email: null };
    lockedEmail.value = check.email;
    if (check.email) form.email = check.email;
    state.value = check.valid ? 'form' : 'invalid';
  } catch {
    state.value = 'invalid';
  }
});

async function submit() {
  errors.value = {};
  serverError.value = null;
  const parsed = acceptInviteSchema.safeParse({ ...form, token });
  if (!parsed.success) {
    for (const issue of parsed.error.issues) errors.value[String(issue.path[0])] ??= issue.message;
  }
  if (form.password !== form.confirm) errors.value.confirm = 'As senhas não conferem';
  if (!parsed.success || errors.value.confirm) return;

  saving.value = true;
  try {
    await invitesService.accept(parsed.data);
    auth.startLogin({ username: parsed.data.username, password: parsed.data.password });
    await router.replace({ name: 'login-session' });
  } catch (e) {
    serverError.value = e instanceof ApiError ? e.message : 'Não foi possível criar a conta.';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <AuthLayout>
    <h1 class="mb-2 text-center text-2xl font-semibold text-neutral-900">Você foi convidado(a)!</h1>
    <p class="mb-8 text-center text-sm text-neutral-600">Crie sua conta para começar.</p>

    <MessageFeed
      v-if="state === 'checking'"
      :items="[{ id: 'check', label: 'Conferindo o convite', status: 'running' }]"
    />

    <template v-else-if="state === 'invalid'">
      <MessageFeed
        tone="error"
        :items="[
          {
            id: 'invalid',
            label: 'Convite inválido, expirado ou já usado',
            status: 'error',
            detail: 'Peça um convite novo a quem te convidou. O link vale por 72 horas.',
          },
        ]"
      />
      <Button variant="outline" class="mt-6 h-11 w-full" @click="router.replace({ name: 'login' })">
        Ir para o login
      </Button>
    </template>

    <form v-else class="flex flex-col gap-4" novalidate @submit.prevent="submit">
      <div class="flex flex-col gap-2">
        <Label for="name" class="font-normal text-neutral-700">
          Nome<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <Input
          id="name"
          v-model="form.name"
          autocomplete="name"
          placeholder="Seu nome"
          class="h-11"
        />
        <p v-if="errors.name" class="text-xs text-destructive">{{ errors.name }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="username" class="font-normal text-neutral-700">
          Usuário<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <Input
          id="username"
          v-model="form.username"
          autocomplete="username"
          autocapitalize="none"
          placeholder="Como você vai entrar (ex.: maria.silva)"
          class="h-11"
        />
        <p v-if="errors.username" class="text-xs text-destructive">{{ errors.username }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="email" class="font-normal text-neutral-700">
          E-mail<span class="text-red-500" aria-hidden="true">*</span>
        </Label>
        <Input
          id="email"
          v-model="form.email"
          type="email"
          autocomplete="email"
          :readonly="!!lockedEmail"
          placeholder="seu@email.com"
          class="h-11"
        />
        <p v-if="lockedEmail" class="text-xs text-neutral-500">Este convite é para este e-mail.</p>
        <p v-if="errors.email" class="text-xs text-destructive">{{ errors.email }}</p>
      </div>
      <div class="flex flex-col gap-2">
        <Label for="password" class="font-normal text-neutral-700">
          Senha<span class="text-red-500" aria-hidden="true">*</span>
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
          Confirme a senha<span class="text-red-500" aria-hidden="true">*</span>
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
        <UserPlus />
        Criar minha conta
      </Button>
    </form>
  </AuthLayout>
</template>
