<script setup lang="ts">
import { Check, Circle, LoaderCircle, X } from '@lucide/vue';
import { computed } from 'vue';

export type FeedStatus = 'pending' | 'running' | 'done' | 'error';

export interface FeedItem {
  id: string;
  label: string;
  status: FeedStatus;
  /** Linha menor abaixo do texto (ex.: a mensagem de erro). */
  detail?: string;
}

/**
 * Feed de mensagens passo a passo (usado na tela de sessão do login). O tom do quadro acompanha
 * o resultado: neutro enquanto roda, verde no sucesso, vermelho no erro.
 */
const props = defineProps<{
  items: FeedItem[];
  tone?: 'neutral' | 'success' | 'error';
}>();

const toneClass = computed(() => {
  switch (props.tone) {
    case 'success':
      return 'border-emerald-300 bg-emerald-50 dark:border-emerald-800 dark:bg-emerald-950/40';
    case 'error':
      return 'border-red-300 bg-red-50 dark:border-red-900 dark:bg-red-950/40';
    default:
      return 'border-border bg-card';
  }
});
</script>

<template>
  <ol
    :class="['flex flex-col gap-3 rounded-xl border p-4 transition-colors', toneClass]"
    aria-live="polite"
  >
    <li
      v-for="item in props.items"
      :key="item.id"
      class="flex items-start gap-3 text-sm"
      :data-status="item.status"
    >
      <span class="mt-0.5 flex size-5 shrink-0 items-center justify-center">
        <LoaderCircle v-if="item.status === 'running'" class="size-4 animate-spin text-primary" />
        <Check v-else-if="item.status === 'done'" class="size-4 text-emerald-600" />
        <X v-else-if="item.status === 'error'" class="size-4 text-red-600" />
        <Circle v-else class="size-3 text-muted-foreground/50" />
      </span>
      <span class="flex flex-col">
        <span
          :class="{
            'text-muted-foreground': item.status === 'pending',
            'font-medium': item.status === 'running',
            'text-red-700 dark:text-red-400': item.status === 'error',
          }"
        >
          {{ item.label }}
        </span>
        <span v-if="item.detail" class="text-xs text-muted-foreground">{{ item.detail }}</span>
      </span>
    </li>
  </ol>
</template>
