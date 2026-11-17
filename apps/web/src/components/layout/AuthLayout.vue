<script setup lang="ts">
import { CalendarClock, CreditCard, PiggyBank } from '@lucide/vue';
</script>

<template>
  <!-- Tela dividida ao meio: marca à esquerda (some no celular), conteúdo à direita. -->
  <div class="grid min-h-dvh md:grid-cols-2">
    <aside
      class="relative hidden flex-col justify-between overflow-hidden bg-primary p-10 text-primary-foreground md:flex"
    >
      <div
        class="pointer-events-none absolute -right-24 -top-24 size-96 rounded-full bg-emerald-500/20 blur-3xl"
      />
      <div
        class="pointer-events-none absolute -bottom-32 -left-16 size-96 rounded-full bg-sky-500/10 blur-3xl"
      />

      <div class="relative flex items-center gap-2 text-lg font-semibold">
        <img src="/favicon.ico" alt="" class="size-7 rounded-md" />
        Fluxo de Caixa
      </div>

      <div class="relative max-w-md">
        <h2 class="text-3xl leading-tight font-semibold">Quanto vai sobrar nos próximos meses?</h2>
        <ul class="mt-8 flex flex-col gap-4 text-sm text-primary-foreground/80">
          <li class="flex items-center gap-3">
            <CreditCard class="size-5" /> Cartões com fatura e parcelas
          </li>
          <li class="flex items-center gap-3">
            <CalendarClock class="size-5" /> Recorrências e contas fixas projetadas
          </li>
          <li class="flex items-center gap-3">
            <PiggyBank class="size-5" /> "Cabe esta compra?" antes de comprar
          </li>
        </ul>
      </div>

      <p class="relative text-xs text-primary-foreground/60">Acesso restrito a convidados.</p>
    </aside>

    <main class="flex items-center justify-center px-4 py-10 sm:px-8">
      <div class="w-full max-w-sm">
        <div class="mb-8 flex items-center gap-2 font-semibold md:hidden">
          <img src="/favicon.ico" alt="" class="size-6 rounded-md" />
          Fluxo de Caixa
        </div>
        <slot />
      </div>
    </main>
  </div>
</template>
