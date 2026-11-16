<script setup lang="ts">
import type { HealthResponse } from '@fluxo/contracts';
import { CircleAlert, CircleCheck, RefreshCw } from '@lucide/vue';
import { onMounted, ref } from 'vue';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
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
  <section class="flex flex-col items-start gap-3">
    <Alert v-if="health" data-testid="api-ok">
      <CircleCheck class="text-emerald-600" />
      <AlertTitle>API no ar</AlertTitle>
      <AlertDescription>
        Versão {{ health.version }}, {{ new Date(health.time).toLocaleString('pt-BR') }}
      </AlertDescription>
    </Alert>
    <Alert v-else-if="error" variant="destructive" data-testid="api-error">
      <CircleAlert />
      <AlertTitle>Não foi possível falar com a API</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>
    <Button variant="outline" size="sm" :disabled="loading" @click="load">
      <RefreshCw :class="{ 'animate-spin': loading }" />
      Verificar de novo
    </Button>
  </section>
</template>
