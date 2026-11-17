<script setup lang="ts">
import type { AdminUser, CreatedPasswordReset } from '@fluxo/contracts';
import { KeyRound, UserCheck, UserX } from '@lucide/vue';
import { ref } from 'vue';
import CopyLinkBox from '@/components/common/CopyLinkBox.vue';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { useAction, useResource } from '@/hooks/useResource';
import { adminService } from '@/services/admin.service';
import { useAuthStore } from '@/store/auth.store';
import { formatDateTime } from '@/utils/format';

/**
 * Admin > Usuários: lista, desativar/reativar (desativar encerra as sessões) e gerar link de
 * redefinição de senha. O admin não vê dados financeiros de ninguém aqui.
 */
const auth = useAuthStore();
const users = useResource(() => adminService.users());
const action = useAction();
const resetLink = ref<{ user: AdminUser; reset: CreatedPasswordReset } | null>(null);

async function toggle(user: AdminUser) {
  const verb = user.active ? 'desativar' : 'reativar';
  if (!window.confirm(`Quer ${verb} ${user.name} (${user.username})?`)) return;
  await action.run(() => adminService.setUserActive(user.id, !user.active));
  if (!action.error.value) await users.reload();
}

async function makeResetLink(user: AdminUser) {
  const reset = await action.run(() => adminService.passwordResetLink(user.id));
  if (reset) resetLink.value = { user, reset };
}
</script>

<template>
  <h1 class="mb-1 text-2xl font-semibold">Usuários</h1>
  <p class="mb-6 text-sm text-muted-foreground">
    Para adicionar alguém, gere um convite na tela de Convites.
  </p>

  <p v-if="users.error.value || action.error.value" class="mb-4 text-sm text-destructive">
    {{ users.error.value ?? action.error.value }}
  </p>

  <div v-if="resetLink" class="mb-6">
    <p class="mb-2 text-sm">
      Link de redefinição de senha para <strong>{{ resetLink.user.name }}</strong>:
    </p>
    <CopyLinkBox
      :link="resetLink.reset.link"
      :expires-label="`Vale até ${formatDateTime(resetLink.reset.expiresAt)}, uma vez só.`"
      whatsapp-text="Use este link para criar uma senha nova no Fluxo de Caixa (vale 1 hora):"
    />
  </div>

  <div class="overflow-x-auto rounded-lg border">
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Nome</TableHead>
          <TableHead>Usuário</TableHead>
          <TableHead class="hidden md:table-cell">E-mail</TableHead>
          <TableHead>Situação</TableHead>
          <TableHead class="hidden md:table-cell">Último acesso</TableHead>
          <TableHead class="text-right">Ações</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="u in users.data.value ?? []" :key="u.id">
          <TableCell class="font-medium">
            {{ u.name }}
            <Badge v-if="u.role === 'admin'" variant="secondary" class="ml-1">admin</Badge>
          </TableCell>
          <TableCell>{{ u.username }}</TableCell>
          <TableCell class="hidden md:table-cell">{{ u.email ?? '—' }}</TableCell>
          <TableCell>
            <Badge :variant="u.active ? 'outline' : 'destructive'">
              {{ u.active ? 'Ativo' : 'Desativado' }}
            </Badge>
          </TableCell>
          <TableCell class="hidden md:table-cell">{{ formatDateTime(u.lastLoginAt) }}</TableCell>
          <TableCell class="text-right">
            <div class="flex justify-end gap-2">
              <Button
                size="sm"
                variant="outline"
                :disabled="action.busy.value || !u.active"
                title="Gerar link de redefinição de senha"
                @click="makeResetLink(u)"
              >
                <KeyRound />
                <span class="hidden sm:inline">Redefinir senha</span>
              </Button>
              <Button
                v-if="u.id !== auth.user?.id"
                size="sm"
                :variant="u.active ? 'destructive' : 'outline'"
                :disabled="action.busy.value"
                @click="toggle(u)"
              >
                <UserX v-if="u.active" />
                <UserCheck v-else />
                <span class="hidden sm:inline">{{ u.active ? 'Desativar' : 'Reativar' }}</span>
              </Button>
            </div>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
