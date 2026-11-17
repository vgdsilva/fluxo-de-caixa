<script setup lang="ts">
import { CircleCheck } from '@lucide/vue';
import MessageFeed from '@/components/common/MessageFeed.vue';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { useLoginSession } from '@/hooks/useLoginSession';

// O formulário some e o login acontece aqui, passo a passo, no feed.
const { items, tone, welcome } = useLoginSession();
</script>

<template>
  <AuthLayout>
    <h1 class="text-2xl font-semibold">
      {{ tone === 'success' ? 'Sessão iniciada' : 'Entrando…' }}
    </h1>
    <p class="mt-1 mb-8 text-sm text-muted-foreground">
      {{ tone === 'success' ? 'Abrindo o app.' : 'Só um instante enquanto conferimos tudo.' }}
    </p>

    <MessageFeed :items="items" :tone="tone" />

    <p
      v-if="welcome"
      class="mt-5 flex items-center gap-2 text-sm font-medium text-emerald-700 dark:text-emerald-400"
      data-testid="login-success"
    >
      <CircleCheck class="size-4" />
      {{ welcome }}
    </p>
  </AuthLayout>
</template>
