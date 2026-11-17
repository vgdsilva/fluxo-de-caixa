<script setup lang="ts">
// Casca comum das telas (cabeçalho, largura máxima). O menu e o usuário logado entram na fatia 6.
</script>

<template>
  <div class="min-h-dvh">
    <header class="border-b">
      <div class="mx-auto flex h-14 max-w-3xl items-center px-4">
        <span class="font-semibold">Fluxo de Caixa</span>
      </div>
    </header>
    <main class="mx-auto max-w-3xl px-4 py-6">
      <slot />
    </main>
  </div>
</template>
