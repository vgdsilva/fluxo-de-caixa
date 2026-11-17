<script setup lang="ts">
/** Marca do app: ícone de barras + "Fluxo de Caixa". `tone` escuro para fundo claro. */
defineProps<{ tone?: 'dark' | 'light'; size?: 'md' | 'lg' }>();
</script>

<template>
  <span class="inline-flex items-center gap-2.5">
    <span
      class="flex items-end gap-[3px] rounded-lg bg-neutral-900 p-1.5"
      :class="size === 'lg' ? 'h-10 w-10' : 'h-8 w-8'"
      aria-hidden="true"
    >
      <span class="h-[40%] flex-1 rounded-sm bg-neutral-400" />
      <span class="h-[65%] flex-1 rounded-sm bg-neutral-200" />
      <span class="h-full flex-1 rounded-sm bg-emerald-500" />
    </span>
    <span
      class="leading-none font-bold tracking-tight"
      :class="[
        size === 'lg' ? 'text-3xl' : 'text-xl',
        tone === 'light' ? 'text-white' : 'text-neutral-900',
      ]"
    >
      Fluxo<span class="font-light"> de Caixa</span>
    </span>
  </span>
</template>
