<script setup lang="ts">
import ApiStatus from '@/features/health/ApiStatus.vue';
</script>

<template>
  <h1 class="mb-2 text-2xl font-semibold">Fluxo de Caixa</h1>
  <p class="mb-6 text-muted-foreground">
    Fase 0: fundação. Ainda não há funcionalidades financeiras.
  </p>
  <ApiStatus />
</template>
