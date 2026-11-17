<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';

/**
 * Painel ilustrativo da tela de login: um "dashboard" desenhado em HTML/CSS (sem foto) com
 * legendas que trocam sozinhas, como um carrossel. Só decorativo.
 */
const slides = [
  { title: 'Veja agora quanto vai sobrar', subtitle: 'nos próximos 12 meses.' },
  { title: 'Cabe esta compra?', subtitle: 'Simule antes de parcelar.' },
];
const current = ref(0);
let timer: ReturnType<typeof setInterval> | undefined;

onMounted(() => {
  timer = setInterval(() => (current.value = (current.value + 1) % slides.length), 6000);
});
onBeforeUnmount(() => clearInterval(timer));

// Barras do gráfico de "sobra por mês" (altura em %); a última é a projeção.
const bars = [38, 52, 44, 63, 58, 71, 66, 80, 74, 88];
</script>

<template>
  <div
    class="relative overflow-hidden rounded-[2rem] bg-white/95 p-5 shadow-2xl ring-1 ring-white/10"
  >
    <!-- Cabeçalho do mini dashboard -->
    <div class="mb-4 flex items-center justify-between">
      <div class="flex items-center gap-2">
        <span class="size-2.5 rounded-full bg-emerald-500" />
        <span class="text-xs font-semibold text-neutral-700">Projeção do caixa</span>
      </div>
      <span class="rounded-full bg-emerald-50 px-2 py-0.5 text-[10px] font-medium text-emerald-700">
        próximos 12 meses
      </span>
    </div>

    <div class="grid grid-cols-3 gap-3">
      <div class="rounded-xl bg-neutral-50 p-3">
        <p class="text-[10px] text-neutral-500">Sobra prevista</p>
        <p class="mt-1 text-sm font-semibold text-neutral-800">R$ 1.063,16</p>
        <p class="text-[10px] font-medium text-emerald-600">+ 8,4%</p>
      </div>
      <div class="rounded-xl bg-neutral-50 p-3">
        <p class="text-[10px] text-neutral-500">Faturas abertas</p>
        <p class="mt-1 text-sm font-semibold text-neutral-800">R$ 4.300,00</p>
        <p class="text-[10px] font-medium text-amber-600">2 cartões</p>
      </div>
      <div class="rounded-xl bg-neutral-50 p-3">
        <p class="text-[10px] text-neutral-500">Reserva</p>
        <p class="mt-1 text-sm font-semibold text-neutral-800">62%</p>
        <div class="mt-1.5 h-1.5 rounded-full bg-neutral-200">
          <div class="h-1.5 w-[62%] rounded-full bg-cyan-500" />
        </div>
      </div>
    </div>

    <div class="mt-4 flex h-28 items-end gap-2 rounded-xl bg-neutral-50 px-3 pt-3 pb-2">
      <div
        v-for="(h, i) in bars"
        :key="i"
        class="flex-1 rounded-t-md"
        :class="i === bars.length - 1 ? 'bg-emerald-300' : 'bg-emerald-600'"
        :style="{ height: `${h}%` }"
      />
    </div>

    <!-- Legenda em "pílulas", trocando sozinha -->
    <div class="relative mt-5 min-h-[4.5rem]">
      <Transition
        enter-active-class="transition duration-500"
        enter-from-class="opacity-0 translate-y-2"
        leave-active-class="absolute inset-x-0 top-0 transition duration-300"
        leave-to-class="opacity-0"
      >
        <div :key="current" class="flex flex-col items-start gap-1.5">
          <span
            class="rounded-xl bg-cyan-400 px-3 py-1 text-lg leading-tight font-bold text-neutral-900"
          >
            {{ slides[current]!.title }}
          </span>
          <span
            class="rounded-xl bg-cyan-400 px-3 py-1 text-lg leading-tight font-bold text-neutral-900"
          >
            {{ slides[current]!.subtitle }}
          </span>
        </div>
      </Transition>
    </div>

    <div class="mt-3 flex justify-center gap-1.5">
      <button
        v-for="(_, i) in slides"
        :key="i"
        type="button"
        :aria-label="`Mostrar mensagem ${i + 1}`"
        class="size-2 rounded-full transition-colors"
        :class="i === current ? 'bg-neutral-800' : 'bg-neutral-300'"
        @click="current = i"
      />
    </div>
  </div>
</template>
