<script setup lang="ts">
import AuthShowcase from '@/components/common/AuthShowcase.vue';
import BrandMark from '@/components/common/BrandMark.vue';
import { useAppVersion } from '@/hooks/useAppVersion';

/**
 * Layout das telas de entrada (login, sessão, erro, setup): tela dividida ao meio.
 * Esquerda (só em telas largas): fundo verde escuro, selo da marca, chamada e painel ilustrativo.
 * Direita: marca, conteúdo da tela (slot) e rodapé com versão.
 */
const version = useAppVersion();
const year = new Date().getFullYear();
</script>

<template>
  <div class="grid min-h-dvh lg:grid-cols-2">
    <aside
      class="relative hidden overflow-hidden bg-[radial-gradient(ellipse_at_top_right,#0f5132_0%,#0b2a1e_45%,#07150f_100%)] px-10 pt-8 pb-10 lg:flex lg:flex-col"
    >
      <!-- Linhas de destaque verdes, saindo do selo -->
      <div class="absolute top-[3.25rem] right-0 left-64 h-1 rounded-l-full bg-emerald-400" />
      <div class="absolute top-28 bottom-[55%] left-12 w-1 rounded-b-full bg-emerald-400" />

      <div
        class="relative z-10 inline-flex w-fit items-center rounded-[1.75rem] border-4 border-emerald-400 bg-white px-6 py-3 shadow-lg"
      >
        <BrandMark size="lg" />
      </div>

      <h2
        class="relative z-10 mx-auto mt-10 max-w-2xl bg-gradient-to-r from-emerald-300 via-teal-300 to-cyan-300 bg-clip-text text-center text-4xl leading-tight font-bold text-transparent xl:text-5xl"
      >
        Casais como vocês estão evitando sustos no fim do mês com projeção de caixa.
      </h2>

      <div class="relative z-10 mx-auto mt-auto w-full max-w-2xl pt-10">
        <AuthShowcase />
      </div>
    </aside>

    <div class="flex min-h-dvh flex-col bg-white">
      <main class="flex flex-1 items-center justify-center px-6 py-10">
        <div class="w-full max-w-md">
          <div class="mb-8 flex justify-center">
            <BrandMark size="lg" />
          </div>
          <slot />
        </div>
      </main>

      <footer class="bg-neutral-100 px-6 py-4 text-center text-xs text-neutral-600">
        <p>
          Versão do Fluxo de Caixa: <strong class="text-neutral-800">{{ version ?? '—' }}</strong>
        </p>
        <p class="mt-1">Copyright © {{ year }} Fluxo de Caixa. Uso pessoal.</p>
      </footer>
    </div>
  </div>
</template>
