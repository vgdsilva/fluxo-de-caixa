<script setup lang="ts">
import type { AuditEntry, AuditQuery } from '@fluxo/contracts';
import { ChevronDown, ChevronRight, Search } from '@lucide/vue';
import { computed, reactive, ref } from 'vue';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
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

/** Admin > Auditoria: quem fez o quê, quando e por qual canal, com antes e depois. */
const ACTIONS: Record<string, string> = {
  create: 'Criou',
  update: 'Alterou',
  delete: 'Removeu',
  restore: 'Restaurou',
  login: 'Entrou',
  login_failed: 'Login falhou',
  logout: 'Saiu',
  account_locked: 'Conta bloqueada',
  password_reset: 'Redefiniu a senha',
  password_changed: 'Trocou a senha',
  session_revoked: 'Encerrou uma sessão',
  other_sessions_revoked: 'Saiu dos outros aparelhos',
  setup_completed: 'Setup concluído',
};
const ENTITY_TYPES = ['User', 'Invite', 'PasswordReset'];
const CHANNELS: Record<string, string> = {
  ui: 'tela',
  chat: 'chat',
  import: 'importação',
  job: 'sistema',
  auth: 'login',
};

const users = useResource(() => adminService.users());
const userName = computed(() => {
  const map = new Map<string, string>();
  for (const u of users.data.value ?? []) map.set(u.id, u.username);
  return (id: string | null) => (id ? (map.get(id) ?? id.slice(0, 8)) : 'sistema');
});

const filters = reactive({ action: '', entityType: '', userId: '', from: '', to: '', limit: 100 });
const entries = ref<AuditEntry[]>([]);
const search = useAction();
const expanded = ref<Set<string>>(new Set());

async function load() {
  const query: Partial<AuditQuery> = {
    action: filters.action || undefined,
    entityType: filters.entityType || undefined,
    userId: filters.userId || undefined,
    // Datas do filtro são dias no fuso de São Paulo (UTC-3).
    from: filters.from ? new Date(`${filters.from}T00:00:00-03:00`).toISOString() : undefined,
    to: filters.to ? new Date(`${filters.to}T23:59:59-03:00`).toISOString() : undefined,
    limit: filters.limit,
  };
  const result = await search.run(() => adminService.audit(query));
  if (result) entries.value = result;
}
void load();

function toggle(id: string) {
  const next = new Set(expanded.value);
  if (next.has(id)) next.delete(id);
  else next.add(id);
  expanded.value = next;
}

function details(e: AuditEntry): string {
  return JSON.stringify(e.changes ?? e.metadata ?? {}, null, 2);
}

const selectClass =
  'h-10 rounded-lg border border-input bg-transparent px-2.5 text-sm outline-none focus-visible:ring-3 focus-visible:ring-ring/50';
</script>

<template>
  <h1 class="mb-6 text-2xl font-semibold">Auditoria</h1>

  <form
    class="mb-6 grid gap-3 rounded-lg border p-4 sm:grid-cols-2 lg:grid-cols-6"
    @submit.prevent="load"
  >
    <div class="flex flex-col gap-1.5">
      <Label for="f-action">Ação</Label>
      <select id="f-action" v-model="filters.action" :class="selectClass">
        <option value="">Todas</option>
        <option v-for="(label, key) in ACTIONS" :key="key" :value="key">{{ label }}</option>
      </select>
    </div>
    <div class="flex flex-col gap-1.5">
      <Label for="f-type">Registro</Label>
      <select id="f-type" v-model="filters.entityType" :class="selectClass">
        <option value="">Todos</option>
        <option v-for="t in ENTITY_TYPES" :key="t" :value="t">{{ t }}</option>
      </select>
    </div>
    <div class="flex flex-col gap-1.5">
      <Label for="f-user">Quem</Label>
      <select id="f-user" v-model="filters.userId" :class="selectClass">
        <option value="">Todos</option>
        <option v-for="u in users.data.value ?? []" :key="u.id" :value="u.id">
          {{ u.username }}
        </option>
      </select>
    </div>
    <div class="flex flex-col gap-1.5">
      <Label for="f-from">De</Label>
      <Input id="f-from" v-model="filters.from" type="date" class="h-10" />
    </div>
    <div class="flex flex-col gap-1.5">
      <Label for="f-to">Até</Label>
      <Input id="f-to" v-model="filters.to" type="date" class="h-10" />
    </div>
    <div class="flex items-end">
      <Button type="submit" class="h-10 w-full" :disabled="search.busy.value">
        <Search />
        Filtrar
      </Button>
    </div>
  </form>

  <p v-if="search.error.value" class="mb-4 text-sm text-destructive">{{ search.error.value }}</p>

  <div class="overflow-x-auto rounded-lg border">
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead class="w-8" />
          <TableHead>Quando</TableHead>
          <TableHead>Quem</TableHead>
          <TableHead>Ação</TableHead>
          <TableHead class="hidden sm:table-cell">Registro</TableHead>
          <TableHead class="hidden md:table-cell">Canal</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <template v-for="e in entries" :key="e.id">
          <TableRow class="cursor-pointer" @click="toggle(e.id)">
            <TableCell>
              <ChevronDown v-if="expanded.has(e.id)" class="size-4" />
              <ChevronRight v-else class="size-4" />
            </TableCell>
            <TableCell class="whitespace-nowrap">{{ formatDateTime(e.at) }}</TableCell>
            <TableCell>{{ userName(e.userId) }}</TableCell>
            <TableCell>
              <Badge
                :variant="
                  ['login_failed', 'account_locked', 'delete'].includes(e.action)
                    ? 'destructive'
                    : 'outline'
                "
              >
                {{ ACTIONS[e.action] ?? e.action }}
              </Badge>
            </TableCell>
            <TableCell class="hidden sm:table-cell">
              <span v-if="e.entityType">{{ e.entityType }} · {{ e.entityId?.slice(0, 8) }}</span>
              <span v-else class="text-muted-foreground">—</span>
            </TableCell>
            <TableCell class="hidden md:table-cell">{{
              CHANNELS[e.channel] ?? e.channel
            }}</TableCell>
          </TableRow>
          <TableRow v-if="expanded.has(e.id)" class="bg-muted/40 hover:bg-muted/40">
            <TableCell />
            <TableCell colspan="5">
              <pre class="overflow-x-auto text-xs whitespace-pre-wrap">{{ details(e) }}</pre>
              <p class="mt-1 text-xs text-muted-foreground">CorrelationId: {{ e.correlationId }}</p>
            </TableCell>
          </TableRow>
        </template>
        <TableRow v-if="!search.busy.value && entries.length === 0">
          <TableCell colspan="6" class="text-center text-muted-foreground">
            Nada encontrado com esses filtros.
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
