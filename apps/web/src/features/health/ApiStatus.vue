<script setup lang="ts">
import type { HealthResponse } from '@fluxo/contracts';
import Button from 'primevue/button';
import Message from 'primevue/message';
import { onMounted, ref } from 'vue';
import { fetchHealth } from './health.api';

const health = ref<HealthResponse | null>(null);
const error = ref<string | null>(null);
const loading = ref(false);

async function load() {
  loading.value = true;
  error.value = null;
  try {
    health.value = await fetchHealth();
  } catch (e) {
    health.value = null;
    error.value = e instanceof Error ? e.message : String(e);
  } finally {
    loading.value = false;
  }
}

onMounted(load);
</script>

<template>
  <section class="api-status">
    <Message v-if="health" severity="success" data-testid="api-ok">
      API no ar (versão {{ health.version }}, {{ new Date(health.time).toLocaleString('pt-BR') }})
    </Message>
    <Message v-else-if="error" severity="error" data-testid="api-error">
      Não foi possível falar com a API: {{ error }}
    </Message>
    <Button label="Verificar de novo" :loading="loading" size="small" @click="load" />
  </section>
</template>

<style scoped>
.api-status {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 12px;
}
</style>
