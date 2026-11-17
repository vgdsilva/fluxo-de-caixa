<script setup lang="ts">
import { type CreatedInvite, createInviteSchema, type Invite } from '@fluxo/contracts';
import { Ban, Send } from '@lucide/vue';
import { ref } from 'vue';
import CopyLinkBox from '@/components/common/CopyLinkBox.vue';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
import { formatDateTime } from '@/utils/format';

/** Admin > Convites: criar (o link aparece uma vez), acompanhar e revogar. */
const invites = useResource(() => adminService.invites());
const create = useAction();
const revokeAction = useAction();
const email = ref('');
const created = ref<CreatedInvite | null>(null);

const statusLabel: Record<
  Invite['status'],
  { text: string; variant: 'outline' | 'secondary' | 'destructive' }
> = {
  pending: { text: 'Aguardando', variant: 'outline' },
  used: { text: 'Usado', variant: 'secondary' },
  revoked: { text: 'Revogado', variant: 'destructive' },
  expired: { text: 'Expirado', variant: 'secondary' },
};

async function submit() {
  created.value = null;
  const parsed = createInviteSchema.safeParse({ email: email.value.trim() || undefined });
  if (!parsed.success) {
    create.error.value = parsed.error.issues[0]?.message ?? 'E-mail inválido.';
    return;
  }
  const result = await create.run(() => adminService.createInvite(parsed.data));
  if (result) {
    created.value = result;
    email.value = '';
    await invites.reload();
  }
}

async function revoke(invite: Invite) {
  if (!window.confirm('Revogar este convite? O link deixa de funcionar.')) return;
  await revokeAction.run(() => adminService.revokeInvite(invite.id));
  if (!revokeAction.error.value) await invites.reload();
}
</script>

<template>
  <h1 class="mb-6 text-2xl font-semibold">Convites</h1>

  <Card class="mb-6">
    <CardHeader>
      <CardTitle>Novo convite</CardTitle>
      <CardDescription>
        O link vale por 72 horas e cria uma conta só. Com e-mail, só aquele e-mail pode usar.
      </CardDescription>
    </CardHeader>
    <CardContent class="flex flex-col gap-4">
      <form class="flex flex-col gap-3 sm:flex-row sm:items-end" @submit.prevent="submit">
        <div class="flex flex-1 flex-col gap-2">
          <Label for="invite-email">E-mail (opcional)</Label>
          <Input
            id="invite-email"
            v-model="email"
            type="email"
            placeholder="pessoa@email.com"
            class="h-10"
          />
        </div>
        <Button type="submit" class="h-10" :disabled="create.busy.value">
          <Send />
          Gerar convite
        </Button>
      </form>
      <p v-if="create.error.value" class="text-sm text-destructive">{{ create.error.value }}</p>
      <CopyLinkBox
        v-if="created"
        :link="created.link"
        :expires-label="`Vale até ${formatDateTime(created.expiresAt)}.`"
        whatsapp-text="Você foi convidado(a) para o Fluxo de Caixa. Crie sua conta por este link (vale 72 horas):"
      />
    </CardContent>
  </Card>

  <p v-if="invites.error.value || revokeAction.error.value" class="mb-4 text-sm text-destructive">
    {{ invites.error.value ?? revokeAction.error.value }}
  </p>

  <div class="overflow-x-auto rounded-lg border">
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Criado em</TableHead>
          <TableHead>E-mail</TableHead>
          <TableHead>Situação</TableHead>
          <TableHead class="hidden md:table-cell">Expira / usado em</TableHead>
          <TableHead class="text-right">Ações</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="i in invites.data.value ?? []" :key="i.id">
          <TableCell>{{ formatDateTime(i.createdAt) }}</TableCell>
          <TableCell>{{ i.email ?? 'qualquer e-mail' }}</TableCell>
          <TableCell>
            <Badge :variant="statusLabel[i.status].variant">{{ statusLabel[i.status].text }}</Badge>
          </TableCell>
          <TableCell class="hidden md:table-cell">
            {{ formatDateTime(i.usedAt ?? i.expiresAt) }}
          </TableCell>
          <TableCell class="text-right">
            <Button
              v-if="i.status === 'pending'"
              size="sm"
              variant="outline"
              :disabled="revokeAction.busy.value"
              @click="revoke(i)"
            >
              <Ban />
              Revogar
            </Button>
          </TableCell>
        </TableRow>
        <TableRow v-if="invites.data.value?.length === 0">
          <TableCell colspan="5" class="text-center text-muted-foreground">
            Nenhum convite ainda.
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
