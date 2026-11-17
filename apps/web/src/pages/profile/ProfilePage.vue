<script setup lang="ts">
import { changePasswordSchema, PASSWORD_MIN_LENGTH, updateMeSchema } from '@fluxo/contracts';
import { KeyRound, LogOut, MonitorSmartphone, Save } from '@lucide/vue';
import { reactive, ref, watch } from 'vue';
import PasswordInput from '@/components/common/PasswordInput.vue';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useAction, useResource } from '@/hooks/useResource';
import { profileService } from '@/services/profile.service';
import { useAuthStore } from '@/store/auth.store';
import { describeDevice, formatDateTime } from '@/utils/format';

/** Meu perfil: nome, troca de senha e aparelhos com sessão aberta. */
const auth = useAuthStore();

// --- Dados ---
const me = useResource(() => profileService.me());
const name = ref('');
watch(
  () => me.data.value,
  (value) => (name.value = value?.name ?? ''),
);
const saveName = useAction();
const nameSaved = ref(false);

async function submitName() {
  nameSaved.value = false;
  const parsed = updateMeSchema.safeParse({ name: name.value, version: me.data.value?.version });
  if (!parsed.success) {
    saveName.error.value = parsed.error.issues[0]?.message ?? 'Nome inválido.';
    return;
  }
  const updated = await saveName.run(() => profileService.update(parsed.data));
  if (updated) {
    me.data.value = updated;
    auth.user = updated;
    nameSaved.value = true;
  }
}

// --- Senha ---
const pwd = reactive({ currentPassword: '', newPassword: '', confirm: '' });
const changePwd = useAction();
const pwdMessage = ref<string | null>(null);

async function submitPassword() {
  pwdMessage.value = null;
  const parsed = changePasswordSchema.safeParse(pwd);
  if (!parsed.success) {
    changePwd.error.value = parsed.error.issues[0]?.message ?? 'Dados inválidos.';
    return;
  }
  if (pwd.newPassword !== pwd.confirm) {
    changePwd.error.value = 'A confirmação não confere com a senha nova.';
    return;
  }
  const result = await changePwd.run(() => profileService.changePassword(parsed.data));
  if (result) {
    pwd.currentPassword = pwd.newPassword = pwd.confirm = '';
    pwdMessage.value =
      result.otherSessionsRevoked > 0
        ? `Senha alterada. ${result.otherSessionsRevoked} sessão(ões) em outros aparelhos foram encerradas.`
        : 'Senha alterada.';
    await sessions.reload();
  }
}

// --- Sessões ---
const sessions = useResource(() => profileService.sessions());
const sessionAction = useAction();

async function revoke(id: string) {
  await sessionAction.run(() => profileService.revokeSession(id));
  if (!sessionAction.error.value) await sessions.reload();
}

async function revokeOthers() {
  if (await sessionAction.run(() => profileService.revokeOtherSessions())) await sessions.reload();
}
</script>

<template>
  <h1 class="mb-6 text-2xl font-semibold">Meu perfil</h1>

  <div class="flex flex-col gap-6">
    <Card>
      <CardHeader>
        <CardTitle>Seus dados</CardTitle>
        <CardDescription>O usuário e o e-mail não mudam por aqui.</CardDescription>
      </CardHeader>
      <CardContent>
        <form class="grid gap-4 sm:grid-cols-2" @submit.prevent="submitName">
          <div class="flex flex-col gap-2 sm:col-span-2">
            <Label for="name">Nome</Label>
            <Input id="name" v-model="name" class="h-10" />
          </div>
          <div class="flex flex-col gap-2">
            <Label>Usuário</Label>
            <Input :model-value="me.data.value?.username ?? ''" disabled class="h-10" />
          </div>
          <div class="flex flex-col gap-2">
            <Label>E-mail</Label>
            <Input :model-value="me.data.value?.email ?? '—'" disabled class="h-10" />
          </div>
          <div class="flex items-center gap-3 sm:col-span-2">
            <Button type="submit" :disabled="saveName.busy.value || !me.data.value">
              <Save />
              Salvar nome
            </Button>
            <span v-if="nameSaved" class="text-sm text-emerald-700">Salvo.</span>
            <span v-if="saveName.error.value" class="text-sm text-destructive">
              {{ saveName.error.value }}
            </span>
          </div>
        </form>
      </CardContent>
    </Card>

    <Card>
      <CardHeader>
        <CardTitle>Trocar senha</CardTitle>
        <CardDescription>
          Mínimo de {{ PASSWORD_MIN_LENGTH }} caracteres. Os outros aparelhos saem da conta.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form class="grid gap-4 sm:grid-cols-3" @submit.prevent="submitPassword">
          <div class="flex flex-col gap-2">
            <Label for="current">Senha atual</Label>
            <PasswordInput
              id="current"
              v-model="pwd.currentPassword"
              autocomplete="current-password"
              class="h-10"
            />
          </div>
          <div class="flex flex-col gap-2">
            <Label for="new">Senha nova</Label>
            <PasswordInput
              id="new"
              v-model="pwd.newPassword"
              autocomplete="new-password"
              class="h-10"
            />
          </div>
          <div class="flex flex-col gap-2">
            <Label for="confirm">Confirme a nova</Label>
            <PasswordInput
              id="confirm"
              v-model="pwd.confirm"
              autocomplete="new-password"
              class="h-10"
            />
          </div>
          <div class="flex flex-wrap items-center gap-3 sm:col-span-3">
            <Button type="submit" :disabled="changePwd.busy.value">
              <KeyRound />
              Trocar senha
            </Button>
            <span v-if="pwdMessage" class="text-sm text-emerald-700">{{ pwdMessage }}</span>
            <span v-if="changePwd.error.value" class="text-sm text-destructive">
              {{ changePwd.error.value }}
            </span>
          </div>
        </form>
      </CardContent>
    </Card>

    <Card>
      <CardHeader>
        <CardTitle>Aparelhos conectados</CardTitle>
        <CardDescription
          >Sessões abertas na sua conta. Encerre as que não reconhecer.</CardDescription
        >
      </CardHeader>
      <CardContent class="flex flex-col gap-3">
        <p v-if="sessions.error.value" class="text-sm text-destructive">
          {{ sessions.error.value }}
        </p>
        <ul class="divide-y rounded-lg border">
          <li
            v-for="s in sessions.data.value ?? []"
            :key="s.id"
            class="flex flex-wrap items-center justify-between gap-3 p-3"
          >
            <div class="flex items-center gap-3">
              <MonitorSmartphone class="size-5 text-muted-foreground" />
              <div>
                <p class="text-sm font-medium">
                  {{ describeDevice(s.userAgent) }}
                  <Badge v-if="s.current" variant="secondary" class="ml-1">este aparelho</Badge>
                </p>
                <p class="text-xs text-muted-foreground">
                  Último uso {{ formatDateTime(s.lastSeenAt) }} · entrou
                  {{ formatDateTime(s.createdAt) }}<span v-if="s.ip"> · IP {{ s.ip }}</span>
                </p>
              </div>
            </div>
            <Button
              v-if="!s.current"
              size="sm"
              variant="outline"
              :disabled="sessionAction.busy.value"
              @click="revoke(s.id)"
            >
              Encerrar
            </Button>
          </li>
        </ul>
        <div class="flex flex-wrap items-center gap-3">
          <Button
            variant="outline"
            :disabled="sessionAction.busy.value || (sessions.data.value?.length ?? 0) <= 1"
            @click="revokeOthers"
          >
            <LogOut />
            Sair de todos os outros aparelhos
          </Button>
          <span v-if="sessionAction.error.value" class="text-sm text-destructive">
            {{ sessionAction.error.value }}
          </span>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
