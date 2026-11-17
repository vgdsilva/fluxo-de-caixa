<script setup lang="ts">
import ApiStatus from '@/components/common/ApiStatus.vue';
import { useAuthStore } from '@/store/auth.store';

const auth = useAuthStore();
</script>

<template>
  <h1 class="mb-2 text-2xl font-semibold">Olá, {{ auth.user?.name.split(' ')[0] }}</h1>
  <p class="mb-6 text-muted-foreground">
    Fase 0: fundação. Ainda não há funcionalidades financeiras.
  </p>
  <ApiStatus />
</template>
